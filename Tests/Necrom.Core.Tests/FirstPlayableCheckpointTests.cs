using System.Threading;
using System.Threading.Tasks;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;
using Necrom.Core.Ports;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class FirstPlayableCheckpointTests
    {
        [Test]
        public async Task SaveCheckpointCapturesAndPersistsCurrentState()
        {
            var battle = new BattleStateMachine();
            battle.Start(0);
            var formation = new Formation();
            var encounter = new FirstPlayableEncounter(new RaiseService(), battle, formation);
            var app = new FirstPlayableApplicationService(encounter);
            var progression = new FirstPlayableProgression(app, encounter);
            var store = new RecordingStore();
            var codec = new RecordingCodec();
            var persistence = new GameStatePersistenceService(store, codec, new SnapshotMigrationRegistry());
            var checkpoints = new FirstPlayableCheckpointService(progression, persistence);

            await checkpoints.SaveAsync("player-1", "1");

            Assert.That(codec.LastSnapshot, Is.Not.Null);
            Assert.That(codec.LastSnapshot.BattlePhase, Is.EqualTo(BattlePhase.Running));
            Assert.That(store.LastPlayerId, Is.EqualTo("player-1"));
            Assert.That(store.LastSchemaVersion, Is.EqualTo("1"));
            Assert.That(store.LastSerializedState, Is.EqualTo("encoded"));
        }

        private sealed class RecordingStore : IGameStateStore
        {
            public string LastPlayerId { get; private set; }
            public string LastSchemaVersion { get; private set; }
            public string LastSerializedState { get; private set; }

            public ValueTask<string> LoadAsync(string playerId, CancellationToken cancellationToken = default)
                => new ValueTask<string>((string)null);

            public ValueTask SaveAsync(string playerId, string schemaVersion, string serializedState, CancellationToken cancellationToken = default)
            {
                LastPlayerId = playerId;
                LastSchemaVersion = schemaVersion;
                LastSerializedState = serializedState;
                return default;
            }
        }

        private sealed class RecordingCodec : IGameStateSnapshotCodec
        {
            public GameStateSnapshot LastSnapshot { get; private set; }
            public string Serialize(GameStateSnapshot snapshot)
            {
                LastSnapshot = snapshot;
                return "encoded";
            }
            public GameStateSnapshot Deserialize(string serializedState) => null;
        }
    }
}
