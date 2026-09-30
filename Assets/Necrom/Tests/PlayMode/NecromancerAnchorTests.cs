using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class NecromancerAnchorTests
    {
        [UnityTest]
        public IEnumerator PlayerCombatantBindsUnderNecromancerZoneWithStableIdentityAndPosition()
        {
            var runtimeType = FindType("Necrom.FirstPlayable.Runtime.NecromancerRuntimeEntity");
            var controllerType = FindType("Necrom.FirstPlayable.Runtime.NecromancerAnchorController");
            Assert.That(runtimeType, Is.Not.Null, "Necromancer runtime entity type must exist.");
            Assert.That(controllerType, Is.Not.Null, "Necromancer anchor controller type must exist.");

            var combatRoot = new GameObject("CombatViewport", typeof(RectTransform));
            var zone = new GameObject("NecromancerSpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(combatRoot.transform, false);
            var controller = combatRoot.AddComponent(controllerType);
            var player = NewCombatant("player-combat-1", "necromancer.prototype", "Player", 100);

            var bind = controllerType.GetMethod("BindNecromancer", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(bind, Is.Not.Null);
            var runtime = bind.Invoke(controller, new object[] { player, zone });
            yield return null;

            Assert.That(runtime, Is.Not.Null);
            var component = (Component)runtime;
            Assert.That(component.transform.parent, Is.EqualTo(zone));
            Assert.That(component.gameObject.name, Is.EqualTo("Necromancer:player-combat-1"));

            var rect = component.transform as RectTransform;
            Assert.That(rect, Is.Not.Null);
            Assert.That(rect.anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(rect.localScale, Is.EqualTo(Vector3.one));
            Assert.That(rect.localRotation, Is.EqualTo(Quaternion.identity));

            var model = runtimeType.GetProperty("Model")?.GetValue(runtime);
            Assert.That(model, Is.SameAs(player));
            var id = model.GetType().GetProperty("Id")?.GetValue(model);
            Assert.That(id.GetType().GetProperty("Value")?.GetValue(id), Is.EqualTo("player-combat-1"));
            Assert.That(model.GetType().GetProperty("Faction")?.GetValue(model)?.ToString(), Is.EqualTo("Player"));

            var current = controllerType.GetProperty("CurrentNecromancer")?.GetValue(controller);
            Assert.That(current, Is.SameAs(runtime));

            Assert.That(component.GetComponent<SpriteRenderer>(), Is.Null);
            Assert.That(component.GetComponent<Animator>(), Is.Null);
            Assert.That(component.GetComponent<AudioSource>(), Is.Null);
            Assert.That(component.GetComponent<ParticleSystem>(), Is.Null);

            foreach (var forbidden in new[] { "OwnerId", "AccountId", "ProfileId" })
            {
                Assert.That(runtimeType.GetProperty(forbidden), Is.Null, forbidden);
                Assert.That(controllerType.GetProperty(forbidden), Is.Null, forbidden);
            }

            UnityEngine.Object.Destroy(combatRoot);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EnemyFactionIsRejectedBeforeSceneMutation()
        {
            var controllerType = FindType("Necrom.FirstPlayable.Runtime.NecromancerAnchorController");
            Assert.That(controllerType, Is.Not.Null, "Necromancer anchor controller type must exist.");

            var combatRoot = new GameObject("CombatViewport", typeof(RectTransform));
            var zone = new GameObject("NecromancerSpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(combatRoot.transform, false);
            var controller = combatRoot.AddComponent(controllerType);
            var enemy = NewCombatant("enemy-should-reject", "enemy.skeleton", "Enemy", 50);
            var bind = controllerType.GetMethod("BindNecromancer", BindingFlags.Instance | BindingFlags.Public);

            var before = zone.childCount;
            var ex = Assert.Throws<TargetInvocationException>(
                () => bind.Invoke(controller, new object[] { enemy, zone }));

            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(zone.childCount, Is.EqualTo(before));
            Assert.That(controllerType.GetProperty("CurrentNecromancer")?.GetValue(controller), Is.Null);

            UnityEngine.Object.Destroy(combatRoot);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DuplicateBindIsRejectedWithoutCreatingSecondAnchor()
        {
            var controllerType = FindType("Necrom.FirstPlayable.Runtime.NecromancerAnchorController");
            Assert.That(controllerType, Is.Not.Null, "Necromancer anchor controller type must exist.");

            var combatRoot = new GameObject("CombatViewport", typeof(RectTransform));
            var zone = new GameObject("NecromancerSpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(combatRoot.transform, false);
            var controller = combatRoot.AddComponent(controllerType);
            var player = NewCombatant("player-combat-1", "necromancer.prototype", "Player", 100);
            var bind = controllerType.GetMethod("BindNecromancer", BindingFlags.Instance | BindingFlags.Public);

            var first = bind.Invoke(controller, new object[] { player, zone });
            Assert.That(first, Is.Not.Null);
            var before = zone.childCount;

            var secondPlayer = NewCombatant("player-combat-2", "necromancer.prototype", "Player", 100);
            var ex = Assert.Throws<TargetInvocationException>(
                () => bind.Invoke(controller, new object[] { secondPlayer, zone }));

            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(zone.childCount, Is.EqualTo(before));

            UnityEngine.Object.Destroy(combatRoot);
            yield return null;
        }

        private static object NewCombatant(string entityId, string archetypeId, string factionName, int health)
        {
            var entityIdType = FindType("Necrom.Core.Domain.EntityId");
            var factionType = FindType("Necrom.Core.Domain.Faction");
            var combatantType = FindType("Necrom.Core.Domain.Combatant");
            Assert.That(entityIdType, Is.Not.Null);
            Assert.That(factionType, Is.Not.Null);
            Assert.That(combatantType, Is.Not.Null);

            var id = Activator.CreateInstance(entityIdType, entityId);
            var faction = Enum.Parse(factionType, factionName);
            return Activator.CreateInstance(combatantType, id, archetypeId, faction, health);
        }

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);
    }
}
