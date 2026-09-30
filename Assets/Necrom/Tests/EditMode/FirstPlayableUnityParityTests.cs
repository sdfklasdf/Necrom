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

            var source = new RaiseSource(new EntityId("source-1"), enemy, "frontline.guard");
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
            var source = new RaiseSource(new EntityId("source-2"), enemy, "frontline.guard");
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

        [Test]
        public void BattleRestartTransitionsResolvedToReadyAndCheckpointHydratesSameRevision()
        {
            var battle = new BattleStateMachine();
            battle.Start(0);
            battle.Resolve(true, 1);
            battle.FinalizeResult(2);

            var restart = typeof(BattleStateMachine).GetMethod("Restart", new[] { typeof(long) });
            Assert.That(restart, Is.Not.Null, "BattleStateMachine.Restart(long) must exist.");

            restart.Invoke(battle, new object[] { 3L });

            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Ready));
            Assert.That(battle.Revision, Is.EqualTo(4));

            var formation = new Formation();
            formation.Assign(0, new EntityId("undead-restart"), 0);
            var snapshot = GameStateSnapshot.Capture("1", battle, formation);
            var restored = GameStateHydrator.Restore(snapshot);

            Assert.That(snapshot.BattlePhase, Is.EqualTo(BattlePhase.Ready));
            Assert.That(snapshot.BattleRevision, Is.EqualTo(4));
            Assert.That(restored.Battle.Phase, Is.EqualTo(BattlePhase.Ready));
            Assert.That(restored.Battle.Revision, Is.EqualTo(4));
            Assert.That(restored.Formation.GetSlot(0).Value, Is.EqualTo(new EntityId("undead-restart")));
            Assert.That(restored.Formation.Revision, Is.EqualTo(1));
        }

        [Test]
        public void BattleRestartRejectsInvalidPhaseAndStaleRevisionWithoutMutation()
        {
            var restart = typeof(BattleStateMachine).GetMethod("Restart", new[] { typeof(long) });
            Assert.That(restart, Is.Not.Null, "BattleStateMachine.Restart(long) must exist.");

            foreach (var phase in new[] { BattlePhase.Ready, BattlePhase.Running, BattlePhase.Victory, BattlePhase.Defeat })
            {
                var battle = BattleStateMachine.Restore(phase, 7);
                var invalid = Assert.Throws<System.Reflection.TargetInvocationException>(
                    () => restart.Invoke(battle, new object[] { 7L }));
                Assert.That(invalid.InnerException, Is.TypeOf<InvalidOperationException>());
                Assert.That(battle.Phase, Is.EqualTo(phase));
                Assert.That(battle.Revision, Is.EqualTo(7));
            }

            var resolved = BattleStateMachine.Restore(BattlePhase.Resolved, 7);
            var stale = Assert.Throws<System.Reflection.TargetInvocationException>(
                () => restart.Invoke(resolved, new object[] { 6L }));
            Assert.That(stale.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(resolved.Phase, Is.EqualTo(BattlePhase.Resolved));
            Assert.That(resolved.Revision, Is.EqualTo(7));
        }

        [Test]
        public void ApplicationExposesBattleStateAndRoutesFinalizeAndRestartCommands()
        {
            var applicationAssembly = typeof(FirstPlayableApplicationService).Assembly;
            var finalizeType = applicationAssembly.GetType("Necrom.Core.Application.FinalizeBattleCommand");
            var restartType = applicationAssembly.GetType("Necrom.Core.Application.RestartBattleCommand");
            Assert.That(finalizeType, Is.Not.Null, "FinalizeBattleCommand must exist.");
            Assert.That(restartType, Is.Not.Null, "RestartBattleCommand must exist.");

            var battle = new BattleStateMachine();
            var encounter = new FirstPlayableEncounter(new RaiseService(), battle, new Formation());
            var application = new FirstPlayableApplicationService(encounter);

            application.Execute(new StartBattleCommand("start-app", 0));
            application.Execute(new ResolveBattleCommand("resolve-app", true, 1));

            var finalize = Activator.CreateInstance(finalizeType, "finalize-app", 2L);
            var finalizeExecute = typeof(FirstPlayableApplicationService).GetMethod("Execute", new[] { finalizeType });
            Assert.That(finalizeExecute, Is.Not.Null, "Finalize command Execute overload must exist.");
            finalizeExecute.Invoke(application, new[] { finalize });

            var phaseProperty = typeof(FirstPlayableApplicationService).GetProperty("BattlePhase");
            var revisionProperty = typeof(FirstPlayableApplicationService).GetProperty("BattleRevision");
            Assert.That(phaseProperty, Is.Not.Null, "BattlePhase read seam must exist.");
            Assert.That(revisionProperty, Is.Not.Null, "BattleRevision read seam must exist.");
            Assert.That(phaseProperty.GetValue(application), Is.EqualTo(BattlePhase.Resolved));
            Assert.That(revisionProperty.GetValue(application), Is.EqualTo(3L));

            var restart = Activator.CreateInstance(restartType, "restart-app", 3L);
            var restartExecute = typeof(FirstPlayableApplicationService).GetMethod("Execute", new[] { restartType });
            Assert.That(restartExecute, Is.Not.Null, "Restart command Execute overload must exist.");
            restartExecute.Invoke(application, new[] { restart });

            Assert.That(phaseProperty.GetValue(application), Is.EqualTo(BattlePhase.Ready));
            Assert.That(revisionProperty.GetValue(application), Is.EqualTo(4L));
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
