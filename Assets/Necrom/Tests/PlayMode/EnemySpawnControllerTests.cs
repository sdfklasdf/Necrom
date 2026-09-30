using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class EnemySpawnControllerTests
    {
        [UnityTest]
        public IEnumerator SpawnCreatesActiveEnemyAndMakesItCurrentTarget()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var definitionType = assemblies.Select(a => a.GetType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"))
                .FirstOrDefault(t => t != null);
            var controllerType = assemblies.Select(a => a.GetType("Necrom.FirstPlayable.Runtime.EnemySpawnController"))
                .FirstOrDefault(t => t != null);
            Assert.That(definitionType, Is.Not.Null);
            Assert.That(controllerType, Is.Not.Null, "Enemy spawn controller type must exist.");

            var definition = Activator.CreateInstance(definitionType, "enemy.skeleton.guard", "frontline.guard", 120);
            var root = new GameObject("CombatViewport", typeof(RectTransform));
            var zone = new GameObject("EnemySpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(root.transform, false);
            var controller = root.AddComponent(controllerType);
            var spawn = controllerType.GetMethod("SpawnEnemy", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(spawn, Is.Not.Null);

            var runtimeEntity = spawn.Invoke(controller, new object[] { "enemy-instance-1", definition, zone });
            Assert.That(runtimeEntity, Is.Not.Null);
            Assert.That(((Component)runtimeEntity).transform.parent, Is.EqualTo(zone));

            var model = runtimeEntity.GetType().GetProperty("Model")?.GetValue(runtimeEntity);
            Assert.That(model, Is.Not.Null);
            Assert.That(model.GetType().GetProperty("ArchetypeId")?.GetValue(model), Is.EqualTo("enemy.skeleton.guard"));
            Assert.That(model.GetType().GetProperty("Faction")?.GetValue(model)?.ToString(), Is.EqualTo("Enemy"));
            Assert.That(model.GetType().GetProperty("LifeState")?.GetValue(model)?.ToString(), Is.EqualTo("Active"));
            Assert.That(runtimeEntity.GetType().GetProperty("RoleId")?.GetValue(runtimeEntity), Is.EqualTo("frontline.guard"));

            var currentTarget = controllerType.GetProperty("CurrentTarget")?.GetValue(controller);
            Assert.That(currentTarget, Is.SameAs(runtimeEntity));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }
    }
}
