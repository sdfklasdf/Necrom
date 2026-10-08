using System;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class CombatantHealthRescaleAwu16Tests
    {
        private static Combatant NewUnit(int hp) =>
            new Combatant(new EntityId("permanent:test"), "test", Faction.Player, hp);

        [Test]
        public void MaxHpIncreasePreservesCurrentPercentageAndIdentity()
        {
            var unit=NewUnit(100);
            var id=unit.Id;
            unit.ApplyDamage(40);
            unit.RescaleMaxHealth(200);
            Assert.That(unit.Health,Is.EqualTo(120));
            Assert.That(unit.MaxHealth,Is.EqualTo(200));
            Assert.That(unit.Id,Is.EqualTo(id));
            Assert.That(unit.LifeState,Is.EqualTo(CombatantLifeState.Active));
        }

        [Test]
        public void MaxHpDecreasePreservesRatioAndClampsLowLivingHp()
        {
            var unit=NewUnit(100);
            unit.ApplyDamage(99);
            unit.RescaleMaxHealth(50);
            Assert.That(unit.Health,Is.EqualTo(1));
            Assert.That(unit.MaxHealth,Is.EqualTo(50));
        }

        [Test]
        public void DefeatedUnitIsNotRevivedBySkillUpgrade()
        {
            var unit=NewUnit(100);
            unit.ApplyDamage(100);
            unit.RescaleMaxHealth(200);
            Assert.That(unit.Health,Is.Zero);
            Assert.That(unit.LifeState,Is.EqualTo(CombatantLifeState.Defeated));
        }

        [Test]
        public void InvalidMaximumDoesNotChangeHp()
        {
            var unit=NewUnit(100);
            Assert.Throws<ArgumentOutOfRangeException>(()=>unit.RescaleMaxHealth(0));
            Assert.That(unit.Health,Is.EqualTo(100));
            Assert.That(unit.MaxHealth,Is.EqualTo(100));
        }
    }
}
