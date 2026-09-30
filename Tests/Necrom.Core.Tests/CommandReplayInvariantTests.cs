using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class CommandReplayInvariantTests
    {
        [Test]
        public void InvalidRaisedEventIdRejectsBeforeRaiseMutation()
        {
            var source = NewSource();
            var formation = new Formation();
            var service = NewService(formation);
            var command = NewRaiseCommand(source, 0, 0);

            Assert.Throws<ArgumentException>(() =>
                service.Execute(command, " ", "event-assigned"));

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
            Assert.That(formation.GetSlot(0).HasValue, Is.False);
            Assert.That(formation.Revision, Is.EqualTo(0));
        }

        [Test]
        public void InvalidAssignedEventIdRejectsBeforeRaiseMutation()
        {
            var source = NewSource();
            var formation = new Formation();
            var service = NewService(formation);
            var command = NewRaiseCommand(source, 0, 0);

            Assert.Throws<ArgumentException>(() =>
                service.Execute(command, "event-raised", " "));

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
            Assert.That(formation.GetSlot(0).HasValue, Is.False);
            Assert.That(formation.Revision, Is.EqualTo(0));
        }

        [Test]
        public void RetryingSameRaiseCommandCannotCreateSecondLocalEffect()
        {
            var source = NewSource();
            var formation = new Formation();
            var service = NewService(formation);
            var command = NewRaiseCommand(source, 0, 0);

            var first = service.Execute(command, "event-raised-1", "event-assigned-1");

            Assert.That(first.Events.Count, Is.EqualTo(2));
            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Consumed));
            Assert.That(source.Revision, Is.EqualTo(1));
            Assert.That(formation.Revision, Is.EqualTo(1));

            Assert.Throws<InvalidOperationException>(() =>
                service.Execute(command, "event-raised-2", "event-assigned-2"));

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Consumed));
            Assert.That(source.Revision, Is.EqualTo(1));
            Assert.That(formation.GetSlot(0).Value.Value, Is.EqualTo("undead-1"));
            Assert.That(formation.Revision, Is.EqualTo(1));
        }

        [Test]
        public void LocalQueueSerializesBattleCommandsAgainstRevisions()
        {
            var battle = new BattleStateMachine();
            var encounter = new FirstPlayableEncounter(new RaiseService(), battle, new Formation());
            var service = new FirstPlayableApplicationService(encounter);
            var queue = new LocalSerializedCommandQueue();

            queue.Enqueue(() => service.Execute(new StartBattleCommand("start-1", 0)));
            queue.Enqueue(() => service.Execute(new ResolveBattleCommand("resolve-1", true, 1)));

            queue.Drain();

            Assert.That(queue.PendingCount, Is.EqualTo(0));
            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Victory));
            Assert.That(battle.Revision, Is.EqualTo(2));
        }

        [Test]
        public void ReplayingStartBattleWithOriginalRevisionCannotStartTwice()
        {
            var battle = new BattleStateMachine();
            var encounter = new FirstPlayableEncounter(new RaiseService(), battle, new Formation());
            var service = new FirstPlayableApplicationService(encounter);
            var command = new StartBattleCommand("start-1", 0);

            service.Execute(command);

            Assert.Throws<InvalidOperationException>(() => service.Execute(command));
            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Running));
            Assert.That(battle.Revision, Is.EqualTo(1));
        }

        private static FirstPlayableApplicationService NewService(Formation formation)
        {
            var encounter = new FirstPlayableEncounter(new RaiseService(), new BattleStateMachine(), formation);
            return new FirstPlayableApplicationService(encounter);
        }

        private static RaiseIntoFormationCommand NewRaiseCommand(RaiseSource source, long sourceRevision, long formationRevision)
            => new RaiseIntoFormationCommand(
                "raise-1",
                source,
                sourceRevision,
                new EntityId("undead-1"),
                7,
                0,
                formationRevision);

        private static RaiseSource NewSource()
        {
            var defeated = new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, 5);
            defeated.ApplyDamage(5);
            return new RaiseSource(new EntityId("source-1"), defeated);
        }
    }
}
