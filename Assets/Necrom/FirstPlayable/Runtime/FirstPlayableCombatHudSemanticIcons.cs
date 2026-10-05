using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    internal enum FirstPlayableCombatHudSemanticIconKind
    {
        Combat,
        Soul,
        Army
    }

    internal static class FirstPlayableCombatHudSemanticIcons
    {
        const int Size = 32;

        public static Sprite Create(FirstPlayableCombatHudSemanticIconKind kind)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.name = "HUD semantic icon texture " + kind;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            var pixels = new Color[Size * Size];
            switch (kind)
            {
                case FirstPlayableCombatHudSemanticIconKind.Combat:
                    DrawCombat(pixels);
                    break;
                case FirstPlayableCombatHudSemanticIconKind.Soul:
                    DrawSoul(pixels);
                    break;
                default:
                    DrawArmy(pixels);
                    break;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(.5f, .5f), Size);
            sprite.name = "HUD Icon " + kind;
            return sprite;
        }

        static void DrawCombat(Color[] pixels)
        {
            DrawLine(pixels, 7, 24, 23, 8, 2.2f);
            DrawLine(pixels, 9, 7, 25, 23, 2.2f);
            DrawLine(pixels, 8, 19, 13, 24, 1.8f);
            DrawLine(pixels, 18, 8, 23, 13, 1.8f);
        }

        static void DrawSoul(Color[] pixels)
        {
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var dx = Mathf.Abs(x - 15.5f);
                var dy = Mathf.Abs(y - 15.5f);
                var diamond = dx + dy;
                var outline = diamond >= 8.5f && diamond <= 11.5f;
                var core = (x - 15.5f) * (x - 15.5f) + (y - 15.5f) * (y - 15.5f) <= 5.5f;
                if (outline || core) Set(pixels, x, y);
            }
        }

        static void DrawArmy(Color[] pixels)
        {
            DrawDisc(pixels, 9, 19, 3.2f);
            DrawDisc(pixels, 16, 22, 3.6f);
            DrawDisc(pixels, 23, 19, 3.2f);
            DrawLine(pixels, 5, 10, 13, 10, 3.2f);
            DrawLine(pixels, 11, 12, 21, 12, 3.5f);
            DrawLine(pixels, 19, 10, 27, 10, 3.2f);
        }

        static void DrawDisc(Color[] pixels, float cx, float cy, float radius)
        {
            var rr = radius * radius;
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var dx = x - cx;
                var dy = y - cy;
                if (dx * dx + dy * dy <= rr) Set(pixels, x, y);
            }
        }

        static void DrawLine(Color[] pixels, float x0, float y0, float x1, float y1, float radius)
        {
            var a = new Vector2(x0, y0);
            var b = new Vector2(x1, y1);
            var ab = b - a;
            var denom = Mathf.Max(.0001f, ab.sqrMagnitude);
            for (var y = 0; y < Size; y++)
            for (var x = 0; x < Size; x++)
            {
                var p = new Vector2(x + .5f, y + .5f);
                var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / denom);
                var q = a + ab * t;
                if (Vector2.Distance(p, q) <= radius) Set(pixels, x, y);
            }
        }

        static void Set(Color[] pixels, int x, int y)
        {
            if (x < 0 || x >= Size || y < 0 || y >= Size) return;
            pixels[y * Size + x] = Color.white;
        }
    }
}
