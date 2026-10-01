using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableAutoCombatLoopTests
    {
        private static int _raiseProviderCalls;
        private static int _commandProviderCalls;
        private static int _damageEventProviderCalls;
        private static int _defeatEventProviderCalls;

        [UnityTest]
        public IEnumerator MissingAttachedBehaviorRejectsBeforeMutation()
        {
            var f = BuildFixture(attachBehavior: false, enemyHealth: 20, damage: 5, intervalMs: 100);
            StartBattle(f);
            var enemyHealth = ReadInt(f.EnemyModel, "Health");

            var ex = Assert.Throws<TargetInvocationException>(
                () => InvokeAdvance(f, 0.1f));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(enemyHealth));
            Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Running"));
            DestroyFixture(f);
            yield return null;
        }
        [UnityTest]
        public IEnumerator BelowCadenceDoesNotAttack()
        {
            var f = BuildFixture(true, 20, 5, 100);
            StartBattle(f);

            InvokeAdvance(f, 0.099f);

            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(20));
            Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Running"));
            DestroyFixture(f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OneCadenceAttacksWithoutInput()
        {
            var f = BuildFixture(true, 20, 5, 100);
            StartBattle(f);

            InvokeAdvance(f, 0.100f);

            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(15));
            Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Running"));
            Assert.That(_damageEventProviderCalls, Is.EqualTo(1));
            Assert.That(_defeatEventProviderCalls, Is.EqualTo(0));
            DestroyFixture(f);
            yield return null;
        }
        [UnityTest]
        public IEnumerator MultipleIntervalsCatchUpAndRetainRemainder()
        {
            var f = BuildFixture(true, 50, 5, 100);
            StartBattle(f);

            InvokeAdvance(f, 0.250f);
            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(40));

            InvokeAdvance(f, 0.049f);
            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(40));

            InvokeAdvance(f, 0.001f);
            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(35));
            DestroyFixture(f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator NonRunningTimeDoesNotPreloadAttack()
        {
            var f = BuildFixture(true, 20, 5, 100);

            InvokeAdvance(f, 1.0f);
            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(20));

            StartBattle(f);
            InvokeAdvance(f, 0.050f);
            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(20));
            DestroyFixture(f);
            yield return null;
        }
        [UnityTest]
        public IEnumerator DefeatedPlayerCannotAttack()
        {
            var f = BuildFixture(true, 20, 5, 100, playerHealth: 1);
            StartBattle(f);
            InvokeMethod(f.PlayerModel, "ApplyDamage", 1);

            InvokeAdvance(f, 0.200f);

            Assert.That(ReadString(f.PlayerModel, "LifeState"), Is.EqualTo("Defeated"));
            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(20));
            Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Running"));
            DestroyFixture(f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DefeatedCurrentTargetIsNotAttackedOrReacquired()
        {
            var f = BuildFixture(true, 5, 5, 100);
            StartBattle(f);
            InvokeMethod(f.EnemyRuntime, "ApplyDamage", 5, NewEntityId("external-raise"));
            Assert.That(ReadString(f.EnemyModel, "LifeState"), Is.EqualTo("Defeated"));

            var raiseBefore = ReadProperty(f.EnemyRuntime, "RaiseSource");
            InvokeAdvance(f, 0.500f);

            Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(0));
            Assert.That(ReadProperty(f.EnemyRuntime, "RaiseSource"), Is.SameAs(raiseBefore));
            Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Running"));
            Assert.That(_damageEventProviderCalls, Is.EqualTo(0));
            Assert.That(_defeatEventProviderCalls, Is.EqualTo(0));
            DestroyFixture(f);
            yield return null;
        }
        [UnityTest]
        public IEnumerator LethalAttackCreatesOneRaiseSourceAndResolvesVictory()
        {
            var f = BuildFixture(true, 10, 10, 100);
            StartBattle(f);

            InvokeAdvance(f, 0.100f);

            Assert.That(ReadString(f.EnemyModel, "LifeState"), Is.EqualTo("Defeated"));
            Assert.That(ReadProperty(f.EnemyRuntime, "RaiseSource"), Is.Not.Null);
            Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Victory"));
            Assert.That(_raiseProviderCalls, Is.EqualTo(1));
            Assert.That(_commandProviderCalls, Is.EqualTo(1));
            Assert.That(_damageEventProviderCalls, Is.EqualTo(1));
            Assert.That(_defeatEventProviderCalls, Is.EqualTo(1));
            DestroyFixture(f);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FurtherAdvanceAfterVictoryHasNoAdditionalSideEffects()
        {
            var f = BuildFixture(true, 10, 10, 100);
            StartBattle(f);
            InvokeAdvance(f, 0.100f);
            var raiseSource = ReadProperty(f.EnemyRuntime, "RaiseSource");

            InvokeAdvance(f, 5.0f);

            Assert.That(ReadProperty(f.EnemyRuntime, "RaiseSource"), Is.SameAs(raiseSource));
            Assert.That(_raiseProviderCalls, Is.EqualTo(1));
            Assert.That(_commandProviderCalls, Is.EqualTo(1));
            Assert.That(_damageEventProviderCalls, Is.EqualTo(1));
            Assert.That(_defeatEventProviderCalls, Is.EqualTo(1));
            Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Victory"));
            DestroyFixture(f);
            yield return null;
        }
        [UnityTest]
        public IEnumerator InvalidDeltaRejectsBeforeMutation()
        {
            foreach (var delta in new[] { -0.1f, float.NaN, float.PositiveInfinity })
            {
                var f = BuildFixture(true, 20, 5, 100);
                StartBattle(f);
                var health = ReadInt(f.EnemyModel, "Health");

                var ex = Assert.Throws<TargetInvocationException>(
                    () => InvokeAdvance(f, delta));
                Assert.That(ex.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
                Assert.That(ReadInt(f.EnemyModel, "Health"), Is.EqualTo(health));
                Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Running"));
                DestroyFixture(f);
            }
            yield return null;
        }

        private static Fixture BuildFixture(
            bool attachBehavior,
            int enemyHealth,
            int damage,
            int intervalMs,
            int playerHealth = 100)
        {
            _raiseProviderCalls = 0;
            _commandProviderCalls = 0;
            _damageEventProviderCalls = 0;
            _defeatEventProviderCalls = 0;
            var loopType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableAutoCombatLoop");
            Assert.That(loopType, Is.Not.Null, "FirstPlayableAutoCombatLoop must exist.");
            var root = new GameObject("AutoCombatRoot", typeof(RectTransform));
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

            var safeArea = root.transform.Find("SafeArea");
            var combat = safeArea.Find("CombatViewport");
            var necromancerZone = combat.Find("NecromancerSpawnZone") as RectTransform;
            var enemyZone = combat.Find("EnemySpawnZone") as RectTransform;

            var necroControllerType = FindType("Necrom.FirstPlayable.Runtime.NecromancerAnchorController");
            var enemyControllerType = FindType("Necrom.FirstPlayable.Runtime.EnemySpawnController");
            var necroController = combat.gameObject.AddComponent(necroControllerType);
            var enemyController = combat.gameObject.AddComponent(enemyControllerType);
            var player = NewCombatant("player-auto-loop", "necromancer.prototype", "Player", playerHealth);
            var playerRuntime = necroControllerType.GetMethod("BindNecromancer")
                .Invoke(necroController, new[] { player, necromancerZone });

            if (attachBehavior)
            {
                var specType = FindType("Necrom.Core.Domain.BasicAutoBehaviorSpec");
                var behaviorType = FindType("Necrom.Core.Domain.NecromancerBasicAutoBehavior");
                var spec = Activator.CreateInstance(specType, damage, intervalMs);
                var behavior = Activator.CreateInstance(behaviorType, player, spec);
                playerRuntime.GetType().GetMethod("AttachAutoBehavior").Invoke(playerRuntime, new[] { behavior });
            }

            var definitionType = FindType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition");
            var definition = Activator.CreateInstance(
                definitionType,
                "enemy.skeleton.guard",
                "frontline.guard",
                enemyHealth);
            var enemyRuntime = enemyControllerType.GetMethod("SpawnEnemy")
                .Invoke(enemyController, new object[] { "enemy-auto-loop", definition, enemyZone });
            var enemyModel = ReadProperty(enemyRuntime, "Model");

            var battleType = FindType("Necrom.Core.Domain.BattleStateMachine");
            var formationType = FindType("Necrom.Core.Domain.Formation");
            var raiseServiceType = FindType("Necrom.Core.Domain.RaiseService");
            var encounterType = FindType("Necrom.Core.Domain.FirstPlayableEncounter");
            var applicationType = FindType("Necrom.Core.Application.FirstPlayableApplicationService");
            var battle = Activator.CreateInstance(battleType);
            var formation = Activator.CreateInstance(formationType);
            var raiseService = Activator.CreateInstance(raiseServiceType);
            var encounter = Activator.CreateInstance(encounterType, raiseService, battle, formation);
            var application = Activator.CreateInstance(applicationType, encounter);

            var battleRuntimeType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableBattleRuntimeController");
            var battleRuntime = root.AddComponent(battleRuntimeType);
            battleRuntimeType.GetMethod("Initialize").Invoke(battleRuntime, new[]
            {
                application,
                (object)boundary,
                config,
                (object)necroController,
                (object)enemyController
            });

            var targetingType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableTargetingController");
            Assert.That(targetingType, Is.Not.Null);
            var targeting = combat.gameObject.AddComponent(targetingType);
            targetingType.GetMethod("Initialize").Invoke(targeting, new[]
            {
                playerRuntime,
                (object)enemyController
            });

            var pipelineType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableDamageDeathPipeline");
            Assert.That(pipelineType, Is.Not.Null);
            var damageDeathPipeline = Activator.CreateInstance(
                pipelineType,
                (Func<string>)ProvideDamageEventId,
                (Func<string>)ProvideDefeatEventId);

            var loop = root.AddComponent(loopType);
            var initialize = loopType.GetMethod("Initialize");
            Assert.That(initialize, Is.Not.Null);

            var entityIdType = FindType("Necrom.Core.Domain.EntityId");
            var funcEntityType = typeof(Func<,>).MakeGenericType(entityIdType, entityIdType);
            var providerMethod = typeof(FirstPlayableAutoCombatLoopTests)
                .GetMethod(nameof(ProvideRaiseSourceId), BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(entityIdType);
            var raiseProvider = Delegate.CreateDelegate(funcEntityType, providerMethod);
            Func<string> commandProvider = ProvideCommandId;
            initialize.Invoke(loop, new object[]
            {
                battleRuntime,
                playerRuntime,
                targeting,
                damageDeathPipeline,
                raiseProvider,
                commandProvider
            });

            return new Fixture(
                root,
                loop,
                battleRuntime,
                playerRuntime,
                player,
                enemyRuntime,
                enemyModel);
        }

        private static void StartBattle(Fixture f)
        {
            var commandType = FindType("Necrom.Core.Application.StartBattleCommand");
            var command = Activator.CreateInstance(
                commandType,
                "start-auto-loop",
                ReadLong(f.BattleRuntime, "Revision"));
            f.BattleRuntime.GetType().GetMethod("StartBattle")
                .Invoke(f.BattleRuntime, new[] { command });
            Assert.That(ReadString(f.BattleRuntime, "Phase"), Is.EqualTo("Running"));
        }

        private static void InvokeAdvance(Fixture f, float seconds)
            => f.Loop.GetType().GetMethod("Advance").Invoke(f.Loop, new object[] { seconds });
        private static T ProvideRaiseSourceId<T>(T enemyId)
        {
            _raiseProviderCalls++;
            var value = enemyId.GetType().GetProperty("Value").GetValue(enemyId).ToString();
            return (T)Activator.CreateInstance(typeof(T), "raise:" + value);
        }

        private static string ProvideCommandId()
        {
            _commandProviderCalls++;
            return "resolve:auto:" + _commandProviderCalls;
        }

        private static string ProvideDamageEventId()
        {
            _damageEventProviderCalls++;
            return "damage:auto:" + _damageEventProviderCalls;
        }

        private static string ProvideDefeatEventId()
        {
            _defeatEventProviderCalls++;
            return "defeat:auto:" + _defeatEventProviderCalls;
        }

        private static object NewCombatant(string entityId, string archetype, string factionName, int health)
        {
            var idType = FindType("Necrom.Core.Domain.EntityId");
            var factionType = FindType("Necrom.Core.Domain.Faction");
            var combatantType = FindType("Necrom.Core.Domain.Combatant");
            var id = Activator.CreateInstance(idType, entityId);
            var faction = Enum.Parse(factionType, factionName);
            return Activator.CreateInstance(combatantType, id, archetype, faction, health);
        }

        private static object NewEntityId(string value)
            => Activator.CreateInstance(FindType("Necrom.Core.Domain.EntityId"), value);
        private static object InvokeMethod(object target, string name, params object[] args)
            => target.GetType().GetMethod(name).Invoke(target, args);

        private static object ReadProperty(object target, string name)
            => target.GetType().GetProperty(name).GetValue(target);

        private static int ReadInt(object target, string name)
            => (int)ReadProperty(target, name);

        private static long ReadLong(object target, string name)
            => (long)ReadProperty(target, name);

        private static string ReadString(object target, string name)
            => ReadProperty(target, name).ToString();

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);

        private static void DestroyFixture(Fixture f)
            => UnityEngine.Object.Destroy(f.Root);
        private sealed class Fixture
        {
            public GameObject Root { get; }
            public Component Loop { get; }
            public Component BattleRuntime { get; }
            public object PlayerRuntime { get; }
            public object PlayerModel { get; }
            public object EnemyRuntime { get; }
            public object EnemyModel { get; }

            public Fixture(
                GameObject root,
                Component loop,
                Component battleRuntime,
                object playerRuntime,
                object playerModel,
                object enemyRuntime,
                object enemyModel)
            {
                Root = root;
                Loop = loop;
                BattleRuntime = battleRuntime;
                PlayerRuntime = playerRuntime;
                PlayerModel = playerModel;
                EnemyRuntime = enemyRuntime;
                EnemyModel = enemyModel;
            }
        }
    }
}
