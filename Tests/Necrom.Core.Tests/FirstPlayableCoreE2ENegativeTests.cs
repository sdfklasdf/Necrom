using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class FirstPlayableCoreE2ENegativeTests
    {
        [Test]
        public void OccupiedFormationSlotRejectsFullRaisePathWithoutConsumingSource()
        {
            var enemy = new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, 5);
            enemy.ApplyDamage(5);
            var source = new RaiseSource(new EntityId("source-1"), enemy);

            var formation = new Formation();
            formation.Assign(0, new EntityId("existing-unit"), 0);

            var encounter = new FirstPlayableEncounter(
                new RaiseService(), new BattleStateMachine(), formation);
            var app = new FirstPlayableApplicationService(encounter);
            var progression = new FirstPlayableProgression(app, encounter);

            var command = new RaiseIntoFormationCommand(
                "raise-1", source, 0, new EntityId("undead-1"), 7, 0, 1);

            Assert.Throws<InvalidOperationException>(() =>
                progression.RaiseForNextCombat(command, "event-raised", "event-assigned"));

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
            Assert.That(formation.GetSlot(0).Value.Value, Is.EqualTo("existing-unit"));
            Assert.That(formation.Revision, Is.EqualTo(1));
            Assert.That(encounter.IsReadyForNextCombat(new EntityId("undead-1")), Is.False);
        }
    }
}
