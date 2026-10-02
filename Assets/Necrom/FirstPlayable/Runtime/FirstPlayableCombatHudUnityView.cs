using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableCombatHudCopy
    {
        public string StateKey { get; }
        public string Primary { get; }
        public string Secondary { get; }
        public string PrimaryCta { get; }

        public FirstPlayableCombatHudCopy(
            string stateKey,
            string primary,
            string secondary,
            string primaryCta)
        {
            StateKey = stateKey ?? string.Empty;
            Primary = primary ?? string.Empty;
            Secondary = secondary ?? string.Empty;
            PrimaryCta = primaryCta ?? string.Empty;
        }
    }

    public interface IFirstPlayableCombatHudCopyProvider
    {        FirstPlayableCombatHudCopy Resolve(
            FirstPlayableCombatHudContentKey contentKey);
    }

    public sealed class FirstPlayableCombatHudCopyProviderAdapter
        : IFirstPlayableCombatHudCopyProvider
    {
        private readonly Func<
            FirstPlayableCombatHudContentKey,
            FirstPlayableCombatHudCopy> _resolve;

        public FirstPlayableCombatHudCopyProviderAdapter(
            Func<
                FirstPlayableCombatHudContentKey,
                FirstPlayableCombatHudCopy> resolve)
        {
            _resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
        }

        public FirstPlayableCombatHudCopy Resolve(
            FirstPlayableCombatHudContentKey contentKey)
            => _resolve(contentKey);
    }

    public interface IFirstPlayableCombatHudFontProvider
    {
        TMP_FontAsset ResolveFontAsset();
    }
    public sealed class FirstPlayableCombatHudFontProviderAdapter
        : IFirstPlayableCombatHudFontProvider
    {
        private readonly Func<TMP_FontAsset> _resolve;

        public FirstPlayableCombatHudFontProviderAdapter(
            Func<TMP_FontAsset> resolve)
        {
            _resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
        }

        public TMP_FontAsset ResolveFontAsset()
            => _resolve();
    }

    public enum FirstPlayableCombatHudRaiseCtaState
    {
        Hidden,
        Default,
        Disabled
    }

    public sealed class FirstPlayableCombatHudUnityView
        : IFirstPlayableCombatHudView, IDisposable
    {        private readonly FirstPlayableCombatHudVerifiedDesignContract _contract;
        private readonly IFirstPlayableCombatHudCopyProvider _copyProvider;
        private readonly IFirstPlayableCombatHudFontProvider _fontProvider;

        private GameObject _overlayHost;
        private RectTransform _safeAreaMirror;
        private RectTransform _targetContainer;
        private RectTransform _raiseContainer;
        private RectTransform _armyContainer;
        private RectTransform _protectedMirror;
        private TextMeshProUGUI _targetState;
        private TextMeshProUGUI _targetPrimary;
        private TextMeshProUGUI _targetSecondary;
        private TextMeshProUGUI _raiseState;
        private TextMeshProUGUI _raisePrimary;
        private TextMeshProUGUI _raiseSecondary;
        private TextMeshProUGUI _armyState;
        private TextMeshProUGUI _armyPrimary;
        private TextMeshProUGUI _armySecondary;
        private Button _primaryCtaButton;
        private TextMeshProUGUI _primaryCtaText;
        private Image _raiseBackground;

        public GameObject OverlayHost => _overlayHost;
        public Button PrimaryCtaButton => _primaryCtaButton;
        public FirstPlayableCombatHudRaiseCtaState LastRaiseCtaState { get; private set; }
        public FirstPlayableCombatHudUnityView(
            FirstPlayableCombatHudVerifiedDesignContract contract,
            IFirstPlayableCombatHudCopyProvider copyProvider,
            IFirstPlayableCombatHudFontProvider fontProvider)
        {
            _contract = contract ?? throw new ArgumentNullException(nameof(contract));
            _copyProvider = copyProvider ?? throw new ArgumentNullException(nameof(copyProvider));
            _fontProvider = fontProvider ?? throw new ArgumentNullException(nameof(fontProvider));
            LastRaiseCtaState = FirstPlayableCombatHudRaiseCtaState.Hidden;
        }

        public void Render(
            FirstPlayableCombatHudPresentation presentation,
            FirstPlayableCombatHudZoneBinding zones)
        {
            if (presentation == null)
                throw new ArgumentNullException(nameof(presentation));
            if (zones == null)
                throw new ArgumentNullException(nameof(zones));
            if (presentation.Raise.IsProcessing)
            {
                throw new InvalidOperationException(
                    "Current synchronous Raise runtime must not render a fake loading state.");
            }

            var font = _fontProvider.ResolveFontAsset();
            if (font == null)            {
                throw new InvalidOperationException(
                    "Combat HUD font asset is unresolved. A provider must supply it before rendering.");
            }

            var targetCopy = RequireCopy(presentation.Target.ContentKey);
            var raiseCopy = RequireCopy(presentation.Raise.ContentKey);
            var armyCopy = RequireCopy(presentation.Army.ContentKey);

            _contract.GetState(presentation.Target.ContentKey);
            _contract.GetState(presentation.Raise.ContentKey);
            _contract.GetState(presentation.Army.ContentKey);

            var safeArea = ValidateSourceGeometry(zones);
            EnsureOverlay(font);
            MirrorLayout(safeArea, _safeAreaMirror);
            MirrorLayout(zones.TargetStatusZone, _targetContainer);
            MirrorLayout(zones.RaiseActionStatusZone, _raiseContainer);
            MirrorLayout(zones.ArmyStatusZone, _armyContainer);
            MirrorLayout(zones.ProtectedCombatReadabilityZone, _protectedMirror);

            ApplyCopy(
                _targetState,
                _targetPrimary,
                _targetSecondary,
                targetCopy);
            ApplyCopy(                _raiseState,
                _raisePrimary,
                _raiseSecondary,
                raiseCopy);
            ApplyCopy(
                _armyState,
                _armyPrimary,
                _armySecondary,
                armyCopy);

            _raiseBackground.color =
                presentation.Raise.ContentKey ==
                FirstPlayableCombatHudContentKey.RaiseInsufficientSoul
                    ? _contract.Theme.Danger.Value
                    : _contract.Theme.PanelBackground.Value;

            ApplyRaiseCta(presentation.Raise.ContentKey, raiseCopy);
        }

        public void Dispose()
        {
            if (_overlayHost == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(_overlayHost);
            else
                UnityEngine.Object.DestroyImmediate(_overlayHost);
            _overlayHost = null;
            _safeAreaMirror = null;
            _targetContainer = null;
            _raiseContainer = null;
            _armyContainer = null;
            _protectedMirror = null;
            _primaryCtaButton = null;
            _primaryCtaText = null;
            LastRaiseCtaState = FirstPlayableCombatHudRaiseCtaState.Hidden;
        }

        private FirstPlayableCombatHudCopy RequireCopy(
            FirstPlayableCombatHudContentKey key)
        {
            var copy = _copyProvider.Resolve(key);
            if (copy == null)
            {
                throw new InvalidOperationException(
                    "Combat HUD copy is unresolved for semantic key: " + key);
            }

            return copy;
        }

        private static RectTransform ValidateSourceGeometry(
            FirstPlayableCombatHudZoneBinding zones)
        {            var safeArea = zones.TargetStatusZone.parent as RectTransform;
            if (safeArea == null ||
                !ReferenceEquals(zones.RaiseActionStatusZone.parent, safeArea) ||
                !ReferenceEquals(zones.ArmyStatusZone.parent, safeArea) ||
                !ReferenceEquals(
                    zones.ProtectedCombatReadabilityZone.parent,
                    safeArea))
            {
                throw new InvalidOperationException(
                    "Combat HUD source zones must remain direct children of one SafeArea.");
            }

            return safeArea;
        }

        private void EnsureOverlay(TMP_FontAsset font)
        {
            if (_overlayHost != null)
            {
                ApplyFont(font);
                return;
            }

            _overlayHost = new GameObject(
                "FirstPlayableCombatHudOverlayCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = _overlayHost.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = _overlayHost.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

            var hostRect = _overlayHost.GetComponent<RectTransform>();
            _safeAreaMirror = CreateRect(hostRect, "SafeAreaMirror");
            StretchToParent(_safeAreaMirror);

            _targetContainer = CreateSection(
                _safeAreaMirror,
                "TargetRenderContainer",
                font,
                out _targetState,
                out _targetPrimary,
                out _targetSecondary,
                out _);
            _raiseContainer = CreateSection(
                _safeAreaMirror,
                "RaiseRenderContainer",
                font,
                out _raiseState,
                out _raisePrimary,
                out _raiseSecondary,
                out _raiseBackground);            _armyContainer = CreateSection(
                _safeAreaMirror,
                "ArmyRenderContainer",
                font,
                out _armyState,
                out _armyPrimary,
                out _armySecondary,
                out _);

            _protectedMirror = CreateRect(
                _safeAreaMirror,
                "ProtectedCombatReadabilityZoneMirror");

            CreatePrimaryCta(_raiseContainer, font);
        }

        private RectTransform CreateSection(
            RectTransform parent,
            string name,
            TMP_FontAsset font,
            out TextMeshProUGUI state,
            out TextMeshProUGUI primary,
            out TextMeshProUGUI secondary,
            out Image background)
        {
            var rect = CreateRect(parent, name);
            background = rect.gameObject.AddComponent<Image>();            background.color = _contract.Theme.PanelBackground.Value;
            background.raycastTarget = false;

            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            var padding = Mathf.RoundToInt(_contract.Theme.PanelPadding.Value);
            layout.padding = new RectOffset(
                padding,
                padding,
                padding,
                padding);
            layout.spacing = _contract.Theme.InnerGap.Value;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            state = CreateText(
                rect,
                "StateKey",
                font,
                _contract.Theme.StateFontSize.Value,
                _contract.Theme.StateLineHeight.Value);
            primary = CreateText(
                rect,                "PrimaryText",
                font,
                _contract.Theme.PrimaryFontSize.Value,
                _contract.Theme.PrimaryLineHeight.Value);
            secondary = CreateText(
                rect,
                "SecondaryText",
                font,
                _contract.Theme.SecondaryFontSize.Value,
                _contract.Theme.SecondaryLineHeight.Value);

            return rect;
        }

        private TextMeshProUGUI CreateText(
            RectTransform parent,
            string name,
            TMP_FontAsset font,
            float fontSize,
            float preferredHeight)
        {
            var gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI),
                typeof(LayoutElement));            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var text = gameObject.GetComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = _contract.Theme.TextOnDark.Value;
            text.raycastTarget = false;
            text.enableWordWrapping = true;

            var element = gameObject.GetComponent<LayoutElement>();
            element.preferredHeight = preferredHeight;
            element.flexibleHeight = 0f;

            return text;
        }

        private void CreatePrimaryCta(
            RectTransform parent,
            TMP_FontAsset font)
        {
            var gameObject = new GameObject(
                "PrimaryCta",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var image = gameObject.GetComponent<Image>();
            image.color = _contract.Theme.SoulAccent.Value;

            _primaryCtaButton = gameObject.GetComponent<Button>();
            _primaryCtaButton.transition = Selectable.Transition.None;

            var element = gameObject.GetComponent<LayoutElement>();
            element.minHeight = _contract.Theme.PrimaryCtaMinHeight.Value;
            element.preferredHeight = _contract.Theme.PrimaryCtaMinHeight.Value;

            _primaryCtaText = CreateText(
                rect,
                "CtaText",
                font,
                _contract.Theme.SecondaryFontSize.Value,
                _contract.Theme.PrimaryCtaMinHeight.Value);
            var textRect = _primaryCtaText.rectTransform;
            StretchToParent(textRect);
            _primaryCtaText.alignment = TextAlignmentOptions.Center;

            _primaryCtaButton.gameObject.SetActive(false);
        }
        private void ApplyFont(TMP_FontAsset font)
        {
            _targetState.font = font;
            _targetPrimary.font = font;
            _targetSecondary.font = font;
            _raiseState.font = font;
            _raisePrimary.font = font;
            _raiseSecondary.font = font;
            _armyState.font = font;
            _armyPrimary.font = font;
            _armySecondary.font = font;
            _primaryCtaText.font = font;
        }

        private void ApplyRaiseCta(
            FirstPlayableCombatHudContentKey key,
            FirstPlayableCombatHudCopy copy)
        {
            switch (key)
            {
                case FirstPlayableCombatHudContentKey.RaiseEligible:
                    LastRaiseCtaState =
                        FirstPlayableCombatHudRaiseCtaState.Default;
                    _primaryCtaButton.gameObject.SetActive(true);
                    _primaryCtaButton.interactable = true;                    _primaryCtaText.text = copy.PrimaryCta;
                    break;

                case FirstPlayableCombatHudContentKey.RaiseTargetNotReady:
                case FirstPlayableCombatHudContentKey.RaiseSourceUnavailableOrConsumed:
                case FirstPlayableCombatHudContentKey.RaiseInsufficientSoul:
                    LastRaiseCtaState =
                        FirstPlayableCombatHudRaiseCtaState.Disabled;
                    _primaryCtaButton.gameObject.SetActive(true);
                    _primaryCtaButton.interactable = false;
                    _primaryCtaText.text = copy.PrimaryCta;
                    break;

                case FirstPlayableCombatHudContentKey.RaiseNoTarget:
                case FirstPlayableCombatHudContentKey.RaiseCommittedAwaitingProof:
                case FirstPlayableCombatHudContentKey.RaiseProofObserved:
                    LastRaiseCtaState =
                        FirstPlayableCombatHudRaiseCtaState.Hidden;
                    _primaryCtaButton.gameObject.SetActive(false);
                    _primaryCtaButton.interactable = false;
                    _primaryCtaText.text = string.Empty;
                    break;

                default:
                    throw new InvalidOperationException(
                        "Non-Raise key supplied to Raise CTA mapping: " + key);
            }        }

        private static void ApplyCopy(
            TextMeshProUGUI state,
            TextMeshProUGUI primary,
            TextMeshProUGUI secondary,
            FirstPlayableCombatHudCopy copy)
        {
            state.text = copy.StateKey;
            primary.text = copy.Primary;
            secondary.text = copy.Secondary;
        }

        private static RectTransform CreateRect(
            Transform parent,
            string name)
        {
            var rect = new GameObject(
                name,
                typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void MirrorLayout(
            RectTransform source,
            RectTransform destination)
        {
            destination.anchorMin = source.anchorMin;            destination.anchorMax = source.anchorMax;
            destination.pivot = source.pivot;
            destination.offsetMin = source.offsetMin;
            destination.offsetMax = source.offsetMax;
            destination.localScale = source.localScale;
            destination.localRotation = source.localRotation;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }
    }
}
