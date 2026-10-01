using System;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    [RequireComponent(typeof(FirstPlayableSceneShell))]
    public sealed class EncounterBoundaryController : MonoBehaviour
    {
        public void StartBoundary(EncounterLayoutConfig config)
            => ApplyValidatedBoundary(config, null);

        public void RestartBoundary(EncounterLayoutConfig config)
            => ApplyValidatedBoundary(config, null);

        public void StartBoundaryWithReadability(
            EncounterLayoutConfig config,
            EncounterReadabilityConfig readabilityConfig)
            => ApplyValidatedBoundary(config, readabilityConfig);

        public void RestartBoundaryWithReadability(
            EncounterLayoutConfig config,
            EncounterReadabilityConfig readabilityConfig)
            => ApplyValidatedBoundary(config, readabilityConfig);

        private void ApplyValidatedBoundary(
            EncounterLayoutConfig config,
            EncounterReadabilityConfig readabilityConfig)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (readabilityConfig != null)
                ValidateReadabilityBeforeMutation(config, readabilityConfig);

            var sceneShell = GetComponent<FirstPlayableSceneShell>();
            if (sceneShell == null)
                throw new InvalidOperationException(
                    "Encounter boundary requires a scene shell.");

            sceneShell.Configure(
                config.ScreenSize,
                config.SafeAreaPixels,
                config.HudFraction);

            var safeArea = transform.Find("SafeArea") as RectTransform;
            var combatViewport = safeArea != null
                ? safeArea.Find("CombatViewport") as RectTransform
                : null;
            if (combatViewport == null)
                throw new InvalidOperationException(
                    "Encounter boundary requires a combat viewport.");

            var anchorMap =
                combatViewport.GetComponent<BattlefieldAnchorMap>();
            if (anchorMap == null)
                anchorMap =
                    combatViewport.gameObject.AddComponent<BattlefieldAnchorMap>();

            anchorMap.Configure(
                config.NecromancerZone,
                config.EnemyZone,
                config.AlliedSpawnZone);

            if (readabilityConfig == null) return;

            var readabilityMap =
                safeArea.GetComponent<FirstPlayableReadabilityZoneMap>();
            if (readabilityMap == null)
            {
                readabilityMap =
                    safeArea.gameObject.AddComponent<FirstPlayableReadabilityZoneMap>();
            }

            readabilityMap.Configure(readabilityConfig);
        }

        private static void ValidateReadabilityBeforeMutation(
            EncounterLayoutConfig layout,
            EncounterReadabilityConfig readability)
        {
            var target = readability.TargetStatusZone;
            var raise = readability.RaiseActionStatusZone;
            var army = readability.ArmyStatusZone;
            var combat = readability.ProtectedCombatReadabilityZone;

            if (Overlaps(target, raise) ||
                Overlaps(target, army) ||
                Overlaps(raise, army))
            {
                throw new InvalidOperationException(
                    "Semantic HUD readability zones must not overlap.");
            }

            if (Overlaps(target, combat) ||
                Overlaps(raise, combat) ||
                Overlaps(army, combat))
            {
                throw new InvalidOperationException(
                    "HUD readability zones must not occlude the protected combat readability region.");
            }

            EnsureInsideHudRegion(
                target,
                layout.HudFraction,
                nameof(readability.TargetStatusZone));
            EnsureInsideHudRegion(
                raise,
                layout.HudFraction,
                nameof(readability.RaiseActionStatusZone));
            EnsureInsideHudRegion(
                army,
                layout.HudFraction,
                nameof(readability.ArmyStatusZone));

            if (combat.yMin < layout.HudFraction)
            {
                throw new InvalidOperationException(
                    "Protected combat readability zone must stay inside CombatViewport.");
            }
        }

        private static void EnsureInsideHudRegion(
            Rect zone,
            float hudFraction,
            string zoneName)
        {
            if (zone.yMin < 0f || zone.yMax > hudFraction)
            {
                throw new InvalidOperationException(
                    zoneName + " must stay inside HudRegion.");
            }
        }

        private static bool Overlaps(Rect a, Rect b)
            => a.xMin < b.xMax &&
               a.xMax > b.xMin &&
               a.yMin < b.yMax &&
               a.yMax > b.yMin;
    }
}