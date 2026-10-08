using System;
using System.IO;
using System.Linq;
using Necrom.FirstPlayable.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Necrom.EditorTools
{
    public static class NecromPlayableSceneRepair
    {
        const string ScenePath = "Assets/Necrom/FirstPlayable/Scenes/FirstPlayable.unity";
        const string PrefabPath = "Assets/Necrom/FirstPlayable/Prefabs/FirstPlayable.prefab";

        [MenuItem("Necrom/실제 전투 게임 화면 복구", false, 101)]
        public static void Repair()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Play 모드 종료 필요", "상단 Play 버튼을 먼저 끄고 다시 실행해 주세요.", "확인");
                return;
            }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null || prefab.GetComponent<FirstPlayableGameplayComposition>() == null)
            {
                EditorUtility.DisplayDialog("복구 중단", "기존 전투 프리팹을 확인할 수 없습니다. 파일은 변경하지 않았습니다.", "확인");
                return;
            }
            var active = SceneManager.GetActiveScene();
            if (!active.IsValid() || active.path != ScenePath)
            {
                EditorUtility.DisplayDialog("씬 선택 필요", "FirstPlayable 씬을 연 상태에서 실행해 주세요.", "확인");
                return;
            }
            if (active.GetRootGameObjects().Any(x => x.name == "FirstPlayableCanonical"))
            {
                EditorUtility.DisplayDialog("이미 복원됨", "FirstPlayableCanonical 전투 루트가 이미 존재합니다. Play로 확인해 주세요.", "확인");
                return;
            }
            var backupDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Necrom_scene_backups"));
            Directory.CreateDirectory(backupDir);
            var backup = Path.Combine(backupDir, "FirstPlayable_before_repair_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + ".unity");
            if (File.Exists(ScenePath)) File.Copy(ScenePath, backup, false);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, active);
            if (instance == null) throw new InvalidOperationException("Prefab instantiation failed.");
            Undo.RegisterCreatedObjectUndo(instance, "Restore Necrom playable combat scene");
            var demo = active.GetRootGameObjects().FirstOrDefault(x => x.name == "NecromDemoUI");
            if (demo != null) { Undo.RecordObject(demo, "Hide prototype overlay"); demo.SetActive(false); }
            var demoSystems = active.GetRootGameObjects().FirstOrDefault(x => x.name == "NecromDemoGameSystems");
            if (demoSystems != null) { Undo.RecordObject(demoSystems, "Pause prototype economy UI"); demoSystems.SetActive(false); }
            EditorSceneManager.MarkSceneDirty(active);
            if (!EditorSceneManager.SaveScene(active))
                throw new IOException("Could not save playable scene.");
            Selection.activeGameObject = instance;
            Debug.Log("NECROM_PLAYABLE_RESTORE_SUCCESS. Scene backup: " + backup);
            EditorUtility.DisplayDialog("실제 전투 화면 복구 완료", "전투 프리팹을 씬에 복원했습니다. 이제 Play를 눌러 주세요. 기존 임시 가챠 UI는 삭제하지 않고 비활성화했습니다.", "확인");
        }
    }
}
