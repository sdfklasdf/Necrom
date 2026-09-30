using System;
using System.Collections.Generic;
using Necrom.Core.Adapters.Local;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;
using NUnit.Framework;

namespace Necrom.Core.UnityTests
{
    public sealed class FirstPlayableUnityParityTests
    {
        [Test]
        public void KillRaiseFormationCheckpointHydrateRoundTrip()
        {
            var enemy = new Combatant(new EntityId("enemy-1"), "skeleton", Faction.Enemy, 5);
            enemy.ApplyDamage(5);
            Assert.That(enemy.LifeState, Is.EqualTo(CombatantLifeState.Defeated));

            var source = new RaiseSource(new EntityId("source-1"), enemy);
            var battle = new BattleStateMachine();
            var formation = new Formation();
            var encounter = new FirstPlayableEncounter(new RaiseService(), battle, formation);
            var app = new FirstPlayableApplicationService(encounter);
            var progression = new FirstPlayableProgression(app, encounter);
            var undeadId = new EntityId("undead-1");

            progression.RaiseForNextCombat(
                new RaiseIntoFormationCommand("raise-1", source, 0, undeadId, 7, 0, 0),
                "event-raised",
                "event-assigned");

            Assert.That(encounter.IsReadyForNextCombat(undeadId), Is.True);

            var store = new InMemoryGameStateStore();
            var codec = new ReferenceCodec();
            var persistence = new GameStatePersistenceService(
                store, codec, new SnapshotMigrationRegistry());
            var checkpoint = new FirstPlayableCheckpointService(progression, persistence);

            checkpoint.SaveAsync("player-1", "1").AsTask().GetAwaiter().GetResult();

            var bootstrap = new FirstPlayableBootstrap(
                persistence,
                () => new HydratedGameState(new BattleStateMachine(), new Formation()));
            var restored = bootstrap.LoadOrNewAsync("player-1", "1").AsTask().GetAwaiter().GetResult();

            Assert.That(restored.IsNew, Is.False);
            Assert.That(restored.State.Formation.GetSlot(0).Value, Is.EqualTo(undeadId));
            Assert.That(restored.State.Formation.Revision, Is.EqualTo(1));
        }

        [Test]
        public void OccupiedFormationSlotRejectsRaiseWithoutConsumingSource()
        {
            var enemy = new Combatant(new EntityId("enemy-2"), "skeleton", Faction.Enemy, 5);
            enemy.ApplyDamage(5);
            var source = new RaiseSource(new EntityId("source-2"), enemy);
            var formation = new Formation();
            formation.Assign(0, new EntityId("existing"), 0);

            var encounter = new FirstPlayableEncounter(
                new RaiseService(), new BattleStateMachine(), formation);
            var progression = new FirstPlayableProgression(
                new FirstPlayableApplicationService(encounter), encounter);

            Assert.Throws<InvalidOperationException>(() =>
                progression.RaiseForNextCombat(
                    new RaiseIntoFormationCommand(
                        "raise-2", source, 0, new EntityId("undead-2"), 7, 0, 1),
                    "event-raised-2",
                    "event-assigned-2"));

            Assert.That(source.State, Is.EqualTo(RaiseSourceState.Available));
            Assert.That(source.Revision, Is.EqualTo(0));
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
