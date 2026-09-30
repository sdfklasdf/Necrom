[Reading 51 lines from start (total: 51 lines, 0 remaining)]

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableSceneShellTests
    {
        [UnityTest]
        public IEnumerator SceneShellBuildsNonOverlappingRegionsInsideSafeArea()
        {
            var shellType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Necrom.FirstPlayable.Runtime.FirstPlayableSceneShell"))
                .FirstOrDefault(t => t != null);
            Assert.That(shellType, Is.Not.Null, "Runtime scene shell type must exist.");

            var host = new GameObject("FirstPlayableSceneShellHost", typeof(RectTransform));
            var shell = host.AddComponent(shellType);
            var configure = shellType.GetMethod("Configure", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(configure, Is.Not.Null, "Configure API must be public.");
            configure.Invoke(shell, new object[]
            {
                new Vector2(390f, 844f),
                new Rect(0f, 34f, 390f, 776f),
                0.22f
            });

            yield return null;

            var safeArea = host.transform.Find("SafeArea") as RectTransform;
            Assert.That(safeArea, Is.Not.Null);
            var combat = safeArea.Find("CombatViewport") as RectTransform;
            var hud = safeArea.Find("HudRegion") as RectTransform;
            Assert.That(combat, Is.Not.Null);
            Assert.That(hud, Is.Not.Null);

            Assert.That(safeArea.anchorMin.y, Is.EqualTo(34f / 844f).Within(0.0001f));
            Assert.That(safeArea.anchorMax.y, Is.EqualTo(810f / 844f).Within(0.0001f));
            Assert.That(hud.anchorMax.y, Is.EqualTo(combat.anchorMin.y).Within(0.0001f));
            Assert.That(hud.anchorMin.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(combat.anchorMax.y, Is.EqualTo(1f).Within(0.0001f));

            UnityEngine.Object.Destroy(host);
            yield return null;
        }
    }
}

[executed on device: DESKTOP-7174LTC (46b0e2c9-d71e-4a77-8023-4823da2159d2)]