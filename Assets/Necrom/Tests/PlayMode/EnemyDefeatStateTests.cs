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
            Assert.That(GetProperty(source, "RoleId"), Is.EqualTo("frontline.guard"));
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

        [UnityTest]
        public IEnumerator EligibleDefeatedEnemyReturnsAvailableRaiseSource()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-eligible", 5, out var root);
            InvokeDamage(runtimeEntity, 5, CreateEntityId("raise-source-eligible"));

            var eligible = InvokeTryGetAvailableRaiseSource(runtimeEntity, out var source);

            Assert.That(eligible, Is.True);
            Assert.That(source, Is.SameAs(GetRaiseSource(runtimeEntity)));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActiveEnemyWithoutRaiseSourceIsNotEligibleBeforeOrAfterNonLethalDamage()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-active", 5, out var root);

            Assert.That(InvokeTryGetAvailableRaiseSource(runtimeEntity, out var beforeDamage), Is.False);
            Assert.That(beforeDamage, Is.Null);

            InvokeDamage(runtimeEntity, 2, CreateEntityId("raise-source-active"));

            Assert.That(InvokeTryGetAvailableRaiseSource(runtimeEntity, out var afterDamage), Is.False);
            Assert.That(afterDamage, Is.Null);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ConsumedRaiseSourceIsNotEligible()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-consumed", 5, out var root);
            InvokeDamage(runtimeEntity, 5, CreateEntityId("raise-source-consumed"));
            var source = GetRaiseSource(runtimeEntity);
            ConsumeRaiseSource(source);

            Assert.That(InvokeTryGetAvailableRaiseSource(runtimeEntity, out var target), Is.False);
            Assert.That(target, Is.Null);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RaiseSourceFromDifferentDefeatedEnemyIsNotEligibleAndIsNotRebound()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-mismatch-a", 5, out var rootA);
            InvokeDamage(runtimeEntity, 5, CreateEntityId("raise-source-mismatch-a"));

            var otherRuntimeEntity = SpawnEnemy("enemy-instance-mismatch-b", 5, out var rootB);
            InvokeDamage(otherRuntimeEntity, 5, CreateEntityId("raise-source-mismatch-b"));
            var mismatchedSource = GetRaiseSource(otherRuntimeEntity);
            ReplaceRaiseSource(runtimeEntity, mismatchedSource);

            var defeatedEntityIdBefore = GetProperty(mismatchedSource, "DefeatedEntityId")?.ToString();

            Assert.That(InvokeTryGetAvailableRaiseSource(runtimeEntity, out var target), Is.False);
            Assert.That(target, Is.Null);
            Assert.That(GetRaiseSource(runtimeEntity), Is.SameAs(mismatchedSource));
            Assert.That(GetProperty(mismatchedSource, "DefeatedEntityId")?.ToString(), Is.EqualTo(defeatedEntityIdBefore));

            UnityEngine.Object.Destroy(rootA);
            UnityEngine.Object.Destroy(rootB);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EligibilityQueryIsSideEffectFree()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-side-effect", 5, out var root);
            InvokeDamage(runtimeEntity, 5, CreateEntityId("raise-source-side-effect"));
            var source = GetRaiseSource(runtimeEntity);
            var model = GetProperty(runtimeEntity, "Model");

            var stateBefore = GetProperty(source, "State")?.ToString();
            var revisionBefore = GetProperty(source, "Revision");
            var healthBefore = GetProperty(model, "Health");
            var lifeStateBefore = GetProperty(model, "LifeState")?.ToString();

            Assert.That(InvokeTryGetAvailableRaiseSource(runtimeEntity, out var first), Is.True);
            Assert.That(InvokeTryGetAvailableRaiseSource(runtimeEntity, out var second), Is.True);

            Assert.That(first, Is.SameAs(source));
            Assert.That(second, Is.SameAs(source));
            Assert.That(GetProperty(source, "State")?.ToString(), Is.EqualTo(stateBefore));
            Assert.That(GetProperty(source, "Revision"), Is.EqualTo(revisionBefore));
            Assert.That(GetProperty(model, "Health"), Is.EqualTo(healthBefore));
            Assert.That(GetProperty(model, "LifeState")?.ToString(), Is.EqualTo(lifeStateBefore));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RaiseConversionPreservesRoleAndLineageAndRejectsSecondConversion()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-conversion", 5, out var root);
            InvokeDamage(runtimeEntity, 5, CreateEntityId("raise-source-conversion"));
            var source = GetRaiseSource(runtimeEntity);
            var result = InvokeRaise(source, "undead-instance-conversion", 7);

            var undead = GetProperty(result, "Undead");
            Assert.That(GetProperty(undead, "Id")?.ToString(), Is.EqualTo("undead-instance-conversion"));
            Assert.That(GetProperty(undead, "ArchetypeId"), Is.EqualTo("enemy.skeleton.guard"));
            Assert.That(GetProperty(undead, "Faction")?.ToString(), Is.EqualTo("Player"));
            Assert.That(GetProperty(result, "SourceId")?.ToString(), Is.EqualTo("raise-source-conversion"));
            Assert.That(GetProperty(result, "DefeatedEntityId")?.ToString(), Is.EqualTo("enemy-instance-conversion"));
            Assert.That(GetProperty(result, "RoleId"), Is.EqualTo("frontline.guard"));

            var secondError = Assert.Throws<TargetInvocationException>(
                () => InvokeRaise(source, "undead-instance-second", 7));
            Assert.That(secondError.InnerException, Is.TypeOf<InvalidOperationException>());

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RaiseSourceRequiresNonBlankRoleId()
        {
            var runtimeEntity = SpawnEnemy("enemy-instance-role-guard", 5, out var root);
            InvokeDamage(runtimeEntity, 5, CreateEntityId("raise-source-role-guard"));
            var source = GetRaiseSource(runtimeEntity);
            var sourceType = source.GetType();
            var model = GetProperty(runtimeEntity, "Model");
            var entityIdType = FindType("Necrom.Core.Domain.EntityId");
            var combatantType = FindType("Necrom.Core.Domain.Combatant");

            var constructor = sourceType.GetConstructor(new[] { entityIdType, combatantType, typeof(string) });
            Assert.That(constructor, Is.Not.Null, "RaiseSource must require RoleId at construction.");

            var error = Assert.Throws<TargetInvocationException>(
                () => constructor.Invoke(new[] { CreateEntityId("raise-source-invalid-role"), model, " " }));
            Assert.That(error.InnerException, Is.TypeOf<ArgumentException>());

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        private static object InvokeRaise(object source, string undeadId, int restoredHealth)
        {
            var serviceType = FindType("Necrom.Core.Domain.RaiseService");
            Assert.That(serviceType, Is.Not.Null);
            var service = Activator.CreateInstance(serviceType);
            var method = serviceType.GetMethod("Raise", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            var revision = GetProperty(source, "Revision");
            return method.Invoke(service, new[] { source, revision, CreateEntityId(undeadId), (object)restoredHealth });
        }
        private static bool InvokeTryGetAvailableRaiseSource(object runtimeEntity, out object source)
        {
            var method = runtimeEntity.GetType().GetMethod(
                "TryGetAvailableRaiseSource",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(
                method,
                Is.Not.Null,
                "EnemyRuntimeEntity must expose the approved side-effect-free raise-target eligibility query.");

            var args = new object[] { null };
            var result = method.Invoke(runtimeEntity, args);
            source = args[0];
            return (bool)result;
        }

        private static void ConsumeRaiseSource(object source)
        {
            var revision = GetProperty(source, "Revision");
            var method = source.GetType().GetMethod("Consume", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null);
            method.Invoke(source, new[] { revision });
        }

        private static void ReplaceRaiseSource(object runtimeEntity, object source)
        {
            var field = runtimeEntity.GetType().GetField(
                "<RaiseSource>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(runtimeEntity, source);
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
