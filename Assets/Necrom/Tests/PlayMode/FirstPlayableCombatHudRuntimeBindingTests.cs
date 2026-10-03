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
