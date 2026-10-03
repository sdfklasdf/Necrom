using System;
using System.IO;
using System.Linq;
using Necrom.FirstPlayable.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Necrom.EditorTools
{
    public static class FirstPlayableCanonicalScene
    {
        public const string ScenePath="Assets/Necrom/FirstPlayable/Scenes/FirstPlayable.unity";
        public const string PrefabPath="Assets/Necrom/FirstPlayable/Prefabs/FirstPlayable.prefab";
        public static void ImportReviewFont()
        {
            var package=Directory.GetFiles("Library/PackageCache","TMP Essential Resources.unitypackage",SearchOption.AllDirectories).Single();
            AssetDatabase.importPackageCompleted += _ => { AssetDatabase.SaveAssets(); Debug.Log("TMP_IMPORT_COMPLETED"); EditorApplication.Exit(0); };
            AssetDatabase.ImportPackage(package,false);
            AssetDatabase.Refresh();
        }
        public static void Generate()
        {
            var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if(font==null) throw new InvalidOperationException("Import TMP Essential Resources review font first.");
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=new GameObject("FirstPlayableCanonical",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster),
                typeof(EncounterBoundaryController),typeof(FirstPlayableCombatHudRuntimeBinding));
            var composition=root.AddComponent<FirstPlayableGameplayComposition>();
            composition.ReviewFont=font;
            composition.ConfigureViewport(new Vector2(390,844),new Rect(0,34,390,776));
            var combat=root.transform.Find("SafeArea/CombatViewport").gameObject;
            combat.AddComponent<NecromancerAnchorController>();
            combat.AddComponent<EnemySpawnController>();
            combat.AddComponent<FirstPlayableAlliedRosterController>();
            combat.AddComponent<FirstPlayableBattleRuntimeController>();
            combat.AddComponent<FirstPlayableTargetingController>();
            combat.AddComponent<FirstPlayableAutoCombatLoop>();
            combat.AddComponent<FirstPlayableAlliedAutoCombatLoop>();
            root.AddComponent<FirstPlayableRaiseCommandInputHook>();
            root.AddComponent<FirstPlayableRaiseActionController>();
            PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
            new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
            var camera=new GameObject("Camera",typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.04f,.05f,.07f);
            camera.orthographic=true;
            if(!EditorSceneManager.SaveScene(scene,ScenePath)) throw new InvalidOperationException("Scene save failed");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Readback();
        }
        public static void BuildVisualPlayer()
        {
            Readback();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{ScenePath}, target=BuildTarget.StandaloneWindows64,
                locationPathName="Artifacts/VisualPlayer/NecromVisual.exe", options=BuildOptions.Development });
            Debug.Log("VISUAL_PLAYER_BUILD "+report.summary.result+" errors="+report.summary.totalErrors);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new Exception("Visual player build failed");
        }
        public static void Readback()
        {
            AssetDatabase.ImportAsset(ScenePath,ImportAssetOptions.ForceUpdate);
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var root=scene.GetRootGameObjects().Single(x=>x.name=="FirstPlayableCanonical");
            if(root.GetComponent<FirstPlayableGameplayComposition>().ReviewFont==null) throw new Exception("Serialized font lost.");
            foreach(var name in new[]{"TargetStatusReadabilityZone","RaiseActionStatusReadabilityZone","ArmyStatusReadabilityZone","ProtectedCombatReadabilityZone","CombatViewport"})
                if(root.transform.Find("SafeArea/"+name)==null) throw new Exception("Missing "+name);
            Debug.Log("CANONICAL_REOPEN_PASS components="+root.GetComponentsInChildren<MonoBehaviour>(true).Length+" scene="+ScenePath+" prefab="+PrefabPath);
            Directory.CreateDirectory("Artifacts");
            File.WriteAllText("Artifacts/AWU4-scene-readback.txt",
                "Canonical scene reopened: "+ScenePath+"\nPrefab: "+PrefabPath+"\nHierarchy:\n"+
                string.Join("\n",root.GetComponentsInChildren<Transform>(true).Select(x=>x.name)));
        }
    }
}
