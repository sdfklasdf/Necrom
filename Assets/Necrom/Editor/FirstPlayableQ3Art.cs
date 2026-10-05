using System;
using System.IO;
using System.Linq;
using Necrom.FirstPlayable.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Necrom.EditorTools
{
    public static class FirstPlayableQ3Art
    {
        const string ArtRootPath = "Assets/Necrom/FirstPlayable/Art/Q3/";
        const string ArtPath = ArtRootPath + "ProductionCandidate/";
        const string AudioPath = "Assets/Necrom/FirstPlayable/Audio/Q3Review/";
        const string ProfilePath = ArtRootPath + "FirstPlayablePresentationProfile.asset";
        const string ProductionFontRoot = "Assets/Necrom/FirstPlayable/Fonts/Production/";
        const string FontPath = ProductionFontRoot + "NotoSansKR-Regular SDF.asset";
        const string FontMediumPath = ProductionFontRoot + "NotoSansKR-Medium SDF.asset";
        const string FontBoldPath = ProductionFontRoot + "NotoSansKR-Bold SDF.asset";

        [MenuItem("Necrom/Q3/Apply approved Obsidian Soul production-candidate art")]
        public static void Apply()
        {
            AssetDatabase.Refresh();

            foreach (var name in new[] { "player", "guard", "raised", "background" })
            {
                var path = ArtPath + name + ".png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = name != "background";
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            var font = EnsureFontAsset(
                ProductionFontRoot + "NotoSansKR-Regular.otf", FontPath, "Regular");
            var mediumFont = EnsureFontAsset(
                ProductionFontRoot + "NotoSansKR-Medium.otf", FontMediumPath, "Medium");
            var boldFont = EnsureFontAsset(
                ProductionFontRoot + "NotoSansKR-Bold.otf", FontBoldPath, "Bold");

            var profile = EnsurePresentationProfile();

            var scene = EditorSceneManager.OpenScene(FirstPlayableCanonicalScene.ScenePath, OpenSceneMode.Single);
            var root = scene.GetRootGameObjects().Single(x => x.name == "FirstPlayableCanonical");
            var camera = scene.GetRootGameObjects().Select(x => x.GetComponent<Camera>()).Single(x => x != null);
            if (camera.GetComponent<AudioListener>() == null)
                camera.gameObject.AddComponent<AudioListener>();
            if (scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<AudioListener>(true)).Count() != 1)
                throw new Exception("Q3 review scene must contain exactly one AudioListener for actual SFX playback.");
            var composition = root.GetComponent<FirstPlayableGameplayComposition>();
            composition.ReviewFont = font;
            composition.ReviewFontMedium = mediumFont;
            composition.ReviewFontBold = boldFont;

            var visual = root.GetComponent<FirstPlayableVisualPresentation>() ??
                         root.AddComponent<FirstPlayableVisualPresentation>();
            visual.NecromancerArt = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath + "player.png");
            visual.GuardArt = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath + "guard.png");
            visual.RaisedGuardArt = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath + "raised.png");
            visual.BackgroundArt = AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath + "background.png");
            visual.AttackLunge = new AnimationCurve(profile.AttackLunge.keys);
            visual.HitFlash = new AnimationCurve(profile.HitFlash.keys);
            visual.DefeatScaleY = new AnimationCurve(profile.DefeatScaleY.keys);
            visual.RaiseScale = new AnimationCurve(profile.RaiseScale.keys);
            visual.AlliedContributionScale = new AnimationCurve(profile.AlliedContributionScale.keys);
            visual.AttackDuration = profile.AttackDuration;
            visual.HitDuration = profile.HitDuration;
            visual.DefeatDuration = profile.DefeatDuration;
            visual.RaiseDuration = profile.RaiseDuration;
            visual.AlliedContributionDuration = profile.AlliedContributionDuration;
            visual.ReviewSfxVolume = profile.ReviewSfxVolume;
            visual.PlayerAttackSfx = profile.PlayerAttackSfx;
            visual.HitSfx = profile.HitSfx;
            visual.DefeatSfx = profile.DefeatSfx;
            visual.RaiseSfx = profile.RaiseSfx;
            visual.AlliedContributionSfx = profile.AlliedContributionSfx;
            EditorUtility.SetDirty(visual);

            if (visual.LoadedArtCount != 4)
                throw new Exception("Four Q3 art references must resolve.");
            if (!visual.HasAuthoredMotion || visual.LoadedAudioClipCount != 5)
                throw new Exception("Q3 authored motion/audio values failed scene serialization input.");

            SaveCanonicalWithReplaceFallback(root, scene);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Readback();
        }

        static void SaveCanonicalWithReplaceFallback(GameObject root, UnityEngine.SceneManagement.Scene scene)
        {
            Directory.CreateDirectory("Artifacts/Q3-save-fallback");
            var prefabBackup = "Artifacts/Q3-save-fallback/FirstPlayable.prefab.pre-write";
            var sceneBackup = "Artifacts/Q3-save-fallback/FirstPlayable.unity.pre-write";
            File.Copy(FirstPlayableCanonicalScene.PrefabPath, prefabBackup, true);
            File.Copy(FirstPlayableCanonicalScene.ScenePath, sceneBackup, true);

            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, FirstPlayableCanonicalScene.PrefabPath, out var prefabSaved);
                if (!prefabSaved)
                {
                    File.Delete(FirstPlayableCanonicalScene.PrefabPath);
                    PrefabUtility.SaveAsPrefabAsset(root, FirstPlayableCanonicalScene.PrefabPath, out prefabSaved);
                }
                if (!prefabSaved)
                    throw new Exception("Q3 prefab save failed after atomic-replace fallback.");

                if (!EditorSceneManager.SaveScene(scene))
                {
                    File.Delete(FirstPlayableCanonicalScene.ScenePath);
                    if (!EditorSceneManager.SaveScene(scene, FirstPlayableCanonicalScene.ScenePath))
                        throw new Exception("Q3 scene save failed after atomic-replace fallback.");
                }
            }
            catch
            {
                if (!File.Exists(FirstPlayableCanonicalScene.PrefabPath))
                    File.Copy(prefabBackup, FirstPlayableCanonicalScene.PrefabPath, true);
                if (!File.Exists(FirstPlayableCanonicalScene.ScenePath))
                    File.Copy(sceneBackup, FirstPlayableCanonicalScene.ScenePath, true);
                throw;
            }
        }

        static TMP_FontAsset EnsureFontAsset(string sourcePath, string assetPath, string weightLabel)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;

            var source = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
            if (source == null)
                throw new Exception("Noto production font source not imported: " + sourcePath);

            var font = TMP_FontAsset.CreateFontAsset(
                source, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic, true);
            if (font == null)
                throw new Exception("Noto production SDF creation failed: " + weightLabel);

            AssetDatabase.CreateAsset(font, assetPath);
            foreach (var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture, font);
            AssetDatabase.AddObjectToAsset(font.material, font);

            var copy = File.ReadAllText("Assets/Necrom/FirstPlayable/Runtime/FirstPlayableGameplayComposition.cs");
            var chars = new string(copy.Where(c => c >= ' ' && c != '\r' && c != '\n').Distinct().ToArray());
            if (!font.TryAddCharacters(chars, out var missing))
                Debug.LogWarning("Q3_PRODUCTION_FONT_MISSING " + weightLabel + " " + missing);

            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            var reloaded = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (reloaded == null || reloaded.sourceFontFile == null)
                throw new Exception("Noto production font failed persisted readback: " + weightLabel);
            return reloaded;
        }

        static FirstPlayablePresentationProfile EnsurePresentationProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<FirstPlayablePresentationProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<FirstPlayablePresentationProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            profile.AttackDuration = .18f;
            profile.HitDuration = .12f;
            profile.DefeatDuration = .34f;
            profile.RaiseDuration = .42f;
            profile.AlliedContributionDuration = .24f;
            profile.ReviewSfxVolume = .28f;

            profile.AttackLunge = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(.30f, 1f),
                new Keyframe(1f, 0f));
            profile.HitFlash = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(.35f, .72f),
                new Keyframe(1f, 0f));
            profile.DefeatScaleY = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(.55f, .52f),
                new Keyframe(1f, .35f));
            profile.RaiseScale = new AnimationCurve(
                new Keyframe(0f, .78f),
                new Keyframe(.34f, 1.18f),
                new Keyframe(1f, 1f));
            profile.AlliedContributionScale = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(.30f, 1.16f),
                new Keyframe(1f, 1f));

            profile.PlayerAttackSfx = LoadClip("attack-review.wav");
            profile.HitSfx = LoadClip("hit-review.wav");
            profile.DefeatSfx = LoadClip("defeat-review.wav");
            profile.RaiseSfx = LoadClip("raise-review.wav");
            profile.AlliedContributionSfx = LoadClip("ally-contribution-review.wav");

            if (!profile.HasAuthoredMotion || profile.LoadedAudioClipCount != 5)
                throw new Exception("Q3 presentation profile incomplete after authoring.");

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            profile = AssetDatabase.LoadAssetAtPath<FirstPlayablePresentationProfile>(ProfilePath);
            if (profile == null || !profile.HasAuthoredMotion || profile.LoadedAudioClipCount != 5)
                throw new Exception("Q3 presentation profile failed persisted-asset readback.");
            return profile;
        }

        static AudioClip LoadClip(string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath + name);
            if (clip == null) throw new Exception("Q3 review SFX not imported: " + name);
            return clip;
        }

        public static void Readback()
        {
            FirstPlayableCanonicalScene.Readback();
            var root = UnityEngine.Object.FindObjectsByType<FirstPlayableGameplayComposition>(FindObjectsSortMode.None).Single();
            var visual = root.GetComponent<FirstPlayableVisualPresentation>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FirstPlayableCanonicalScene.PrefabPath);
            var prefabVisual = prefab.GetComponent<FirstPlayableVisualPresentation>();

            if (visual == null || visual.LoadedArtCount != 4 ||
                prefabVisual == null || prefabVisual.LoadedArtCount != 4)
                throw new Exception("Q3 scene/prefab lost art references.");

            foreach (var texture in new[]
                     {
                         visual.NecromancerArt, visual.GuardArt,
                         visual.RaisedGuardArt, visual.BackgroundArt
                     })
            {
                var path = AssetDatabase.GetAssetPath(texture);
                if (string.IsNullOrEmpty(path) || !path.StartsWith(ArtPath, StringComparison.Ordinal))
                    throw new Exception("Q3 canonical art is not bound to the approved ProductionCandidate path: " + path);
            }
            if (root.ReviewFont == null || root.ReviewFont.sourceFontFile == null ||
                root.ReviewFontMedium == null || root.ReviewFontMedium.sourceFontFile == null ||
                root.ReviewFontBold == null || root.ReviewFontBold.sourceFontFile == null)
                throw new Exception("Q3 production Regular/Medium/Bold font set unresolved.");

            foreach (var font in new[] { root.ReviewFont, root.ReviewFontMedium, root.ReviewFontBold })
            {
                var sourcePath = AssetDatabase.GetAssetPath(font.sourceFontFile);
                if (string.IsNullOrEmpty(sourcePath) ||
                    !sourcePath.StartsWith(ProductionFontRoot, StringComparison.Ordinal))
                    throw new Exception("Q3 production font source escaped approved Production path: " + sourcePath);
            }
            if (!visual.HasAuthoredMotion || visual.LoadedAudioClipCount != 5 ||
                !prefabVisual.HasAuthoredMotion || prefabVisual.LoadedAudioClipCount != 5)
                throw new Exception("Q3 scene/prefab lost motion/audio profile.");

            File.WriteAllText(
                "Artifacts/Q3-presentation-readback.txt",
                "Q3_PRESENTATION_REOPEN_PASS\n" +
                "Scene: " + FirstPlayableCanonicalScene.ScenePath +
                "\nPrefab: " + FirstPlayableCanonicalScene.PrefabPath +
                "\nArt references: 4 / 4" +
                "\nMotion profile: AUTHORED_CURVES_5" +
                "\nAudio clips: 5 / 5" +
                "\nFonts: " + root.ReviewFont.name + " / " +
                    root.ReviewFontMedium.name + " / " + root.ReviewFontBold.name +
                "\nSource fonts: " + root.ReviewFont.sourceFontFile.name + " / " +
                    root.ReviewFontMedium.sourceFontFile.name + " / " + root.ReviewFontBold.sourceFontFile.name +
                "\nOBSIDIAN_SOUL production-candidate art bound." +
                "\nFinal release-rights / production sound / physical-device acceptance NOT RUN.");

            Debug.Log("Q3_PRESENTATION_REOPEN_PASS art=4 motion=5 audio=5");
        }
    }
}
