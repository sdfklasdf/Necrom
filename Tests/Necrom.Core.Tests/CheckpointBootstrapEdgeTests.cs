using System;
using System.Threading;
using System.Threading.Tasks;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;
using Necrom.Core.Ports;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class CheckpointBootstrapEdgeTests
    {
        [Test]
        public void InvalidSnapshotIsRejectedBeforeCodecOrStore()
        {
            var store = new RecordingStore();
            var codec = new RecordingCodec();
            var persistence = new GameStatePersistenceService(store, codec, new SnapshotMigrationRegistry());
            var invalid = new GameStateSnapshot(
                "1",
                BattlePhase.Ready,
                -1,
                0,
                Array.Empty<FormationSlotSnapshot>());

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await persistence.SaveAsync("player-1", invalid));

            Assert.That(codec.SerializeCalls, Is.EqualTo(0));
            Assert.That(store.SaveCalls, Is.EqualTo(0));
        }

        [Test]
        public void NullSerializedStateIsRejectedBeforeStoreMutation()
        {
            var store = new RecordingStore();
            var codec = new RecordingCodec { ReturnNullSerializedState = true };
            var persistence = new GameStatePersistenceService(store, codec, new SnapshotMigrationRegistry());
            var snapshot = GameStateSnapshot.Capture("1", new BattleStateMachine(), new Formation());

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await persistence.SaveAsync("player-1", snapshot));

            Assert.That(codec.SerializeCalls, Is.EqualTo(1));
            Assert.That(store.SaveCalls, Is.EqualTo(0));
        }

        [Test]
        public void CheckpointStoreFailurePropagates()
        {
            var store = new RecordingStore { ThrowOnSave = true };
            var codec = new RecordingCodec();
            var checkpoints = NewCheckpointService(store, codec);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await checkpoints.SaveAsync("player-1", "1"));

            Assert.That(codec.SerializeCalls, Is.EqualTo(1));
            Assert.That(store.SaveCalls, Is.EqualTo(1));
            Assert.That(store.SuccessfulSaveCalls, Is.EqualTo(0));
        }

        [Test]
        public void CheckpointCancellationPropagates()
        {
            var store = new RecordingStore { HonorCancellation = true };
            var codec = new RecordingCodec();
            var checkpoints = NewCheckpointService(store, codec);
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await checkpoints.SaveAsync("player-1", "1", cts.Token));

            Assert.That(store.SaveCalls, Is.EqualTo(1));
            Assert.That(store.SuccessfulSaveCalls, Is.EqualTo(0));
        }

        [Test]
        public void InvalidTargetSchemaRejectsBeforeStoreLoad()
        {
            var store = new RecordingStore();
            var persistence = new GameStatePersistenceService(store, new RecordingCodec(), new SnapshotMigrationRegistry());
            var bootstrap = new FirstPlayableBootstrap(persistence, NewState);

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await bootstrap.LoadOrNewAsync("player-1", " "));

            Assert.That(store.LoadCalls, Is.EqualTo(0));
        }

        [Test]
        public void MissingSaveWithNullFactoryResultIsRejected()
        {
            var store = new RecordingStore();
            var persistence = new GameStatePersistenceService(store, new RecordingCodec(), new SnapshotMigrationRegistry());
            var bootstrap = new FirstPlayableBootstrap(persistence, () => null);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await bootstrap.LoadOrNewAsync("player-1", "1"));

            Assert.That(store.LoadCalls, Is.EqualTo(1));
        }

        private static FirstPlayableCheckpointService NewCheckpointService(IGameStateStore store, IGameStateSnapshotCodec codec)
        {
            var battle = new BattleStateMachine();
            var formation = new Formation();
            var encounter = new FirstPlayableEncounter(new RaiseService(), battle, formation);
            var application = new FirstPlayableApplicationService(encounter);
            var progression = new FirstPlayableProgression(application, encounter);
            var persistence = new GameStatePersistenceService(store, codec, new SnapshotMigrationRegistry());
            return new FirstPlayableCheckpointService(progression, persistence);
        }

        private static HydratedGameState NewState()
            => new HydratedGameState(new BattleStateMachine(), new Formation());

        private sealed class RecordingStore : IGameStateStore
        {
            public bool ThrowOnSave { get; set; }
            public bool HonorCancellation { get; set; }
            public int LoadCalls { get; private set; }
            public int SaveCalls { get; private set; }
            public int SuccessfulSaveCalls { get; private set; }

            public ValueTask<string> LoadAsync(string playerId, CancellationToken cancellationToken = default)
            {
                LoadCalls++;
                return new ValueTask<string>((string)null);
            }

            public ValueTask SaveAsync(
                string playerId,
                string schemaVersion,
                string serializedState,
                CancellationToken cancellationToken = default)
            {
                SaveCalls++;
                if (HonorCancellation) cancellationToken.ThrowIfCancellationRequested();
                if (ThrowOnSave) throw new InvalidOperationException("save failed");
                SuccessfulSaveCalls++;
                return default;
            }
        }

        private sealed class RecordingCodec : IGameStateSnapshotCodec
        {
            public bool ReturnNullSerializedState { get; set; }
            public int SerializeCalls { get; private set; }

            public string Serialize(GameStateSnapshot snapshot)
            {
                SerializeCalls++;
                return ReturnNullSerializedState ? null : "encoded";
            }

            public GameStateSnapshot Deserialize(string serializedState)
                => throw new InvalidOperationException("Unexpected deserialize.");
        }
    }
}
