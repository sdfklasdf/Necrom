using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableCombatHudRuntimeBindingTests
    {
        private const string RuntimeNs = "Necrom.FirstPlayable.Runtime.";
        private static string _suffix;
        private static string _undeadId;
        private static object _formationForCommand;
        private static int _raiseCost;
        private static ScriptableObject _fontAsset;
        private static Material _fontMaterial;
        private static Texture2D _fontAtlas;

        [UnityTest]
        public IEnumerator NormalGameplayAutomaticallyProjectsTargetRaiseAndArmyOnOneOverlay()
        {
            var f = NewFixture("normal");
            var hud = NewHudDependencies();

            var binding = f.Root.AddComponent(
                RequireType(RuntimeNs + "FirstPlayableCombatHudRuntimeBinding"));

            Invoke(
                binding,
                "Initialize",
                f.BattleRuntime,
                f.Enemies,
                f.SoulBridge,
                f.Formation,
                f.Roster,
                f.Session,
                f.SafeArea,
                hud.DesignContract,
                hud.CopyProvider,
                hud.FontProvider);

            Assert.That(ReadBool(binding, "IsInitialized"), Is.True);
            AssertKeys(
                binding,
                "TargetActive",
                "RaiseTargetNotReady",
                "ArmyEmpty");

            var firstOverlay = (GameObject)Read(binding, "OverlayHost");
            Assert.That(firstOverlay, Is.Not.Null);

            // Automatic LateUpdate refresh must reuse the exact same overlay.
            yield return null;
            Assert.That(Read(binding, "OverlayHost"), Is.SameAs(firstOverlay));
            AssertKeys(
                binding,
                "TargetActive",
                "RaiseTargetNotReady",
                "ArmyEmpty");

            Invoke(
                f.BattleRuntime,
                "StartBattle",
                NewCommand(
                    "Necrom.Core.Application.StartBattleCommand",
                    "start:hud-binding:normal",
                    ReadLong(f.Battle, "Revision")));

            // A real enemy defeat creates the RaiseSource consumed by the
            // projector; no test state adapter is used here.
            Invoke(
                f.Enemy,
                "ApplyDamage",
                5,
                NewEntityId("source:hud-binding:normal"));

            Invoke(
                f.BattleRuntime,
                "ResolveFromCombatResult",
                NewResolveCommand(
                    "resolve:hud-binding:normal",
                    true,
                    ReadLong(f.Battle, "Revision")));
            Invoke(
                f.BattleRuntime,
                "FinalizeBattle",
                NewCommand(
                    "Necrom.Core.Application.FinalizeBattleCommand",
                    "finalize:hud-binding:normal",
                    ReadLong(f.Battle, "Revision")));

            yield return null;
            Assert.That(Read(binding, "OverlayHost"), Is.SameAs(firstOverlay));
            AssertKeys(
                binding,
                "TargetDefeated",
                "RaiseEligible",
                "ArmyEmpty");

            ExecuteResourceRaise(f);
            yield return null;

            Assert.That(Read(binding, "OverlayHost"), Is.SameAs(firstOverlay));
            AssertKeys(
                binding,
                "TargetDefeated",
                "RaiseCommittedAwaitingProof",
                "ArmyProofPending");
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(1));

            // Duplicate Raise is an illegal transition after source
            // consumption. It must fail without a second ally or HUD overlay.
            var duplicateRaise = Assert.Throws<TargetInvocationException>(
                () => ExecuteResourceRaise(f));
            Assert.That(
                duplicateRaise.InnerException,
                Is.TypeOf<InvalidOperationException>());

            yield return null;
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(1));
            Assert.That(Read(binding, "OverlayHost"), Is.SameAs(firstOverlay));
            AssertKeys(
                binding,
                "TargetDefeated",
                "RaiseCommittedAwaitingProof",
                "ArmyProofPending");

            // Restart preserves the committed Formation/roster ownership.
            Invoke(
                f.BattleRuntime,
                "RestartBattle",
                NewCommand(
                    "Necrom.Core.Application.RestartBattleCommand",
                    "restart:hud-binding:normal",
                    ReadLong(f.Battle, "Revision")));

            f.Enemy = Invoke(
                f.Enemies,
                "SpawnEnemy",
                "enemy-next-normal",
                Activator.CreateInstance(
                    RequireType(RuntimeNs + "EnemyArchetypeDefinition"),
                    "enemy.next.guard",
                    "frontline.guard",
                    10),
                f.EnemyZone);

            Invoke(
                f.BattleRuntime,
                "StartBattle",
                NewCommand(
                    "Necrom.Core.Application.StartBattleCommand",
                    "start-next:hud-binding:normal",
                    ReadLong(f.Battle, "Revision")));

            yield return null;
            Assert.That(Read(binding, "OverlayHost"), Is.SameAs(firstOverlay));
            AssertKeys(
                binding,
                "TargetActive",
                "RaiseCommittedAwaitingProof",
                "ArmyProofPending");

            // Runtime re-entry must restore semantic truth from the same
            // gameplay dependencies without retaining the previous overlay.
            Invoke(binding, "Shutdown");
            Assert.That(ReadBool(binding, "IsInitialized"), Is.False);
            Assert.That(Read(binding, "OverlayHost"), Is.Null);
            yield return null;
            Assert.That(firstOverlay == null, Is.True);

            Invoke(
                binding,
                "Initialize",
                f.BattleRuntime,
                f.Enemies,
                f.SoulBridge,
                f.Formation,
                f.Roster,
                f.Session,
                f.SafeArea,
                hud.DesignContract,
                hud.CopyProvider,
                hud.FontProvider);

            var secondOverlay = (GameObject)Read(binding, "OverlayHost");
            Assert.That(secondOverlay, Is.Not.Null);
            Assert.That(secondOverlay, Is.Not.SameAs(firstOverlay));
            AssertKeys(
                binding,
                "TargetActive",
                "RaiseCommittedAwaitingProof",
                "ArmyProofPending");

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
            Assert.That(secondOverlay == null, Is.True);
        }

        [UnityTest]
        public IEnumerator DuplicateBindingInitializationRejectsBeforeReplacingLivePresentation()
        {
            var f = NewFixture("duplicate-binding");
            var hud = NewHudDependencies();
            var binding = f.Root.AddComponent(
                RequireType(RuntimeNs + "FirstPlayableCombatHudRuntimeBinding"));

            Invoke(
                binding,
                "Initialize",
                f.BattleRuntime,
                f.Enemies,
                f.SoulBridge,
                f.Formation,
                f.Roster,
                f.Session,
                f.SafeArea,
                hud.DesignContract,
                hud.CopyProvider,
                hud.FontProvider);

            var overlay = Read(binding, "OverlayHost");
            var before = Read(binding, "LastPresentation");

            var duplicate = Assert.Throws<TargetInvocationException>(
                () => Invoke(
                    binding,
                    "Initialize",
                    f.BattleRuntime,
                    f.Enemies,
                    f.SoulBridge,
                    f.Formation,
                    f.Roster,
                    f.Session,
                    f.SafeArea,
                    hud.DesignContract,
                    hud.CopyProvider,
                    hud.FontProvider));

            Assert.That(
                duplicate.InnerException,
                Is.TypeOf<InvalidOperationException>());
            Assert.That(Read(binding, "OverlayHost"), Is.SameAs(overlay));
            Assert.That(Read(binding, "LastPresentation"), Is.SameAs(before));
            Assert.That(ReadBool(binding, "IsInitialized"), Is.True);

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }


        [UnityTest]
        public IEnumerator DisableDestroyAndRecreateCleansOverlayAndRestoresTruth()
        {
            var f = NewFixture("lifecycle-reentry");
            var hud = NewHudDependencies();
            var binding = CreateBinding(f, hud, f.Session);

            AssertKeys(binding, "TargetActive", "RaiseTargetNotReady", "ArmyEmpty");
            var firstOverlay = (GameObject)Read(binding, "OverlayHost");
            Assert.That(firstOverlay, Is.Not.Null);

            ((Behaviour)binding).enabled = false;
            Assert.That(ReadBool(binding, "IsInitialized"), Is.False);
            Assert.That(Read(binding, "OverlayHost"), Is.Null);
            yield return null;
            Assert.That(firstOverlay == null, Is.True);

            ((Behaviour)binding).enabled = true;
            InitializeBinding(binding, f, hud, f.Session);
            var secondOverlay = (GameObject)Read(binding, "OverlayHost");
            Assert.That(secondOverlay, Is.Not.Null);
            AssertKeys(binding, "TargetActive", "RaiseTargetNotReady", "ArmyEmpty");

            UnityEngine.Object.Destroy(binding);
            yield return null;
            Assert.That(secondOverlay == null, Is.True);

            var recreated = CreateBinding(f, hud, f.Session);
            var thirdOverlay = (GameObject)Read(recreated, "OverlayHost");
            Assert.That(thirdOverlay, Is.Not.Null);
            AssertKeys(recreated, "TargetActive", "RaiseTargetNotReady", "ArmyEmpty");

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
            Assert.That(thirdOverlay == null, Is.True);
        }

        [UnityTest]
        public IEnumerator StaleAndIllegalRestartRepairLayoutWithoutChangingHudTruth()
        {
            var illegal = NewFixture("illegal-restart");
            var hud = NewHudDependencies();
            var illegalBinding = CreateBinding(illegal, hud, illegal.Session);
            var illegalOverlay = Read(illegalBinding, "OverlayHost");

            var illegalMin = new Vector2(0.123f, 0.234f);
            var illegalMax = new Vector2(0.876f, 0.934f);
            illegal.SafeArea.anchorMin = illegalMin;
            illegal.SafeArea.anchorMax = illegalMax;

            var illegalError = Assert.Throws<TargetInvocationException>(
                () => Invoke(
                    illegal.BattleRuntime,
                    "RestartBattle",
                    NewCommand(
                        "Necrom.Core.Application.RestartBattleCommand",
                        "restart:illegal:hud",
                        ReadLong(illegal.Battle, "Revision"))));

            Assert.That(
                illegalError.InnerException,
                Is.TypeOf<InvalidOperationException>());
            Assert.That(Read(illegal.Battle, "Phase").ToString(), Is.EqualTo("Ready"));
            Assert.That(ReadLong(illegal.Battle, "Revision"), Is.EqualTo(0L));
            Assert.That(illegal.SafeArea.anchorMin.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(illegal.SafeArea.anchorMin.y, Is.EqualTo(34f / 844f).Within(0.0001f));
            Assert.That(illegal.SafeArea.anchorMax.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(illegal.SafeArea.anchorMax.y, Is.EqualTo(810f / 844f).Within(0.0001f));
            Invoke(illegalBinding, "RefreshNow");
            Assert.That(Read(illegalBinding, "OverlayHost"), Is.SameAs(illegalOverlay));
            AssertKeys(
                illegalBinding,
                "TargetActive",
                "RaiseTargetNotReady",
                "ArmyEmpty");

            UnityEngine.Object.Destroy(illegal.Root);
            yield return null;

            var stale = NewFixture("stale-restart");
            var staleBinding = CreateBinding(stale, hud, stale.Session);
            MoveToResolvedWithDefeatedTarget(stale, "stale-restart");
            Invoke(staleBinding, "RefreshNow");
            AssertKeys(staleBinding, "TargetDefeated", "RaiseEligible", "ArmyEmpty");
            var staleOverlay = Read(staleBinding, "OverlayHost");

            var staleMin = new Vector2(0.141f, 0.241f);
            var staleMax = new Vector2(0.841f, 0.941f);
            stale.SafeArea.anchorMin = staleMin;
            stale.SafeArea.anchorMax = staleMax;
            var revisionBefore = ReadLong(stale.Battle, "Revision");

            var staleError = Assert.Throws<TargetInvocationException>(
                () => Invoke(
                    stale.BattleRuntime,
                    "RestartBattle",
                    NewCommand(
                        "Necrom.Core.Application.RestartBattleCommand",
                        "restart:stale:hud",
                        revisionBefore - 1L)));

            Assert.That(
                staleError.InnerException,
                Is.TypeOf<InvalidOperationException>());
            Assert.That(Read(stale.Battle, "Phase").ToString(), Is.EqualTo("Resolved"));
            Assert.That(ReadLong(stale.Battle, "Revision"), Is.EqualTo(revisionBefore));
            Assert.That(stale.SafeArea.anchorMin.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(stale.SafeArea.anchorMin.y, Is.EqualTo(34f / 844f).Within(0.0001f));
            Assert.That(stale.SafeArea.anchorMax.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(stale.SafeArea.anchorMax.y, Is.EqualTo(810f / 844f).Within(0.0001f));
            Invoke(staleBinding, "RefreshNow");
            Assert.That(Read(staleBinding, "OverlayHost"), Is.SameAs(staleOverlay));
            AssertKeys(staleBinding, "TargetDefeated", "RaiseEligible", "ArmyEmpty");

            UnityEngine.Object.Destroy(stale.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RosterMismatchRejectsRestartWithoutPromotingStaleProof()
        {
            var f = NewFixture("roster-mismatch");
            var hud = NewHudDependencies();
            var binding = CreateBinding(f, hud, f.Session);

            MoveToResolvedWithDefeatedTarget(f, "roster-mismatch");
            ExecuteResourceRaise(f);
            Invoke(binding, "RefreshNow");
            AssertKeys(
                binding,
                "TargetDefeated",
                "RaiseCommittedAwaitingProof",
                "ArmyProofPending");

            var slotsField = f.Roster.GetType().GetField(
                "_slots",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(slotsField, Is.Not.Null);
            var slots = (Array)slotsField.GetValue(f.Roster);
            var runtimeAlly = (Component)slots.GetValue(0);
            Assert.That(runtimeAlly, Is.Not.Null);
            slots.SetValue(null, 0);
            UnityEngine.Object.Destroy(runtimeAlly.gameObject);
            yield return null;

            Invoke(binding, "RefreshNow");
            AssertKeys(
                binding,
                "TargetDefeated",
                "RaiseSourceUnavailableOrConsumed",
                "ArmyOwned");
            var overlay = Read(binding, "OverlayHost");

            var markerMin = new Vector2(0.161f, 0.261f);
            var markerMax = new Vector2(0.861f, 0.961f);
            f.SafeArea.anchorMin = markerMin;
            f.SafeArea.anchorMax = markerMax;
            var revisionBefore = ReadLong(f.Battle, "Revision");

            var restartError = Assert.Throws<TargetInvocationException>(
                () => Invoke(
                    f.BattleRuntime,
                    "RestartBattle",
                    NewCommand(
                        "Necrom.Core.Application.RestartBattleCommand",
                        "restart:roster-mismatch:hud",
                        revisionBefore)));

            Assert.That(
                restartError.InnerException,
                Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadLong(f.Battle, "Revision"), Is.EqualTo(revisionBefore));
            Assert.That(Read(f.Battle, "Phase").ToString(), Is.EqualTo("Resolved"));
            Assert.That(f.SafeArea.anchorMin, Is.EqualTo(markerMin));
            Assert.That(f.SafeArea.anchorMax, Is.EqualTo(markerMax));

            Invoke(binding, "RefreshNow");
            Assert.That(Read(binding, "OverlayHost"), Is.SameAs(overlay));
            AssertKeys(
                binding,
                "TargetDefeated",
                "RaiseSourceUnavailableOrConsumed",
                "ArmyOwned");

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SessionProofCarriesWithSameSessionAndResetsWithNewSession()
        {
            var f = NewFixture("session-boundary");
            var hud = NewHudDependencies();
            var binding = CreateBinding(f, hud, f.Session);

            MoveToResolvedWithDefeatedTarget(f, "session-boundary");
            ExecuteResourceRaise(f);
            Invoke(
                f.BattleRuntime,
                "RestartBattle",
                NewCommand(
                    "Necrom.Core.Application.RestartBattleCommand",
                    "restart:session-boundary:hud",
                    ReadLong(f.Battle, "Revision")));

            f.Enemy = Invoke(
                f.Enemies,
                "SpawnEnemy",
                "enemy-next-session-boundary",
                Activator.CreateInstance(
                    RequireType(RuntimeNs + "EnemyArchetypeDefinition"),
                    "enemy.next.guard",
                    "frontline.guard",
                    10),
                f.EnemyZone);

            Invoke(
                f.BattleRuntime,
                "StartBattle",
                NewCommand(
                    "Necrom.Core.Application.StartBattleCommand",
                    "start-next:session-boundary:hud",
                    ReadLong(f.Battle, "Revision")));

            Assert.That(
                (bool)Invoke(
                    f.Session,
                    "ObserveContribution",
                    NewEntityId("undead-session-boundary"),
                    NewEntityId("enemy-next-session-boundary"),
                    4),
                Is.True);

            Invoke(binding, "RefreshNow");
            AssertKeys(
                binding,
                "TargetActive",
                "RaiseProofObserved",
                "ArmyProofObserved");

            Invoke(binding, "Shutdown");
            yield return null;
            InitializeBinding(binding, f, hud, f.Session);
            AssertKeys(
                binding,
                "TargetActive",
                "RaiseProofObserved",
                "ArmyProofObserved");

            Invoke(binding, "Shutdown");
            yield return null;
            var freshSession = Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudSession"));
            InitializeBinding(binding, f, hud, freshSession);
            AssertKeys(
                binding,
                "TargetActive",
                "RaiseTargetNotReady",
                "ArmyOwned");

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        private static Component CreateBinding(
            Fixture f,
            HudDependencies hud,
            object session)
        {
            var binding = f.Root.AddComponent(
                RequireType(RuntimeNs + "FirstPlayableCombatHudRuntimeBinding"));
            InitializeBinding(binding, f, hud, session);
            return binding;
        }

        private static void InitializeBinding(
            Component binding,
            Fixture f,
            HudDependencies hud,
            object session)
        {
            Invoke(
                binding,
                "Initialize",
                f.BattleRuntime,
                f.Enemies,
                f.SoulBridge,
                f.Formation,
                f.Roster,
                session,
                f.SafeArea,
                hud.DesignContract,
                hud.CopyProvider,
                hud.FontProvider);
        }

        private static void MoveToResolvedWithDefeatedTarget(
            Fixture f,
            string suffix)
        {
            Invoke(
                f.BattleRuntime,
                "StartBattle",
                NewCommand(
                    "Necrom.Core.Application.StartBattleCommand",
                    "start:" + suffix + ":hud",
                    ReadLong(f.Battle, "Revision")));

            Invoke(
                f.Enemy,
                "ApplyDamage",
                5,
                NewEntityId("source:" + suffix + ":hud"));

            Invoke(
                f.BattleRuntime,
                "ResolveFromCombatResult",
                NewResolveCommand(
                    "resolve:" + suffix + ":hud",
                    true,
                    ReadLong(f.Battle, "Revision")));

            Invoke(
                f.BattleRuntime,
                "FinalizeBattle",
                NewCommand(
                    "Necrom.Core.Application.FinalizeBattleCommand",
                    "finalize:" + suffix + ":hud",
                    ReadLong(f.Battle, "Revision")));
        }

        private static Fixture NewFixture(string suffix)
        {
            _suffix = suffix;
            _undeadId = "undead-" + suffix;
            _raiseCost = 3;

            var root = new GameObject(
                "HudRuntimeBinding-" + suffix,
                typeof(RectTransform));

            var boundary = root.AddComponent(
                RequireType(RuntimeNs + "EncounterBoundaryController"));

            var layout = Activator.CreateInstance(
                RequireType(RuntimeNs + "EncounterLayoutConfig"),
                new object[]
                {
                    new Vector2(390f, 844f),
                    new Rect(0f, 34f, 390f, 776f),
                    0.22f,
                    new Rect(0.12f, 0.08f, 0.30f, 0.24f),
                    new Rect(0.58f, 0.58f, 0.30f, 0.28f),
                    new Rect(0.12f, 0.34f, 0.44f, 0.18f)
                });

            var readability = Activator.CreateInstance(
                RequireType(RuntimeNs + "EncounterReadabilityConfig"),
                new object[]
                {
                    new Rect(0.02f, 0.02f, 0.28f, 0.08f),
                    new Rect(0.36f, 0.02f, 0.28f, 0.08f),
                    new Rect(0.70f, 0.02f, 0.28f, 0.08f),
                    new Rect(0.05f, 0.28f, 0.90f, 0.65f)
                });

            Invoke(
                boundary,
                "StartBoundaryWithReadability",
                layout,
                readability);

            var safeArea = root.transform.Find("SafeArea") as RectTransform;
            var combat = safeArea.Find("CombatViewport") as RectTransform;
            var necromancerZone =
                combat.Find("NecromancerSpawnZone") as RectTransform;
            var enemyZone =
                combat.Find("EnemySpawnZone") as RectTransform;
            var alliedZone =
                combat.Find("AlliedSpawnZone") as RectTransform;

            Assert.That(safeArea, Is.Not.Null);
            Assert.That(combat, Is.Not.Null);
            Assert.That(necromancerZone, Is.Not.Null);
            Assert.That(enemyZone, Is.Not.Null);
            Assert.That(alliedZone, Is.Not.Null);

            var formation = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.Formation"));
            _formationForCommand = formation;

            var battle = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.BattleStateMachine"));
            var encounter = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.FirstPlayableEncounter"),
                Activator.CreateInstance(
                    RequireType("Necrom.Core.Domain.RaiseService")),
                battle,
                formation);
            var app = Activator.CreateInstance(
                RequireType(
                    "Necrom.Core.Application.FirstPlayableApplicationService"),
                encounter);
            var progression = Activator.CreateInstance(
                RequireType(
                    "Necrom.Core.Application.FirstPlayableProgression"),
                app,
                encounter);

            var necromancer = combat.gameObject.AddComponent(
                RequireType(RuntimeNs + "NecromancerAnchorController"));
            Invoke(
                necromancer,
                "BindNecromancer",
                NewCombatant(
                    "player-" + suffix,
                    "necromancer.prototype",
                    "Player",
                    100),
                necromancerZone);

            var enemies = combat.gameObject.AddComponent(
                RequireType(RuntimeNs + "EnemySpawnController"));
            var enemy = Invoke(
                enemies,
                "SpawnEnemy",
                "enemy-" + suffix,
                Activator.CreateInstance(
                    RequireType(RuntimeNs + "EnemyArchetypeDefinition"),
                    "enemy.skeleton.guard",
                    "frontline.guard",
                    5),
                enemyZone);

            var roster = combat.gameObject.AddComponent(
                RequireType(
                    RuntimeNs + "FirstPlayableAlliedRosterController"));
            Invoke(roster, "Initialize", formation, alliedZone);

            var battleRuntime = combat.gameObject.AddComponent(
                RequireType(
                    RuntimeNs + "FirstPlayableBattleRuntimeController"));
            Invoke(
                battleRuntime,
                "Initialize",
                app,
                boundary,
                layout,
                necromancer,
                enemies);
            Invoke(
                battleRuntime,
                "ConfigureAlliedRestartInvariant",
                roster);

            var hook = root.AddComponent(
                RequireType(
                    RuntimeNs + "FirstPlayableRaiseCommandInputHook"));
            Invoke(hook, "Initialize", progression);

            var account = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.SoulResourceAccount"),
                10);
            var bridgeType =
                RequireType(RuntimeNs + "FirstPlayableSoulResourceBridge");
            var bridgeCtor = bridgeType.GetConstructors().Single();
            var bridgeParameters = bridgeCtor.GetParameters();
            var soulBridge = bridgeCtor.Invoke(new object[]
            {
                account,
                BuildUnaryDelegate(
                    bridgeParameters[1].ParameterType,
                    nameof(GrantPolicyObject)),
                BuildUnaryDelegate(
                    bridgeParameters[2].ParameterType,
                    nameof(CostPolicyObject)),
                BuildUnaryDelegate(
                    bridgeParameters[3].ParameterType,
                    nameof(TransactionIdObject))
            });

            var session = Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudSession"));

            var raiseAction = root.AddComponent(
                RequireType(
                    RuntimeNs + "FirstPlayableRaiseActionController"));
            var initializeRaise = raiseAction.GetType().GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.Public);
            var raiseParameters = initializeRaise.GetParameters();
            initializeRaise.Invoke(
                raiseAction,
                new object[]
                {
                    enemies,
                    hook,
                    BuildUnaryDelegate(
                        raiseParameters[2].ParameterType,
                        nameof(BuildRaiseCommandObject)),
                    (Func<string>)(() =>
                        "raised:hud-binding:" + suffix),
                    (Func<string>)(() =>
                        "assigned:hud-binding:" + suffix)
                });

            var configureAllied = raiseAction.GetType().GetMethod(
                "ConfigureAlliedActivation",
                BindingFlags.Instance | BindingFlags.Public);
            var alliedParameters = configureAllied.GetParameters();
            configureAllied.Invoke(
                raiseAction,
                new object[]
                {
                    roster,
                    BuildUnaryDelegate(
                        alliedParameters[1].ParameterType,
                        nameof(BuildBehaviorSpecObject))
                });
            Invoke(raiseAction, "ConfigureHudSession", session);

            return new Fixture
            {
                Root = root,
                SafeArea = safeArea,
                EnemyZone = enemyZone,
                Formation = formation,
                Battle = battle,
                Enemies = enemies,
                Enemy = enemy,
                Roster = roster,
                BattleRuntime = battleRuntime,
                Account = account,
                SoulBridge = soulBridge,
                Session = session,
                RaiseAction = raiseAction
            };
        }

        private static HudDependencies NewHudDependencies()
        {
            var designContract =
                RequireType(
                    RuntimeNs +
                    "FirstPlayableCombatHudVerifiedDesignContract")
                .GetMethod(
                    "Create",
                    BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            var copyAdapterType =
                RequireType(
                    RuntimeNs +
                    "FirstPlayableCombatHudCopyProviderAdapter");
            var copyDelegateType =
                copyAdapterType.GetConstructors().Single()
                    .GetParameters()[0].ParameterType;
            var copyProvider = Activator.CreateInstance(
                copyAdapterType,
                BuildUnaryDelegate(
                    copyDelegateType,
                    nameof(ResolveCopyObject)));

            var fontAdapterType =
                RequireType(
                    RuntimeNs +
                    "FirstPlayableCombatHudFontProviderAdapter");
            var fontDelegateType =
                fontAdapterType.GetConstructors().Single()
                    .GetParameters()[0].ParameterType;
            var fontProvider = Activator.CreateInstance(
                fontAdapterType,
                BuildZeroDelegate(
                    fontDelegateType,
                    nameof(ResolveFontObject)));

            return new HudDependencies
            {
                DesignContract = designContract,
                CopyProvider = copyProvider,
                FontProvider = fontProvider
            };
        }

        private static void ExecuteResourceRaise(Fixture f)
        {
            var method = f.SoulBridge.GetType().GetMethod(
                "ExecuteRaise",
                BindingFlags.Instance | BindingFlags.Public);
            method.Invoke(
                f.SoulBridge,
                new[]
                {
                    (object)f.Enemies,
                    f.RaiseAction,
                    Read(f.Account, "Revision")
                });
        }

        private static void AssertKeys(
            object binding,
            string target,
            string raise,
            string army)
        {
            var presentation = Read(binding, "LastPresentation");
            Assert.That(presentation, Is.Not.Null);

            Assert.That(
                Read(Read(presentation, "Target"), "ContentKey").ToString(),
                Is.EqualTo(target));
            Assert.That(
                Read(Read(presentation, "Raise"), "ContentKey").ToString(),
                Is.EqualTo(raise));
            Assert.That(
                Read(Read(presentation, "Army"), "ContentKey").ToString(),
                Is.EqualTo(army));
        }

        private static object NewCommand(
            string fullName,
            string commandId,
            long revision)
            => Activator.CreateInstance(
                RequireType(fullName),
                commandId,
                revision);

        private static object NewResolveCommand(
            string commandId,
            bool playerWon,
            long revision)
            => Activator.CreateInstance(
                RequireType(
                    "Necrom.Core.Application.ResolveBattleCommand"),
                commandId,
                playerWon,
                revision);

        private static object NewCombatant(
            string entityId,
            string archetypeId,
            string factionName,
            int health)
        {
            var faction = Enum.Parse(
                RequireType("Necrom.Core.Domain.Faction"),
                factionName);
            return Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.Combatant"),
                NewEntityId(entityId),
                archetypeId,
                faction,
                health);
        }

        private static object NewEntityId(string value)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.EntityId"),
                value);

        private static object BuildRaiseCommandObject(object source)
            => Activator.CreateInstance(
                RequireType(
                    "Necrom.Core.Application.RaiseIntoFormationCommand"),
                "raise:hud-binding:" + _suffix,
                source,
                ReadLong(source, "Revision"),
                NewEntityId(_undeadId),
                7,
                0,
                ReadLong(_formationForCommand, "Revision"));

        private static object BuildBehaviorSpecObject(object command)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.BasicAutoBehaviorSpec"),
                4,
                100);

        private static int GrantPolicyObject(object value)
            => 4;

        private static int CostPolicyObject(object value)
            => _raiseCost;

        private static string TransactionIdObject(object value)
            => "soul-spend:hud-binding:" + _suffix;

        private static object ResolveCopyObject(object key)
        {
            var text = key.ToString();
            return Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudCopy"),
                text,
                "Primary:" + text,
                "Secondary:" + text,
                "CTA:" + text);
        }

        private static object ResolveFontObject()
        {
            if (_fontAsset != null)
                return _fontAsset;

            var settingsType = RequireType("TMPro.TMP_Settings");
            var settingsField = settingsType.GetField(
                "s_Instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(settingsField, Is.Not.Null);
            if (settingsField.GetValue(null) == null)
            {
                settingsField.SetValue(
                    null,
                    ScriptableObject.CreateInstance(settingsType));
            }

            var fontAssetType = RequireType("TMPro.TMP_FontAsset");
            _fontAsset = ScriptableObject.CreateInstance(fontAssetType);
            _fontAsset.name = "TEST-ONLY Runtime Binding Font";

            var versionField = fontAssetType.GetField(
                "m_Version",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(versionField, Is.Not.Null);
            versionField.SetValue(_fontAsset, "1.1.0");

            var shader = Shader.Find("UI/Default");
            Assert.That(shader, Is.Not.Null);
            _fontAtlas = new Texture2D(2, 2);
            _fontMaterial = new Material(shader);
            _fontMaterial.mainTexture = _fontAtlas;

            fontAssetType.GetProperty("material")
                .SetValue(_fontAsset, _fontMaterial);
            fontAssetType.GetProperty("atlasTextures")
                .SetValue(_fontAsset, new[] { _fontAtlas });

            return _fontAsset;
        }

        private static Delegate BuildZeroDelegate(
            Type delegateType,
            string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var helper =
                typeof(FirstPlayableCombatHudRuntimeBindingTests)
                .GetMethod(
                    helperName,
                    BindingFlags.Static | BindingFlags.NonPublic);

            return Expression.Lambda(
                    delegateType,
                    Expression.Convert(
                        Expression.Call(helper),
                        invoke.ReturnType))
                .Compile();
        }

        private static Delegate BuildUnaryDelegate(
            Type delegateType,
            string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var input = Expression.Parameter(
                invoke.GetParameters()[0].ParameterType,
                "value");
            var helper =
                typeof(FirstPlayableCombatHudRuntimeBindingTests)
                .GetMethod(
                    helperName,
                    BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(
                helper,
                Expression.Convert(input, typeof(object)));

            return Expression.Lambda(
                    delegateType,
                    Expression.Convert(call, invoke.ReturnType),
                    input)
                .Compile();
        }

        private static object Invoke(
            object target,
            string method,
            params object[] args)
        {
            var candidates = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(candidate =>
                    candidate.Name == method &&
                    candidate.GetParameters().Length == args.Length)
                .Where(candidate =>
                    ParametersAccept(
                        candidate.GetParameters(),
                        args))
                .ToArray();

            Assert.That(
                candidates.Length,
                Is.EqualTo(1),
                target.GetType().Name + "." + method);
            return candidates[0].Invoke(target, args);
        }

        private static bool ParametersAccept(
            ParameterInfo[] parameters,
            object[] args)
        {
            for (var index = 0; index < parameters.Length; index++)
            {
                if (args[index] == null)
                {
                    if (parameters[index].ParameterType.IsValueType)
                        return false;
                    continue;
                }

                if (!parameters[index].ParameterType.IsInstanceOfType(
                        args[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static object Read(object target, string property)
        {
            if (target == null)
                return null;

            var info = target.GetType().GetProperty(
                property,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(
                info,
                Is.Not.Null,
                target.GetType().Name + "." + property);
            return info.GetValue(target);
        }

        private static int ReadInt(object target, string property)
            => Convert.ToInt32(Read(target, property));

        private static long ReadLong(object target, string property)
            => Convert.ToInt64(Read(target, property));

        private static bool ReadBool(object target, string property)
            => Convert.ToBoolean(Read(target, property));

        private static Type RequireType(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName))
                .FirstOrDefault(candidate => candidate != null);
            Assert.That(type, Is.Not.Null, fullName + " must exist.");
            return type;
        }

        private sealed class Fixture
        {
            public GameObject Root;
            public RectTransform SafeArea;
            public RectTransform EnemyZone;
            public object Formation;
            public object Battle;
            public Component Enemies;
            public object Enemy;
            public Component Roster;
            public Component BattleRuntime;
            public object Account;
            public object SoulBridge;
            public object Session;
            public Component RaiseAction;
        }

        private sealed class HudDependencies
        {
            public object DesignContract;
            public object CopyProvider;
            public object FontProvider;
        }
    }
}
