using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableCombatHudStateProjectionTests
    {
        private static string _suffix;
        private static string _undeadId;
        private static object _formationForCommand;
        private static int _raiseCost;
        private static int _costPolicyCalls;
        private static int _transactionIdCalls;

        [UnityTest]
        public IEnumerator ApprovedA1ContractsExist()
        {
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudState"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudProjector"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudSession"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.SoulResourceRaiseQuote"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseAvailabilityReason"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableProofStatus"), Is.Not.Null);
            Assert.That(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableSoulResourceBridge").GetMethod("QuoteRaise"), Is.Not.Null);
            Assert.That(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseActionController").GetMethod("ConfigureHudSession"), Is.Not.Null);
            Assert.That(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedAutoCombatLoop").GetMethod("ConfigureHudSession"), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProjectionReflectsBattleTargetAndTruthfulRaiseReasons()
        {
            var active = NewFixture("active", true, false, 10, 3);
            Invoke(active.App, "Execute", NewCommand("Necrom.Core.Application.StartBattleCommand", "start:hud:active", ReadLong(active.Battle, "Revision")));
            var activeState = Capture(active.Projector);
            Assert.That(Read(activeState, "BattlePhase").ToString(), Is.EqualTo("Running"));
            Assert.That(ReadLong(activeState, "BattleRevision"), Is.EqualTo(1));
            var target = Read(activeState, "Target");
            Assert.That(Read(target, "EntityId").ToString(), Is.EqualTo("enemy-active"));
            Assert.That(Read(target, "ArchetypeId").ToString(), Is.EqualTo("enemy.skeleton.guard"));
            Assert.That(Read(target, "RoleId").ToString(), Is.EqualTo("frontline.guard"));
            Assert.That(ReadInt(target, "Health"), Is.EqualTo(5));
            Assert.That(Read(target, "LifeState").ToString(), Is.EqualTo("Active"));
            Assert.That(Read(activeState, "RaiseReason").ToString(), Is.EqualTo("TargetNotRaiseReady"));
            Assert.That(ReadBool(activeState, "IsRaiseProcessing"), Is.False);
            UnityEngine.Object.Destroy(active.Root);

            var none = NewFixture("none", false, false, 10, 3);
            Assert.That(Read(Capture(none.Projector), "RaiseReason").ToString(), Is.EqualTo("NoTarget"));
            UnityEngine.Object.Destroy(none.Root);

            var consumed = NewFixture("consumed", true, true, 10, 3);
            Invoke(consumed.Source, "Consume", ReadLong(consumed.Source, "Revision"));
            Assert.That(Read(Capture(consumed.Projector), "RaiseReason").ToString(), Is.EqualTo("SourceUnavailableOrConsumed"));
            UnityEngine.Object.Destroy(consumed.Root);

            var poor = NewFixture("poor", true, true, 2, 3);
            Assert.That(Read(Capture(poor.Projector), "RaiseReason").ToString(), Is.EqualTo("InsufficientSoul"));
            UnityEngine.Object.Destroy(poor.Root);

            var eligible = NewFixture("eligible", true, true, 10, 3);
            var eligibleState = Capture(eligible.Projector);
            Assert.That(Read(eligibleState, "RaiseReason").ToString(), Is.EqualTo("Eligible"));
            var quote = Read(eligibleState, "SoulQuote");
            Assert.That(ReadInt(quote, "Balance"), Is.EqualTo(10));
            Assert.That(ReadLong(quote, "Revision"), Is.EqualTo(0));
            Assert.That(ReadInt(quote, "Cost"), Is.EqualTo(3));
            Assert.That(ReadBool(quote, "CanAfford"), Is.True);
            UnityEngine.Object.Destroy(eligible.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SoulQuoteUsesExistingPolicyWithoutMutation()
        {
            var f = NewFixture("quote", true, true, 10, 3);
            var balanceBefore = ReadInt(f.Account, "Balance");
            var accountRevisionBefore = ReadLong(f.Account, "Revision");
            var sourceRevisionBefore = ReadLong(f.Source, "Revision");
            var formationRevisionBefore = ReadLong(f.Formation, "Revision");
            _costPolicyCalls = 0;
            _transactionIdCalls = 0;

            var quoteMethod = f.SoulBridge.GetType().GetMethod("QuoteRaise", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(quoteMethod, Is.Not.Null);
            var quote = quoteMethod.Invoke(f.SoulBridge, new[] { f.Source });

            Assert.That(ReadInt(quote, "Balance"), Is.EqualTo(10));
            Assert.That(ReadLong(quote, "Revision"), Is.EqualTo(0));
            Assert.That(ReadInt(quote, "Cost"), Is.EqualTo(3));
            Assert.That(ReadBool(quote, "CanAfford"), Is.True);
            Assert.That(_costPolicyCalls, Is.EqualTo(1));
            Assert.That(_transactionIdCalls, Is.EqualTo(0));
            Assert.That(ReadInt(f.Account, "Balance"), Is.EqualTo(balanceBefore));
            Assert.That(ReadLong(f.Account, "Revision"), Is.EqualTo(accountRevisionBefore));
            Assert.That(ReadLong(f.Source, "Revision"), Is.EqualTo(sourceRevisionBefore));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(formationRevisionBefore));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FormationOwnershipIsTruthAndSuccessfulRaiseCreatesPendingReceipt()
        {
            var ownership = NewFixture("ownership", false, false, 10, 3);
            Invoke(ownership.Formation, "Assign", 0, NewEntityId("owned-formation"), ReadLong(ownership.Formation, "Revision"));
            var ownershipState = Capture(ownership.Projector);
            var slots = ReadList(ownershipState, "FormationSlots");
            Assert.That(slots.Count, Is.EqualTo(5));
            Assert.That(ReadInt(slots[0], "Slot"), Is.EqualTo(0));
            Assert.That(Read(slots[0], "OwnedUnitId").ToString(), Is.EqualTo("owned-formation"));
            Assert.That(Read(slots[0], "RuntimeUnitId"), Is.Null);
            Assert.That(ReadBool(slots[0], "OwnershipMatchesRuntime"), Is.False);
            Assert.That(GetFormationSlot(ownership.Formation, 0).ToString(), Is.EqualTo("owned-formation"));
            UnityEngine.Object.Destroy(ownership.Root);

            var raised = NewFixture("receipt", true, true, 10, 3);
            ConfigureRaise(raised, "undead-receipt");
            ExecuteResourceRaise(raised);
            var raisedState = Capture(raised.Projector);
            var receipt = Read(raisedState, "LastCommittedRaise");
            Assert.That(receipt, Is.Not.Null);
            Assert.That(Read(receipt, "UnitId").ToString(), Is.EqualTo("undead-receipt"));
            Assert.That(ReadInt(receipt, "Slot"), Is.EqualTo(0));
            Assert.That(Read(raisedState, "ProofStatus").ToString(), Is.EqualTo("Pending"));
            Assert.That(ReadBool(raisedState, "IsRaiseProcessing"), Is.False);
            Assert.That(GetFormationSlot(raised.Formation, 0).ToString(), Is.EqualTo("undead-receipt"));
            var runtimeAlly = GetRosterSlot(raised.Roster, 0);
            Assert.That(Read(Read(runtimeAlly, "Model"), "Id").ToString(), Is.EqualTo("undead-receipt"));
            var persistenceFields = raised.Session.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.Name.IndexOf("snapshot", StringComparison.OrdinalIgnoreCase) >= 0 || x.Name.IndexOf("persist", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToArray();
            Assert.That(persistenceFields, Is.Empty);

            UnityEngine.Object.Destroy(raised.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OnlyExactCommittedRaisedUnitActualContributionMarksProofObserved()
        {
            var f = NewFixture("proof", true, true, 10, 3);
            ConfigureRaise(f, "undead-proof");
            ExecuteResourceRaise(f);

            Invoke(f.Session, "ObserveContribution", NewEntityId("other-ally"), NewEntityId("enemy-other"), 4);
            Assert.That(Read(Capture(f.Projector), "ProofStatus").ToString(), Is.EqualTo("Pending"));

            var next = StartNextCombat(f, 10, 4, 100);
            ConfigureLoopSession(next.Loop, f.Session);
            Invoke(next.Loop, "Advance", 0.100f);

            var state = Capture(f.Projector);
            Assert.That(Read(state, "ProofStatus").ToString(), Is.EqualTo("Observed"));
            var contribution = Read(state, "ObservedContribution");
            Assert.That(Read(contribution, "ActingUnitId").ToString(), Is.EqualTo("undead-proof"));
            Assert.That(Read(contribution, "TargetId").ToString(), Is.EqualTo("enemy-next-proof"));
            Assert.That(ReadInt(contribution, "ActualContribution"), Is.EqualTo(4));
            Assert.That(ReadInt(Read(next.Enemy, "Model"), "Health"), Is.EqualTo(6));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StaleReceiptAndDefeatedAllyCannotCreateFalseProof()
        {
            var f = NewFixture("staleproof", true, true, 10, 3);
            ConfigureRaise(f, "undead-staleproof");
            ExecuteResourceRaise(f);

            var staleFormation = Activator.CreateInstance(RequireType("Necrom.Core.Domain.Formation"));
            var staleProjector = NewProjector(f.BattleRuntime, f.Enemies, f.SoulBridge, staleFormation, f.Roster, f.Session);
            var staleState = Capture(staleProjector);
            Assert.That(Read(staleState, "LastCommittedRaise"), Is.Null);
            Assert.That(Read(staleState, "ProofStatus").ToString(), Is.EqualTo("None"));

            var ally = GetRosterSlot(f.Roster, 0);
            var allyModel = Read(ally, "Model");
            Invoke(allyModel, "ApplyDamage", ReadInt(allyModel, "Health"));
            var next = StartNextCombat(f, 10, 4, 100);
            ConfigureLoopSession(next.Loop, f.Session);
            Invoke(next.Loop, "Advance", 0.500f);

            var liveState = Capture(f.Projector);
            Assert.That(Read(allyModel, "LifeState").ToString(), Is.EqualTo("Defeated"));
            Assert.That(ReadInt(Read(next.Enemy, "Model"), "Health"), Is.EqualTo(10));
            Assert.That(Read(liveState, "ProofStatus").ToString(), Is.EqualTo("Pending"));
            Assert.That(Read(liveState, "ObservedContribution"), Is.Null);

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        private static Fixture NewFixture(string suffix, bool spawnEnemy, bool defeatEnemy, int initialSoul, int raiseCost)
        {
            _suffix = suffix;
            _raiseCost = raiseCost;
            _costPolicyCalls = 0;
            _transactionIdCalls = 0;

            var root = new GameObject("HudA1-" + suffix, typeof(RectTransform));
            var combat = NewZone(root.transform, "CombatViewport");
            var necroZone = NewZone(combat, "NecromancerSpawnZone");
            var enemyZone = NewZone(combat, "EnemySpawnZone");
            var alliedZone = NewZone(combat, "AlliedSpawnZone");

            var formation = Activator.CreateInstance(RequireType("Necrom.Core.Domain.Formation"));
            var battle = Activator.CreateInstance(RequireType("Necrom.Core.Domain.BattleStateMachine"));
            var encounter = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.FirstPlayableEncounter"),
                Activator.CreateInstance(RequireType("Necrom.Core.Domain.RaiseService")),
                battle,
                formation);
            var app = Activator.CreateInstance(RequireType("Necrom.Core.Application.FirstPlayableApplicationService"), encounter);
            var progression = Activator.CreateInstance(RequireType("Necrom.Core.Application.FirstPlayableProgression"), app, encounter);

            var boundary = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.EncounterBoundaryController"));
            var layout = Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.EncounterLayoutConfig"),
                new object[]
                {
                    new Vector2(390f, 844f),
                    new Rect(0f, 34f, 390f, 776f),
                    0.22f,
                    new Rect(0.12f, 0.08f, 0.30f, 0.24f),
                    new Rect(0.58f, 0.58f, 0.30f, 0.28f),
                    new Rect(0.12f, 0.34f, 0.44f, 0.18f)
                });

            var necro = combat.gameObject.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.NecromancerAnchorController"));
            Invoke(necro, "BindNecromancer", NewCombatant("player-" + suffix, "necromancer.prototype", "Player", 100), necroZone);

            var enemies = combat.gameObject.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.EnemySpawnController"));
            object enemy = null;
            object source = null;
            if (spawnEnemy)
            {
                enemy = Invoke(
                    enemies,
                    "SpawnEnemy",
                    "enemy-" + suffix,
                    Activator.CreateInstance(RequireType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"), "enemy.skeleton.guard", "frontline.guard", 5),
                    enemyZone);
                if (defeatEnemy)
                {
                    Invoke(enemy, "ApplyDamage", 5, NewEntityId("source-" + suffix));
                    source = Read(enemy, "RaiseSource");
                }
            }

            var roster = combat.gameObject.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedRosterController"));
            Invoke(roster, "Initialize", formation, alliedZone);

            var battleRuntime = combat.gameObject.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableBattleRuntimeController"));
            Invoke(battleRuntime, "Initialize", app, boundary, layout, necro, enemies);

            var hook = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseCommandInputHook"));
            Invoke(hook, "Initialize", progression);

            var account = Activator.CreateInstance(RequireType("Necrom.Core.Domain.SoulResourceAccount"), initialSoul);
            var bridgeType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableSoulResourceBridge");
            var ctor = bridgeType.GetConstructors().Single();
            var cp = ctor.GetParameters();
            var soulBridge = ctor.Invoke(new object[]
            {
                account,
                BuildUnaryDelegate(cp[1].ParameterType, nameof(GrantPolicyObject)),
                BuildUnaryDelegate(cp[2].ParameterType, nameof(CostPolicyObject)),
                BuildUnaryDelegate(cp[3].ParameterType, nameof(TransactionIdObject))
            });

            var session = Activator.CreateInstance(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudSession"));
            var projector = NewProjector(battleRuntime, enemies, soulBridge, formation, roster, session);

            return new Fixture
            {
                Suffix = suffix,
                Root = root,
                Combat = combat,
                Necro = necro,
                EnemyZone = enemyZone,
                AlliedZone = alliedZone,
                Formation = formation,
                Battle = battle,
                App = app,
                Enemies = enemies,
                Enemy = enemy,
                Source = source,
                Roster = roster,
                BattleRuntime = battleRuntime,
                Hook = hook,
                Account = account,
                SoulBridge = soulBridge,
                Session = session,
                Projector = projector
            };
        }

        private static void ConfigureRaise(Fixture f, string undeadId)
        {
            _suffix = f.Suffix;
            _undeadId = undeadId;
            _formationForCommand = f.Formation;

            var action = f.Root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseActionController"));
            var init = action.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
            var ip = init.GetParameters();
            init.Invoke(action, new object[]
            {
                f.Enemies,
                f.Hook,
                BuildUnaryDelegate(ip[2].ParameterType, nameof(BuildRaiseCommandObject)),
                (Func<string>)(() => "raised:hud:" + undeadId),
                (Func<string>)(() => "assigned:hud:" + undeadId)
            });

            var configure = action.GetType().GetMethod("ConfigureAlliedActivation", BindingFlags.Instance | BindingFlags.Public);
            var ap = configure.GetParameters();
            configure.Invoke(action, new object[]
            {
                f.Roster,
                BuildUnaryDelegate(ap[1].ParameterType, nameof(BuildBehaviorSpecObject))
            });

            var hud = action.GetType().GetMethod("ConfigureHudSession", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(hud, Is.Not.Null);
            hud.Invoke(action, new[] { f.Session });
            f.RaiseAction = action;
        }

        private static void ExecuteResourceRaise(Fixture f)
        {
            var method = f.SoulBridge.GetType().GetMethod("ExecuteRaise", BindingFlags.Instance | BindingFlags.Public);
            method.Invoke(f.SoulBridge, new object[] { f.Enemies, f.RaiseAction, ReadLong(f.Account, "Revision") });
        }

        private static CombatFixture StartNextCombat(Fixture f, int enemyHealth, int allyDamage, int intervalMs)
        {
            var enemy = Invoke(
                f.Enemies,
                "SpawnEnemy",
                "enemy-next-" + f.Suffix,
                Activator.CreateInstance(RequireType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"), "enemy.next.guard", "frontline.guard", enemyHealth),
                f.EnemyZone);

            var targeting = f.Root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableTargetingController"));
            Invoke(targeting, "Initialize", Read(f.Necro, "CurrentNecromancer"), f.Enemies);

            var pipeline = Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableDamageDeathPipeline"),
                (Func<string>)(() => "damage:hud:" + Guid.NewGuid().ToString("N")),
                (Func<string>)(() => "defeat:hud:" + Guid.NewGuid().ToString("N")));

            if (Read(f.Battle, "Phase").ToString() == "Ready")
            {
                Invoke(f.App, "Execute", NewCommand("Necrom.Core.Application.StartBattleCommand", "start:hud:" + f.Suffix, ReadLong(f.Battle, "Revision")));
            }

            var loop = f.Root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedAutoCombatLoop"));
            var init = loop.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
            var ip = init.GetParameters();
            init.Invoke(loop, new object[]
            {
                f.App,
                f.Roster,
                targeting,
                pipeline,
                BuildUnaryDelegate(ip[4].ParameterType, nameof(BuildRaiseSourceIdObject)),
                (Func<string>)(() => "resolve:hud:" + Guid.NewGuid().ToString("N"))
            });

            return new CombatFixture { Loop = loop, Enemy = enemy };
        }

        private static void ConfigureLoopSession(Component loop, object session)
        {
            var method = loop.GetType().GetMethod("ConfigureHudSession", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            method.Invoke(loop, new[] { session });
        }

        private static object NewProjector(object battleRuntime, object enemies, object soulBridge, object formation, object roster, object session)
        {
            var type = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudProjector");
            return Activator.CreateInstance(type, battleRuntime, enemies, soulBridge, formation, roster, session);
        }

        private static object Capture(object projector) => Invoke(projector, "Capture");

        private static object NewCommand(string fullName, string commandId, long revision)
            => Activator.CreateInstance(RequireType(fullName), commandId, revision);

        private static object NewCombatant(string entityId, string archetypeId, string factionName, int health)
        {
            var faction = Enum.Parse(RequireType("Necrom.Core.Domain.Faction"), factionName);
            return Activator.CreateInstance(RequireType("Necrom.Core.Domain.Combatant"), NewEntityId(entityId), archetypeId, faction, health);
        }

        private static object NewEntityId(string value)
            => Activator.CreateInstance(RequireType("Necrom.Core.Domain.EntityId"), value);

        private static object GetFormationSlot(object formation, int slot)
            => Invoke(formation, "GetSlot", slot);

        private static object GetRosterSlot(Component roster, int slot)
            => Invoke(roster, "GetSlot", slot);

        private static RectTransform NewZone(Transform parent, string name)
        {
            var zone = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(parent, false);
            return zone;
        }

        private static Delegate BuildUnaryDelegate(Type delegateType, string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var inputType = invoke.GetParameters()[0].ParameterType;
            var returnType = invoke.ReturnType;
            var input = Expression.Parameter(inputType, "value");
            var helper = typeof(FirstPlayableCombatHudStateProjectionTests).GetMethod(helperName, BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper, Expression.Convert(input, typeof(object)));
            return Expression.Lambda(delegateType, Expression.Convert(call, returnType), input).Compile();
        }

        private static int GrantPolicyObject(object value) => 4;

        private static int CostPolicyObject(object value)
        {
            _costPolicyCalls++;
            return _raiseCost;
        }

        private static string TransactionIdObject(object value)
        {
            _transactionIdCalls++;
            return "soul-spend:" + _suffix;
        }

        private static object BuildRaiseCommandObject(object source)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Application.RaiseIntoFormationCommand"),
                "raise:hud:" + _undeadId,
                source,
                ReadLong(source, "Revision"),
                NewEntityId(_undeadId),
                7,
                0,
                ReadLong(_formationForCommand, "Revision"));

        private static object BuildBehaviorSpecObject(object command)
            => Activator.CreateInstance(RequireType("Necrom.Core.Domain.BasicAutoBehaviorSpec"), 4, 100);

        private static object BuildRaiseSourceIdObject(object enemyId)
            => NewEntityId("raise:hud:" + enemyId);

        private static IList<object> ReadList(object target, string property)
            => ((IEnumerable)Read(target, property)).Cast<object>().ToList();

        private static object Invoke(object target, string method, params object[] args)
        {
            var candidates = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name == method)
                .Where(m => m.GetParameters().Length == args.Length)
                .Where(m => ParametersAccept(m.GetParameters(), args))
                .ToArray();
            Assert.That(candidates.Length, Is.EqualTo(1), target.GetType().Name + "." + method);
            return candidates[0].Invoke(target, args);
        }

        private static bool ParametersAccept(ParameterInfo[] parameters, object[] args)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (args[i] == null)
                {
                    if (parameters[i].ParameterType.IsValueType) return false;
                    continue;
                }
                if (!parameters[i].ParameterType.IsInstanceOfType(args[i])) return false;
            }
            return true;
        }

        private static object Read(object target, string property)
        {
            if (target == null) return null;
            var info = target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(info, Is.Not.Null, target.GetType().Name + "." + property);
            return info.GetValue(target);
        }

        private static int ReadInt(object target, string property) => Convert.ToInt32(Read(target, property));
        private static long ReadLong(object target, string property) => Convert.ToInt64(Read(target, property));
        private static bool ReadBool(object target, string property) => Convert.ToBoolean(Read(target, property));

        private static Type RequireType(string fullName)
        {
            var type = FindType(fullName);
            Assert.That(type, Is.Not.Null, fullName + " must exist.");
            return type;
        }

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName)).FirstOrDefault(t => t != null);

        private sealed class Fixture
        {
            public string Suffix;
            public GameObject Root;
            public RectTransform Combat;
            public Component Necro;
            public RectTransform EnemyZone;
            public RectTransform AlliedZone;
            public object Formation;
            public object Battle;
            public object App;
            public Component Enemies;
            public object Enemy;
            public object Source;
            public Component Roster;
            public Component BattleRuntime;
            public Component Hook;
            public object Account;
            public object SoulBridge;
            public Component RaiseAction;
            public object Session;
            public object Projector;
        }

        private sealed class CombatFixture
        {
            public Component Loop;
            public object Enemy;
        }
    }
}