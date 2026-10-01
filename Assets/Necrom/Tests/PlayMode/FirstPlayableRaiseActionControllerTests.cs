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
    public sealed class FirstPlayableRaiseActionControllerTests
    {
        private static int _factoryCalls;
        private static int _raisedEventCalls;
        private static int _assignedEventCalls;
        private static string _suffix;
        private static object _factorySourceOverride;

        [UnityTest]
        public IEnumerator ApprovedActionControllerTypeExists()
        {
            Assert.That(
                FindType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseActionController"),
                Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EligibleCurrentTargetExecutesExactlyOnceAndReturnsResult()
        {
            var f = NewFixture("valid", defeated: true);
            var action = NewAction(f);
            ConfigureFactory("valid");

            Initialize(action, f);
            Assert.That(CanExecute(action), Is.True);

            var result = Execute(action);

            Assert.That(Read(result, "CommandId").ToString(), Is.EqualTo("raise-action-valid"));
            Assert.That(_factoryCalls, Is.EqualTo(1));
            Assert.That(_raisedEventCalls, Is.EqualTo(1));
            Assert.That(_assignedEventCalls, Is.EqualTo(1));
            Assert.That(Read(f.Source, "State").ToString(), Is.EqualTo("Consumed"));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(1));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-valid"));
            Assert.That(CanExecute(action), Is.False);

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActiveCurrentTargetRejectsBeforeFactoryOrSubmit()
        {
            var f = NewFixture("active", defeated: false);
            var action = NewAction(f);
            ConfigureFactory("active");
            Initialize(action, f);

            Assert.That(CanExecute(action), Is.False);
            var error = Assert.Throws<TargetInvocationException>(() => Execute(action));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(_factoryCalls, Is.EqualTo(0));
            Assert.That(_raisedEventCalls, Is.EqualTo(0));
            Assert.That(_assignedEventCalls, Is.EqualTo(0));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(0));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MissingCurrentTargetRejectsBeforeFactoryOrSubmit()
        {
            var f = NewFixtureWithoutEnemy("missing");
            var action = NewAction(f);
            ConfigureFactory("missing");
            Initialize(action, f);

            Assert.That(CanExecute(action), Is.False);
            var error = Assert.Throws<TargetInvocationException>(() => Execute(action));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(_factoryCalls, Is.EqualTo(0));
            Assert.That(_raisedEventCalls, Is.EqualTo(0));
            Assert.That(_assignedEventCalls, Is.EqualTo(0));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(0));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FactoryUsingDifferentRaiseSourceIsRejectedBeforeSubmit()
        {
            var f = NewFixture("current", defeated: true);
            var other = NewFixture("other", defeated: true);
            var action = NewAction(f);
            ConfigureFactory("wrong-source");
            _factorySourceOverride = other.Source;
            Initialize(action, f);

            var error = Assert.Throws<TargetInvocationException>(() => Execute(action));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(_factoryCalls, Is.EqualTo(1));
            Assert.That(_raisedEventCalls, Is.EqualTo(0));
            Assert.That(_assignedEventCalls, Is.EqualTo(0));
            Assert.That(Read(f.Source, "State").ToString(), Is.EqualTo("Available"));
            Assert.That(Read(other.Source, "State").ToString(), Is.EqualTo("Available"));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(0));

            UnityEngine.Object.Destroy(f.Root);
            UnityEngine.Object.Destroy(other.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SecondActionAfterSuccessCannotCreateSecondEffect()
        {
            var f = NewFixture("repeat", defeated: true);
            var action = NewAction(f);
            ConfigureFactory("repeat");
            Initialize(action, f);

            Execute(action);
            var firstSourceRevision = ReadLong(f.Source, "Revision");
            var firstFormationRevision = ReadLong(f.Formation, "Revision");

            var error = Assert.Throws<TargetInvocationException>(() => Execute(action));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(_factoryCalls, Is.EqualTo(1));
            Assert.That(_raisedEventCalls, Is.EqualTo(1));
            Assert.That(_assignedEventCalls, Is.EqualTo(1));
            Assert.That(ReadLong(f.Source, "Revision"), Is.EqualTo(firstSourceRevision));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(firstFormationRevision));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-repeat"));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        private static object NewAction(Fixture f)
        {
            var type = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseActionController");
            Assert.That(type, Is.Not.Null, "Approved one-action Raise controller must exist.");
            return f.Root.AddComponent(type);
        }

        private static void ConfigureFactory(string suffix)
        {
            _suffix = suffix;
            _factoryCalls = 0;
            _raisedEventCalls = 0;
            _assignedEventCalls = 0;
            _factorySourceOverride = null;
        }

        private static void Initialize(object action, Fixture f)
        {
            var method = action.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            var parameters = method.GetParameters();
            Assert.That(parameters.Length, Is.EqualTo(5));

            var factory = BuildFactoryDelegate(parameters[2].ParameterType);
            method.Invoke(action, new object[]
            {
                f.SpawnController,
                f.InputHook,
                factory,
                (Func<string>)ProvideRaisedEventId,
                (Func<string>)ProvideAssignedEventId
            });
        }

        private static Delegate BuildFactoryDelegate(Type delegateType)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var parameterType = invoke.GetParameters()[0].ParameterType;
            var returnType = invoke.ReturnType;
            var source = Expression.Parameter(parameterType, "source");
            var helper = typeof(FirstPlayableRaiseActionControllerTests)
                .GetMethod(nameof(BuildCommandObject), BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper, Expression.Convert(source, typeof(object)));
            var body = Expression.Convert(call, returnType);
            return Expression.Lambda(delegateType, body, source).Compile();
        }

        private static object BuildCommandObject(object discoveredSource)
        {
            _factoryCalls++;
            var source = _factorySourceOverride ?? discoveredSource;
            var commandType = RequireType("Necrom.Core.Application.RaiseIntoFormationCommand");
            return Activator.CreateInstance(
                commandType,
                "raise-action-" + _suffix,
                source,
                ReadLong(source, "Revision"),
                NewEntityId("undead-" + _suffix),
                7,
                0,
                (long)0);
        }

        private static string ProvideRaisedEventId()
        {
            _raisedEventCalls++;
            return "event:raised:" + _suffix;
        }

        private static string ProvideAssignedEventId()
        {
            _assignedEventCalls++;
            return "event:assigned:" + _suffix;
        }

        private static bool CanExecute(object action)
            => (bool)action.GetType()
                .GetMethod("CanExecute", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(action, null);

        private static object Execute(object action)
            => action.GetType()
                .GetMethod("Execute", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(action, null);

        private static Fixture NewFixture(string suffix, bool defeated)
        {
            var fixture = NewFixtureWithoutEnemy(suffix);
            var definitionType = RequireType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition");
            var definition = Activator.CreateInstance(
                definitionType,
                "enemy.skeleton.guard",
                "frontline.guard",
                5);

            var spawn = fixture.SpawnController.GetType()
                .GetMethod("SpawnEnemy", BindingFlags.Instance | BindingFlags.Public);
            var enemy = spawn.Invoke(
                fixture.SpawnController,
                new object[] { "enemy-" + suffix, definition, fixture.SpawnZone });

            if (defeated)
            {
                enemy.GetType().GetMethod("ApplyDamage")
                    .Invoke(enemy, new object[] { 5, NewEntityId("source-" + suffix) });
                fixture.Source = Read(enemy, "RaiseSource");
            }

            fixture.Enemy = enemy;
            return fixture;
        }

        private static Fixture NewFixtureWithoutEnemy(string suffix)
        {
            var root = new GameObject("RaiseAction-" + suffix, typeof(RectTransform));
            var spawnType = RequireType("Necrom.FirstPlayable.Runtime.EnemySpawnController");
            var spawnController = root.AddComponent(spawnType);
            var zone = new GameObject("EnemySpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(root.transform, false);

            var formation = Activator.CreateInstance(RequireType("Necrom.Core.Domain.Formation"));
            var battle = Activator.CreateInstance(RequireType("Necrom.Core.Domain.BattleStateMachine"));
            var raiseService = Activator.CreateInstance(RequireType("Necrom.Core.Domain.RaiseService"));
            var encounterType = RequireType("Necrom.Core.Domain.FirstPlayableEncounter");
            var encounter = Activator.CreateInstance(encounterType, raiseService, battle, formation);
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

            return new Fixture
            {
                Root = root,
                SpawnController = spawnController,
                SpawnZone = zone,
                InputHook = hook,
                Formation = formation
            };
        }

        private static object GetFormationSlot(object formation, int slot)
            => formation.GetType().GetMethod("GetSlot").Invoke(formation, new object[] { slot });

        private static object NewEntityId(string value)
            => Activator.CreateInstance(RequireType("Necrom.Core.Domain.EntityId"), value);

        private static object Read(object target, string property)
            => target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public).GetValue(target);

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

        private sealed class Fixture
        {
            public GameObject Root;
            public Component SpawnController;
            public RectTransform SpawnZone;
            public Component InputHook;
            public object Enemy;
            public object Source;
            public object Formation;
        }
    }
}