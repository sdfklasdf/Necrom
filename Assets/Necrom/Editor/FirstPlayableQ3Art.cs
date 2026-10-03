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
        const string ArtPath="Assets/Necrom/FirstPlayable/Art/Q3/";
        const string FontPath="Assets/Necrom/FirstPlayable/Fonts/NotoSansCJKkr-Regular Review SDF.asset";
        [MenuItem("Necrom/Q3/Apply representative review art")]
        public static void Apply()
        {
            AssetDatabase.Refresh();
            foreach(var name in new[]{"player","guard","raised","background"})
            {
                var path=ArtPath+name+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.alphaIsTransparency=name!="background";importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.maxTextureSize=2048;importer.mipmapEnabled=false;importer.SaveAndReimport();
            }
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if(font==null)
            {
                var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/Necrom/FirstPlayable/Fonts/NotoSansCJKkr-Regular.otf");
                if(source==null)throw new Exception("Noto review font source not imported.");
                font=TMP_FontAsset.CreateFontAsset(source,90,9,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
                if(font==null)throw new Exception("Noto review SDF creation failed.");
                AssetDatabase.CreateAsset(font,FontPath);
                foreach(var texture in font.atlasTextures)AssetDatabase.AddObjectToAsset(texture,font);
                AssetDatabase.AddObjectToAsset(font.material,font);
                var copy=File.ReadAllText("Assets/Necrom/FirstPlayable/Runtime/FirstPlayableGameplayComposition.cs");
                var chars=new string(copy.Where(c=>c>=' '&&c!='\r'&&c!='\n').Distinct().ToArray());
                if(!font.TryAddCharacters(chars,out var missing))Debug.LogWarning("Q3_FONT_MISSING "+missing);
                EditorUtility.SetDirty(font);AssetDatabase.SaveAssets();
            }
            var scene=EditorSceneManager.OpenScene(FirstPlayableCanonicalScene.ScenePath,OpenSceneMode.Single);
            var root=scene.GetRootGameObjects().Single(x=>x.name=="FirstPlayableCanonical");
            var composition=root.GetComponent<FirstPlayableGameplayComposition>();composition.ReviewFont=font;
            var visual=root.GetComponent<FirstPlayableVisualPresentation>()??root.AddComponent<FirstPlayableVisualPresentation>();
            visual.NecromancerArt=AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath+"player.png");
            visual.GuardArt=AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath+"guard.png");
            visual.RaisedGuardArt=AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath+"raised.png");
            visual.BackgroundArt=AssetDatabase.LoadAssetAtPath<Texture2D>(ArtPath+"background.png");
            if(visual.LoadedArtCount!=4)throw new Exception("Four Q3 art references must resolve.");
            PrefabUtility.SaveAsPrefabAsset(root,FirstPlayableCanonicalScene.PrefabPath);
            if(!EditorSceneManager.SaveScene(scene))throw new Exception("Q3 scene save failed.");
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Readback();
        }
        public static void Readback()
        {
            FirstPlayableCanonicalScene.Readback();
            var root=UnityEngine.Object.FindObjectsByType<FirstPlayableGameplayComposition>(FindObjectsSortMode.None).Single();
            var visual=root.GetComponent<FirstPlayableVisualPresentation>();
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(FirstPlayableCanonicalScene.PrefabPath);
            if(visual==null||visual.LoadedArtCount!=4||prefab.GetComponent<FirstPlayableVisualPresentation>().LoadedArtCount!=4)
                throw new Exception("Q3 scene/prefab lost art references.");
            if(root.ReviewFont==null||root.ReviewFont.sourceFontFile==null)throw new Exception("Q3 font source unresolved.");
            File.WriteAllText("Artifacts/Q3-art-readback.txt","Q3_REOPEN_PASS\nScene: "+FirstPlayableCanonicalScene.ScenePath+
                "\nPrefab: "+FirstPlayableCanonicalScene.PrefabPath+"\nArt references: 4 / 4\nFont: "+root.ReviewFont.name+
                "\nSource font: "+root.ReviewFont.sourceFontFile.name+"\nReview candidate, rights NOT REVIEWED.");
            Debug.Log("Q3_REOPEN_PASS art=4 scene+prefab font="+root.ReviewFont.name);
        }
    }
}
