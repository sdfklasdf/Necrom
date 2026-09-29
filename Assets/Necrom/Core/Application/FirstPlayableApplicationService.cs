using System;
using System.Collections.Generic;
using Necrom.Core.Domain;

namespace Necrom.Core.Application
{
    public sealed class CommandResult
    {
        public string CommandId { get; }
        public IReadOnlyList<DomainEvent> Events { get; }

        public CommandResult(string commandId, IReadOnlyList<DomainEvent> events)
        {
            if (string.IsNullOrWhiteSpace(commandId)) throw new ArgumentException("Command id is required.", nameof(commandId));
            CommandId = commandId;
            Events = events ?? throw new ArgumentNullException(nameof(events));
        }
    }

    public sealed class FirstPlayableApplicationService
    {
        private readonly FirstPlayableEncounter _encounter;

        public FirstPlayableApplicationService(FirstPlayableEncounter encounter)
            => _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));

        public CommandResult Execute(RaiseIntoFormationCommand command, string raisedEventId, string assignedEventId)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            // Validate all presentation-independent result metadata before mutating domain state.
            // This prevents a malformed event id from turning a successful mutation into an apparent failed command.
            if (string.IsNullOrWhiteSpace(raisedEventId)) throw new ArgumentException("Raised event id is required.", nameof(raisedEventId));
            if (string.IsNullOrWhiteSpace(assignedEventId)) throw new ArgumentException("Assigned event id is required.", nameof(assignedEventId));

            var raised = _encounter.RaiseIntoFormation(
                command.Source,
                command.ExpectedSourceRevision,
                command.UndeadId,
                command.RestoredHealth,
                command.Slot,
                command.ExpectedFormationRevision);

            var events = new DomainEvent[]
            {
                new UnitRaised(raisedEventId, raised.SourceId, raised.Undead.Id, raised.Undead.ArchetypeId),
                new UnitAssignedToFormation(assignedEventId, raised.Undead.Id, command.Slot)
            };
            return new CommandResult(command.CommandId, Array.AsReadOnly(events));
        }

        public CommandResult Execute(StartBattleCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _encounter.Battle.Start(command.ExpectedBattleRevision);
            return new CommandResult(command.CommandId, Array.AsReadOnly(new DomainEvent[0]));
        }

        public CommandResult Execute(ResolveBattleCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _encounter.Battle.Resolve(command.PlayerWon, command.ExpectedBattleRevision);
            return new CommandResult(command.CommandId, Array.AsReadOnly(new DomainEvent[0]));
        }
    }
}
