using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableDamageDeathPipelineTests
    {
        [UnityTest]
        public IEnumerator ApprovedTypesExist()
        {
            Assert.That(FindType("Necrom.Core.Domain.DamageApplied"), Is.Not.Null);
            Assert.That(FindType("Necrom.Core.Domain.CombatantDefeated"), Is.Not.Null);
            Assert.That(FindType("Necrom.Core.Domain.DamageDeathResult"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableDamageDeathPipeline"), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator NonLethalDamageEmitsOneDamageEventWithExactDeltaAndIdentity()
        {
            var enemy = SpawnEnemy("enemy-damage-nonlethal", 10, out var root);
            var damageCalls = 0;
            var defeatCalls = 0;
            var pipeline = NewPipeline(
                () => { damageCalls++; return "event:damage:1"; },
                () => { defeatCalls++; return "event:defeat:1"; });

            var result = Apply(pipeline, enemy, 3, "raise:nonlethal");
            var events = ReadEvents(result);

            Assert.That(ReadBool(result, "Changed"), Is.True);
            Assert.That(ReadBool(result, "BecameDefeated"), Is.False);
            Assert.That(events.Length, Is.EqualTo(1));
            AssertDamageEvent(events[0], "event:damage:1", "enemy-damage-nonlethal", 3, 3, 10, 7);
            Assert.That(damageCalls, Is.EqualTo(1));
            Assert.That(defeatCalls, Is.EqualTo(0));
            Assert.That(ReadInt(Read(enemy, "Model"), "Health"), Is.EqualTo(7));
            Assert.That(Read(enemy, "RaiseSource"), Is.Null);
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator LethalDamageEmitsOrderedDamageThenDefeatWithRaiseSourceIdentity()
        {
            var enemy = SpawnEnemy("enemy-damage-lethal", 5, out var root);
            var pipeline = NewPipeline(
                () => "event:damage:lethal",
                () => "event:defeat:lethal");

            var result = Apply(pipeline, enemy, 5, "raise:lethal");
            var events = ReadEvents(result);

            Assert.That(ReadBool(result, "Changed"), Is.True);
            Assert.That(ReadBool(result, "BecameDefeated"), Is.True);
            Assert.That(events.Length, Is.EqualTo(2));
            AssertDamageEvent(events[0], "event:damage:lethal", "enemy-damage-lethal", 5, 5, 5, 0);
            Assert.That(ReadString(events[1], "EventType"), Is.EqualTo("combat.combatant_defeated"));
            Assert.That(ReadString(events[1], "EventId"), Is.EqualTo("event:defeat:lethal"));
            Assert.That(Read(events[1], "TargetId").ToString(), Is.EqualTo("enemy-damage-lethal"));
            Assert.That(Read(events[1], "SourceId").ToString(), Is.EqualTo("raise:lethal"));
            var source = Read(enemy, "RaiseSource");
            Assert.That(source, Is.Not.Null);
            Assert.That(Read(source, "SourceId").ToString(), Is.EqualTo("raise:lethal"));
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator OverkillActualDamageUsesHealthDeltaNotRequestedAmount()
        {
            var enemy = SpawnEnemy("enemy-damage-overkill", 5, out var root);
            var pipeline = NewPipeline(
                () => "event:damage:overkill",
                () => "event:defeat:overkill");

            var result = Apply(pipeline, enemy, 99, "raise:overkill");
            var events = ReadEvents(result);

            AssertDamageEvent(events[0], "event:damage:overkill", "enemy-damage-overkill", 99, 5, 5, 0);
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InvalidDamageEventIdRejectsBeforeMutation()
        {
            var enemy = SpawnEnemy("enemy-invalid-damage-id", 10, out var root);
            var pipeline = NewPipeline(() => " ", () => "event:defeat:unused");

            var ex = Assert.Throws<TargetInvocationException>(
                () => Apply(pipeline, enemy, 3, "raise:invalid-damage-id"));

            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
            Assert.That(ReadInt(Read(enemy, "Model"), "Health"), Is.EqualTo(10));
            Assert.That(ReadString(Read(enemy, "Model"), "LifeState"), Is.EqualTo("Active"));
            Assert.That(Read(enemy, "RaiseSource"), Is.Null);
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator InvalidDefeatEventIdRejectsBeforeLethalMutation()
        {
            var enemy = SpawnEnemy("enemy-invalid-defeat-id", 5, out var root);
            var pipeline = NewPipeline(() => "event:damage:valid", () => "");

            var ex = Assert.Throws<TargetInvocationException>(
                () => Apply(pipeline, enemy, 5, "raise:invalid-defeat-id"));

            Assert.That(ex.InnerException, Is.TypeOf<ArgumentException>());
            Assert.That(ReadInt(Read(enemy, "Model"), "Health"), Is.EqualTo(5));
            Assert.That(ReadString(Read(enemy, "Model"), "LifeState"), Is.EqualTo("Active"));
            Assert.That(Read(enemy, "RaiseSource"), Is.Null);
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator AlreadyDefeatedProducesNoDuplicateEventsOrSource()
        {
            var enemy = SpawnEnemy("enemy-repeat-event", 5, out var root);
            var first = NewPipeline(
                () => "event:damage:first",
                () => "event:defeat:first");
            Apply(first, enemy, 5, "raise:first");
            var originalSource = Read(enemy, "RaiseSource");

            Func<string> forbidden = () => throw new InvalidOperationException("event id provider must not run");
            var retry = NewPipeline(forbidden, forbidden);
            var result = Apply(retry, enemy, 1, "raise:retry");

            Assert.That(ReadBool(result, "Changed"), Is.False);
            Assert.That(ReadBool(result, "BecameDefeated"), Is.False);
            Assert.That(ReadEvents(result), Is.Empty);
            Assert.That(Read(enemy, "RaiseSource"), Is.SameAs(originalSource));
            Assert.That(ReadInt(Read(enemy, "Model"), "Health"), Is.EqualTo(0));
            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        private static object NewPipeline(Func<string> damageIdProvider, Func<string> defeatIdProvider)
        {
            var type = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableDamageDeathPipeline");
            Assert.That(type, Is.Not.Null, "FirstPlayableDamageDeathPipeline must exist.");
            return Activator.CreateInstance(type, damageIdProvider, defeatIdProvider);
        }

        private static object Apply(object pipeline, object enemy, int amount, string raiseSourceId)
        {
            var method = pipeline.GetType().GetMethod("Apply", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(pipeline, new[] { enemy, (object)amount, NewEntityId(raiseSourceId) });
        }

        private static object SpawnEnemy(string instanceId, int maxHealth, out GameObject root)
        {
            var definitionType = FindType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition");
            var controllerType = FindType("Necrom.FirstPlayable.Runtime.EnemySpawnController");
            Assert.That(definitionType, Is.Not.Null);
            Assert.That(controllerType, Is.Not.Null);

            var definition = Activator.CreateInstance(
                definitionType,
                "enemy.skeleton.guard",
                "frontline.guard",
                maxHealth);

            root = new GameObject("DamagePipelineRoot", typeof(RectTransform));
            var zone = new GameObject("EnemySpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(root.transform, false);
            var controller = root.AddComponent(controllerType);
            var spawn = controllerType.GetMethod("SpawnEnemy", BindingFlags.Instance | BindingFlags.Public);
            return spawn.Invoke(controller, new[] { instanceId, definition, zone });
        }

        private static object NewEntityId(string value)
            => Activator.CreateInstance(FindType("Necrom.Core.Domain.EntityId"), value);

        private static object[] ReadEvents(object result)
            => ((IEnumerable)Read(result, "Events")).Cast<object>().ToArray();

        private static void AssertDamageEvent(
            object value,
            string eventId,
            string targetId,
            int requested,
            int actual,
            int before,
            int after)
        {
            Assert.That(ReadString(value, "EventType"), Is.EqualTo("combat.damage_applied"));
            Assert.That(ReadString(value, "EventId"), Is.EqualTo(eventId));
            Assert.That(Read(value, "TargetId").ToString(), Is.EqualTo(targetId));
            Assert.That(ReadInt(value, "RequestedDamage"), Is.EqualTo(requested));
            Assert.That(ReadInt(value, "ActualDamage"), Is.EqualTo(actual));
            Assert.That(ReadInt(value, "HealthBefore"), Is.EqualTo(before));
            Assert.That(ReadInt(value, "HealthAfter"), Is.EqualTo(after));
        }

        private static object Read(object target, string property)
            => target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public).GetValue(target);

        private static int ReadInt(object target, string property)
            => (int)Read(target, property);

        private static bool ReadBool(object target, string property)
            => (bool)Read(target, property);

        private static string ReadString(object target, string property)
            => Read(target, property).ToString();

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);
    }
}