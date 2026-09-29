using System;
using System.Threading;
using System.Threading.Tasks;
using Necrom.Core.Persistence;

namespace Necrom.Core.Application
{
    public sealed class FirstPlayableCheckpointService
    {
        private readonly FirstPlayableProgression _progression;
        private readonly GameStatePersistenceService _persistence;

        public FirstPlayableCheckpointService(FirstPlayableProgression progression, GameStatePersistenceService persistence)
        {
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        }

        public ValueTask SaveAsync(string playerId, string schemaVersion, CancellationToken cancellationToken = default)
        {
            var snapshot = _progression.CaptureCheckpoint(schemaVersion);
            return _persistence.SaveAsync(playerId, snapshot, cancellationToken);
        }
    }
}
