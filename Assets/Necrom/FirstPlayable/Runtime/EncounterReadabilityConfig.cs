using System;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class EncounterReadabilityConfig
    {
        public Rect TargetStatusZone { get; }
        public Rect RaiseActionStatusZone { get; }
        public Rect ArmyStatusZone { get; }
        public Rect ProtectedCombatReadabilityZone { get; }

        public EncounterReadabilityConfig(
            Rect targetStatusZone,
            Rect raiseActionStatusZone,
            Rect armyStatusZone,
            Rect protectedCombatReadabilityZone)
        {
            ValidateNormalized(targetStatusZone, nameof(targetStatusZone));
            ValidateNormalized(raiseActionStatusZone, nameof(raiseActionStatusZone));
            ValidateNormalized(armyStatusZone, nameof(armyStatusZone));
            ValidateNormalized(
                protectedCombatReadabilityZone,
                nameof(protectedCombatReadabilityZone));

            TargetStatusZone = targetStatusZone;
            RaiseActionStatusZone = raiseActionStatusZone;
            ArmyStatusZone = armyStatusZone;
            ProtectedCombatReadabilityZone = protectedCombatReadabilityZone;
        }

        private static void ValidateNormalized(Rect zone, string parameterName)
        {
            if (zone.width <= 0f ||
                zone.height <= 0f ||
                zone.xMin < 0f ||
                zone.yMin < 0f ||
                zone.xMax > 1f ||
                zone.yMax > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    "Readability zones must be positive normalized rectangles inside the SafeArea.");
            }
        }
    }
}