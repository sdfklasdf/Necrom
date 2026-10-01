using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableReadabilityZoneTests
    {
        [UnityTest]
        public IEnumerator ApprovedReadabilityZoneContractExists()
        {
            var configType = FindType("Necrom.FirstPlayable.Runtime.EncounterReadabilityConfig");
            var mapType = FindType("Necrom.FirstPlayable.Runtime.FirstPlayableReadabilityZoneMap");
            var controllerType = RequireType("Necrom.FirstPlayable.Runtime.EncounterBoundaryController");

            Assert.That(configType, Is.Not.Null);
            Assert.That(mapType, Is.Not.Null);
            Assert.That(controllerType.GetMethod("StartBoundaryWithReadability"), Is.Not.Null);
            Assert.That(controllerType.GetMethod("RestartBoundaryWithReadability"), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ValidInjectedZonesAreDeterministicAndPreserveSpawnZones()
        {
            var fixture = NewFixture();
            var readability = NewReadabilityConfig(
                new Rect(0.02f, 0.02f, 0.28f, 0.08f),
                new Rect(0.36f, 0.02f, 0.28f, 0.08f),
                new Rect(0.70f, 0.02f, 0.28f, 0.08f),
                new Rect(0.05f, 0.28f, 0.90f, 0.65f));

            InvokeBoundary(fixture.Controller, "StartBoundaryWithReadability", fixture.Layout, readability);
            yield return null;

            var safeArea = fixture.Root.transform.Find("SafeArea") as RectTransform;
            Assert.That(safeArea, Is.Not.Null);
            var hud = safeArea.Find("HudRegion") as RectTransform;
            var combat = safeArea.Find("CombatViewport") as RectTransform;
            Assert.That(hud, Is.Not.Null);
            Assert.That(combat, Is.Not.Null);

            AssertRect(safeArea.Find("TargetStatusReadabilityZone") as RectTransform,
                new Rect(0.02f, 0.02f, 0.28f, 0.08f));
            AssertRect(safeArea.Find("RaiseActionStatusReadabilityZone") as RectTransform,
                new Rect(0.36f, 0.02f, 0.28f, 0.08f));
            AssertRect(safeArea.Find("ArmyStatusReadabilityZone") as RectTransform,
                new Rect(0.70f, 0.02f, 0.28f, 0.08f));
            AssertRect(safeArea.Find("ProtectedCombatReadabilityZone") as RectTransform,
                new Rect(0.05f, 0.28f, 0.90f, 0.65f));

            AssertRect(combat.Find("NecromancerSpawnZone") as RectTransform,
                new Rect(0.12f, 0.08f, 0.30f, 0.24f));
            AssertRect(combat.Find("EnemySpawnZone") as RectTransform,
                new Rect(0.58f, 0.58f, 0.30f, 0.28f));
            AssertRect(combat.Find("AlliedSpawnZone") as RectTransform,
                new Rect(0.12f, 0.34f, 0.44f, 0.18f));

            var rootChildren = fixture.Root.transform.childCount;
            var safeChildren = safeArea.childCount;
            var combatChildren = combat.childCount;

            (safeArea.Find("TargetStatusReadabilityZone") as RectTransform).anchorMin =
                new Vector2(0.8f, 0.8f);
            (combat.Find("NecromancerSpawnZone") as RectTransform).anchorMin =
                new Vector2(0.8f, 0.8f);

            InvokeBoundary(fixture.Controller, "RestartBoundaryWithReadability", fixture.Layout, readability);
            yield return null;

            Assert.That(fixture.Root.transform.childCount, Is.EqualTo(rootChildren));
            Assert.That(safeArea.childCount, Is.EqualTo(safeChildren));
            Assert.That(combat.childCount, Is.EqualTo(combatChildren));
            AssertRect(safeArea.Find("TargetStatusReadabilityZone") as RectTransform,
                new Rect(0.02f, 0.02f, 0.28f, 0.08f));
            AssertRect(combat.Find("NecromancerSpawnZone") as RectTransform,
                new Rect(0.12f, 0.08f, 0.30f, 0.24f));

            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [Test]
        public void OverlappingSemanticHudZonesRejectBeforeSceneMutation()
        {
            var fixture = NewFixture();
            var readability = NewReadabilityConfig(
                new Rect(0.02f, 0.02f, 0.45f, 0.10f),
                new Rect(0.30f, 0.02f, 0.30f, 0.10f),
                new Rect(0.70f, 0.02f, 0.28f, 0.08f),
                new Rect(0.05f, 0.28f, 0.90f, 0.65f));

            AssertRejectedBeforeMutation(fixture, readability);
            UnityEngine.Object.DestroyImmediate(fixture.Root);
        }

        [Test]
        public void HudZoneOverProtectedCombatRejectsBeforeSceneMutation()
        {
            var fixture = NewFixture();
            var readability = NewReadabilityConfig(
                new Rect(0.02f, 0.02f, 0.28f, 0.08f),
                new Rect(0.36f, 0.02f, 0.28f, 0.08f),
                new Rect(0.70f, 0.02f, 0.28f, 0.08f),
                new Rect(0.20f, 0.06f, 0.60f, 0.70f));

            AssertRejectedBeforeMutation(fixture, readability);
            UnityEngine.Object.DestroyImmediate(fixture.Root);
        }

        [Test]
        public void SemanticHudZoneOutsideHudRegionRejectsBeforeSceneMutation()
        {
            var fixture = NewFixture();
            var readability = NewReadabilityConfig(
                new Rect(0.02f, 0.18f, 0.28f, 0.08f),
                new Rect(0.36f, 0.02f, 0.28f, 0.08f),
                new Rect(0.70f, 0.02f, 0.28f, 0.08f),
                new Rect(0.05f, 0.28f, 0.90f, 0.65f));

            AssertRejectedBeforeMutation(fixture, readability);
            UnityEngine.Object.DestroyImmediate(fixture.Root);
        }

        [Test]
        public void ProtectedCombatZoneOutsideCombatViewportRejectsBeforeSceneMutation()
        {
            var fixture = NewFixture();
            var readability = NewReadabilityConfig(
                new Rect(0.02f, 0.02f, 0.28f, 0.08f),
                new Rect(0.36f, 0.02f, 0.28f, 0.08f),
                new Rect(0.70f, 0.02f, 0.28f, 0.08f),
                new Rect(0.05f, 0.18f, 0.90f, 0.70f));

            AssertRejectedBeforeMutation(fixture, readability);
            UnityEngine.Object.DestroyImmediate(fixture.Root);
        }

        [Test]
        public void OutOfBoundsReadabilityRectIsRejected()
        {
            var configType = RequireType("Necrom.FirstPlayable.Runtime.EncounterReadabilityConfig");
            var ex = Assert.Throws<TargetInvocationException>(() =>
                Activator.CreateInstance(configType, new object[]
                {
                    new Rect(-0.01f, 0.02f, 0.28f, 0.08f),
                    new Rect(0.36f, 0.02f, 0.28f, 0.08f),
                    new Rect(0.70f, 0.02f, 0.28f, 0.08f),
                    new Rect(0.05f, 0.28f, 0.90f, 0.65f)
                }));

            Assert.That(ex.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        private static Fixture NewFixture()
        {
            var root = new GameObject("ReadabilityFixture", typeof(RectTransform));
            var controllerType = RequireType("Necrom.FirstPlayable.Runtime.EncounterBoundaryController");
            var controller = root.AddComponent(controllerType);
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
            return new Fixture(root, controller, layout);
        }

        private static object NewReadabilityConfig(
            Rect targetStatus,
            Rect raiseAction,
            Rect armyStatus,
            Rect protectedCombat)
            => Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.EncounterReadabilityConfig"),
                new object[] { targetStatus, raiseAction, armyStatus, protectedCombat });

        private static void AssertRejectedBeforeMutation(Fixture fixture, object readability)
        {
            var ex = Assert.Throws<TargetInvocationException>(() =>
                InvokeBoundary(
                    fixture.Controller,
                    "StartBoundaryWithReadability",
                    fixture.Layout,
                    readability));
            Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(fixture.Root.transform.Find("SafeArea"), Is.Null);
            Assert.That(fixture.Root.transform.childCount, Is.EqualTo(0));
        }

        private static object InvokeBoundary(
            Component controller,
            string methodName,
            object layout,
            object readability)
        {
            var method = controller.GetType().GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, methodName + " must exist.");
            return method.Invoke(controller, new[] { layout, readability });
        }

        private static void AssertRect(RectTransform actual, Rect expected)
        {
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.anchorMin.x, Is.EqualTo(expected.xMin).Within(0.0001f));
            Assert.That(actual.anchorMin.y, Is.EqualTo(expected.yMin).Within(0.0001f));
            Assert.That(actual.anchorMax.x, Is.EqualTo(expected.xMax).Within(0.0001f));
            Assert.That(actual.anchorMax.y, Is.EqualTo(expected.yMax).Within(0.0001f));
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

        private sealed class Fixture
        {
            public GameObject Root { get; }
            public Component Controller { get; }
            public object Layout { get; }

            public Fixture(GameObject root, Component controller, object layout)
            {
                Root = root;
                Controller = controller;
                Layout = layout;
            }
        }
    }
}