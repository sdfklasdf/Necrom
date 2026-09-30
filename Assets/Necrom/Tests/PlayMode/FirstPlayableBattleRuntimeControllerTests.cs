using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableBattleRuntimeControllerTests
    {
        [UnityTest]
        public IEnumerator StartRejectsMissingNecromancerBeforeBattleMutation()
        {
            var fixture = BuildFixture(bindPlayer: false, spawnEnemy: true);
            var runtimeType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableBattleRuntimeController");
            Assert.That(runtimeType, Is.Not.Null, "FirstPlayableBattleRuntimeController must exist.");

            var runtime = fixture.Root.AddComponent(runtimeType);
            InitializeRuntime(runtimeType, runtime, fixture);
            var command = NewCommand("Necrom.Core.Application.StartBattleCommand", "start-missing-player", 0L);

            var ex = Assert.Throws<TargetInvocationException>(
                () => runtimeType.GetMethod("StartBattle").Invoke(runtime, new[] { command }));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            AssertBattle(fixture.Battle, "Ready", 0);

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartRejectsMissingActiveEnemyBeforeBattleMutation()
        {
            var fixture = BuildFixture(bindPlayer: true, spawnEnemy: false);
            var runtimeType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableBattleRuntimeController");
            Assert.That(runtimeType, Is.Not.Null, "FirstPlayableBattleRuntimeController must exist.");

            var runtime = fixture.Root.AddComponent(runtimeType);
            InitializeRuntime(runtimeType, runtime, fixture);
            var command = NewCommand("Necrom.Core.Application.StartBattleCommand", "start-missing-enemy", 0L);

            var ex = Assert.Throws<TargetInvocationException>(
                () => runtimeType.GetMethod("StartBattle").Invoke(runtime, new[] { command }));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            AssertBattle(fixture.Battle, "Ready", 0);

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ValidActorsDriveStartResolveFinalizeRestartWithoutEntityLifecycleSideEffects()
        {
            var fixture = BuildFixture(bindPlayer: true, spawnEnemy: true);
            var runtimeType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableBattleRuntimeController");
            Assert.That(runtimeType, Is.Not.Null, "FirstPlayableBattleRuntimeController must exist.");

            var runtime = fixture.Root.AddComponent(runtimeType);
            InitializeRuntime(runtimeType, runtime, fixture);

            var necroBefore = fixture.NecromancerZone.childCount;
            var enemyBefore = fixture.EnemyZone.childCount;
            var playerRef = fixture.NecromancerController.GetType().GetProperty("CurrentNecromancer").GetValue(fixture.NecromancerController);
            var enemyRef = fixture.EnemyController.GetType().GetProperty("CurrentTarget").GetValue(fixture.EnemyController);

            runtimeType.GetMethod("StartBattle").Invoke(runtime, new[]
            {
                NewCommand("Necrom.Core.Application.StartBattleCommand", "start-valid", 0L)
            });
            AssertBattle(fixture.Battle, "Running", 1);

            runtimeType.GetMethod("ResolveFromCombatResult").Invoke(runtime, new[]
            {
                NewResolveCommand("resolve-valid", true, 1L)
            });
            AssertBattle(fixture.Battle, "Victory", 2);

            runtimeType.GetMethod("FinalizeBattle").Invoke(runtime, new[]
            {
                NewCommand("Necrom.Core.Application.FinalizeBattleCommand", "finalize-valid", 2L)
            });
            AssertBattle(fixture.Battle, "Resolved", 3);

            var safeArea = fixture.Root.transform.Find("SafeArea") as RectTransform;
            var combat = safeArea.Find("CombatViewport") as RectTransform;
            combat.anchorMin = new Vector2(0f, 0.91f);

            runtimeType.GetMethod("RestartBattle").Invoke(runtime, new[]
            {
                NewCommand("Necrom.Core.Application.RestartBattleCommand", "restart-valid", 3L)
            });
            AssertBattle(fixture.Battle, "Ready", 4);

            Assert.That(combat.anchorMin.y, Is.EqualTo(0.22f).Within(0.0001f));
            Assert.That(fixture.NecromancerZone.childCount, Is.EqualTo(necroBefore));
            Assert.That(fixture.EnemyZone.childCount, Is.EqualTo(enemyBefore));
            Assert.That(fixture.NecromancerController.GetType().GetProperty("CurrentNecromancer").GetValue(fixture.NecromancerController), Is.SameAs(playerRef));
            Assert.That(fixture.EnemyController.GetType().GetProperty("CurrentTarget").GetValue(fixture.EnemyController), Is.SameAs(enemyRef));
            Assert.That(runtimeType.GetProperty("Phase").GetValue(runtime).ToString(), Is.EqualTo("Ready"));
            Assert.That(runtimeType.GetProperty("Revision").GetValue(runtime), Is.EqualTo(4L));

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StaleRestartRepairsLayoutButKeepsResolvedDomainState()
        {
            var fixture = BuildFixture(bindPlayer: true, spawnEnemy: true);
            var runtimeType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableBattleRuntimeController");
            Assert.That(runtimeType, Is.Not.Null, "FirstPlayableBattleRuntimeController must exist.");

            var runtime = fixture.Root.AddComponent(runtimeType);
            InitializeRuntime(runtimeType, runtime, fixture);

            runtimeType.GetMethod("StartBattle").Invoke(runtime, new[]
            {
                NewCommand("Necrom.Core.Application.StartBattleCommand", "start-stale", 0L)
            });
            runtimeType.GetMethod("ResolveFromCombatResult").Invoke(runtime, new[]
            {
                NewResolveCommand("resolve-stale", false, 1L)
            });
            runtimeType.GetMethod("FinalizeBattle").Invoke(runtime, new[]
            {
                NewCommand("Necrom.Core.Application.FinalizeBattleCommand", "finalize-stale", 2L)
            });
            AssertBattle(fixture.Battle, "Resolved", 3);

            var safeArea = fixture.Root.transform.Find("SafeArea") as RectTransform;
            var combat = safeArea.Find("CombatViewport") as RectTransform;
            combat.anchorMin = new Vector2(0f, 0.91f);

            var ex = Assert.Throws<TargetInvocationException>(() =>
                runtimeType.GetMethod("RestartBattle").Invoke(runtime, new[]
                {
                    NewCommand("Necrom.Core.Application.RestartBattleCommand", "restart-stale", 2L)
                }));

            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            AssertBattle(fixture.Battle, "Resolved", 3);
            Assert.That(combat.anchorMin.y, Is.EqualTo(0.22f).Within(0.0001f));

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        private static Fixture BuildFixture(bool bindPlayer, bool spawnEnemy)
        {
            var root = new GameObject("EncounterRoot", typeof(RectTransform));
            var boundaryType = FindType("Necrom.FirstPlayable.Runtime.EncounterBoundaryController");
            var boundary = root.AddComponent(boundaryType);
            var configType = FindType("Necrom.FirstPlayable.Runtime.EncounterLayoutConfig");
            var config = Activator.CreateInstance(configType, new object[]
            {
                new Vector2(390f, 844f),
                new Rect(0f, 34f, 390f, 776f),
                0.22f,
                new Rect(0.12f, 0.08f, 0.30f, 0.24f),
                new Rect(0.58f, 0.58f, 0.30f, 0.28f),
                new Rect(0.12f, 0.34f, 0.44f, 0.18f)
            });
            boundaryType.GetMethod("StartBoundary").Invoke(boundary, new[] { config });

            var safeArea = root.transform.Find("SafeArea");
            var combat = safeArea.Find("CombatViewport");
            var necromancerZone = combat.Find("NecromancerSpawnZone") as RectTransform;
            var enemyZone = combat.Find("EnemySpawnZone") as RectTransform;

            var necromancerType = FindType("Necrom.FirstPlayable.Runtime.NecromancerAnchorController");
            var enemyControllerType = FindType("Necrom.FirstPlayable.Runtime.EnemySpawnController");
            var necromancerController = combat.gameObject.AddComponent(necromancerType);
            var enemyController = combat.gameObject.AddComponent(enemyControllerType);

            if (bindPlayer)
            {
                var player = NewCombatant("player-battle", "necromancer.prototype", "Player", 100);
                necromancerType.GetMethod("BindNecromancer").Invoke(necromancerController, new[] { player, necromancerZone });
            }

            if (spawnEnemy)
            {
                var definitionType = FindType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition");
                var definition = Activator.CreateInstance(definitionType, "enemy.skeleton.guard", "frontline.guard", 25);
                enemyControllerType.GetMethod("SpawnEnemy").Invoke(enemyController, new object[] { "enemy-battle", definition, enemyZone });
            }

            var battleType = FindType("Necrom.Core.Domain.BattleStateMachine");
            var formationType = FindType("Necrom.Core.Domain.Formation");
            var raiseServiceType = FindType("Necrom.Core.Domain.RaiseService");
            var encounterType = FindType("Necrom.Core.Domain.FirstPlayableEncounter");
            var applicationType = FindType("Necrom.Core.Application.FirstPlayableApplicationService");

            var battle = Activator.CreateInstance(battleType);
            var formation = Activator.CreateInstance(formationType);
            var raiseService = Activator.CreateInstance(raiseServiceType);
            var encounter = Activator.CreateInstance(encounterType, raiseService, battle, formation);
            var application = Activator.CreateInstance(applicationType, encounter);

            return new Fixture(root, boundary, config, necromancerController, enemyController, necromancerZone, enemyZone, battle, application);
        }

        private static void InitializeRuntime(Type runtimeType, Component runtime, Fixture fixture)
        {
            var initialize = runtimeType.GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(initialize, Is.Not.Null);
            initialize.Invoke(runtime, new[]
            {
                fixture.Application,
                (object)fixture.Boundary,
                fixture.Config,
                (object)fixture.NecromancerController,
                (object)fixture.EnemyController
            });
        }

        private static object NewCommand(string fullName, string commandId, long expectedRevision)
        {
            var type = FindType(fullName);
            Assert.That(type, Is.Not.Null, fullName + " must exist.");
            return Activator.CreateInstance(type, commandId, expectedRevision);
        }

        private static object NewResolveCommand(string commandId, bool playerWon, long expectedRevision)
        {
            var type = FindType("Necrom.Core.Application.ResolveBattleCommand");
            Assert.That(type, Is.Not.Null);
            return Activator.CreateInstance(type, commandId, playerWon, expectedRevision);
        }

        private static object NewCombatant(string entityId, string archetypeId, string factionName, int health)
        {
            var entityIdType = FindType("Necrom.Core.Domain.EntityId");
            var factionType = FindType("Necrom.Core.Domain.Faction");
            var combatantType = FindType("Necrom.Core.Domain.Combatant");
            var id = Activator.CreateInstance(entityIdType, entityId);
            var faction = Enum.Parse(factionType, factionName);
            return Activator.CreateInstance(combatantType, id, archetypeId, faction, health);
        }

        private static void AssertBattle(object battle, string expectedPhase, long expectedRevision)
        {
            var type = battle.GetType();
            Assert.That(type.GetProperty("Phase").GetValue(battle).ToString(), Is.EqualTo(expectedPhase));
            Assert.That(type.GetProperty("Revision").GetValue(battle), Is.EqualTo(expectedRevision));
        }

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);

        private sealed class Fixture
        {
            public GameObject Root { get; }
            public Component Boundary { get; }
            public object Config { get; }
            public Component NecromancerController { get; }
            public Component EnemyController { get; }
            public RectTransform NecromancerZone { get; }
            public RectTransform EnemyZone { get; }
            public object Battle { get; }
            public object Application { get; }

            public Fixture(
                GameObject root,
                Component boundary,
                object config,
                Component necromancerController,
                Component enemyController,
                RectTransform necromancerZone,
                RectTransform enemyZone,
                object battle,
                object application)
            {
                Root = root;
                Boundary = boundary;
                Config = config;
                NecromancerController = necromancerController;
                EnemyController = enemyController;
                NecromancerZone = necromancerZone;
                EnemyZone = enemyZone;
                Battle = battle;
                Application = application;
            }
        }
    }
}
