using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;
using Necrom.Core.Ports;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class PersistenceRecoveryTests
    {
        [Test]
        public void UnknownSchemaDoesNotSilentlyCreateNewState()
        {
            var snapshot = new GameStateSnapshot("old", BattlePhase.Ready, 0, 0,
                Array.Empty<FormationSlotSnapshot>());
            var store = new StubStore("saved");
            var persistence = new GameStatePersistenceService(store, new FixedCodec(snapshot), new SnapshotMigrationRegistry());
            var factoryCalls = 0;
            var bootstrap = new FirstPlayableBootstrap(persistence, () =>
            {
                factoryCalls++;
                return NewState();
            });

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await bootstrap.LoadOrNewAsync("player-1", "current"));
            Assert.That(factoryCalls, Is.EqualTo(0));
        }

        [Test]
        public void InvalidDecodedSnapshotDoesNotSilentlyCreateNewState()
        {
            var snapshot = new GameStateSnapshot("1", (BattlePhase)999, 0, 0,
                Array.Empty<FormationSlotSnapshot>());
            var store = new StubStore("saved");
            var persistence = new GameStatePersistenceService(store, new FixedCodec(snapshot), new SnapshotMigrationRegistry());
            var factoryCalls = 0;
            var bootstrap = new FirstPlayableBootstrap(persistence, () =>
            {
                factoryCalls++;
                return NewState();
            });

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await bootstrap.LoadOrNewAsync("player-1", "1"));
            Assert.That(factoryCalls, Is.EqualTo(0));
        }

        [Test]
        public void FailedSavePropagatesAndCannotBeReportedAsSuccess()
        {
            var store = new StubStore(null) { ThrowOnSave = true };
            var persistence = new GameStatePersistenceService(store, new FixedCodec(null), new SnapshotMigrationRegistry());
            var snapshot = GameStateSnapshot.Capture("1", new BattleStateMachine(), new Formation());

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await persistence.SaveAsync("player-1", snapshot));
            Assert.That(store.SuccessfulSaveCount, Is.EqualTo(0));
        }

        private static HydratedGameState NewState()
            => new HydratedGameState(new BattleStateMachine(), new Formation());

        private sealed class StubStore : IGameStateStore
        {
            private readonly string _value;
            public bool ThrowOnSave { get; set; }
            public int SuccessfulSaveCount { get; private set; }
            public StubStore(string value) => _value = value;
            public ValueTask<string> LoadAsync(string playerId, CancellationToken cancellationToken = default)
                => new ValueTask<string>(_value);
            public ValueTask SaveAsync(string playerId, string schemaVersion, string serializedState, CancellationToken cancellationToken = default)
            {
                if (ThrowOnSave) throw new InvalidOperationException("save failed");
                SuccessfulSaveCount++;
                return default;
            }
        }

        private sealed class FixedCodec : IGameStateSnapshotCodec
        {
            private readonly GameStateSnapshot _snapshot;
            public FixedCodec(GameStateSnapshot snapshot) => _snapshot = snapshot;
            public string Serialize(GameStateSnapshot snapshot) => "encoded";
            public GameStateSnapshot Deserialize(string serializedState) => _snapshot;
        }
    }
}
