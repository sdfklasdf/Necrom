using System;
using System.Linq;
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
        TMP_FontAsset ResolveRegularFontAsset();
        TMP_FontAsset ResolveMediumFontAsset();
        TMP_FontAsset ResolveBoldFontAsset();
    }

    public sealed class FirstPlayableCombatHudFontProviderAdapter
        : IFirstPlayableCombatHudFontProvider
    {
        private readonly Func<TMP_FontAsset> _regular;
        private readonly Func<TMP_FontAsset> _medium;
        private readonly Func<TMP_FontAsset> _bold;

        public FirstPlayableCombatHudFontProviderAdapter(
            Func<TMP_FontAsset> resolve)
            : this(resolve, resolve, resolve)
        {
        }

        private FirstPlayableCombatHudFontProviderAdapter(
            Func<TMP_FontAsset> regular,
            Func<TMP_FontAsset> medium,
            Func<TMP_FontAsset> bold)
        {
            _regular = regular ?? throw new ArgumentNullException(nameof(regular));
            _medium = medium ?? throw new ArgumentNullException(nameof(medium));
            _bold = bold ?? throw new ArgumentNullException(nameof(bold));
        }

        public static FirstPlayableCombatHudFontProviderAdapter WithWeights(
            Func<TMP_FontAsset> regular,
            Func<TMP_FontAsset> medium,
            Func<TMP_FontAsset> bold)
            => new FirstPlayableCombatHudFontProviderAdapter(regular, medium, bold);

        public TMP_FontAsset ResolveRegularFontAsset() => _regular();
        public TMP_FontAsset ResolveMediumFontAsset() => _medium();
        public TMP_FontAsset ResolveBoldFontAsset() => _bold();
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
        private Sprite _panelSprite, _ctaSprite;
        private Sprite _combatIconSprite, _soulIconSprite, _armyIconSprite;

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

            var regularFont = _fontProvider.ResolveRegularFontAsset();
            var mediumFont = _fontProvider.ResolveMediumFontAsset();
            var boldFont = _fontProvider.ResolveBoldFontAsset();
            if (regularFont == null || mediumFont == null || boldFont == null)
            {
                throw new InvalidOperationException(
                    "Combat HUD production font set is unresolved. Regular / Medium / Bold are required.");
            }

            var targetCopy = RequireCopy(presentation.Target.ContentKey);
            var raiseCopy = RequireCopy(presentation.Raise.ContentKey);
            var armyCopy = RequireCopy(presentation.Army.ContentKey);

            _contract.GetState(presentation.Target.ContentKey);
            _contract.GetState(presentation.Raise.ContentKey);
            _contract.GetState(presentation.Army.ContentKey);

            var safeArea = ValidateSourceGeometry(zones);
            EnsureOverlay(regularFont, mediumFont, boldFont);
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

            _raiseBackground.color = _contract.Theme.PanelBackground.Value;
            ApplyAccent(_targetContainer, presentation.Target.ContentKey);
            ApplyAccent(_raiseContainer, presentation.Raise.ContentKey);
            ApplyAccent(_armyContainer, presentation.Army.ContentKey);

            ApplyRaiseCta(presentation.Raise.ContentKey, raiseCopy,
                presentation.Army.FormationSlots.Any(slot => slot == null || !slot.OwnedUnitId.HasValue));
        }

        public void Dispose()
        {
            if (_overlayHost == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(_overlayHost);
            else
                UnityEngine.Object.DestroyImmediate(_overlayHost);
            ReleaseSprite(_panelSprite);
            ReleaseSprite(_ctaSprite);
            ReleaseSprite(_combatIconSprite);
            ReleaseSprite(_soulIconSprite);
            ReleaseSprite(_armyIconSprite);
            _panelSprite = _ctaSprite = null;
            _combatIconSprite = _soulIconSprite = _armyIconSprite = null;
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

        private void EnsureOverlay(
            TMP_FontAsset regularFont,
            TMP_FontAsset mediumFont,
            TMP_FontAsset boldFont)
        {
            if (_overlayHost != null)
            {
                ApplyFont(regularFont, mediumFont, boldFont);
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
                regularFont,
                mediumFont,
                boldFont,
                out _targetState,
                out _targetPrimary,
                out _targetSecondary,
                out _);
            _raiseContainer = CreateSection(
                _safeAreaMirror,
                "RaiseRenderContainer",
                regularFont,
                mediumFont,
                boldFont,
                out _raiseState,
                out _raisePrimary,
                out _raiseSecondary,
                out _raiseBackground);
            _armyContainer = CreateSection(
                _safeAreaMirror,
                "ArmyRenderContainer",
                regularFont,
                mediumFont,
                boldFont,
                out _armyState,
                out _armyPrimary,
                out _armySecondary,
                out _);

            _protectedMirror = CreateRect(
                _safeAreaMirror,
                "ProtectedCombatReadabilityZoneMirror");

            CreatePrimaryCta(_raiseContainer, mediumFont);
        }

        private RectTransform CreateSection(
            RectTransform parent,
            string name,
            TMP_FontAsset regularFont,
            TMP_FontAsset mediumFont,
            TMP_FontAsset boldFont,
            out TextMeshProUGUI state,
            out TextMeshProUGUI primary,
            out TextMeshProUGUI secondary,
            out Image background)
        {
            var rect = CreateRect(parent, name);
            background = rect.gameObject.AddComponent<Image>();            background.color = _contract.Theme.PanelBackground.Value;
            background.sprite = _panelSprite ?? (_panelSprite = RoundedSprite(18f));
            background.type = Image.Type.Sliced;
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
                mediumFont,
                _contract.Theme.StateFontSize.Value,
                _contract.Theme.StateLineHeight.Value);
            primary = CreateText(
                rect,
                "PrimaryText",
                boldFont,
                _contract.Theme.PrimaryFontSize.Value,
                _contract.Theme.PrimaryLineHeight.Value);
            secondary = CreateText(
                rect,
                "SecondaryText",
                regularFont,
                _contract.Theme.SecondaryFontSize.Value,
                _contract.Theme.SecondaryLineHeight.Value);

            // StateKey remains a direct child for existing renderer consumers.
            // The semantic icon supplies shape redundancy in addition to label + color.
            state.margin = new Vector4(22f, state.margin.y, 0f, state.margin.w);
            var accent = CreateRect(rect, "StateAccent");
            accent.anchorMin = accent.anchorMax = new Vector2(0f, 1f);
            accent.pivot = new Vector2(0f, 1f);
            accent.anchoredPosition = new Vector2(padding, -padding-1f);
            accent.sizeDelta = new Vector2(14f, 14f);
            accent.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var icon = accent.gameObject.AddComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
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
            // Match CSS/Figma fixed line boxes even when a CJK face has wider vertical metrics.
            // Negative leading preserves the verified row height; glyph ink fits and the 8px gap remains.
            var naturalHeight=(font.faceInfo.ascentLine-font.faceInfo.descentLine)*fontSize/font.faceInfo.pointSize;
            var leading=Mathf.Max(0f,naturalHeight-preferredHeight+.1f)/2f;
            text.margin=new Vector4(0,-leading,0,-leading);

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
                typeof(CanvasGroup),
                typeof(LayoutElement));            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);

            var image = gameObject.GetComponent<Image>();
            image.color = _contract.Theme.SoulAccent.Value;
            image.sprite = _ctaSprite ?? (_ctaSprite = RoundedSprite(12f));
            image.type = Image.Type.Sliced;

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
        private void ApplyFont(
            TMP_FontAsset regularFont,
            TMP_FontAsset mediumFont,
            TMP_FontAsset boldFont)
        {
            _targetState.font = mediumFont;
            _targetPrimary.font = boldFont;
            _targetSecondary.font = regularFont;
            _raiseState.font = mediumFont;
            _raisePrimary.font = boldFont;
            _raiseSecondary.font = regularFont;
            _armyState.font = mediumFont;
            _armyPrimary.font = boldFont;
            _armySecondary.font = regularFont;
            _primaryCtaText.font = mediumFont;
        }

        private void ApplyRaiseCta(
            FirstPlayableCombatHudContentKey key,
            FirstPlayableCombatHudCopy copy,
            bool hasFormationSlot)
        {
            switch (key)
            {
                case FirstPlayableCombatHudContentKey.RaiseEligible:
                    LastRaiseCtaState =
                        hasFormationSlot ? FirstPlayableCombatHudRaiseCtaState.Default
                        : FirstPlayableCombatHudRaiseCtaState.Disabled;
                    _primaryCtaButton.gameObject.SetActive(true);
                    _primaryCtaButton.interactable = hasFormationSlot;
                    _primaryCtaText.text = copy.PrimaryCta;
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
            }
            var disabled = LastRaiseCtaState == FirstPlayableCombatHudRaiseCtaState.Disabled;
            _primaryCtaButton.GetComponent<Image>().color = disabled
                ? new Color(77f/255f,85f/255f,102f/255f)
                : _contract.Theme.SoulAccent.Value;
            _primaryCtaButton.GetComponent<CanvasGroup>().alpha = disabled ? .55f : 1f;
        }

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

        private void ApplyAccent(RectTransform panel, FirstPlayableCombatHudContentKey key)
        {
            // Q3 cute-reboot semantic palette: blue combat, mint soul-friend, purple team, coral warning.
            Color color = new Color(160f/255f,158f/255f,184f/255f);
            switch(key)
            {
                case FirstPlayableCombatHudContentKey.TargetActive:
                    color = new Color(82f/255f,171f/255f,1f); break;
                case FirstPlayableCombatHudContentKey.RaiseTargetNotReady:
                case FirstPlayableCombatHudContentKey.RaiseCommittedAwaitingProof:
                    color = new Color(79f/255f,212f/255f,174f/255f); break;
                case FirstPlayableCombatHudContentKey.ArmyOwned:
                case FirstPlayableCombatHudContentKey.ArmyProofPending:
                    color = new Color(140f/255f,120f/255f,1f); break;
                case FirstPlayableCombatHudContentKey.RaiseSourceUnavailableOrConsumed:
                    color = new Color(160f/255f,158f/255f,184f/255f); break;
                case FirstPlayableCombatHudContentKey.RaiseInsufficientSoul:
                    color = new Color(1f,122f/255f,120f/255f); break;
                case FirstPlayableCombatHudContentKey.TargetDefeated:
                case FirstPlayableCombatHudContentKey.RaiseEligible:
                case FirstPlayableCombatHudContentKey.RaiseProofObserved:
                case FirstPlayableCombatHudContentKey.ArmyProofObserved:
                    color = new Color(79f/255f,212f/255f,174f/255f); break;
            }
            var icon = panel.Find("StateAccent").GetComponent<Image>();
            icon.color = color;
            icon.sprite = ResolveSemanticIcon(key);
        }

        private Sprite ResolveSemanticIcon(FirstPlayableCombatHudContentKey key)
        {
            if (key.ToString().StartsWith("Target", StringComparison.Ordinal))
            {
                if (_combatIconSprite == null)
                    _combatIconSprite = FirstPlayableCombatHudSemanticIcons.Create(
                        FirstPlayableCombatHudSemanticIconKind.Combat);
                return _combatIconSprite;
            }

            if (key.ToString().StartsWith("Raise", StringComparison.Ordinal))
            {
                if (_soulIconSprite == null)
                    _soulIconSprite = FirstPlayableCombatHudSemanticIcons.Create(
                        FirstPlayableCombatHudSemanticIconKind.Soul);
                return _soulIconSprite;
            }

            if (_armyIconSprite == null)
                _armyIconSprite = FirstPlayableCombatHudSemanticIcons.Create(
                    FirstPlayableCombatHudSemanticIconKind.Army);
            return _armyIconSprite;
        }

        internal static Sprite RoundedSprite(float radius)
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Runtime semantic rounded surface";
            texture.wrapMode = TextureWrapMode.Clamp;
            var colors = new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                var point = new Vector2(x+.5f,y+.5f);
                var center = new Vector2(Mathf.Clamp(point.x,radius,size-radius),
                    Mathf.Clamp(point.y,radius,size-radius));
                colors[y*size+x] = new Color(1,1,1,Mathf.Clamp01(radius-Vector2.Distance(point,center)+.5f));
            }
            texture.SetPixels(colors);texture.Apply();
            return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100f,0,
                SpriteMeshType.FullRect,new Vector4(radius,radius,radius,radius));
        }

        private static void ReleaseSprite(Sprite sprite)
        {
            if(sprite==null) return;
            if(Application.isPlaying) { UnityEngine.Object.Destroy(sprite.texture);UnityEngine.Object.Destroy(sprite); }
            else { UnityEngine.Object.DestroyImmediate(sprite.texture);UnityEngine.Object.DestroyImmediate(sprite); }
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
