using System;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class CombatantInvariantTests
    {
        [Test]
        public void PositiveDamageReducesHealth()
        {
            var combatant = NewCombatant(10);

            var changed = combatant.ApplyDamage(3);

            Assert.That(changed, Is.True);
            Assert.That(combatant.Health, Is.EqualTo(7));
            Assert.That(combatant.LifeState, Is.EqualTo(CombatantLifeState.Active));
        }

        [Test]
        public void LethalDamageDefeatsAtZeroHealth()
        {
            var combatant = NewCombatant(5);

            combatant.ApplyDamage(9);

            Assert.That(combatant.Health, Is.EqualTo(0));
            Assert.That(combatant.LifeState, Is.EqualTo(CombatantLifeState.Defeated));
        }

        [Test]
        public void DamageAfterDefeatHasNoSecondEffect()
        {
            var combatant = NewCombatant(5);
            combatant.ApplyDamage(5);

            var changed = combatant.ApplyDamage(1);

            Assert.That(changed, Is.False);
            Assert.That(combatant.Health, Is.EqualTo(0));
            Assert.That(combatant.LifeState, Is.EqualTo(CombatantLifeState.Defeated));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveDamageIsRejected(int amount)
        {
            var combatant = NewCombatant(5);

            Assert.Throws<ArgumentOutOfRangeException>(() => combatant.ApplyDamage(amount));
            Assert.That(combatant.Health, Is.EqualTo(5));
            Assert.That(combatant.LifeState, Is.EqualTo(CombatantLifeState.Active));
        }

        private static Combatant NewCombatant(int health)
            => new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, health);
    }
}
