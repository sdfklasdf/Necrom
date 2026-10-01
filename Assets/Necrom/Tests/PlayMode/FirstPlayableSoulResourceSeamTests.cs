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
    public sealed class FirstPlayableSoulResourceSeamTests
    {
        private static string _suffix;
        private static object _factorySourceOverride;
        private static int _grantPolicyCalls;
        private static int _costPolicyCalls;
        private static int _raiseTransactionIdCalls;

        [UnityTest]
        public IEnumerator ApprovedResourceSeamTypesExist()
        {
            Assert.That(FindType("Necrom.Core.Domain.SoulResourceAccount"), Is.Not.Null);
            Assert.That(FindType("Necrom.Core.Domain.SoulResourceTransactionResult"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableSoulResourceBridge"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.SoulResourceRaiseResult"), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator NonLethalDamageDoesNotGrantOrCallPolicy()
        {
            Configure("nonlethal");
            var account = NewAccount(10);
            var bridge = NewBridge(account);
            var enemy = SpawnEnemy("enemy-resource-nonlethal", 5, out var root);
            var pipeline = NewDamagePipeline("nonlethal");

            var damageResult = ApplyDamage(pipeline, enemy, 2, "raise:resource:nonlethal");
            var grantResult = ApplyDefeatGrant(bridge, damageResult, 0);

            Assert.That(grantResult, Is.Null);
            Assert.That(_grantPolicyCalls, Is.EqualTo(0));
            Assert.That(ReadInt(account, "Balance"), Is.EqualTo(10));
            Assert.That(ReadLong(account, "Revision"), Is.EqualTo(0));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DefeatGrantUsesInjectedFixtureAmountAndAdvancesRevisionOnce()
        {
            Configure("grant");
            var account = NewAccount(10);
            var bridge = NewBridge(account);
            var enemy = SpawnEnemy("enemy-resource-grant", 5, out var root);
            var pipeline = NewDamagePipeline("grant");

            var damageResult = ApplyDamage(pipeline, enemy, 5, "raise:resource:grant");
            var grantResult = ApplyDefeatGrant(bridge, damageResult, 0);

            Assert.That(_grantPolicyCalls, Is.EqualTo(1));
            Assert.That(ReadInt(grantResult, "Amount"), Is.EqualTo(4));
            Assert.That(ReadInt(grantResult, "BalanceBefore"), Is.EqualTo(10));
            Assert.That(ReadInt(grantResult, "BalanceAfter"), Is.EqualTo(14));
            Assert.That(ReadInt(account, "Balance"), Is.EqualTo(14));
            Assert.That(ReadLong(account, "Revision"), Is.EqualTo(1));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReplayedDefeatEventCannotDoubleGrant()
        {
            Configure("grant-replay");
            var account = NewAccount(10);
            var bridge = NewBridge(account);
            var enemy = SpawnEnemy("enemy-resource-grant-replay", 5, out var root);
            var pipeline = NewDamagePipeline("grant-replay");
            var damageResult = ApplyDamage(pipeline, enemy, 5, "raise:resource:grant-replay");

            ApplyDefeatGrant(bridge, damageResult, 0);
            var error = Assert.Throws<TargetInvocationException>(
                () => ApplyDefeatGrant(bridge, damageResult, 1));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadInt(account, "Balance"), Is.EqualTo(14));
            Assert.That(ReadLong(account, "Revision"), Is.EqualTo(1));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StaleResourceRevisionRejectsGrantWithoutMutation()
        {
            Configure("stale");
            var account = NewAccount(10);
            GrantDirect(account, "seed:stale", 2, 0);

            var error = Assert.Throws<TargetInvocationException>(
                () => GrantDirect(account, "grant:stale", 4, 0));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadInt(account, "Balance"), Is.EqualTo(12));
            Assert.That(ReadLong(account, "Revision"), Is.EqualTo(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator InsufficientResourceRejectsBeforeRaiseMutation()
        {
            Configure("insufficient");
            var fixture = NewRaiseFixture("insufficient", defeated: true, wrongFactorySource: false);
            var account = NewAccount(2);
            var bridge = NewBridge(account);

            var error = Assert.Throws<TargetInvocationException>(
                () => ExecuteRaise(bridge, fixture.SpawnController, fixture.ActionController, 0));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(Read(fixture.Source, "State").ToString(), Is.EqualTo("Available"));
            Assert.That(ReadLong(fixture.Formation, "Revision"), Is.EqualTo(0));
            Assert.That(ReadInt(account, "Balance"), Is.EqualTo(2));
            Assert.That(ReadLong(account, "Revision"), Is.EqualTo(0));

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SuccessfulRaiseSpendsConfiguredFixtureCostExactlyOnce()
        {
            Configure("success");
            var fixture = NewRaiseFixture("success", defeated: true, wrongFactorySource: false);
            var account = NewAccount(10);
            var bridge = NewBridge(account);

            var result = ExecuteRaise(bridge, fixture.SpawnController, fixture.ActionController, 0);
            var transaction = Read(result, "ResourceTransaction");
            var commandResult = Read(result, "CommandResult");

            Assert.That(_costPolicyCalls, Is.EqualTo(1));
            Assert.That(_raiseTransactionIdCalls, Is.EqualTo(1));
            Assert.That(Read(commandResult, "CommandId").ToString(), Is.EqualTo("raise-resource-success"));
            Assert.That(ReadInt(transaction, "Amount"), Is.EqualTo(3));
            Assert.That(ReadInt(account, "Balance"), Is.EqualTo(7));
            Assert.That(ReadLong(account, "Revision"), Is.EqualTo(1));
            Assert.That(Read(fixture.Source, "State").ToString(), Is.EqualTo("Consumed"));
            Assert.That(ReadLong(fixture.Formation, "Revision"), Is.EqualTo(1));

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FailedRaiseDoesNotSpendResource()
        {
            Configure("failed");
            var fixture = NewRaiseFixture("failed", defeated: true, wrongFactorySource: true);
            var account = NewAccount(10);
            var bridge = NewBridge(account);

            var error = Assert.Throws<TargetInvocationException>(
                () => ExecuteRaise(bridge, fixture.SpawnController, fixture.ActionController, 0));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadInt(account, "Balance"), Is.EqualTo(10));
            Assert.That(ReadLong(account, "Revision"), Is.EqualTo(0));
            Assert.That(Read(fixture.Source, "State").ToString(), Is.EqualTo("Available"));
            Assert.That(ReadLong(fixture.Formation, "Revision"), Is.EqualTo(0));

            UnityEngine.Object.Destroy(fixture.Root);
            if (fixture.OtherRoot != null) UnityEngine.Object.Destroy(fixture.OtherRoot);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DuplicateRaiseCannotSpendTwice()
        {
            Configure("duplicate");
            var fixture = NewRaiseFixture("duplicate", defeated: true, wrongFactorySource: false);
            var account = NewAccount(10);
            var bridge = NewBridge(account);

            ExecuteRaise(bridge, fixture.SpawnController, fixture.ActionController, 0);
            var balanceAfterFirst = ReadInt(account, "Balance");
            var revisionAfterFirst = ReadLong(account, "Revision");

            var error = Assert.Throws<TargetInvocationException>(
                () => ExecuteRaise(bridge, fixture.SpawnController, fixture.ActionController, revisionAfterFirst));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadInt(account, "Balance"), Is.EqualTo(balanceAfterFirst));
            Assert.That(ReadLong(account, "Revision"), Is.EqualTo(revisionAfterFirst));
            Assert.That(ReadLong(fixture.Formation, "Revision"), Is.EqualTo(1));

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        private static void Configure(string suffix)
        {
            _suffix = suffix;
            _factorySourceOverride = null;
            _grantPolicyCalls = 0;
            _costPolicyCalls = 0;
            _raiseTransactionIdCalls = 0;
        }

        private static object NewAccount(int initialBalance)
        {
            var type = RequireType("Necrom.Core.Domain.SoulResourceAccount");
            return Activator.CreateInstance(type, initialBalance);
        }

        private static object NewBridge(object account)
        {
            var type = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableSoulResourceBridge");
            var ctor = type.GetConstructors().Single();
            var p = ctor.GetParameters();

            var grantPolicy = BuildObjectToValueDelegate(p[1].ParameterType, nameof(GrantPolicyObject));
            var costPolicy = BuildObjectToValueDelegate(p[2].ParameterType, nameof(CostPolicyObject));
            var txIdProvider = BuildObjectToValueDelegate(p[3].ParameterType, nameof(RaiseTransactionIdObject));

            return ctor.Invoke(new[] { account, grantPolicy, costPolicy, txIdProvider });
        }

        private static Delegate BuildObjectToValueDelegate(Type delegateType, string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var inputType = invoke.GetParameters()[0].ParameterType;
            var returnType = invoke.ReturnType;
            var input = Expression.Parameter(inputType, "value");
            var helper = typeof(FirstPlayableSoulResourceSeamTests)
                .GetMethod(helperName, BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper, Expression.Convert(input, typeof(object)));
            var body = Expression.Convert(call, returnType);
            return Expression.Lambda(delegateType, body, input).Compile();
        }

        private static int GrantPolicyObject(object defeatedEvent)
        {
            _grantPolicyCalls++;
            return 4; // Test fixture only; not production balance.
        }

        private static int CostPolicyObject(object source)
        {
            _costPolicyCalls++;
            return 3; // Test fixture only; not production balance.
        }

        private static string RaiseTransactionIdObject(object source)
        {
            _raiseTransactionIdCalls++;
            return "soul-spend:" + _suffix;
        }

        private static object ApplyDefeatGrant(object bridge, object damageResult, long expectedRevision)
        {
            var method = bridge.GetType().GetMethod("ApplyDefeatGrant", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(bridge, new[] { damageResult, (object)expectedRevision });
        }

        private static object ExecuteRaise(object bridge, object spawnController, object actionController, long expectedRevision)
        {
            var method = bridge.GetType().GetMethod("ExecuteRaise", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(bridge, new[] { spawnController, actionController, (object)expectedRevision });
        }

        private static object GrantDirect(object account, string transactionId, int amount, long expectedRevision)
        {
            var method = account.GetType().GetMethod("Grant", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(account, new object[] { transactionId, amount, expectedRevision });
        }

        private static object NewDamagePipeline(string suffix)
        {
            var type = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableDamageDeathPipeline");
            return Activator.CreateInstance(
                type,
                (Func<string>)(() => "damage:" + suffix),
                (Func<string>)(() => "defeat:" + suffix));
        }

        private static object ApplyDamage(object pipeline, object enemy, int amount, string raiseSourceId)
            => pipeline.GetType().GetMethod("Apply").Invoke(
                pipeline,
                new[] { enemy, (object)amount, NewEntityId(raiseSourceId) });

        private static object SpawnEnemy(string instanceId, int maxHealth, out GameObject root)
        {
            var definitionType = RequireType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition");
            var controllerType = RequireType("Necrom.FirstPlayable.Runtime.EnemySpawnController");
            var definition = Activator.CreateInstance(
                definitionType,
                "enemy.skeleton.guard",
                "frontline.guard",
                maxHealth);

            root = new GameObject("SoulResourceDamageRoot", typeof(RectTransform));
            var zone = new GameObject("EnemySpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(root.transform, false);
            var controller = root.AddComponent(controllerType);
            return controllerType.GetMethod("SpawnEnemy").Invoke(
                controller,
                new[] { instanceId, definition, zone });
        }

        private static RaiseFixture NewRaiseFixture(string suffix, bool defeated, bool wrongFactorySource)
        {
            var root = new GameObject("SoulResourceRaise-" + suffix, typeof(RectTransform));
            var spawnType = RequireType("Necrom.FirstPlayable.Runtime.EnemySpawnController");
            var spawnController = root.AddComponent(spawnType);
            var zone = new GameObject("EnemySpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(root.transform, false);

            var formation = Activator.CreateInstance(RequireType("Necrom.Core.Domain.Formation"));
            var battle = Activator.CreateInstance(RequireType("Necrom.Core.Domain.BattleStateMachine"));
            var raiseService = Activator.CreateInstance(RequireType("Necrom.Core.Domain.RaiseService"));
            var encounter = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.FirstPlayableEncounter"),
                raiseService,
                battle,
                formation);
            var app = Activator.CreateInstance(
                RequireType("Necrom.Core.Application.FirstPlayableApplicationService"),
                encounter);
            var progression = Activator.CreateInstance(
                RequireType("Necrom.Core.Application.FirstPlayableProgression"),
                app,
                encounter);

            var hookType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseCommandInputHook");
            var hook = root.AddComponent(hookType);
            hookType.GetMethod("Initialize").Invoke(hook, new[] { progression });

            var definition = Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"),
                "enemy.skeleton.guard",
                "frontline.guard",
                5);
            var enemy = spawnType.GetMethod("SpawnEnemy").Invoke(
                spawnController,
                new[] { "enemy-" + suffix, definition, zone });

            object source = null;
            if (defeated)
            {
                enemy.GetType().GetMethod("ApplyDamage")
                    .Invoke(enemy, new object[] { 5, NewEntityId("source-" + suffix) });
                source = Read(enemy, "RaiseSource");
            }

            GameObject otherRoot = null;
            if (wrongFactorySource)
            {
                var other = SpawnEnemy("enemy-other-" + suffix, 5, out otherRoot);
                other.GetType().GetMethod("ApplyDamage")
                    .Invoke(other, new object[] { 5, NewEntityId("source-other-" + suffix) });
                _factorySourceOverride = Read(other, "RaiseSource");
            }

            var actionType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseActionController");
            var action = root.AddComponent(actionType);
            var init = actionType.GetMethod("Initialize");
            var p = init.GetParameters();
            var factory = BuildCommandFactoryDelegate(p[2].ParameterType);
            init.Invoke(action, new object[]
            {
                spawnController,
                hook,
                factory,
                (Func<string>)(() => "raised:" + suffix),
                (Func<string>)(() => "assigned:" + suffix)
            });

            return new RaiseFixture
            {
                Root = root,
                OtherRoot = otherRoot,
                SpawnController = spawnController,
                ActionController = action,
                Source = source,
                Formation = formation
            };
        }

        private static Delegate BuildCommandFactoryDelegate(Type delegateType)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var inputType = invoke.GetParameters()[0].ParameterType;
            var returnType = invoke.ReturnType;
            var input = Expression.Parameter(inputType, "source");
            var helper = typeof(FirstPlayableSoulResourceSeamTests)
                .GetMethod(nameof(BuildRaiseCommandObject), BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper, Expression.Convert(input, typeof(object)));
            var body = Expression.Convert(call, returnType);
            return Expression.Lambda(delegateType, body, input).Compile();
        }

        private static object BuildRaiseCommandObject(object discoveredSource)
        {
            var source = _factorySourceOverride ?? discoveredSource;
            return Activator.CreateInstance(
                RequireType("Necrom.Core.Application.RaiseIntoFormationCommand"),
                "raise-resource-" + _suffix,
                source,
                ReadLong(source, "Revision"),
                NewEntityId("undead-resource-" + _suffix),
                7,
                0,
                (long)0);
        }

        private static object NewEntityId(string value)
            => Activator.CreateInstance(RequireType("Necrom.Core.Domain.EntityId"), value);

        private static object Read(object target, string property)
            => target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public).GetValue(target);

        private static int ReadInt(object target, string property)
            => Convert.ToInt32(Read(target, property));

        private static long ReadLong(object target, string property)
            => Convert.ToInt64(Read(target, property));

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

        private sealed class RaiseFixture
        {
            public GameObject Root;
            public GameObject OtherRoot;
            public Component SpawnController;
            public Component ActionController;
            public object Source;
            public object Formation;
        }
    }
}