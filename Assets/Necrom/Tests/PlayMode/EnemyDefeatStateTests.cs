using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class EnemyDefeatStateTests
    {
        [UnityTest]
        public IEnumerator LethalDamageCreatesExactlyOneAvailableRaiseSourceFromSameEnemy()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-lethal", 5, out var root);
            var sourceId = CreateEntityId("raise-source-lethal");

            InvokeDamage(runtimeEntity, 5, sourceId);

            var model = GetProperty(runtimeEntity, "Model");
            Assert.That(GetProperty(model, "Health"), Is.EqualTo(0));
            Assert.That(GetProperty(model, "LifeState")?.ToString(), Is.EqualTo("Defeated"));

            var source = GetRaiseSource(runtimeEntity);
            Assert.That(source, Is.Not.Null);
            Assert.That(GetProperty(source, "State")?.ToString(), Is.EqualTo("Available"));
            Assert.That(GetProperty(source, "DefeatedEntityId")?.ToString(), Is.EqualTo("enemy-instance-lethal"));
            Assert.That(GetProperty(source, "ArchetypeId"), Is.EqualTo("enemy.skeleton.guard"));
            Assert.That(GetProperty(source, "SourceId")?.ToString(), Is.EqualTo("raise-source-lethal"));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator NonLethalDamageKeepsEnemyActiveAndDoesNotCreateRaiseSource()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-nonlethal", 5, out var root);
            var sourceId = CreateEntityId("raise-source-nonlethal");

            InvokeDamage(runtimeEntity, 2, sourceId);

            var model = GetProperty(runtimeEntity, "Model");
            Assert.That(GetProperty(model, "Health"), Is.EqualTo(3));
            Assert.That(GetProperty(model, "LifeState")?.ToString(), Is.EqualTo("Active"));
            Assert.That(GetRaiseSource(runtimeEntity), Is.Null);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DamageAfterDefeatDoesNotCreateSecondRaiseSourceOrMutateOriginal()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-repeat", 5, out var root);
            var initialSourceId = CreateEntityId("raise-source-original");

            InvokeDamage(runtimeEntity, 5, initialSourceId);
            var originalSource = GetRaiseSource(runtimeEntity);
            Assert.That(originalSource, Is.Not.Null);

            var originalSourceId = GetProperty(originalSource, "SourceId")?.ToString();
            var originalState = GetProperty(originalSource, "State")?.ToString();
            var originalRevision = GetProperty(originalSource, "Revision");

            var retrySourceId = CreateEntityId("raise-source-retry");
            InvokeDamage(runtimeEntity, 1, retrySourceId);

            var sourceAfterRetry = GetRaiseSource(runtimeEntity);
            var model = GetProperty(runtimeEntity, "Model");

            Assert.That(sourceAfterRetry, Is.SameAs(originalSource));
            Assert.That(GetProperty(sourceAfterRetry, "SourceId")?.ToString(), Is.EqualTo(originalSourceId));
            Assert.That(GetProperty(sourceAfterRetry, "State")?.ToString(), Is.EqualTo(originalState));
            Assert.That(GetProperty(sourceAfterRetry, "Revision"), Is.EqualTo(originalRevision));
            Assert.That(GetProperty(model, "Health"), Is.EqualTo(0));
            Assert.That(GetProperty(model, "LifeState")?.ToString(), Is.EqualTo("Defeated"));

            UnityEngine.Object.Destroy(root);
            yield return null;
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

            root = new GameObject("CombatViewport", typeof(RectTransform));
            var zone = new GameObject("EnemySpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(root.transform, false);

            var controller = root.AddComponent(controllerType);
            var spawn = controllerType.GetMethod("SpawnEnemy", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(spawn, Is.Not.Null);

            var runtimeEntity = spawn.Invoke(controller, new[] { instanceId, definition, zone });
            Assert.That(runtimeEntity, Is.Not.Null);
            return runtimeEntity;
        }

        private static object CreateEntityId(string value)
        {
            var entityIdType = FindType("Necrom.Core.Domain.EntityId");
            Assert.That(entityIdType, Is.Not.Null);
            return Activator.CreateInstance(entityIdType, value);
        }

        private static void InvokeDamage(object runtimeEntity, int amount, object sourceId)
        {
            var method = runtimeEntity.GetType().GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, "EnemyRuntimeEntity must expose the approved damage-to-defeat transition API.");
            method.Invoke(runtimeEntity, new[] { (object)amount, sourceId });
        }

        private static object GetRaiseSource(object runtimeEntity)
        {
            var property = runtimeEntity.GetType().GetProperty("RaiseSource", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, "EnemyRuntimeEntity must expose its logical RaiseSource when defeated.");
            return property.GetValue(runtimeEntity);
        }

        private static object GetProperty(object instance, string propertyName)
        {
            Assert.That(instance, Is.Not.Null);
            var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null, $"{instance.GetType().Name}.{propertyName} must exist.");
            return property.GetValue(instance);
        }

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName))
                .FirstOrDefault(type => type != null);
    }
}
