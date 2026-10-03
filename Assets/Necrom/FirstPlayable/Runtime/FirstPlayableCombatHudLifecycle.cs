using System;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    [DisallowMultipleComponent]
    public sealed class FirstPlayableCombatHudLifecycle : MonoBehaviour
    {
        private FirstPlayableCombatHudPresenter _presenter;
        private FirstPlayableCombatHudUnityView _view;

        public bool IsInitialized => _presenter != null && _view != null;
        public GameObject OverlayHost => _view?.OverlayHost;

        public void Initialize(
            IFirstPlayableCombatHudStateSource stateSource,
            RectTransform safeArea,
            FirstPlayableCombatHudVerifiedDesignContract designContract,
            IFirstPlayableCombatHudCopyProvider copyProvider,
            IFirstPlayableCombatHudFontProvider fontProvider)
        {
            if (_presenter != null || _view != null)
            {
                throw new InvalidOperationException(
                    "Combat HUD lifecycle is already initialized.");
            }

            if (stateSource == null)
                throw new ArgumentNullException(nameof(stateSource));
            if (safeArea == null)
                throw new ArgumentNullException(nameof(safeArea));
            if (designContract == null)
                throw new ArgumentNullException(nameof(designContract));
            if (copyProvider == null)
                throw new ArgumentNullException(nameof(copyProvider));
            if (fontProvider == null)
                throw new ArgumentNullException(nameof(fontProvider));

            // Bind and validate the canonical semantic zones before owning any
            // renderer state. A bad scene layout therefore cannot leave an
            // orphan overlay behind.
            var zones = FirstPlayableCombatHudZoneBinding.Bind(safeArea);
            var view = new FirstPlayableCombatHudUnityView(
                designContract,
                copyProvider,
                fontProvider);

            _view = view;
            _presenter = new FirstPlayableCombatHudPresenter(
                stateSource,
                view,
                zones);
        }

        public FirstPlayableCombatHudPresentation Refresh()
        {
            EnsureInitialized();
            return _presenter.Refresh();
        }

        public void Shutdown()
        {
            var view = _view;
            _presenter = null;
            _view = null;
            view?.Dispose();
        }

        private void OnDestroy()
        {
            Shutdown();
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "Combat HUD lifecycle is not initialized.");
            }
        }
    }
}
