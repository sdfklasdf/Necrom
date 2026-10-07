using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(FirstPlayableCombatHudLifecycle))]
    public sealed class FirstPlayableCombatHudRuntimeBinding : MonoBehaviour
    {
        private FirstPlayableCombatHudLifecycle _lifecycle;
        private FirstPlayableCombatHudProjector _projector;
        private bool _initialized;

        public bool IsInitialized => _initialized;
        public FirstPlayableCombatHudPresentation LastPresentation { get; private set; }
        public GameObject OverlayHost => _lifecycle?.OverlayHost;

        public void Initialize(
            FirstPlayableBattleRuntimeController battle,
            EnemySpawnController enemies,
            FirstPlayableSoulResourceBridge soul,
            Formation formation,
            FirstPlayableAlliedRosterController roster,
            FirstPlayableCombatHudSession session,
            RectTransform safeArea,
            FirstPlayableCombatHudVerifiedDesignContract designContract,
            IFirstPlayableCombatHudCopyProvider copyProvider,
            IFirstPlayableCombatHudFontProvider fontProvider)
        {
            if (_initialized)
            {
                throw new InvalidOperationException(
                    "Combat HUD runtime binding is already initialized.");
            }

            if (battle == null) throw new ArgumentNullException(nameof(battle));
            if (enemies == null) throw new ArgumentNullException(nameof(enemies));
            if (soul == null) throw new ArgumentNullException(nameof(soul));
            if (formation == null) throw new ArgumentNullException(nameof(formation));
            if (roster == null) throw new ArgumentNullException(nameof(roster));
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (safeArea == null) throw new ArgumentNullException(nameof(safeArea));
            if (designContract == null)
                throw new ArgumentNullException(nameof(designContract));
            if (copyProvider == null)
                throw new ArgumentNullException(nameof(copyProvider));
            if (fontProvider == null)
                throw new ArgumentNullException(nameof(fontProvider));

            var lifecycle = GetComponent<FirstPlayableCombatHudLifecycle>();
            if (lifecycle == null)
            {
                throw new InvalidOperationException(
                    "Combat HUD runtime binding requires its lifecycle component.");
            }

            if (lifecycle.IsInitialized)
            {
                throw new InvalidOperationException(
                    "Combat HUD lifecycle is already owned by another binding.");
            }

            var projector = new FirstPlayableCombatHudProjector(
                battle,
                enemies,
                soul,
                formation,
                roster,
                session);

            try
            {
                lifecycle.Initialize(
                    projector,
                    safeArea,
                    designContract,
                    copyProvider,
                    fontProvider);

                // Render immediately so a successfully initialized normal-game
                // path never waits one frame with a stale or missing HUD.
                var initialPresentation = lifecycle.Refresh();

                _lifecycle = lifecycle;
                _projector = projector;
                LastPresentation = initialPresentation;
                _initialized = true;
            }
            catch
            {
                // Initialization is transactional from the binding's point of
                // view. Provider/layout failures must not leave an orphan
                // overlay or a half-owned lifecycle behind.
                lifecycle.Shutdown();
                throw;
            }
        }

        public void ConfigureDefenseWave(
            FirstPlayableDefenseWaveRuntimeController defenseWave)
        {
            EnsureInitialized();
            _projector.ConfigureDefenseWave(defenseWave);
            RefreshNow();
        }

        public FirstPlayableCombatHudPresentation RefreshNow()
        {
            EnsureInitialized();
            LastPresentation = _lifecycle.Refresh();
            return LastPresentation;
        }

        public void Shutdown()
        {
            if (!_initialized)
                return;

            var lifecycle = _lifecycle;

            _initialized = false;
            LastPresentation = null;
            _projector = null;
            _lifecycle = null;

            lifecycle?.Shutdown();
        }

        private void LateUpdate()
        {
            if (!_initialized)
                return;

            RefreshNow();
        }

        private void OnDisable()
        {
            Shutdown();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void EnsureInitialized()
        {
            if (!_initialized ||
                _lifecycle == null ||
                _projector == null)
            {
                throw new InvalidOperationException(
                    "Combat HUD runtime binding is not initialized.");
            }
        }
    }
}
