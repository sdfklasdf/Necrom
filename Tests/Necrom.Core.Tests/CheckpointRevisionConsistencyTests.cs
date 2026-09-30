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
    public sealed class CheckpointRevisionConsistencyTests
    {
        [Test]
        public async Task CheckpointPersistsCurrentBattleAndFormationRevisionsExactly()
        {
            var battle = new BattleStateMachine();
            var formation = new Formation();
            var encounter = new FirstPlayableEncounter(new RaiseService(), battle, formation);
            var app = new FirstPlayableApplicationService(encounter);
            var progression = new FirstPlayableProgression(app, encounter);

            app.Execute(new StartBattleCommand("start-1", 0));

            var enemy = new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, 5);
            enemy.ApplyDamage(5);
            var source = new RaiseSource(new EntityId("source-1"), enemy);
            progression.RaiseForNextCombat(
                new RaiseIntoFormationCommand("raise-1", source, 0, new EntityId("undead-1"), 7, 0, 0),
                "event-raised",
                "event-assigned");

            var store = new RecordingStore();
            var codec = new CapturingCodec();
            var persistence = new GameStatePersistenceService(store, codec, new SnapshotMigrationRegistry());
            var checkpoints = new FirstPlayableCheckpointService(progression, persistence);

            await checkpoints.SaveAsync("player-1", "1");

            Assert.That(codec.Captured.BattleRevision, Is.EqualTo(battle.Revision));
            Assert.That(codec.Captured.FormationRevision, Is.EqualTo(formation.Revision));
            Assert.That(codec.Captured.BattlePhase, Is.EqualTo(battle.Phase));
            Assert.That(codec.Captured.Formation.Count, Is.EqualTo(1));
            Assert.That(codec.Captured.Formation[0].UnitId, Is.EqualTo("undead-1"));
            Assert.That(store.SaveCalls, Is.EqualTo(1));
        }

        private sealed class RecordingStore : IGameStateStore
        {
            public int SaveCalls { get; private set; }
            public ValueTask<string> LoadAsync(string playerId, CancellationToken cancellationToken = default)
                => new ValueTask<string>((string)null);
            public ValueTask SaveAsync(string playerId, string schemaVersion, string serializedState, CancellationToken cancellationToken = default)
            {
                SaveCalls++;
                return default;
            }
        }

        private sealed class CapturingCodec : IGameStateSnapshotCodec
        {
            public GameStateSnapshot Captured { get; private set; }

            public string Serialize(GameStateSnapshot snapshot)
            {
                Captured = snapshot;
                return "snapshot";
            }

            public GameStateSnapshot Deserialize(string serializedState)
                => throw new System.InvalidOperationException("Unexpected deserialize.");
        }
    }
}
