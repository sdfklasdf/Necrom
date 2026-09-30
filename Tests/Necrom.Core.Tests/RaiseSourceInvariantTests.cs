using System;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class RaiseSourceInvariantTests
    {
        [Test]
        public void OnlyDefeatedCombatantCanBecomeRaiseSource()
        {
            var active = new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, 5);

            Assert.Throws<InvalidOperationException>(() =>
                new RaiseSource(new EntityId("source-1"), active));
        }

        [Test]
        public void DefeatedCombatantCreatesAvailableRaiseSource()
        {
            var defeated = DefeatedCombatant();

            var source = new RaiseSource(new EntityId("source-1"), defeated);

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
            Assert.That(source.ArchetypeId, Is.EqualTo("skeleton"));
            Assert.That(source.DefeatedEntityId, Is.EqualTo(defeated.Id));
        }

        [Test]
        public void ConsumeOccursExactlyOnce()
        {
            var source = NewSource();

            source.Consume(0);

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Consumed));
            Assert.That(source.Revision, Is.EqualTo(1));
            Assert.Throws<InvalidOperationException>(() => source.Consume(1));
            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Consumed));
            Assert.That(source.Revision, Is.EqualTo(1));
        }

        [Test]
        public void StaleRevisionIsRejectedWithoutMutation()
        {
            var source = NewSource();

            Assert.Throws<InvalidOperationException>(() => source.Consume(1));
            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
        }

        private static RaiseSource NewSource()
            => new RaiseSource(new EntityId("source-1"), DefeatedCombatant());

        private static Combatant DefeatedCombatant()
        {
            var combatant = new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, 5);
            combatant.ApplyDamage(5);
            return combatant;
        }
    }
}
