using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Necrom.EditorTools
{
    public static class AndroidSmokeBuild
    {
        private const string TempRoot = "Assets/__NecromSmoke";
        private const string ScenePath = TempRoot + "/Smoke.unity";
        private const string OutputPath = "Artifacts/Necrom-android-smoke.apk";

        public static void Build()
        {
            Directory.CreateDirectory(TempRoot);
            Directory.CreateDirectory("Artifacts");

            try
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                new GameObject("NecromSmokeRoot");
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new InvalidOperationException("Failed to save temporary smoke scene.");

                if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                    throw new InvalidOperationException("Failed to switch active build target to Android.");

                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = OutputPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.Development
                };

                var report = BuildPipeline.BuildPlayer(options);
                Debug.Log($"NECROM_ANDROID_SMOKE result={report.summary.result} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings} size={report.summary.totalSize} path={OutputPath}");

                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException($"Android smoke build failed: {report.summary.result}");
            }
            finally
            {
                AssetDatabase.DeleteAsset(TempRoot);
                AssetDatabase.Refresh();
            }
        }
    }
}
