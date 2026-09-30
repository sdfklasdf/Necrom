using System;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    [RequireComponent(typeof(FirstPlayableSceneShell))]
    public sealed class EncounterBoundaryController : MonoBehaviour
    {
        public void StartBoundary(EncounterLayoutConfig config)
            => ApplyValidatedBoundary(config);

        public void RestartBoundary(EncounterLayoutConfig config)
            => ApplyValidatedBoundary(config);

        private void ApplyValidatedBoundary(EncounterLayoutConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var sceneShell = GetComponent<FirstPlayableSceneShell>();
            if (sceneShell == null)
                throw new InvalidOperationException("Encounter boundary requires a scene shell.");

            sceneShell.Configure(config.ScreenSize, config.SafeAreaPixels, config.HudFraction);

            var safeArea = transform.Find("SafeArea");
            var combatViewport = safeArea != null
                ? safeArea.Find("CombatViewport") as RectTransform
                : null;
            if (combatViewport == null)
                throw new InvalidOperationException("Encounter boundary requires a combat viewport.");

            var anchorMap = combatViewport.GetComponent<BattlefieldAnchorMap>();
            if (anchorMap == null)
                anchorMap = combatViewport.gameObject.AddComponent<BattlefieldAnchorMap>();

            anchorMap.Configure(
                config.NecromancerZone,
                config.EnemyZone,
                config.AlliedSpawnZone);
        }
    }
}
