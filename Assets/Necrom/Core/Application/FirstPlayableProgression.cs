using System;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;

namespace Necrom.Core.Application
{
    public sealed class FirstPlayableProgression
    {
        private readonly FirstPlayableApplicationService _application;
        private readonly FirstPlayableEncounter _encounter;

        public FirstPlayableProgression(FirstPlayableApplicationService application, FirstPlayableEncounter encounter)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _encounter = encounter ?? throw new ArgumentNullException(nameof(encounter));
        }

        public CommandResult RaiseForNextCombat(RaiseIntoFormationCommand command, string raisedEventId, string assignedEventId)
        {
            var result = _application.Execute(command, raisedEventId, assignedEventId);
            if (!_encounter.IsReadyForNextCombat(command.UndeadId))
                throw new InvalidOperationException("Raised unit was not committed to the next-combat formation.");
            return result;
        }

        public GameStateSnapshot CaptureCheckpoint(string schemaVersion)
            => GameStateSnapshot.Capture(schemaVersion, _encounter.Battle, _encounter.Formation);
    }
}
