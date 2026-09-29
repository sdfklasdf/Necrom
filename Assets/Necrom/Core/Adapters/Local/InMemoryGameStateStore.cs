using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Necrom.Core.Ports;

namespace Necrom.Core.Adapters.Local
{
    public sealed class InMemoryGameStateStore : IGameStateStore
    {
        private readonly Dictionary<string, string> _states = new Dictionary<string, string>(StringComparer.Ordinal);

        public ValueTask<string> LoadAsync(string playerId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequirePlayerId(playerId);
            _states.TryGetValue(playerId, out var state);
            return new ValueTask<string>(state);
        }

        public ValueTask SaveAsync(string playerId, string schemaVersion, string serializedState, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RequirePlayerId(playerId);
            if (string.IsNullOrWhiteSpace(schemaVersion)) throw new ArgumentException("Schema version is required.", nameof(schemaVersion));
            if (serializedState == null) throw new ArgumentNullException(nameof(serializedState));

            _states[playerId] = serializedState;
            return default;
        }

        private static void RequirePlayerId(string playerId)
        {
            if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Player id is required.", nameof(playerId));
        }
    }
}
