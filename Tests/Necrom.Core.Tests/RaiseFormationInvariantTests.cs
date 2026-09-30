using System;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class RaiseFormationInvariantTests
    {
        [Test]
        public void SuccessfulRaisePreservesArchetypeCreatesPlayerUnitAndAssignsFormation()
        {
            var source = NewSource("skeleton");
            var formation = new Formation();
            var encounter = NewEncounter(formation);
            var undeadId = new EntityId("undead-1");

            var result = encounter.RaiseIntoFormation(source, 0, undeadId, 7, 2, 0);

            Assert.That(result.Undead.Id, Is.EqualTo(undeadId));
            Assert.That(result.Undead.ArchetypeId, Is.EqualTo("skeleton"));
            Assert.That(result.Undead.Faction, Is.EqualTo(Faction.Player));
            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Consumed));
            Assert.That(source.Revision, Is.EqualTo(1));
            Assert.That(formation.GetSlot(2).Value, Is.EqualTo(undeadId));
            Assert.That(formation.Revision, Is.EqualTo(1));
            Assert.That(encounter.IsReadyForNextCombat(undeadId), Is.True);
        }

        [Test]
        public void OccupiedSlotPreflightRejectsBeforeSourceConsumption()
        {
            var source = NewSource("skeleton");
            var formation = new Formation();
            formation.Assign(0, new EntityId("existing-unit"), 0);
            var encounter = NewEncounter(formation);

            Assert.Throws<InvalidOperationException>(() =>
                encounter.RaiseIntoFormation(source, 0, new EntityId("undead-1"), 7, 0, 1));

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
            Assert.That(formation.GetSlot(0).Value.Value, Is.EqualTo("existing-unit"));
            Assert.That(formation.Revision, Is.EqualTo(1));
        }

        [Test]
        public void StaleFormationRevisionRejectsBeforeSourceConsumption()
        {
            var source = NewSource("skeleton");
            var formation = new Formation();
            formation.Assign(0, new EntityId("existing-unit"), 0);
            var encounter = NewEncounter(formation);

            Assert.Throws<InvalidOperationException>(() =>
                encounter.RaiseIntoFormation(source, 0, new EntityId("undead-1"), 7, 1, 0));

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
            Assert.That(formation.GetSlot(1).HasValue, Is.False);
            Assert.That(formation.Revision, Is.EqualTo(1));
        }

        [Test]
        public void StaleSourceRevisionRejectsWithoutFormationMutation()
        {
            var source = NewSource("skeleton");
            var formation = new Formation();
            var encounter = NewEncounter(formation);

            Assert.Throws<InvalidOperationException>(() =>
                encounter.RaiseIntoFormation(source, 1, new EntityId("undead-1"), 7, 1, 0));

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
            Assert.That(formation.GetSlot(1).HasValue, Is.False);
            Assert.That(formation.Revision, Is.EqualTo(0));
        }

        private static FirstPlayableEncounter NewEncounter(Formation formation)
            => new FirstPlayableEncounter(new RaiseService(), new BattleStateMachine(), formation);

        private static RaiseSource NewSource(string archetype)
        {
            var defeated = new Combatant(new EntityId("enemy-1"), archetype, Faction.Enemy, 5);
            defeated.ApplyDamage(5);
            return new RaiseSource(new EntityId("source-1"), defeated, "frontline.guard");
        }
    }
}
