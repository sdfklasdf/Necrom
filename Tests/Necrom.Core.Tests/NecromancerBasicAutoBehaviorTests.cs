using System;
using System.Reflection;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class NecromancerBasicAutoBehaviorTests
    {
        [Test]
        public void ActivePlayerCreatesDeterministicIntentWithoutMutation()
        {
            var player = NewCombatant("player-auto-1", Faction.Player, 100);
            var behavior = NewBehavior(player, damage: 7, intervalMs: 250);
            var healthBefore = player.Health;
            var stateBefore = player.LifeState;

            var result = InvokeTryCreate(behavior, out var intent);

            Assert.That(result, Is.True);
            Assert.That(intent, Is.Not.Null);
            Assert.That(ReadEntityId(intent, "ActorId"), Is.EqualTo("player-auto-1"));
            Assert.That(ReadInt(intent, "Damage"), Is.EqualTo(7));
            Assert.That(player.Health, Is.EqualTo(healthBefore));
            Assert.That(player.LifeState, Is.EqualTo(stateBefore));
        }
        [Test]
        public void DefeatedPlayerCannotCreateIntent()
        {
            var player = NewCombatant("player-auto-2", Faction.Player, 1);
            var behavior = NewBehavior(player, damage: 5, intervalMs: 300);
            Assert.That(player.ApplyDamage(1), Is.True);
            Assert.That(player.LifeState, Is.EqualTo(CombatantLifeState.Defeated));

            var result = InvokeTryCreate(behavior, out var intent);

            Assert.That(result, Is.False);
            Assert.That(intent, Is.Null);
            Assert.That(player.Health, Is.EqualTo(0));
            Assert.That(player.LifeState, Is.EqualTo(CombatantLifeState.Defeated));
        }

        [Test]
        public void EnemyFactionIsRejected()
        {
            var enemy = NewCombatant("enemy-auto-1", Faction.Enemy, 100);
            var error = Assert.Throws<TargetInvocationException>(
                () => NewBehavior(enemy, damage: 5, intervalMs: 300));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
        }
        [TestCase(0, 300)]
        [TestCase(-1, 300)]
        [TestCase(5, 0)]
        [TestCase(5, -1)]
        public void InvalidSpecIsRejected(int damage, int intervalMs)
        {
            var specType = RequireType("Necrom.Core.Domain.BasicAutoBehaviorSpec");
            var error = Assert.Throws<TargetInvocationException>(
                () => Activator.CreateInstance(specType, damage, intervalMs));
            Assert.That(error.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void SpecExposesInjectedCadenceAndDamageWithoutUnityOrTargetSemantics()
        {
            var spec = NewSpec(damage: 9, intervalMs: 425);
            Assert.That(ReadInt(spec, "DamagePerAction"), Is.EqualTo(9));
            Assert.That(ReadInt(spec, "AttackIntervalMilliseconds"), Is.EqualTo(425));

            var intentType = RequireType("Necrom.Core.Domain.BasicAttackIntent");
            Assert.That(intentType.GetProperty("TargetId"), Is.Null);
            Assert.That(intentType.GetProperty("Timestamp"), Is.Null);
            Assert.That(intentType.GetProperty("CommandId"), Is.Null);
        }
        private static object NewBehavior(Combatant actor, int damage, int intervalMs)
        {
            var behaviorType = RequireType("Necrom.Core.Domain.NecromancerBasicAutoBehavior");
            var spec = NewSpec(damage, intervalMs);
            return Activator.CreateInstance(behaviorType, actor, spec);
        }

        private static object NewSpec(int damage, int intervalMs)
        {
            var specType = RequireType("Necrom.Core.Domain.BasicAutoBehaviorSpec");
            return Activator.CreateInstance(specType, damage, intervalMs);
        }

        private static bool InvokeTryCreate(object behavior, out object intent)
        {
            var method = behavior.GetType().GetMethod("TryCreateBasicAttack");
            Assert.That(method, Is.Not.Null);
            var args = new object[] { null };
            var result = (bool)method.Invoke(behavior, args);
            intent = args[0];
            return result;
        }
        private static Type RequireType(string name)
        {
            var type = typeof(Combatant).Assembly.GetType(name);
            Assert.That(type, Is.Not.Null, name + " must exist.");
            return type;
        }

        private static string ReadEntityId(object target, string propertyName)
        {
            var entityId = target.GetType().GetProperty(propertyName)?.GetValue(target);
            Assert.That(entityId, Is.Not.Null);
            return entityId.GetType().GetProperty("Value")?.GetValue(entityId)?.ToString();
        }

        private static int ReadInt(object target, string propertyName)
            => (int)target.GetType().GetProperty(propertyName).GetValue(target);

        private static Combatant NewCombatant(string id, Faction faction, int health)
            => new Combatant(new EntityId(id), "necromancer.prototype", faction, health);
    }
}
