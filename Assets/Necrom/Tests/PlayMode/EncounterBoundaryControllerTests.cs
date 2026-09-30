using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class EncounterBoundaryControllerTests
    {
        [UnityTest]
        public IEnumerator StartAndRestartUseSameBoundaryWithoutDuplicationAndRepairDrift()
        {
            var configType = FindType("Necrom.FirstPlayable.Runtime.EncounterLayoutConfig");
            var controllerType = FindType("Necrom.FirstPlayable.Runtime.EncounterBoundaryController");
            Assert.That(configType, Is.Not.Null, "Encounter layout config type must exist.");
            Assert.That(controllerType, Is.Not.Null, "Encounter boundary controller type must exist.");

            var host = new GameObject("EncounterRoot", typeof(RectTransform));
            var controller = host.AddComponent(controllerType);
            var config = Activator.CreateInstance(configType, new object[]
            {
                new Vector2(390f, 844f),
                new Rect(0f, 34f, 390f, 776f),
                0.22f,
                new Rect(0.12f, 0.08f, 0.30f, 0.24f),
                new Rect(0.58f, 0.58f, 0.30f, 0.28f),
                new Rect(0.12f, 0.34f, 0.44f, 0.18f)
            });

            var start = controllerType.GetMethod("StartBoundary", BindingFlags.Instance | BindingFlags.Public);
            var restart = controllerType.GetMethod("RestartBoundary", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(start, Is.Not.Null);
            Assert.That(restart, Is.Not.Null);

            start.Invoke(controller, new[] { config });
            yield return null;

            var safeArea = host.transform.Find("SafeArea") as RectTransform;
            Assert.That(safeArea, Is.Not.Null);
            var hud = safeArea.Find("HudRegion") as RectTransform;
            var combat = safeArea.Find("CombatViewport") as RectTransform;
            Assert.That(hud, Is.Not.Null);
            Assert.That(combat, Is.Not.Null);

            var necro = combat.Find("NecromancerSpawnZone") as RectTransform;
            var enemy = combat.Find("EnemySpawnZone") as RectTransform;
            var ally = combat.Find("AlliedSpawnZone") as RectTransform;
            Assert.That(necro, Is.Not.Null);
            Assert.That(enemy, Is.Not.Null);
            Assert.That(ally, Is.Not.Null);

            var rootChildren = host.transform.childCount;
            var safeChildren = safeArea.childCount;
            var combatChildren = combat.childCount;

            hud.anchorMax = new Vector2(1f, 0.9f);
            combat.anchorMin = new Vector2(0f, 0.9f);
            necro.anchorMin = new Vector2(0.8f, 0.8f);
            necro.anchorMax = new Vector2(0.9f, 0.9f);

            restart.Invoke(controller, new[] { config });
            yield return null;

            Assert.That(host.transform.childCount, Is.EqualTo(rootChildren));
            Assert.That(safeArea.childCount, Is.EqualTo(safeChildren));
            Assert.That(combat.childCount, Is.EqualTo(combatChildren));
            Assert.That(hud.anchorMax.y, Is.EqualTo(0.22f).Within(0.0001f));
            Assert.That(combat.anchorMin.y, Is.EqualTo(0.22f).Within(0.0001f));
            AssertRect(necro, new Rect(0.12f, 0.08f, 0.30f, 0.24f));
            AssertRect(enemy, new Rect(0.58f, 0.58f, 0.30f, 0.28f));
            AssertRect(ally, new Rect(0.12f, 0.34f, 0.44f, 0.18f));

            UnityEngine.Object.Destroy(host);
            yield return null;
        }

        [Test]
        public void InvalidConfigIsRejectedBeforeBoundaryCanMutateScene()
        {
            var configType = FindType("Necrom.FirstPlayable.Runtime.EncounterLayoutConfig");
            Assert.That(configType, Is.Not.Null, "Encounter layout config type must exist.");

            var ex = Assert.Throws<TargetInvocationException>(() => Activator.CreateInstance(configType, new object[]
            {
                new Vector2(390f, 844f),
                new Rect(0f, 34f, 390f, 776f),
                0.22f,
                new Rect(0.12f, 0.08f, 0.30f, 0.24f),
                new Rect(0.80f, 0.80f, 0.30f, 0.30f),
                new Rect(0.12f, 0.34f, 0.44f, 0.18f)
            }));

            Assert.That(ex.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void EncounterLayoutDoesNotEnterGameStateSnapshotSchema()
        {
            var snapshotType = FindType("Necrom.Core.Persistence.GameStateSnapshot");
            Assert.That(snapshotType, Is.Not.Null);

            var forbidden = new[]
            {
                "ScreenSize", "SafeAreaPixels", "HudFraction",
                "NecromancerZone", "EnemyZone", "AlliedSpawnZone"
            };

            foreach (var propertyName in forbidden)
                Assert.That(snapshotType.GetProperty(propertyName), Is.Null, propertyName);
        }

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);

        private static void AssertRect(RectTransform actual, Rect expected)
        {
            Assert.That(actual.anchorMin.x, Is.EqualTo(expected.xMin).Within(0.0001f));
            Assert.That(actual.anchorMin.y, Is.EqualTo(expected.yMin).Within(0.0001f));
            Assert.That(actual.anchorMax.x, Is.EqualTo(expected.xMax).Within(0.0001f));
            Assert.That(actual.anchorMax.y, Is.EqualTo(expected.yMax).Within(0.0001f));
        }
    }
}
