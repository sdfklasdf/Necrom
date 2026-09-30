using System;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class EncounterLayoutConfig
    {
        public Vector2 ScreenSize { get; }
        public Rect SafeAreaPixels { get; }
        public float HudFraction { get; }
        public Rect NecromancerZone { get; }
        public Rect EnemyZone { get; }
        public Rect AlliedSpawnZone { get; }

        public EncounterLayoutConfig(
            Vector2 screenSize,
            Rect safeAreaPixels,
            float hudFraction,
            Rect necromancerZone,
            Rect enemyZone,
            Rect alliedSpawnZone)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(screenSize));
            if (hudFraction <= 0f || hudFraction >= 1f)
                throw new ArgumentOutOfRangeException(nameof(hudFraction));
            if (safeAreaPixels.xMin < 0f || safeAreaPixels.yMin < 0f ||
                safeAreaPixels.xMax > screenSize.x || safeAreaPixels.yMax > screenSize.y ||
                safeAreaPixels.width <= 0f || safeAreaPixels.height <= 0f)
                throw new ArgumentOutOfRangeException(nameof(safeAreaPixels));

            ValidateNormalized(necromancerZone, nameof(necromancerZone));
            ValidateNormalized(enemyZone, nameof(enemyZone));
            ValidateNormalized(alliedSpawnZone, nameof(alliedSpawnZone));

            ScreenSize = screenSize;
            SafeAreaPixels = safeAreaPixels;
            HudFraction = hudFraction;
            NecromancerZone = necromancerZone;
            EnemyZone = enemyZone;
            AlliedSpawnZone = alliedSpawnZone;
        }

        private static void ValidateNormalized(Rect zone, string parameterName)
        {
            if (zone.width <= 0f || zone.height <= 0f ||
                zone.xMin < 0f || zone.yMin < 0f ||
                zone.xMax > 1f || zone.yMax > 1f)
                throw new ArgumentOutOfRangeException(parameterName);
        }
    }
}
