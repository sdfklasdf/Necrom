using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class NecromancerAutoBehaviorHookTests
    {
        [UnityTest]
        public IEnumerator ExactModelBehaviorAttachesAndIsReadable()
        {
            var runtimeType = RequireType("Necrom.FirstPlayable.Runtime.NecromancerRuntimeEntity");
            var go = new GameObject("NecromancerAutoHook");
            var runtime = go.AddComponent(runtimeType);
            var player = NewCombatant("player-hook-1", "Player", 100);
            runtimeType.GetMethod("Initialize").Invoke(runtime, new[] { player });
            var behavior = NewBehavior(player);

            var property = runtimeType.GetProperty("AutoBehavior");
            var attach = runtimeType.GetMethod("AttachAutoBehavior");
            Assert.That(property, Is.Not.Null, "AutoBehavior read seam must exist.");
            Assert.That(attach, Is.Not.Null, "AttachAutoBehavior must exist.");
            attach.Invoke(runtime, new[] { behavior });

            Assert.That(property.GetValue(runtime), Is.SameAs(behavior));
            UnityEngine.Object.Destroy(go);
            yield return null;
        }
        [UnityTest]
        public IEnumerator AttachBeforeModelInitializationIsRejected()
        {
            var runtimeType = RequireType("Necrom.FirstPlayable.Runtime.NecromancerRuntimeEntity");
            var go = new GameObject("NecromancerAutoHook");
            var runtime = go.AddComponent(runtimeType);
            var player = NewCombatant("player-hook-2", "Player", 100);
            var behavior = NewBehavior(player);
            var attach = runtimeType.GetMethod("AttachAutoBehavior");
            Assert.That(attach, Is.Not.Null, "AttachAutoBehavior must exist.");

            var error = Assert.Throws<TargetInvocationException>(
                () => attach.Invoke(runtime, new[] { behavior }));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(runtimeType.GetProperty("AutoBehavior")?.GetValue(runtime), Is.Null);

            UnityEngine.Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MismatchedActorIsRejectedWithoutMutation()
        {
            var runtimeType = RequireType("Necrom.FirstPlayable.Runtime.NecromancerRuntimeEntity");
            var go = new GameObject("NecromancerAutoHook");
            var runtime = go.AddComponent(runtimeType);
            var bound = NewCombatant("player-bound", "Player", 100);
            var other = NewCombatant("player-other", "Player", 100);
            runtimeType.GetMethod("Initialize").Invoke(runtime, new[] { bound });
            var behavior = NewBehavior(other);
            var attach = runtimeType.GetMethod("AttachAutoBehavior");
            Assert.That(attach, Is.Not.Null, "AttachAutoBehavior must exist.");
            var error = Assert.Throws<TargetInvocationException>(
                () => attach.Invoke(runtime, new[] { behavior }));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(runtimeType.GetProperty("AutoBehavior")?.GetValue(runtime), Is.Null);

            UnityEngine.Object.Destroy(go);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DuplicateAttachIsRejectedWithoutReplacement()
        {
            var runtimeType = RequireType("Necrom.FirstPlayable.Runtime.NecromancerRuntimeEntity");
            var go = new GameObject("NecromancerAutoHook");
            var runtime = go.AddComponent(runtimeType);
            var player = NewCombatant("player-hook-3", "Player", 100);
            runtimeType.GetMethod("Initialize").Invoke(runtime, new[] { player });
            var first = NewBehavior(player, damage: 5, intervalMs: 300);
            var second = NewBehavior(player, damage: 8, intervalMs: 450);
            var attach = runtimeType.GetMethod("AttachAutoBehavior");
            var property = runtimeType.GetProperty("AutoBehavior");
            Assert.That(attach, Is.Not.Null, "AttachAutoBehavior must exist.");
            Assert.That(property, Is.Not.Null, "AutoBehavior read seam must exist.");
            attach.Invoke(runtime, new[] { first });
            var error = Assert.Throws<TargetInvocationException>(
                () => attach.Invoke(runtime, new[] { second }));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(property.GetValue(runtime), Is.SameAs(first));

            UnityEngine.Object.Destroy(go);
            yield return null;
        }

        private static object NewBehavior(object player, int damage = 7, int intervalMs = 250)
        {
            var specType = RequireType("Necrom.Core.Domain.BasicAutoBehaviorSpec");
            var behaviorType = RequireType("Necrom.Core.Domain.NecromancerBasicAutoBehavior");
            var spec = Activator.CreateInstance(specType, damage, intervalMs);
            return Activator.CreateInstance(behaviorType, player, spec);
        }

        private static object NewCombatant(string id, string factionName, int health)
        {
            var entityIdType = RequireType("Necrom.Core.Domain.EntityId");
            var factionType = RequireType("Necrom.Core.Domain.Faction");
            var combatantType = RequireType("Necrom.Core.Domain.Combatant");
            var entityId = Activator.CreateInstance(entityIdType, id);
            var faction = Enum.Parse(factionType, factionName);
            return Activator.CreateInstance(
                combatantType,
                entityId,
                "necromancer.prototype",
                faction,
                health);
        }
        private static Type RequireType(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, fullName + " must exist.");
            return type;
        }
    }
}
