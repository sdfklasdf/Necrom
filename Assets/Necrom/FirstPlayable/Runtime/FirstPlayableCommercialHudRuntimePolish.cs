using System;
using Necrom.Core.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Q3 commercial-presentation layer. It only reshapes the verified HUD projection;
    // gameplay, Raise, Formation, wave and gate truth remain owned by their existing systems.
    [DefaultExecutionOrder(1100)]
    [DisallowMultipleComponent]
    public sealed class FirstPlayableCommercialHudRuntimePolish : MonoBehaviour
    {
        public const string FigmaFileKey = "eXqKU1qHXsn52SJfIGltZo";
        public const string FigmaRunningNodeId = "52:309";
        public const string FigmaClearedNodeId = "52:378";
        public const string FigmaFailedNodeId = "52:447";

        static readonly Color Soul = new Color(8f/255f,127f/255f,91f/255f);
        static readonly Color Danger = new Color(201f/255f,42f/255f,42f/255f);
        static readonly Color Panel = new Color(32f/255f,37f/255f,50f/255f,.96f);
        static readonly Color SlotEmpty = new Color(77f/255f,85f/255f,102f/255f,.75f);

        FirstPlayableCombatHudRuntimeBinding _hud;
        FirstPlayableDefenseWaveRuntimeController _defense;
        GameObject _boundOverlay;
        RectTransform _safe, _dock, _target, _raise, _army, _divider;
        Image _dockImage;
        Outline _dockOutline;
        readonly Image[] _formationPips = new Image[Formation.Capacity];
        Sprite _dockSprite, _pipSprite;

        public bool IsInitialized => _hud != null && _defense != null;
        public GameObject DockRoot => _dock == null ? null : _dock.gameObject;

        public void Initialize(
            FirstPlayableCombatHudRuntimeBinding hud,
            FirstPlayableDefenseWaveRuntimeController defense)
        {
            if (IsInitialized)
                throw new InvalidOperationException("Commercial HUD polish is already initialized.");
            _hud = hud ?? throw new ArgumentNullException(nameof(hud));
            _defense = defense ?? throw new ArgumentNullException(nameof(defense));
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (!IsInitialized || !_hud.IsInitialized || _hud.OverlayHost == null)
                return;
            EnsureView();
            ApplyGeometry();
            ApplyPresentation();
        }

        void LateUpdate() => RefreshNow();

        void EnsureView()
        {
            var overlay = _hud.OverlayHost;
            if (_boundOverlay == overlay && _dock != null)
                return;

            ReleaseView();
            _boundOverlay = overlay;
            _safe = overlay.transform.Find("SafeAreaMirror") as RectTransform;
            if (_safe == null)
                throw new InvalidOperationException("Commercial HUD requires SafeAreaMirror.");

            _target = _safe.Find("TargetRenderContainer") as RectTransform;
            _raise = _safe.Find("RaiseRenderContainer") as RectTransform;
            _army = _safe.Find("ArmyRenderContainer") as RectTransform;
            if (_target == null || _raise == null || _army == null)
                throw new InvalidOperationException("Commercial HUD requires all semantic render containers.");

            // One visually unified command dock sits behind the three semantic containers.
            _dock = NewRect(_safe, "CommercialCommandDockBackground");
            _dockImage = _dock.gameObject.AddComponent<Image>();
            _dockSprite ??= FirstPlayableCombatHudUnityView.RoundedSprite(20f);
            _dockImage.sprite = _dockSprite;
            _dockImage.type = Image.Type.Sliced;
            _dockImage.color = Panel;
            _dockImage.raycastTarget = false;
            _dockOutline = _dock.gameObject.AddComponent<Outline>();
            _dockOutline.effectDistance = new Vector2(1f,-1f);
            _dockOutline.useGraphicAlpha = true;
            _dock.SetSiblingIndex(0);

            _divider = NewRect(_safe, "CommercialCommandDockDivider");
            var dividerImage = _divider.gameObject.AddComponent<Image>();
            dividerImage.color = new Color(70f/255f,78f/255f,97f/255f,.82f);
            dividerImage.raycastTarget = false;

            PrepareSection(_target);
            PrepareSection(_raise);
            PrepareSection(_army);

            for (var i = 0; i < Formation.Capacity; i++)
            {
                var pip = NewRect(_army, "FormationPip" + i);
                var image = pip.gameObject.AddComponent<Image>();
                _pipSprite ??= FirstPlayableCombatHudUnityView.RoundedSprite(6f);
                image.sprite = _pipSprite;
                image.type = Image.Type.Sliced;
                image.raycastTarget = false;
                _formationPips[i] = image;
            }
        }

        static void PrepareSection(RectTransform section)
        {
            var image = section.GetComponent<Image>();
            if (image != null) image.color = Color.clear;
            var layout = section.GetComponent<VerticalLayoutGroup>();
            if (layout != null) layout.enabled = false;
            foreach (var element in section.GetComponentsInChildren<LayoutElement>(true))
                element.enabled = false;
        }

        void ApplyGeometry()
        {
            // Matches Figma 52:309/378/447. Fixed logical dimensions remain centered
            // inside the existing responsive SafeArea; source readability zones remain untouched.
            SetBottomRect(_dock, -171f, 16f, 342f, 206f);
            SetBottomRect(_divider, -155f, 126f, 310f, 1f);
            SetBottomRect(_target, -155f, 130f, 205f, 78f);
            SetBottomRect(_army, 64f, 130f, 91f, 78f);
            SetBottomRect(_raise, -155f, 34f, 310f, 82f);

            StyleSection(_target, 190f, 205f, false);
            StyleSection(_raise, 190f, 205f, false);
            StyleSection(_army, 91f, 91f, true);

            var cta = _raise.Find("PrimaryCta") as RectTransform;
            if (cta == null)
                throw new InvalidOperationException("Commercial HUD lost canonical Raise CTA.");
            cta.anchorMin = cta.anchorMax = new Vector2(1f,1f);
            cta.pivot = new Vector2(1f,1f);
            cta.anchoredPosition = new Vector2(0f,-6f);
            cta.sizeDelta = new Vector2(100f,58f);

            for (var i = 0; i < _formationPips.Length; i++)
            {
                var rect = _formationPips[i].rectTransform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f,1f);
                rect.pivot = new Vector2(0f,1f);
                rect.anchoredPosition = new Vector2(2f + i*18f,-57f);
                rect.sizeDelta = new Vector2(12f,12f);
            }
        }

        static void StyleSection(
            RectTransform section,
            float primaryWidth,
            float secondaryWidth,
            bool rightAligned)
        {
            var state = RequireText(section, "StateKey");
            var primary = RequireText(section, "PrimaryText");
            var secondary = RequireText(section, "SecondaryText");
            var accent = section.Find("StateAccent") as RectTransform;

            state.margin = Vector4.zero;
            primary.margin = Vector4.zero;
            secondary.margin = Vector4.zero;
            state.fontSize = 11f;
            primary.fontSize = rightAligned ? 18f : section.name.StartsWith("Raise") ? 14f : 18f;
            secondary.fontSize = section.name.StartsWith("Raise") ? 11f : 12f;
            state.alignment = rightAligned ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
            primary.alignment = rightAligned ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
            secondary.alignment = rightAligned ? TextAlignmentOptions.TopRight : TextAlignmentOptions.TopLeft;
            secondary.gameObject.SetActive(!rightAligned);

            SetTopLeft(state.rectTransform, rightAligned ? 25f : 26f, 2f,
                rightAligned ? 66f : 54f, 18f);
            SetTopLeft(primary.rectTransform, 0f, 27f, primaryWidth, 28f);
            SetTopLeft(secondary.rectTransform, 0f, 54f, secondaryWidth, 18f);
            if (accent != null)
            {
                accent.anchorMin = accent.anchorMax = new Vector2(0f,1f);
                accent.pivot = new Vector2(0f,1f);
                accent.anchoredPosition = new Vector2(rightAligned ? 0f : 0f,-3f);
                accent.sizeDelta = new Vector2(18f,18f);
            }
        }

        void ApplyPresentation()
        {
            var presentation = _hud.LastPresentation;
            if (presentation == null)
                return;

            for (var i = 0; i < _formationPips.Length; i++)
            {
                var slot = presentation.Army.FormationSlots[i];
                var owned = slot != null && slot.OwnedUnitId.HasValue;
                _formationPips[i].color = owned ? Soul : SlotEmpty;
            }

            var failed = _defense.Phase == DefenseWavePhase.Failed;
            _dockOutline.effectColor = failed
                ? new Color(Danger.r,Danger.g,Danger.b,.92f)
                : new Color(Soul.r,Soul.g,Soul.b,.72f);
        }

        static TextMeshProUGUI RequireText(Transform root, string name)
        {
            var child = root.Find(name);
            var text = child == null ? null : child.GetComponent<TextMeshProUGUI>();
            if (text == null)
                throw new InvalidOperationException("Commercial HUD missing text node: " + name);
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            return text;
        }

        static void SetBottomRect(
            RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(.5f,0f);
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x,y);
            rect.sizeDelta = new Vector2(width,height);
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }

        static void SetTopLeft(
            RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f,1f);
            rect.pivot = new Vector2(0f,1f);
            rect.anchoredPosition = new Vector2(x,-y);
            rect.sizeDelta = new Vector2(width,height);
        }

        static RectTransform NewRect(Transform parent, string name)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false);
            return rect;
        }

        void ReleaseView()
        {
            if (_dock != null) DestroyOwned(_dock.gameObject);
            if (_divider != null) DestroyOwned(_divider.gameObject);
            _dock = _divider = null;
            Array.Clear(_formationPips,0,_formationPips.Length);
        }

        static void DestroyOwned(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        void OnDestroy()
        {
            ReleaseView();
            ReleaseSprite(_dockSprite);
            ReleaseSprite(_pipSprite);
        }

        static void ReleaseSprite(Sprite sprite)
        {
            if (sprite == null) return;
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
    }
}
