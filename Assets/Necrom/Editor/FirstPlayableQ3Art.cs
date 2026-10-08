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
        const string ForestFriendPath = ArtRootPath + "ForestFriends/";
        const string CloudKnightPath = ArtRootPath + "CloudKnights/";
        const string AudioPath = "Assets/Necrom/FirstPlayable/Audio/Q3ProductionCandidate/";
        const string ProfilePath = ArtRootPath + "FirstPlayablePresentationProfile.asset";
        const string ProductionFontRoot = "Assets/Necrom/FirstPlayable/Fonts/Production/";
        const string FontPath = ProductionFontRoot + "NotoSansKR-Regular SDF.asset";
        const string FontMediumPath = ProductionFontRoot + "NotoSansKR-Medium SDF.asset";
        const string FontBoldPath = ProductionFontRoot + "NotoSansKR-Bold SDF.asset";

        [MenuItem("Necrom/Q3/Apply active Cute Necro production-candidate art")]
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

            foreach (var path in new[]
                     {
                         ForestFriendPath + "forest_chr_001.png",
                         ForestFriendPath + "forest_chr_002.png",
                         ForestFriendPath + "forest_chr_003.png",
                         ForestFriendPath + "forest_chr_004.png",
                         ForestFriendPath + "forest_chr_005.png",
                         ForestFriendPath + "forest_chr_006.png",
                         ForestFriendPath + "forest_chr_007.png",
                         ForestFriendPath + "forest_chr_008.png",
                         ForestFriendPath + "forest_chr_009.png",
                         ForestFriendPath + "forest_chr_010.png",
                         CloudKnightPath + "cloud_chr_011.png",
                         CloudKnightPath + "cloud_chr_012.png",
                         CloudKnightPath + "cloud_chr_013.png",
                         CloudKnightPath + "cloud_chr_014.png",
                         CloudKnightPath + "cloud_chr_015.png",
                         CloudKnightPath + "cloud_chr_016.png",
                         CloudKnightPath + "cloud_chr_017.png"
                     })
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                if (importer == null)
                    throw new Exception("Character production art missing: " + path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 512;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            var font = EnsureFontAsset(
                ProductionFontRoot + "NotoSansKR-Regular.otf", FontPath, "Regular");
            var mediumFont = EnsureFontAsset(
                ProductionFontRoot + "NotoSansKR-Medium.otf", FontMediumPath, "Medium");
            var boldFont = EnsureFontAsset(
                ProductionFontRoot + "NotoSansKR-Bold.otf", FontBoldPath, "Bold");

            ConfigureProductionAudioImport();
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
            visual.CharacterVisuals = new[]
            {
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr001",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_001.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.LeafBarrier,
                    Accent = new Color(.49f, .78f, .36f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr002",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_002.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.PineSlash,
                    Accent = new Color(1f, .48f, .47f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr003",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_003.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.StarArrow,
                    Accent = new Color(.32f, .67f, 1f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr004",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_004.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.CloverWind,
                    Accent = new Color(.31f, .83f, .68f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr005",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_005.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.ChestnutStar,
                    Accent = new Color(.62f, .42f, .95f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr006",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_006.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.DewHeal,
                    Accent = new Color(.32f, .67f, 1f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr007",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_007.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.BerryBurst,
                    Accent = new Color(1f, .35f, .42f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr008",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_008.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.GrowthRingBarrier,
                    Accent = new Color(.78f, .48f, .20f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr009",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_009.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.SeedlingCall,
                    Accent = new Color(.31f, .83f, .47f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "forest.chr010",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        ForestFriendPath + "forest_chr_010.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.FireflyTrail,
                    Accent = new Color(.62f, .42f, .95f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "cloud.chr011",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        CloudKnightPath + "cloud_chr_011.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.SkyBulwark,
                    Accent = new Color(.36f, .68f, 1f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "cloud.chr012",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        CloudKnightPath + "cloud_chr_012.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.BoltLance,
                    Accent = new Color(.54f, .47f, .96f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "cloud.chr013",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        CloudKnightPath + "cloud_chr_013.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.Rainbolt,
                    Accent = new Color(.31f, .65f, .92f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "cloud.chr014",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        CloudKnightPath + "cloud_chr_014.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.BellGust,
                    Accent = new Color(.29f, .81f, .73f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "cloud.chr015",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        CloudKnightPath + "cloud_chr_015.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.SunsetSigil,
                    Accent = new Color(.92f, .45f, .66f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "cloud.chr016",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        CloudKnightPath + "cloud_chr_016.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.MistMend,
                    Accent = new Color(.47f, .72f, .92f)
                },
                new FirstPlayableCharacterVisualBinding
                {
                    ArchetypeId = "cloud.chr017",
                    Art = AssetDatabase.LoadAssetAtPath<Texture2D>(
                        CloudKnightPath + "cloud_chr_017.png"),
                    VfxIdentity = FirstPlayableCharacterVfxIdentity.HailPop,
                    Accent = new Color(.43f, .55f, .93f)
                }
            };
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
            visual.SfxMasterVolume = profile.SfxMasterVolume;
            visual.PlayerAttackSfxGain = profile.PlayerAttackSfxGain;
            visual.HitSfxGain = profile.HitSfxGain;
            visual.DefeatSfxGain = profile.DefeatSfxGain;
            visual.RaiseSfxGain = profile.RaiseSfxGain;
            visual.AlliedContributionSfxGain = profile.AlliedContributionSfxGain;
            visual.PlayerAttackSfx = profile.PlayerAttackSfx;
            visual.HitSfx = profile.HitSfx;
            visual.DefeatSfx = profile.DefeatSfx;
            visual.RaiseSfx = profile.RaiseSfx;
            visual.AlliedContributionSfx = profile.AlliedContributionSfx;
            EditorUtility.SetDirty(visual);

            if (visual.LoadedArtCount != 4)
                throw new Exception("Four Q3 base art references must resolve.");
            if (visual.LoadedCharacterArtCount != 17)
                throw new Exception("Seventeen implemented character art references must resolve.");
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

        static void ConfigureProductionAudioImport()
        {
            foreach (var name in new[]
                     {
                         "attack-production.mp3",
                         "hit-production.mp3",
                         "defeat-production.mp3",
                         "raise-production.mp3",
                         "ally-contribution-production.mp3"
                     })
            {
                var path = AudioPath + name;
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                    throw new Exception("Q3 production SFX source not imported: " + path);

                importer.forceToMono = true;
                importer.loadInBackground = false;
                var settings = importer.defaultSampleSettings;
                settings.preloadAudioData = true;
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.PCM;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                settings.quality = 1f;
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }

        static FirstPlayablePresentationProfile EnsurePresentationProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<FirstPlayablePresentationProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<FirstPlayablePresentationProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            profile.AttackDuration = .24f;
            profile.HitDuration = .16f;
            profile.DefeatDuration = .50f;
            profile.RaiseDuration = .66f;
            profile.AlliedContributionDuration = .20f;
            profile.SfxMasterVolume = .34f;
            profile.PlayerAttackSfxGain = .72f;
            profile.HitSfxGain = .48f;
            profile.DefeatSfxGain = .82f;
            profile.RaiseSfxGain = 1.00f;
            profile.AlliedContributionSfxGain = .38f;

            // Production-candidate timing: readable anticipation, decisive snap, weighted collapse,
            // signature Raise reform and a short fatigue-safe local ally pulse.
            profile.AttackLunge = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(.12f, -.18f),
                new Keyframe(.42f, 1.12f),
                new Keyframe(.68f, .30f),
                new Keyframe(1f, 0f));
            profile.HitFlash = new AnimationCurve(
                new Keyframe(0f, .20f),
                new Keyframe(.08f, 1f),
                new Keyframe(.35f, .55f),
                new Keyframe(1f, 0f));
            profile.DefeatScaleY = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(.18f, .96f),
                new Keyframe(.55f, .46f),
                new Keyframe(.82f, .28f),
                new Keyframe(1f, .35f));
            profile.RaiseScale = new AnimationCurve(
                new Keyframe(0f, .62f),
                new Keyframe(.18f, .72f),
                new Keyframe(.48f, 1.24f),
                new Keyframe(.72f, .96f),
                new Keyframe(1f, 1f));
            profile.AlliedContributionScale = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(.18f, 1.12f),
                new Keyframe(.48f, .97f),
                new Keyframe(1f, 1f));

            profile.PlayerAttackSfx = LoadClip("attack-production.mp3");
            profile.HitSfx = LoadClip("hit-production.mp3");
            profile.DefeatSfx = LoadClip("defeat-production.mp3");
            profile.RaiseSfx = LoadClip("raise-production.mp3");
            profile.AlliedContributionSfx = LoadClip("ally-contribution-production.mp3");

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
            if (clip == null) throw new Exception("Q3 production-candidate SFX not imported: " + name);
            return clip;
        }

        static string AudioStats(AudioClip clip)
        {
            if (clip == null) throw new Exception("Q3 production audio clip is null.");
            if (!clip.LoadAudioData())
                throw new Exception("Q3 production audio failed to load: " + clip.name);

            var samples = new float[Math.Max(1, clip.samples * clip.channels)];
            if (!clip.GetData(samples, 0))
                throw new Exception("Q3 production audio PCM readback failed: " + clip.name);

            double sumSquares = 0d;
            var peak = 0f;
            foreach (var sample in samples)
            {
                var absolute = Mathf.Abs(sample);
                if (absolute > peak) peak = absolute;
                sumSquares += sample * sample;
            }

            var rms = Mathf.Sqrt((float)(sumSquares / samples.Length));
            if (peak < .01f || rms < .001f)
                throw new Exception("Q3 production audio is effectively silent: " + clip.name);

            return clip.name +
                   " length=" + clip.length.ToString("F3") +
                   "s channels=" + clip.channels +
                   " hz=" + clip.frequency +
                   " peak=" + peak.ToString("F4") +
                   " rms=" + rms.ToString("F4");
        }

        public static void Readback()
        {
            FirstPlayableCanonicalScene.Readback();
            var root = UnityEngine.Object.FindObjectsByType<FirstPlayableGameplayComposition>(FindObjectsSortMode.None).Single();
            var visual = root.GetComponent<FirstPlayableVisualPresentation>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FirstPlayableCanonicalScene.PrefabPath);
            var prefabVisual = prefab.GetComponent<FirstPlayableVisualPresentation>();

            if (visual == null || visual.LoadedArtCount != 4 || visual.LoadedCharacterArtCount != 17 ||
                prefabVisual == null || prefabVisual.LoadedArtCount != 4 || prefabVisual.LoadedCharacterArtCount != 17)
                throw new Exception("Q3 scene/prefab lost base or character art references.");

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
            foreach (var binding in visual.CharacterVisuals)
            {
                if (binding == null ||
                    string.IsNullOrWhiteSpace(binding.ArchetypeId) ||
                    binding.Art == null ||
                    binding.VfxIdentity == FirstPlayableCharacterVfxIdentity.None)
                    throw new Exception(
                        "Character visual binding is incomplete.");
                var path = AssetDatabase.GetAssetPath(binding.Art);
                if (string.IsNullOrEmpty(path) ||
                    (!path.StartsWith(ForestFriendPath, StringComparison.Ordinal) &&
                     !path.StartsWith(CloudKnightPath, StringComparison.Ordinal)))
                    throw new Exception(
                        "Character art escaped approved family paths: " +
                        path);
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

            var audioClips = new[]
            {
                visual.PlayerAttackSfx,
                visual.HitSfx,
                visual.DefeatSfx,
                visual.RaiseSfx,
                visual.AlliedContributionSfx
            };
            var audioStats = audioClips.Select(clip =>
            {
                var path = AssetDatabase.GetAssetPath(clip);
                if (string.IsNullOrEmpty(path) || !path.StartsWith(AudioPath, StringComparison.Ordinal))
                    throw new Exception("Q3 production audio escaped approved path: " + path);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null)
                    throw new Exception("Q3 production audio importer missing: " + path);
                var settings = importer.defaultSampleSettings;
                if (settings.loadType != AudioClipLoadType.DecompressOnLoad ||
                    settings.compressionFormat != AudioCompressionFormat.PCM)
                    throw new Exception("Q3 production audio import contract drifted: " + path);
                return AudioStats(clip);
            }).ToArray();

            File.WriteAllText(
                "Artifacts/Q3-presentation-readback.txt",
                "Q3_PRESENTATION_REOPEN_PASS\n" +
                "Scene: " + FirstPlayableCanonicalScene.ScenePath +
                "\nPrefab: " + FirstPlayableCanonicalScene.PrefabPath +
                "\nArt references: 4 / 4" +
                "\nMotion profile: PRODUCTION_CANDIDATE_CURVES_5" +
                "\nMotion durations: attack=" + visual.AttackDuration.ToString("F2") +
                    " hit=" + visual.HitDuration.ToString("F2") +
                    " defeat=" + visual.DefeatDuration.ToString("F2") +
                    " raise=" + visual.RaiseDuration.ToString("F2") +
                    " ally=" + visual.AlliedContributionDuration.ToString("F2") +
                "\nAudio clips: 5 / 5 from " + AudioPath +
                "\nSFX mix: master=" + visual.SfxMasterVolume.ToString("F2") +
                    " attack=" + visual.PlayerAttackSfxGain.ToString("F2") +
                    " hit=" + visual.HitSfxGain.ToString("F2") +
                    " defeat=" + visual.DefeatSfxGain.ToString("F2") +
                    " raise=" + visual.RaiseSfxGain.ToString("F2") +
                    " ally=" + visual.AlliedContributionSfxGain.ToString("F2") +
                "\nAudio PCM readback:\n" + string.Join("\n", audioStats) +
                "\nFonts: " + root.ReviewFont.name + " / " +
                    root.ReviewFontMedium.name + " / " + root.ReviewFontBold.name +
                "\nSource fonts: " + root.ReviewFont.sourceFontFile.name + " / " +
                    root.ReviewFontMedium.sourceFontFile.name + " / " + root.ReviewFontBold.sourceFontFile.name +
                "\nOBSIDIAN_SOUL production-candidate art bound." +
                "\nFinal release-rights / physical-device mix / accessibility / user acceptance NOT RUN.");

            Debug.Log("Q3_PRESENTATION_REOPEN_PASS art=4 motion=5 productionAudio=5");
        }
    }
}
