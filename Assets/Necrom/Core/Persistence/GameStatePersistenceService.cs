using System;
using System.Threading;
using System.Threading.Tasks;
using Necrom.Core.Ports;

namespace Necrom.Core.Persistence
{
    public interface IGameStateSnapshotCodec
    {
        string Serialize(GameStateSnapshot snapshot);
        GameStateSnapshot Deserialize(string serializedState);
    }

    public sealed class GameStatePersistenceService
    {
        private readonly IGameStateStore _store;
        private readonly IGameStateSnapshotCodec _codec;
        private readonly SnapshotMigrationRegistry _migrations;

        public GameStatePersistenceService(IGameStateStore store, IGameStateSnapshotCodec codec, SnapshotMigrationRegistry migrations)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _codec = codec ?? throw new ArgumentNullException(nameof(codec));
            _migrations = migrations ?? throw new ArgumentNullException(nameof(migrations));
        }

        public async ValueTask SaveAsync(string playerId, GameStateSnapshot snapshot, CancellationToken cancellationToken = default)
        {
            GameStateSnapshotValidator.Validate(snapshot);
            var serialized = _codec.Serialize(snapshot);
            if (serialized == null) throw new InvalidOperationException("Snapshot codec returned null serialized state.");

            // A failed store write propagates. No success result/event is emitted here.
            await _store.SaveAsync(playerId, snapshot.SchemaVersion, serialized, cancellationToken);
        }

        public async ValueTask<HydratedGameState> LoadAsync(string playerId, string targetSchemaVersion, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(targetSchemaVersion)) throw new ArgumentException("Target schema version is required.", nameof(targetSchemaVersion));

            var serialized = await _store.LoadAsync(playerId, cancellationToken);
            if (serialized == null) return null;

            var decoded = _codec.Deserialize(serialized);
            if (decoded == null) throw new InvalidOperationException("Snapshot codec returned null snapshot.");

            GameStateSnapshotValidator.Validate(decoded);
            var migrated = _migrations.MigrateTo(decoded, targetSchemaVersion);
            return GameStateHydrator.Restore(migrated);
        }
    }
}
