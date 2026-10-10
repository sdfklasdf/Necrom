using System;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Separate presentation surface; account progression and combat remain unchanged.
    [DefaultExecutionOrder(1200)]
    [DisallowMultipleComponent]
    public sealed class AccountExperienceBarUI : MonoBehaviour
    {
        private LevelManager level;
        private Necrom.Core.Domain.LocalizationManager localization;
        private Canvas canvas;
        private CanvasScaler scaler;
        private RectTransform safeRoot, panel;
        private Slider slider;
        private Text label;
        private readonly RectTransform[] obstacles = new RectTransform[4];
        private readonly Vector3[] corners = new Vector3[4];
        private static readonly string[] ObstacleNames =
            { "DefenseWaveRenderContainer", "OpenGacha", "OpenDeckFormation", "OpenSkillTree" };

        public void Initialize(LevelManager manager)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (level != null) level.Changed -= Refresh;
            if(localization!=null)localization.LanguageChanged-=Refresh;
            localization=LocalizationRuntime.Manager;
            if(isActiveAndEnabled)localization.LanguageChanged+=Refresh;
            level = manager;
            if (canvas == null) Build();
            level.Changed += Refresh;
            Refresh();
        }

        private void Build()
        {
            // Root Canvas owns its scale. Safe-area anchors belong to its child, not the root Canvas.
            var root = new GameObject("AccountExpCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 29990; // Above HUD; below all full-screen modal presenters (30000+).
            scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            safeRoot = NewRect(root.transform, "AccountExpSafeArea");
            Stretch(safeRoot);
            panel = NewRect(safeRoot, "AccountExpPanel");
            panel.anchorMin = panel.anchorMax = new Vector2(0, 1);
            panel.pivot = new Vector2(0, 1);
            var surface = panel.gameObject.AddComponent<Image>();
            surface.color = new Color(.91f, .95f, 1f, 1f);
            surface.raycastTarget = false;

            var lr = NewRect(panel, "AccountExpLabel");
            lr.anchorMin = new Vector2(0, 1); lr.anchorMax = Vector2.one;
            lr.pivot = new Vector2(.5f, 1);
            lr.anchoredPosition = new Vector2(0, -4);
            lr.sizeDelta = new Vector2(-16, 24);
            label = lr.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 16; label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 12; label.resizeTextMaxSize = 16;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(56f/255f, 51f/255f, 79f/255f);
            label.raycastTarget = false;

            var track = NewRect(panel, "AccountExpSlider");
            track.anchorMin = Vector2.zero; track.anchorMax = new Vector2(1, 0);
            track.pivot = new Vector2(.5f, 0);
            track.anchoredPosition = new Vector2(0, 8);
            track.sizeDelta = new Vector2(-20, 8);
            slider = track.gameObject.AddComponent<Slider>();
            slider.minValue = 0; slider.maxValue = 1; slider.interactable = false;
            var bg = NewRect(track, "Background"); Stretch(bg);
            var background = bg.gameObject.AddComponent<Image>();
            background.color = new Color(.77f, .81f, .90f);
            background.raycastTarget = false;
            var fillArea = NewRect(track, "FillArea"); Stretch(fillArea);
            var fill = NewRect(fillArea, "Fill"); Stretch(fill);
            var image = fill.gameObject.AddComponent<Image>();
            image.color = new Color(.18f, .60f, .88f); image.raycastTarget = false;
            slider.fillRect = fill; slider.targetGraphic = background;
            ApplyLayout(Screen.safeArea);
            Debug.Log("[EXP-UI] Readable safe-area EXP panel created; sorting=29990.");
        }

        private void LateUpdate()
        {
            if (canvas == null || Screen.width <= 0 || Screen.height <= 0) return;
            // HUD is rebuilt/repositioned after viewport changes; observe actual screen-space bounds.
            for (int i = 0; i < obstacles.Length; i++)
                if (obstacles[i] == null)
                {
                    var found = GameObject.Find(ObstacleNames[i]);
                    if (found != null) obstacles[i] = found.GetComponent<RectTransform>();
                }
            ApplyLayout(Screen.safeArea);
        }

        private void ApplyLayout(Rect safe)
        {
            if (safeRoot == null || safe.width <= 0 || safe.height <= 0 || Screen.width <= 0 || Screen.height <= 0) return;
            float scale = Mathf.Min(safe.width / 390f, safe.height / 776f);
            scale = Mathf.Max(.01f, scale);
            scaler.scaleFactor = scale;
            canvas.scaleFactor = scale;
            safeRoot.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;

            float left = safe.xMin + 24 * scale;
            float right = Mathf.Min(safe.xMax - 24 * scale, left + 342 * scale);
            float top = safe.yMax - 8 * scale;
            const float height = 44;
            if (obstacles[0] != null && obstacles[0].gameObject.activeInHierarchy)
                top = Mathf.Min(top, Bounds(obstacles[0]).yMin - 8 * scale);
            // Reserve the navigation column only when it intersects this row.
            for (int i = 1; i < obstacles.Length; i++)
                if (obstacles[i] != null && obstacles[i].gameObject.activeInHierarchy)
                {
                    Rect ob = Bounds(obstacles[i]);
                    if (ob.yMin < top && ob.yMax > top - height * scale && ob.xMax > left)
                        right = Mathf.Min(right, ob.xMin - 8 * scale);
                }
            if (right - left < 160 * scale)
            {
                top = safe.yMax - 8 * scale;
                foreach (var ob in obstacles)
                    if (ob != null && ob.gameObject.activeInHierarchy)
                        top = Mathf.Min(top, Bounds(ob).yMin - 8 * scale);
                right = Mathf.Min(safe.xMax - 24 * scale, left + 342 * scale);
            }
            top = Mathf.Clamp(top, safe.yMin + (height + 8) * scale, safe.yMax - 8 * scale);
            panel.anchoredPosition = new Vector2((left-safe.xMin)/scale, -(safe.yMax-top)/scale);
            panel.sizeDelta = new Vector2((right-left)/scale, height);
        }

        private Rect Bounds(RectTransform rect)
        {
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
        private static RectTransform NewRect(Transform parent, string name)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); return rect;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private void Refresh()
        {
            if (level == null || slider == null) return;
            slider.value = level.Progress;
            label.text = localization.Format("ui.account.exp",level.Level,level.Progress*100f);
        }
        private void OnEnable() { if(canvas!=null)canvas.enabled=true; if(localization!=null){localization.LanguageChanged-=Refresh;localization.LanguageChanged+=Refresh;Refresh();} }
        private void OnDisable() { if(canvas!=null)canvas.enabled=false; if(localization!=null)localization.LanguageChanged-=Refresh; }
        private void OnDestroy()
        {
            if (level != null) level.Changed -= Refresh;
            if(localization!=null)localization.LanguageChanged-=Refresh;
            if (canvas != null) Destroy(canvas.gameObject);
        }
    }
}
