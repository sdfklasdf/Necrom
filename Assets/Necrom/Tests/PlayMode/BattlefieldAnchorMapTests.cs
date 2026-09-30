using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class BattlefieldAnchorMapTests
    {
        [UnityTest]
        public IEnumerator AnchorMapCreatesStableNormalizedSpawnZones()
        {
            var mapType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("Necrom.FirstPlayable.Runtime.BattlefieldAnchorMap"))
                .FirstOrDefault(t => t != null);
            Assert.That(mapType, Is.Not.Null, "Battlefield anchor map type must exist.");

            var host = new GameObject("CombatViewport", typeof(RectTransform));
            var map = host.AddComponent(mapType);
            var configure = mapType.GetMethod("Configure", BindingFlags.Instance | BindingFlags.Public);
            Assert.That(configure, Is.Not.Null);

            var necro = new Rect(0.12f, 0.08f, 0.30f, 0.24f);
            var enemy = new Rect(0.58f, 0.58f, 0.30f, 0.28f);
            var ally = new Rect(0.12f, 0.34f, 0.44f, 0.18f);
            configure.Invoke(map, new object[] { necro, enemy, ally });
            yield return null;

            AssertZone(host.transform, "NecromancerSpawnZone", necro);
            AssertZone(host.transform, "EnemySpawnZone", enemy);
            AssertZone(host.transform, "AlliedSpawnZone", ally);

            var childCount = host.transform.childCount;
            configure.Invoke(map, new object[] { necro, enemy, ally });
            Assert.That(host.transform.childCount, Is.EqualTo(childCount), "Reconfigure must not duplicate zones.");

            UnityEngine.Object.Destroy(host);
            yield return null;
        }

        private static void AssertZone(Transform parent, string name, Rect expected)
        {
            var zone = parent.Find(name) as RectTransform;
            Assert.That(zone, Is.Not.Null, name);
            Assert.That(zone.anchorMin.x, Is.EqualTo(expected.xMin).Within(0.0001f));
            Assert.That(zone.anchorMin.y, Is.EqualTo(expected.yMin).Within(0.0001f));
            Assert.That(zone.anchorMax.x, Is.EqualTo(expected.xMax).Within(0.0001f));
            Assert.That(zone.anchorMax.y, Is.EqualTo(expected.yMax).Within(0.0001f));
        }
    }
}
