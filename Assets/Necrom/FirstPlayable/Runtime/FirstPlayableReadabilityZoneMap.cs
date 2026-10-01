using System;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableReadabilityZoneMap : MonoBehaviour
    {
        public void Configure(EncounterReadabilityConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var parent = transform as RectTransform;
            if (parent == null)
                throw new InvalidOperationException(
                    "Readability zone map requires a RectTransform SafeArea.");

            ConfigureZone(
                parent,
                "TargetStatusReadabilityZone",
                config.TargetStatusZone);
            ConfigureZone(
                parent,
                "RaiseActionStatusReadabilityZone",
                config.RaiseActionStatusZone);
            ConfigureZone(
                parent,
                "ArmyStatusReadabilityZone",
                config.ArmyStatusZone);
            ConfigureZone(
                parent,
                "ProtectedCombatReadabilityZone",
                config.ProtectedCombatReadabilityZone);
        }

        private static void ConfigureZone(
            RectTransform parent,
            string zoneName,
            Rect normalizedZone)
        {
            var zone = parent.Find(zoneName) as RectTransform;
            if (zone == null)
            {
                zone = new GameObject(
                    zoneName,
                    typeof(RectTransform)).GetComponent<RectTransform>();
                zone.SetParent(parent, false);
            }

            zone.anchorMin = new Vector2(normalizedZone.xMin, normalizedZone.yMin);
            zone.anchorMax = new Vector2(normalizedZone.xMax, normalizedZone.yMax);
            zone.pivot = new Vector2(0.5f, 0.5f);
            zone.offsetMin = Vector2.zero;
            zone.offsetMax = Vector2.zero;
            zone.localScale = Vector3.one;
            zone.localRotation = Quaternion.identity;
        }
    }
}