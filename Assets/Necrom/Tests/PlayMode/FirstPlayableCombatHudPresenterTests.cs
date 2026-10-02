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
    public sealed class FirstPlayableCombatHudPresenterTests
    {
        private static object _nextState;
        private static int _captureCalls;
        private static int _renderCalls;
        private static object _renderedPresentation;
        private static object _renderedZones;

        [UnityTest]
        public IEnumerator ApprovedA21ContractsExist()
        {
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudPresenter"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.IFirstPlayableCombatHudView"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudPresentation"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudZoneBinding"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudContentKey"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudStateSourceAdapter"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudViewAdapter"), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RefreshCapturesOnceAndRoutesBattleTargetRaiseAndArmy()
        {
            var state = NewState(
                targetLifeState: "Active",
                raiseReason: "Eligible",
                includeQuote: true,
                ownedUnitId: null,
                runtimeUnitId: null,
                proofStatus: "None",
                includeReceipt: false,
                includeContribution: false);

            var fixture = NewPresenterFixture(state);
            var presentation = Invoke(fixture.Presenter, "Refresh");

            Assert.That(_captureCalls, Is.EqualTo(1));
            Assert.That(_renderCalls, Is.EqualTo(1));
            Assert.That(ReferenceEquals(presentation, _renderedPresentation), Is.True);
            Assert.That(ReferenceEquals(fixture.Zones, _renderedZones), Is.True);

            var target = Read(presentation, "Target");
            Assert.That(Read(target, "ContentKey").ToString(), Is.EqualTo("TargetActive"));
            Assert.That(Read(target, "BattlePhase").ToString(), Is.EqualTo("Ready"));
            Assert.That(ReadLong(target, "BattleRevision"), Is.EqualTo(4));
            Assert.That(Read(Read(target, "Target"), "EntityId").ToString(), Is.EqualTo("enemy-a2"));

            var raise = Read(presentation, "Raise");
            Assert.That(Read(raise, "ContentKey").ToString(), Is.EqualTo("RaiseEligible"));
            Assert.That(Read(raise, "RaiseReason").ToString(), Is.EqualTo("Eligible"));
            Assert.That(ReadBool(raise, "IsProcessing"), Is.False);
            Assert.That(ReadInt(Read(raise, "SoulQuote"), "Balance"), Is.EqualTo(10));

            var army = Read(presentation, "Army");
            Assert.That(Read(army, "ContentKey").ToString(), Is.EqualTo("ArmyEmpty"));
            Assert.That(ReadList(army, "FormationSlots").Count, Is.EqualTo(5));

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RaiseMachineStatesUseSemanticKeysWithoutFakeProcessing()
        {
            var cases = new[]
            {
                new[] { "NoTarget", "RaiseNoTarget" },
                new[] { "TargetNotRaiseReady", "RaiseTargetNotReady" },
                new[] { "SourceUnavailableOrConsumed", "RaiseSourceUnavailableOrConsumed" },
                new[] { "InsufficientSoul", "RaiseInsufficientSoul" },
                new[] { "Eligible", "RaiseEligible" }
            };

            foreach (var item in cases)
            {
                var state = NewState(
                    targetLifeState: item[0] == "NoTarget" ? null : "Defeated",
                    raiseReason: item[0],
                    includeQuote: item[0] == "InsufficientSoul" || item[0] == "Eligible",
                    ownedUnitId: null,
                    runtimeUnitId: null,
                    proofStatus: "None",
                    includeReceipt: false,
                    includeContribution: false);

                var fixture = NewPresenterFixture(state);
                var raise = Read(Invoke(fixture.Presenter, "Refresh"), "Raise");
                Assert.That(Read(raise, "ContentKey").ToString(), Is.EqualTo(item[1]));
                Assert.That(ReadBool(raise, "IsProcessing"), Is.False);
                UnityEngine.Object.Destroy(fixture.Root);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator CommittedAndObservedProofKeysRequireExactReceiptMatch()
        {
            var pending = NewState(
                targetLifeState: "Defeated",
                raiseReason: "SourceUnavailableOrConsumed",
                includeQuote: false,
                ownedUnitId: "undead-a2",
                runtimeUnitId: "undead-a2",
                proofStatus: "Pending",
                includeReceipt: true,
                includeContribution: false);
            var pendingFixture = NewPresenterFixture(pending);
            var pendingPresentation = Invoke(pendingFixture.Presenter, "Refresh");
            Assert.That(Read(Read(pendingPresentation, "Raise"), "ContentKey").ToString(),
                Is.EqualTo("RaiseCommittedAwaitingProof"));
            Assert.That(Read(Read(pendingPresentation, "Army"), "ContentKey").ToString(),
                Is.EqualTo("ArmyProofPending"));
            UnityEngine.Object.Destroy(pendingFixture.Root);

            var observed = NewState(
                targetLifeState: "Active",
                raiseReason: "TargetNotRaiseReady",
                includeQuote: false,
                ownedUnitId: "undead-a2",
                runtimeUnitId: "undead-a2",
                proofStatus: "Observed",
                includeReceipt: true,
                includeContribution: true);
            var observedFixture = NewPresenterFixture(observed);
            var observedPresentation = Invoke(observedFixture.Presenter, "Refresh");
            Assert.That(Read(Read(observedPresentation, "Raise"), "ContentKey").ToString(),
                Is.EqualTo("RaiseProofObserved"));
            Assert.That(Read(Read(observedPresentation, "Army"), "ContentKey").ToString(),
                Is.EqualTo("ArmyProofObserved"));
            UnityEngine.Object.Destroy(observedFixture.Root);

            var stale = NewState(
                targetLifeState: "Active",
                raiseReason: "TargetNotRaiseReady",
                includeQuote: false,
                ownedUnitId: "undead-a2",
                runtimeUnitId: "undead-a2",
                proofStatus: "Observed",
                includeReceipt: false,
                includeContribution: true);
            var staleFixture = NewPresenterFixture(stale);
            var stalePresentation = Invoke(staleFixture.Presenter, "Refresh");
            Assert.That(Read(Read(stalePresentation, "Raise"), "ContentKey").ToString(),
                Is.EqualTo("RaiseTargetNotReady"));
            Assert.That(Read(Read(stalePresentation, "Army"), "ContentKey").ToString(),
                Is.EqualTo("ArmyOwned"));
            UnityEngine.Object.Destroy(staleFixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ArmyProjectionPreservesFormationOwnershipWhenRuntimeDiffers()
        {
            var state = NewState(
                targetLifeState: null,
                raiseReason: "NoTarget",
                includeQuote: false,
                ownedUnitId: "owned-truth",
                runtimeUnitId: "runtime-other",
                proofStatus: "None",
                includeReceipt: false,
                includeContribution: false);
            var fixture = NewPresenterFixture(state);
            var army = Read(Invoke(fixture.Presenter, "Refresh"), "Army");
            var slots = ReadList(army, "FormationSlots");

            Assert.That(Read(army, "ContentKey").ToString(), Is.EqualTo("ArmyOwned"));
            Assert.That(Read(slots[0], "OwnedUnitId").ToString(), Is.EqualTo("owned-truth"));
            Assert.That(Read(slots[0], "RuntimeUnitId").ToString(), Is.EqualTo("runtime-other"));
            Assert.That(ReadBool(slots[0], "OwnershipMatchesRuntime"), Is.False);

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ZoneBindingRejectsMissingOrWrongZonesWithoutGeometryMutation()
        {
            var bindingType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudZoneBinding");
            var bind = bindingType.GetMethod("Bind", BindingFlags.Public | BindingFlags.Static);
            Assert.That(bind, Is.Not.Null);

            var root = new GameObject("A2MissingZone", typeof(RectTransform));
            var safe = NewZone(root.transform, "SafeArea");
            var target = NewZone(safe, "TargetStatusReadabilityZone");
            var raise = NewZone(safe, "RaiseActionStatusReadabilityZone");
            var protectedZone = NewZone(safe, "ProtectedCombatReadabilityZone");
            target.anchorMin = new Vector2(0.1f, 0.2f);
            raise.anchorMin = new Vector2(0.3f, 0.4f);
            protectedZone.anchorMin = new Vector2(0.5f, 0.6f);

            var childCount = safe.childCount;
            var targetAnchor = target.anchorMin;
            var raiseAnchor = raise.anchorMin;
            var protectedAnchor = protectedZone.anchorMin;

            var ex = Assert.Throws<TargetInvocationException>(() => bind.Invoke(null, new object[] { safe }));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(safe.childCount, Is.EqualTo(childCount));
            Assert.That(target.anchorMin, Is.EqualTo(targetAnchor));
            Assert.That(raise.anchorMin, Is.EqualTo(raiseAnchor));
            Assert.That(protectedZone.anchorMin, Is.EqualTo(protectedAnchor));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        private static PresenterFixture NewPresenterFixture(object state)
        {
            _nextState = state;
            _captureCalls = 0;
            _renderCalls = 0;
            _renderedPresentation = null;
            _renderedZones = null;

            var root = new GameObject("A2PresenterFixture", typeof(RectTransform));
            var safe = NewZone(root.transform, "SafeArea");
            NewZone(safe, "TargetStatusReadabilityZone");
            NewZone(safe, "RaiseActionStatusReadabilityZone");
            NewZone(safe, "ArmyStatusReadabilityZone");
            NewZone(safe, "ProtectedCombatReadabilityZone");

            var bindingType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudZoneBinding");
            var zones = bindingType.GetMethod("Bind", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { safe });

            var sourceType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudStateSourceAdapter");
            var sourceCtor = sourceType.GetConstructors().Single();
            var sourceDelegate = BuildZeroDelegate(
                sourceCtor.GetParameters()[0].ParameterType,
                nameof(CaptureStateObject));
            var source = sourceCtor.Invoke(new object[] { sourceDelegate });

            var viewType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudViewAdapter");
            var viewCtor = viewType.GetConstructors().Single();
            var viewDelegate = BuildBinaryDelegate(
                viewCtor.GetParameters()[0].ParameterType,
                nameof(RecordViewObject));
            var view = viewCtor.Invoke(new object[] { viewDelegate });

            var presenterType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudPresenter");
            var presenter = Activator.CreateInstance(presenterType, source, view, zones);
            return new PresenterFixture(root, presenter, zones);
        }

        private static object NewState(
            string targetLifeState,
            string raiseReason,
            bool includeQuote,
            string ownedUnitId,
            string runtimeUnitId,
            string proofStatus,
            bool includeReceipt,
            bool includeContribution)
        {
            var targetType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudTargetState");
            object target = null;
            if (targetLifeState != null)
            {
                target = Activator.CreateInstance(
                    targetType,
                    NewEntityId("enemy-a2"),
                    "enemy.skeleton.guard",
                    "frontline.guard",
                    targetLifeState == "Defeated" ? 0 : 5,
                    ParseEnum("Necrom.Core.Domain.CombatantLifeState", targetLifeState));
            }

            object quote = null;
            if (includeQuote)
            {
                quote = Activator.CreateInstance(
                    RequireType("Necrom.FirstPlayable.Runtime.SoulResourceRaiseQuote"),
                    10L == 10L ? 10 : 10,
                    2L,
                    3,
                    raiseReason != "InsufficientSoul");
            }

            var slotType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudFormationSlotState");
            var slots = Array.CreateInstance(slotType, 5);
            for (var i = 0; i < 5; i++)
            {
                object owned = i == 0 && ownedUnitId != null ? NewEntityId(ownedUnitId) : null;
                object runtime = i == 0 && runtimeUnitId != null ? NewEntityId(runtimeUnitId) : null;
                object runtimeLife = runtime != null
                    ? ParseEnum("Necrom.Core.Domain.CombatantLifeState", "Active")
                    : null;
                var matches = owned != null && runtime != null &&
                    owned.ToString() == runtime.ToString();
                slots.SetValue(
                    Activator.CreateInstance(
                        slotType,
                        i,
                        owned,
                        runtime,
                        runtimeLife,
                        matches),
                    i);
            }

            object receipt = null;
            if (includeReceipt)
            {
                receipt = Activator.CreateInstance(
                    RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseReceipt"),
                    "raise:a2",
                    NewEntityId("source-a2"),
                    NewEntityId("undead-a2"),
                    "enemy.skeleton.guard",
                    0);
            }

            object contribution = null;
            if (includeContribution)
            {
                contribution = Activator.CreateInstance(
                    RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableContributionReceipt"),
                    NewEntityId("undead-a2"),
                    NewEntityId("enemy-next-a2"),
                    4);
            }

            return Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudState"),
                ParseEnum("Necrom.Core.Domain.BattlePhase", "Ready"),
                4L,
                target,
                ParseEnum("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseAvailabilityReason", raiseReason),
                quote,
                slots,
                receipt,
                ParseEnum("Necrom.FirstPlayable.Runtime.FirstPlayableProofStatus", proofStatus),
                contribution);
        }

        private static object CaptureStateObject()
        {
            _captureCalls++;
            return _nextState;
        }

        private static void RecordViewObject(object presentation, object zones)
        {
            _renderCalls++;
            _renderedPresentation = presentation;
            _renderedZones = zones;
        }

        private static Delegate BuildZeroDelegate(Type delegateType, string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var helper = typeof(FirstPlayableCombatHudPresenterTests)
                .GetMethod(helperName, BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper);
            return Expression.Lambda(
                delegateType,
                Expression.Convert(call, invoke.ReturnType))
                .Compile();
        }

        private static Delegate BuildBinaryDelegate(Type delegateType, string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var parameters = invoke.GetParameters();
            var first = Expression.Parameter(parameters[0].ParameterType, "first");
            var second = Expression.Parameter(parameters[1].ParameterType, "second");
            var helper = typeof(FirstPlayableCombatHudPresenterTests)
                .GetMethod(helperName, BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(
                helper,
                Expression.Convert(first, typeof(object)),
                Expression.Convert(second, typeof(object)));
            return Expression.Lambda(delegateType, call, first, second).Compile();
        }

        private static object Invoke(object target, string method, params object[] args)
        {
            var candidates = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name == method && m.GetParameters().Length == args.Length)
                .ToArray();
            Assert.That(candidates.Length, Is.EqualTo(1), target.GetType().Name + "." + method);
            return candidates[0].Invoke(target, args);
        }

        private static IList ReadList(object target, string property)
            => (IList)Read(target, property);

        private static object Read(object target, string property)
        {
            if (target == null) return null;
            var info = target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(info, Is.Not.Null, target.GetType().Name + "." + property);
            return info.GetValue(target);
        }

        private static int ReadInt(object target, string property)
            => Convert.ToInt32(Read(target, property));

        private static long ReadLong(object target, string property)
            => Convert.ToInt64(Read(target, property));

        private static bool ReadBool(object target, string property)
            => Convert.ToBoolean(Read(target, property));

        private static object NewEntityId(string value)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.EntityId"),
                value);

        private static object ParseEnum(string fullName, string value)
            => Enum.Parse(RequireType(fullName), value);

        private static RectTransform NewZone(Transform parent, string name)
        {
            var zone = new GameObject(name, typeof(RectTransform))
                .GetComponent<RectTransform>();
            zone.SetParent(parent, false);
            return zone;
        }

        private static Type RequireType(string fullName)
        {
            var type = FindType(fullName);
            Assert.That(type, Is.Not.Null, fullName + " must exist.");
            return type;
        }

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);

        private sealed class PresenterFixture
        {
            public GameObject Root { get; }
            public object Presenter { get; }
            public object Zones { get; }

            public PresenterFixture(GameObject root, object presenter, object zones)
            {
                Root = root;
                Presenter = presenter;
                Zones = zones;
            }
        }
    }
}