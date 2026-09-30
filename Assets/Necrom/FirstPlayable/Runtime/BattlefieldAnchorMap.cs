using System;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class BattlefieldAnchorMap : MonoBehaviour
    {
        public void Configure(Rect necromancerZone, Rect enemyZone, Rect alliedSpawnZone)
        {
            ConfigureZone("NecromancerSpawnZone", necromancerZone);
            ConfigureZone("EnemySpawnZone", enemyZone);
            ConfigureZone("AlliedSpawnZone", alliedSpawnZone);
        }

        private void ConfigureZone(string zoneName, Rect normalizedZone)
        {
            ValidateNormalized(normalizedZone, zoneName);

            var parent = transform as RectTransform;
            if (parent == null)
                throw new InvalidOperationException("BattlefieldAnchorMap requires a RectTransform.");

            var zone = parent.Find(zoneName) as RectTransform;
            if (zone == null)
            {
                zone = new GameObject(zoneName, typeof(RectTransform)).GetComponent<RectTransform>();
                zone.SetParent(parent, false);
            }

            zone.anchorMin = new Vector2(normalizedZone.xMin, normalizedZone.yMin);
            zone.anchorMax = new Vector2(normalizedZone.xMax, normalizedZone.yMax);
            zone.offsetMin = Vector2.zero;
            zone.offsetMax = Vector2.zero;
            zone.localScale = Vector3.one;
            zone.localRotation = Quaternion.identity;
        }

        private static void ValidateNormalized(Rect zone, string zoneName)
        {
            if (zone.width <= 0f || zone.height <= 0f ||
                zone.xMin < 0f || zone.yMin < 0f ||
                zone.xMax > 1f || zone.yMax > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    zoneName,
                    "Spawn zones must be positive normalized rectangles inside the combat viewport.");
            }
        }
    }
}
