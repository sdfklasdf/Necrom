using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableTargetingControllerTests
    {
        [UnityTest]
        public IEnumerator ControllerTypeExists()
        {
            Assert.That(TargetingType(), Is.Not.Null, "FirstPlayableTargetingController must exist.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActiveCurrentTargetIsAcquiredWithExactIdentity()
        {
            var f = BuildFixture(true, 20);
            Assert.That(TryAcquire(f.Targeting, out var target), Is.True);
            Assert.That(target, Is.SameAs(f.Enemy));
            Assert.That(IdValue(CurrentTargetId(f.Targeting)), Is.EqualTo(IdValue(Read(f.EnemyModel, "Id"))));
            Destroy(f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MissingCurrentTargetReturnsFalseAndNull()
        {
            var f = BuildFixture(false, 20);
            Assert.That(TryAcquire(f.Targeting, out var target), Is.False);
            Assert.That(target, Is.Null);
            Assert.That(CurrentTargetId(f.Targeting), Is.Null);
            Destroy(f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UninitializedCurrentTargetIsRejectedWithoutSourceMutation()
        {
            var f = BuildFixture(false, 20);
            var invalidObject = new GameObject("InvalidEnemy");
            var invalid = invalidObject.AddComponent(FindType("Necrom.FirstPlayable.Runtime.EnemyRuntimeEntity"));
            invalidObject.transform.SetParent(f.EnemyZone, false);
            f.Enemies.GetType().GetProperty("CurrentTarget").GetSetMethod(true)
                .Invoke(f.Enemies, new object[] { invalid });

            Assert.That(TryAcquire(f.Targeting, out var target), Is.False);
            Assert.That(target, Is.Null);
            Assert.That(Read(f.Enemies, "CurrentTarget"), Is.SameAs(invalid));
            Assert.That(CurrentTargetId(f.Targeting), Is.Null);
            Destroy(f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DefeatedCurrentTargetIsRejected()
        {
            var f = BuildFixture(true, 5);
            f.Enemy.GetType().GetMethod("ApplyDamage")
                .Invoke(f.Enemy, new object[] { 5, NewEntityId("raise:test") });
            Assert.That(Read(f.EnemyModel, "LifeState").ToString(), Is.EqualTo("Defeated"));
            Assert.That(TryAcquire(f.Targeting, out var target), Is.False);
            Assert.That(target, Is.Null);
            Assert.That(CurrentTargetId(f.Targeting), Is.Null);
            Assert.That(Read(f.Enemies, "CurrentTarget"), Is.SameAs(f.Enemy));
            Destroy(f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RepeatedAcquisitionHoldsSpawnAnchorsAndActorTransforms()
        {
            var f = BuildFixture(true, 20);
            var before = new[]
            {
                Snap(((Component)f.Player).transform),
                Snap(((Component)f.Enemy).transform),
                Snap(f.NecromancerZone),
                Snap(f.EnemyZone)
            };
            for (var i = 0; i < 5; i++)
                Assert.That(TryAcquire(f.Targeting, out _), Is.True);
            var after = new[]
            {
                Snap(((Component)f.Player).transform),
                Snap(((Component)f.Enemy).transform),
                Snap(f.NecromancerZone),
                Snap(f.EnemyZone)
            };
            Assert.That(after, Is.EqualTo(before));
            Destroy(f);
            yield return null;
        }

        private static Fixture BuildFixture(bool spawnEnemy, int enemyHealth)
        {
            var targetingType = TargetingType();
            Assert.That(targetingType, Is.Not.Null, "FirstPlayableTargetingController must exist.");

            var root = new GameObject("TargetingRoot", typeof(RectTransform));
            var boundaryType = FindType("Necrom.FirstPlayable.Runtime.EncounterBoundaryController");
            var boundary = root.AddComponent(boundaryType);
            var configType = FindType("Necrom.FirstPlayable.Runtime.EncounterLayoutConfig");
            var config = Activator.CreateInstance(configType, new object[]
            {
                new Vector2(390f, 844f),
                new Rect(0f, 34f, 390f, 776f),
                0.22f,
                new Rect(0.12f, 0.08f, 0.30f, 0.24f),
                new Rect(0.58f, 0.58f, 0.30f, 0.28f),
                new Rect(0.12f, 0.34f, 0.44f, 0.18f)
            });
            boundaryType.GetMethod("StartBoundary").Invoke(boundary, new[] { config });

            var combat = root.transform.Find("SafeArea").Find("CombatViewport");
            var necroZone = (RectTransform)combat.Find("NecromancerSpawnZone");
            var enemyZone = (RectTransform)combat.Find("EnemySpawnZone");
            var necroType = FindType("Necrom.FirstPlayable.Runtime.NecromancerAnchorController");
            var enemiesType = FindType("Necrom.FirstPlayable.Runtime.EnemySpawnController");
            var necro = combat.gameObject.AddComponent(necroType);
            var enemies = combat.gameObject.AddComponent(enemiesType);
            var playerModel = NewCombatant("player-targeting", "necromancer.prototype", "Player", 100);
            var player = necroType.GetMethod("BindNecromancer").Invoke(necro, new[] { playerModel, necroZone });

            object enemy = null;
            object enemyModel = null;
            if (spawnEnemy)
            {
                var defType = FindType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition");
                var def = Activator.CreateInstance(defType, "enemy.skeleton.guard", "frontline.guard", enemyHealth);
                enemy = enemiesType.GetMethod("SpawnEnemy")
                    .Invoke(enemies, new object[] { "enemy-targeting", def, enemyZone });
                enemyModel = Read(enemy, "Model");
            }

            var targeting = combat.gameObject.AddComponent(targetingType);
            targetingType.GetMethod("Initialize").Invoke(targeting, new[] { player, (object)enemies });
            return new Fixture(root, targeting, player, enemies, enemy, enemyModel, necroZone, enemyZone);
        }

        private static Type TargetingType()
            => FindType("Necrom.FirstPlayable.Runtime.FirstPlayableTargetingController");

        private static Type FindType(string name)
            => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

        private static bool TryAcquire(Component targeting, out object target)
        {
            var args = new object[] { null };
            var result = (bool)targeting.GetType().GetMethod("TryAcquireTarget").Invoke(targeting, args);
            target = args[0];
            return result;
        }

        private static object CurrentTargetId(Component targeting)
            => targeting.GetType().GetProperty("CurrentTargetId").GetValue(targeting);

        private static object NewEntityId(string value)
            => Activator.CreateInstance(FindType("Necrom.Core.Domain.EntityId"), value);

        private static object NewCombatant(string id, string archetype, string faction, int health)
        {
            var idType = FindType("Necrom.Core.Domain.EntityId");
            var factionType = FindType("Necrom.Core.Domain.Faction");
            var combatantType = FindType("Necrom.Core.Domain.Combatant");
            return Activator.CreateInstance(
                combatantType,
                Activator.CreateInstance(idType, id),
                archetype,
                Enum.Parse(factionType, faction),
                health);
        }

        private static object Read(object target, string property)
            => target.GetType().GetProperty(property).GetValue(target);

        private static string IdValue(object id)
            => id == null ? null : Read(id, "Value").ToString();

        private static Snapshot Snap(Transform t)
            => new Snapshot(t.localPosition, t.localRotation, t.localScale);

        private static void Destroy(Fixture f)
            => UnityEngine.Object.Destroy(f.Root);

        private readonly struct Snapshot : IEquatable<Snapshot>
        {
            private readonly Vector3 _position;
            private readonly Quaternion _rotation;
            private readonly Vector3 _scale;
            public Snapshot(Vector3 p, Quaternion r, Vector3 s)
            {
                _position = p;
                _rotation = r;
                _scale = s;
            }
            public bool Equals(Snapshot other)
                => _position == other._position && _rotation == other._rotation && _scale == other._scale;
            public override bool Equals(object obj) => obj is Snapshot other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(_position, _rotation, _scale);
        }

        private sealed class Fixture
        {
            public GameObject Root { get; }
            public Component Targeting { get; }
            public object Player { get; }
            public Component Enemies { get; }
            public object Enemy { get; }
            public object EnemyModel { get; }
            public RectTransform NecromancerZone { get; }
            public RectTransform EnemyZone { get; }
            public Fixture(
                GameObject root,
                Component targeting,
                object player,
                Component enemies,
                object enemy,
                object enemyModel,
                RectTransform necromancerZone,
                RectTransform enemyZone)
            {
                Root = root;
                Targeting = targeting;
                Player = player;
                Enemies = enemies;
                Enemy = enemy;
                EnemyModel = enemyModel;
                NecromancerZone = necromancerZone;
                EnemyZone = enemyZone;
            }
        }
    }
}