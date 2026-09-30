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
    public sealed class FirstPlayableCoreE2ETests
    {
        [Test]
        public async Task KillRaiseFormationCheckpointHydrateRoundTripPreservesNextCombatReadiness()
        {
            var enemy = new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, 5);
            enemy.ApplyDamage(5);
            Assert.That(enemy.LifeState, Is.EqualTo(CombatantLifeState.Defeated));

            var source = new RaiseSource(new EntityId("source-1"), enemy, "frontline.guard");
            var battle = new BattleStateMachine();
            var formation = new Formation();
            var encounter = new FirstPlayableEncounter(new RaiseService(), battle, formation);
            var app = new FirstPlayableApplicationService(encounter);
            var progression = new FirstPlayableProgression(app, encounter);
            var undeadId = new EntityId("undead-1");

            var command = new RaiseIntoFormationCommand(
                "raise-1", source, 0, undeadId, 7, 0, 0);

            var result = progression.RaiseForNextCombat(
                command, "event-raised", "event-assigned");

            Assert.That(result.Events.Count, Is.EqualTo(2));
            Assert.That(encounter.IsReadyForNextCombat(undeadId), Is.True);

            var store = new InMemoryGameStateStore();
            var codec = new ReferenceCodec();
            var persistence = new GameStatePersistenceService(
                store, codec, new SnapshotMigrationRegistry());
            var checkpoint = new FirstPlayableCheckpointService(progression, persistence);

            await checkpoint.SaveAsync("player-1", "1");

            var bootstrap = new FirstPlayableBootstrap(
                persistence,
                () => new HydratedGameState(new BattleStateMachine(), new Formation()));
            var restored = await bootstrap.LoadOrNewAsync("player-1", "1");

            Assert.That(restored.IsNew, Is.False);
            Assert.That(restored.State.Formation.GetSlot(0).Value, Is.EqualTo(undeadId));
            Assert.That(restored.State.Formation.Revision, Is.EqualTo(1));

            var restoredEncounter = new FirstPlayableEncounter(
                new RaiseService(), restored.State.Battle, restored.State.Formation);
            Assert.That(restoredEncounter.IsReadyForNextCombat(undeadId), Is.True);
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
