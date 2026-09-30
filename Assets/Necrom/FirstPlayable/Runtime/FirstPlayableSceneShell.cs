using System;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableSceneShell : MonoBehaviour
    {
        public void Configure(Vector2 screenSize, Rect safeAreaPixels, float hudFraction)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(screenSize));
            if (hudFraction <= 0f || hudFraction >= 1f)
                throw new ArgumentOutOfRangeException(nameof(hudFraction));
            if (safeAreaPixels.xMin < 0f || safeAreaPixels.yMin < 0f ||
                safeAreaPixels.xMax > screenSize.x || safeAreaPixels.yMax > screenSize.y)
                throw new ArgumentOutOfRangeException(nameof(safeAreaPixels));

            var root = transform as RectTransform;
            if (root == null)
                throw new InvalidOperationException("FirstPlayableSceneShell requires a RectTransform.");

            var safeArea = GetOrCreateChild(root, "SafeArea");
            SetRegion(
                safeArea,
                new Vector2(safeAreaPixels.xMin / screenSize.x, safeAreaPixels.yMin / screenSize.y),
                new Vector2(safeAreaPixels.xMax / screenSize.x, safeAreaPixels.yMax / screenSize.y));
            var hud = GetOrCreateChild(safeArea, "HudRegion");
            SetRegion(hud, Vector2.zero, new Vector2(1f, hudFraction));

            var combat = GetOrCreateChild(safeArea, "CombatViewport");
            SetRegion(combat, new Vector2(0f, hudFraction), Vector2.one);
        }

        private static RectTransform GetOrCreateChild(RectTransform parent, string childName)
        {
            var existing = parent.Find(childName) as RectTransform;
            if (existing != null) return existing;

            var child = new GameObject(childName, typeof(RectTransform)).GetComponent<RectTransform>();
            child.SetParent(parent, false);
            return child;
        }

        private static void SetRegion(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }
    }
}
