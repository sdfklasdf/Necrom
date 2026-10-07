using System;
using Necrom.Core.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    [DisallowMultipleComponent]
    public sealed class FirstPlayableDefenseWaveHudRuntimeBinding : MonoBehaviour
    {
        public const string FigmaFileKey = "eXqKU1qHXsn52SJfIGltZo";
        public const string FigmaNodeId = "46:309";

        private FirstPlayableDefenseWaveRuntimeController _defense;
        private FirstPlayableCombatHudRuntimeBinding _combatHud;
        private TMP_FontAsset _regular;
        private TMP_FontAsset _medium;
        private TMP_FontAsset _bold;
        private GameObject _boundOverlay;
        private RectTransform _container;
        private TextMeshProUGUI _stateText;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _detailText;
        private Image _background;
        private Outline _outline;
        private RectTransform _gateFillRect;
        private Image _gateFill;
        private Sprite _panelSprite;
        private Sprite _barSprite;

        public bool IsInitialized =>
            _defense != null &&
            _combatHud != null &&
            _regular != null &&
            _medium != null &&
            _bold != null;

        public GameObject OverlayRoot => _container == null ? null : _container.gameObject;
        public FirstPlayableDefenseWaveHudState LastState { get; private set; }

        public void Initialize(
            FirstPlayableDefenseWaveRuntimeController defense,
            FirstPlayableCombatHudRuntimeBinding combatHud,
            TMP_FontAsset regular,
            TMP_FontAsset medium,
            TMP_FontAsset bold)
        {
            if (IsInitialized)
                throw new InvalidOperationException(
                    "Defense HUD runtime binding is already initialized.");

            _defense = defense ?? throw new ArgumentNullException(nameof(defense));
            _combatHud = combatHud ?? throw new ArgumentNullException(nameof(combatHud));
            _regular = regular ?? throw new ArgumentNullException(nameof(regular));
            _medium = medium ?? throw new ArgumentNullException(nameof(medium));
            _bold = bold ?? throw new ArgumentNullException(nameof(bold));
        }

        public FirstPlayableDefenseWaveHudState RefreshNow()
        {
            EnsureInitialized();
            EnsureView();
            ApplyResponsiveGeometry();

            LastState = _defense.CaptureHudState();
            Apply(LastState);
            return LastState;
        }

        private void LateUpdate()
        {
            if (!IsInitialized || !_combatHud.IsInitialized)
                return;

            RefreshNow();
        }

        private void EnsureView()
        {
            if (!_combatHud.IsInitialized || _combatHud.OverlayHost == null)
                throw new InvalidOperationException(
                    "Defense HUD requires the active canonical combat HUD overlay.");

            var overlay = _combatHud.OverlayHost;
            if (_boundOverlay == overlay && _container != null)
                return;

            if (_container != null)
            {
                if (Application.isPlaying)
                    Destroy(_container.gameObject);
                else
                    DestroyImmediate(_container.gameObject);
            }

            var safe = overlay.transform.Find("SafeAreaMirror") as RectTransform;
            if (safe == null)
                throw new InvalidOperationException(
                    "Defense HUD requires the canonical SafeAreaMirror.");

            _boundOverlay = overlay;
            _container = NewRect(safe, "DefenseWaveRenderContainer");
            _container.sizeDelta = new Vector2(342f, 58f);

            _background = _container.gameObject.AddComponent<Image>();
            _panelSprite ??= FirstPlayableCombatHudUnityView.RoundedSprite(16f);
            _background.sprite = _panelSprite;
            _background.type = Image.Type.Sliced;
            _background.color = new Color(32f/255f, 37f/255f, 50f/255f, .97f);
            _background.raycastTarget = false;

            _outline = _container.gameObject.AddComponent<Outline>();
            _outline.effectDistance = new Vector2(1f, -1f);
            _outline.useGraphicAlpha = true;

            _stateText = NewText(
                _container, "DefenseState", _medium, 11f,
                new Vector2(16f, -7f), new Vector2(76f, 16f));
            _titleText = NewText(
                _container, "DefenseWaveTitle", _bold, 16f,
                new Vector2(96f, -5f), new Vector2(230f, 20f));
            _detailText = NewText(
                _container, "DefenseWaveDetail", _regular, 12f,
                new Vector2(16f, -27f), new Vector2(310f, 16f));
            _stateText.color = new Color(8f/255f, 127f/255f, 91f/255f);
            _titleText.color = Color.white;
            _detailText.color = new Color(193f/255f, 199f/255f, 208f/255f);

            var bar = NewRect(_container, "GateIntegrityBar");
            bar.anchorMin = bar.anchorMax = new Vector2(0f, 1f);
            bar.pivot = new Vector2(0f, 1f);
            bar.anchoredPosition = new Vector2(16f, -48f);
            bar.sizeDelta = new Vector2(310f, 5f);
            var barImage = bar.gameObject.AddComponent<Image>();
            _barSprite ??= FirstPlayableCombatHudUnityView.RoundedSprite(3f);
            barImage.sprite = _barSprite;
            barImage.type = Image.Type.Sliced;
            barImage.color = new Color(52f/255f, 59f/255f, 75f/255f);
            barImage.raycastTarget = false;

            _gateFillRect = NewRect(bar, "GateIntegrityFill");
            _gateFillRect.anchorMin = _gateFillRect.anchorMax = new Vector2(0f, .5f);
            _gateFillRect.pivot = new Vector2(0f, .5f);
            _gateFillRect.anchoredPosition = Vector2.zero;
            _gateFillRect.sizeDelta = new Vector2(310f, 5f);
            _gateFill = _gateFillRect.gameObject.AddComponent<Image>();
            _gateFill.sprite = _barSprite;
            _gateFill.type = Image.Type.Sliced;
            _gateFill.raycastTarget = false;
        }

        private void ApplyResponsiveGeometry()
        {
            var safe = _boundOverlay.transform.Find("SafeAreaMirror") as RectTransform;
            var protectedMirror = safe == null
                ? null
                : safe.Find("ProtectedCombatReadabilityZoneMirror") as RectTransform;
            if (safe == null || protectedMirror == null)
                throw new InvalidOperationException(
                    "Defense HUD requires the mirrored protected-combat zone.");

            _container.anchorMin = _container.anchorMax =
                new Vector2(.5f, protectedMirror.anchorMax.y);
            _container.pivot = new Vector2(.5f, 0f);
            _container.anchoredPosition = new Vector2(0f, 4f);
            _container.sizeDelta = new Vector2(342f, 58f);
        }

        private void Apply(FirstPlayableDefenseWaveHudState state)
        {
            Color accent;
            switch (state.Phase)
            {
                case DefenseWavePhase.Running:
                    accent = new Color(8f/255f, 127f/255f, 91f/255f);
                    _stateText.text = "RUNNING";
                    _titleText.text = "웨이브 " + state.WaveNumber;
                    _detailText.text =
                        "묘지 " + state.GateIntegrity + " / " + state.GateMaxIntegrity +
                        " · 위협 " + state.ActiveEnemyCount +
                        " · 대기 " + state.RemainingEnemiesToSpawn;
                    break;

                case DefenseWavePhase.Cleared:
                    accent = new Color(49f/255f, 212f/255f, 155f/255f);
                    _stateText.text = "CLEARED";
                    _titleText.text = "웨이브 클리어";
                    _detailText.text =
                        "묘지 " + state.GateIntegrity + " / " + state.GateMaxIntegrity +
                        " · 다음 웨이브 준비";
                    break;

                case DefenseWavePhase.Failed:
                    accent = new Color(201f/255f, 42f/255f, 42f/255f);
                    _stateText.text = "FAILED";
                    _titleText.text = "묘지가 무너졌습니다";
                    _detailText.text =
                        "묘지 " + state.GateIntegrity + " / " + state.GateMaxIntegrity +
                        " · 방어 실패";
                    break;

                default:
                    accent = new Color(127f/255f, 137f/255f, 156f/255f);
                    _stateText.text = "READY";
                    _titleText.text = "웨이브 준비";
                    _detailText.text =
                        "묘지 " + state.GateIntegrity + " / " + state.GateMaxIntegrity;
                    break;
            }

            _stateText.color = accent;
            _outline.effectColor = new Color(accent.r, accent.g, accent.b, .9f);
            _gateFill.color = accent;

            var ratio = state.GateMaxIntegrity <= 0
                ? 0f
                : Mathf.Clamp01((float)state.GateIntegrity / state.GateMaxIntegrity);
            _gateFillRect.sizeDelta = new Vector2(310f * ratio, 5f);
        }

        private static TextMeshProUGUI NewText(
            RectTransform parent,
            string name,
            TMP_FontAsset font,
            float size,
            Vector2 anchoredPosition,
            Vector2 dimensions)
        {
            var rect = NewRect(parent, name);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = dimensions;

            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = size;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            var naturalHeight =
                (font.faceInfo.ascentLine - font.faceInfo.descentLine) *
                size / font.faceInfo.pointSize;
            var leading = Mathf.Max(
                0f,
                naturalHeight - dimensions.y + .1f) / 2f;
            text.margin = new Vector4(0f, -leading, 0f, -leading);
            return text;
        }

        private static RectTransform NewRect(Transform parent, string name)
        {
            var rect = new GameObject(name, typeof(RectTransform))
                .GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private void OnDestroy()
        {
            Release(_panelSprite);
            Release(_barSprite);
        }

        private static void Release(Sprite sprite)
        {
            if (sprite == null)
                return;

            if (Application.isPlaying)
            {
                Destroy(sprite.texture);
                Destroy(sprite);
            }
            else
            {
                DestroyImmediate(sprite.texture);
                DestroyImmediate(sprite);
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException(
                    "Defense HUD runtime binding is not initialized.");
        }
    }
}
