using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Necrom.Core.Adapters.Local;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class LocalPersistenceOrchestrationTests
    {
        [Test]
        public async Task MissingSaveCreatesOnceThenCheckpointPersistsAndNextBootstrapRestores()
        {
            var store = new InMemoryGameStateStore();
            var codec = new ReferenceCodec();
            var persistence = new GameStatePersistenceService(
                store, codec, new SnapshotMigrationRegistry());

            var factoryCalls = 0;
            var firstBootstrap = new FirstPlayableBootstrap(
                persistence,
                () =>
                {
                    factoryCalls++;
                    return new HydratedGameState(new BattleStateMachine(), new Formation());
                });

            var first = await firstBootstrap.LoadOrNewAsync("player-1", "1");

            Assert.That(first.IsNew, Is.True);
            Assert.That(factoryCalls, Is.EqualTo(1));

            var encounter = new FirstPlayableEncounter(
                new RaiseService(), first.State.Battle, first.State.Formation);
            var app = new FirstPlayableApplicationService(encounter);
            var progression = new FirstPlayableProgression(app, encounter);

            var enemy = new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, 5);
            enemy.ApplyDamage(5);
            var source = new RaiseSource(new EntityId("source-1"), enemy);
            progression.RaiseForNextCombat(
                new RaiseIntoFormationCommand(
                    "raise-1", source, 0, new EntityId("undead-1"), 7, 0, 0),
                "event-raised",
                "event-assigned");

            var checkpoints = new FirstPlayableCheckpointService(progression, persistence);
            await checkpoints.SaveAsync("player-1", "1");

            var unexpectedFactoryCalls = 0;
            var secondBootstrap = new FirstPlayableBootstrap(
                persistence,
                () =>
                {
                    unexpectedFactoryCalls++;
                    return new HydratedGameState(new BattleStateMachine(), new Formation());
                });

            var restored = await secondBootstrap.LoadOrNewAsync("player-1", "1");

            Assert.That(restored.IsNew, Is.False);
            Assert.That(unexpectedFactoryCalls, Is.EqualTo(0));
            Assert.That(restored.State.Formation.GetSlot(0).Value.Value, Is.EqualTo("undead-1"));
            Assert.That(restored.State.Formation.Revision, Is.EqualTo(1));
        }

        private sealed class ReferenceCodec : IGameStateSnapshotCodec
        {
            private readonly Dictionary<string, GameStateSnapshot> _snapshots =
                new Dictionary<string, GameStateSnapshot>(StringComparer.Ordinal);
            private int _nextId;

            public string Serialize(GameStateSnapshot snapshot)
            {
                var key = "snapshot-" + _nextId++;
                _snapshots.Add(key, snapshot);
                return key;
            }

            public GameStateSnapshot Deserialize(string serializedState)
                => _snapshots[serializedState];
        }
    }
}
