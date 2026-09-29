using System;
using System.Threading;
using System.Threading.Tasks;
using Necrom.Core.Persistence;

namespace Necrom.Core.Application
{
    public sealed class FirstPlayableBootstrapResult
    {
        public HydratedGameState State { get; }
        public bool IsNew { get; }

        public FirstPlayableBootstrapResult(HydratedGameState state, bool isNew)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            IsNew = isNew;
        }
    }

    public sealed class FirstPlayableBootstrap
    {
        private readonly GameStatePersistenceService _persistence;
        private readonly Func<HydratedGameState> _newStateFactory;

        public FirstPlayableBootstrap(GameStatePersistenceService persistence, Func<HydratedGameState> newStateFactory)
        {
            _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            _newStateFactory = newStateFactory ?? throw new ArgumentNullException(nameof(newStateFactory));
        }

        public async ValueTask<FirstPlayableBootstrapResult> LoadOrNewAsync(
            string playerId,
            string targetSchemaVersion,
            CancellationToken cancellationToken = default)
        {
            var restored = await _persistence.LoadAsync(playerId, targetSchemaVersion, cancellationToken);
            if (restored != null) return new FirstPlayableBootstrapResult(restored, false);

            var created = _newStateFactory();
            if (created == null) throw new InvalidOperationException("New-state factory returned no state.");
            return new FirstPlayableBootstrapResult(created, true);
        }
    }
}
