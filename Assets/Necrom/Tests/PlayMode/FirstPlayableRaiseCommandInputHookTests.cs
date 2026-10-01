using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableRaiseCommandInputHookTests
    {
        [UnityTest]
        public IEnumerator ApprovedInputHookTypeExists()
        {
            Assert.That(
                FindType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseCommandInputHook"),
                Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SubmitBeforeInitializationIsRejectedWithoutMutation()
        {
            var fixture = NewFixture("uninitialized");
            var hook = NewHook(out var root);

            var error = Assert.Throws<TargetInvocationException>(
                () => Submit(hook, fixture.Command, "event:raised:u", "event:assigned:u"));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(Read(fixture.Source, "State").ToString(), Is.EqualTo("Available"));
            Assert.That(ReadLong(fixture.Source, "Revision"), Is.EqualTo(0));
            Assert.That(ReadLong(fixture.Formation, "Revision"), Is.EqualTo(0));
            Assert.That(GetFormationSlot(fixture.Formation, 0), Is.Null);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ValidSubmitForwardsExactlyOnceAndReturnsExistingResult()
        {
            var fixture = NewFixture("valid");
            var hook = NewHook(out var root);
            Initialize(hook, fixture.Progression);

            var result = Submit(hook, fixture.Command, "event:raised:v", "event:assigned:v");
            var events = ((IEnumerable)Read(result, "Events")).Cast<object>().ToArray();

            Assert.That(Read(result, "CommandId").ToString(), Is.EqualTo("raise-input-valid"));
            Assert.That(events.Length, Is.EqualTo(2));
            Assert.That(Read(events[0], "EventType").ToString(), Is.EqualTo("unit.raised"));
            Assert.That(Read(events[1], "EventType").ToString(), Is.EqualTo("formation.unit_assigned"));
            Assert.That(Read(fixture.Source, "State").ToString(), Is.EqualTo("Consumed"));
            Assert.That(ReadLong(fixture.Source, "Revision"), Is.EqualTo(1));
            Assert.That(ReadLong(fixture.Formation, "Revision"), Is.EqualTo(1));
            Assert.That(GetFormationSlot(fixture.Formation, 0).ToString(), Is.EqualTo("undead-valid"));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DuplicateSubmitCannotCreateSecondLocalEffect()
        {
            var fixture = NewFixture("duplicate");
            var hook = NewHook(out var root);
            Initialize(hook, fixture.Progression);

            Submit(hook, fixture.Command, "event:raised:d1", "event:assigned:d1");

            var error = Assert.Throws<TargetInvocationException>(
                () => Submit(hook, fixture.Command, "event:raised:d2", "event:assigned:d2"));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(Read(fixture.Source, "State").ToString(), Is.EqualTo("Consumed"));
            Assert.That(ReadLong(fixture.Source, "Revision"), Is.EqualTo(1));
            Assert.That(ReadLong(fixture.Formation, "Revision"), Is.EqualTo(1));
            Assert.That(GetFormationSlot(fixture.Formation, 0).ToString(), Is.EqualTo("undead-duplicate"));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DuplicateInitializationIsRejectedWithoutReplacingProgression()
        {
            var first = NewFixture("first");
            var second = NewFixture("second");
            var hook = NewHook(out var root);
            Initialize(hook, first.Progression);

            var initialize = hook.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
            var error = Assert.Throws<TargetInvocationException>(
                () => initialize.Invoke(hook, new[] { second.Progression }));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());

            var result = Submit(hook, first.Command, "event:raised:first", "event:assigned:first");
            Assert.That(Read(result, "CommandId").ToString(), Is.EqualTo("raise-input-first"));
            Assert.That(Read(first.Source, "State").ToString(), Is.EqualTo("Consumed"));
            Assert.That(Read(second.Source, "State").ToString(), Is.EqualTo("Available"));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        private static object NewHook(out GameObject root)
        {
            var type = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseCommandInputHook");
            Assert.That(type, Is.Not.Null, "Approved Raise command input hook must exist.");
            root = new GameObject("RaiseCommandInputHook");
            return root.AddComponent(type);
        }

        private static void Initialize(object hook, object progression)
        {
            var method = hook.GetType().GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            method.Invoke(hook, new[] { progression });
        }

        private static object Submit(object hook, object command, string raisedEventId, string assignedEventId)
        {
            var method = hook.GetType().GetMethod("Submit", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(hook, new[] { command, raisedEventId, assignedEventId });
        }

        private static Fixture NewFixture(string suffix)
        {
            var combatantType = RequireType("Necrom.Core.Domain.Combatant");
            var factionType = RequireType("Necrom.Core.Domain.Faction");
            var enemy = Activator.CreateInstance(
                combatantType,
                NewEntityId("enemy-" + suffix),
                "enemy.skeleton.guard",
                Enum.Parse(factionType, "Enemy"),
                5);
            combatantType.GetMethod("ApplyDamage").Invoke(enemy, new object[] { 5 });

            var sourceType = RequireType("Necrom.Core.Domain.RaiseSource");
            var source = Activator.CreateInstance(
                sourceType,
                NewEntityId("source-" + suffix),
                enemy,
                "frontline.guard");

            var formationType = RequireType("Necrom.Core.Domain.Formation");
            var formation = Activator.CreateInstance(formationType);
            var battle = Activator.CreateInstance(RequireType("Necrom.Core.Domain.BattleStateMachine"));
            var raiseService = Activator.CreateInstance(RequireType("Necrom.Core.Domain.RaiseService"));
            var encounterType = RequireType("Necrom.Core.Domain.FirstPlayableEncounter");
            var encounter = Activator.CreateInstance(encounterType, raiseService, battle, formation);
            var appType = RequireType("Necrom.Core.Application.FirstPlayableApplicationService");
            var app = Activator.CreateInstance(appType, encounter);
            var progressionType = RequireType("Necrom.Core.Application.FirstPlayableProgression");
            var progression = Activator.CreateInstance(progressionType, app, encounter);

            var commandType = RequireType("Necrom.Core.Application.RaiseIntoFormationCommand");
            var command = Activator.CreateInstance(
                commandType,
                "raise-input-" + suffix,
                source,
                (long)0,
                NewEntityId("undead-" + suffix),
                7,
                0,
                (long)0);

            return new Fixture
            {
                Source = source,
                Formation = formation,
                Progression = progression,
                Command = command
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
            public object Source;
            public object Formation;
            public object Progression;
            public object Command;
        }
    }
}