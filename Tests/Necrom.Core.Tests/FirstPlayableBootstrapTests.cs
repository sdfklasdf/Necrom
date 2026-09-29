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
    public sealed class FirstPlayableBootstrapTests
    {
        [Test]
        public async Task MissingSaveCreatesNewState()
        {
            var store = new StubStore(null);
            var codec = new StubCodec();
            var persistence = new GameStatePersistenceService(store, codec, new SnapshotMigrationRegistry());
            var expected = NewState();
            var bootstrap = new FirstPlayableBootstrap(persistence, () => expected);

            var result = await bootstrap.LoadOrNewAsync("player-1", "1");

            Assert.That(result.State, Is.SameAs(expected));
            Assert.That(result.IsNew, Is.True);
        }

        [Test]
        public void CorruptSaveDoesNotSilentlyCreateNewState()
        {
            var store = new StubStore("corrupt");
            var codec = new StubCodec { ThrowOnDeserialize = true };
            var persistence = new GameStatePersistenceService(store, codec, new SnapshotMigrationRegistry());
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

        private static HydratedGameState NewState()
            => new HydratedGameState(new BattleStateMachine(), new Formation());

        private sealed class StubStore : IGameStateStore
        {
            private readonly string _value;
            public StubStore(string value) => _value = value;
            public ValueTask<string> LoadAsync(string playerId, CancellationToken cancellationToken = default)
                => new ValueTask<string>(_value);
            public ValueTask SaveAsync(string playerId, string schemaVersion, string serializedState, CancellationToken cancellationToken = default)
                => default;
        }

        private sealed class StubCodec : IGameStateSnapshotCodec
        {
            public bool ThrowOnDeserialize { get; set; }
            public string Serialize(GameStateSnapshot snapshot) => "unused";
            public GameStateSnapshot Deserialize(string serializedState)
            {
                if (ThrowOnDeserialize) throw new InvalidOperationException("Corrupt snapshot.");
                throw new InvalidOperationException("Unexpected deserialize.");
            }
        }
    }
}
