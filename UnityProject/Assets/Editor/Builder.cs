using UnityEditor;
using UnityEngine;

namespace PuzzlePals.Editor
{
    public static class Builder
    {
        [MenuItem("Build/Build Android APK")]
        public static void BuildAndroid()
        {
            string[] scenes = {
                "Assets/Scenes/MainMenuScene.unity",
                "Assets/Scenes/Level_1.unity",
                "Assets/Scenes/Level_2.unity",
                "Assets/Scenes/Level_3.unity"
            };

            string buildPath = "Builds/Android/PuzzlePalsAdventure.apk";

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log("[Builder] Starting Android build...");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;

            if (summary.result == UnityEditor.BuildReporting.BuildResult.Succeeded)
            {
                Debug.Log($"[Builder] Build Succeeded! Output path: {buildPath}");
            }
            else if (summary.result == UnityEditor.BuildReporting.BuildResult.Failed)
            {
                Debug.LogError($"[Builder] Build Failed! Errors: {summary.totalErrors}");
            }
        }
    }
}
