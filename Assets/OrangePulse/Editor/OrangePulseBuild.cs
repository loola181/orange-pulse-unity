using System.IO;
using OrangePulse.Presentation;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OrangePulse.Editor
{
    public static class OrangePulseBuild
    {
        private const string ScenePath = "Assets/OrangePulse/Scenes/Main.unity";

        public static void Prepare()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = File.Exists(ScenePath)
                ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single)
                : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject app = GameObject.Find("OrangePulseApp");
            if (app == null) app = new GameObject("OrangePulseApp");
            if (app.GetComponent<OrangePulseRoot>() == null) app.AddComponent<OrangePulseRoot>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

            PlayerSettings.productName = "Orange Football";
            PlayerSettings.companyName = "Orange Football Studio";
            PlayerSettings.bundleVersion = "1.5.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.loola181.orangefootball");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.statusBarHidden = false;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.bundleVersionCode = 6;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/OrangePulse/Resources/app-icon.png");
            if (icon != null)
                PlayerSettings.SetIcons(NamedBuildTarget.Android, new[] { icon }, IconKind.Any);

            AssetDatabase.SaveAssets();
        }

        public static void BuildDevelopment()
        {
            Prepare();
            Build("Build/OrangeFootball-dev.apk", BuildOptions.Development | BuildOptions.AllowDebugging);
        }

        public static void BuildRelease()
        {
            Prepare();
            Build("Build/OrangeFootball.apk", BuildOptions.None);
        }

        public static void ExportAndroidProject()
        {
            Prepare();

            const string exportDirectory = "ci/android-export";
            if (Directory.Exists(exportDirectory)) Directory.Delete(exportDirectory, true);

            string output = Path.GetFullPath(exportDirectory);
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
            EditorUserBuildSettings.buildAppBundle = false;

            try
            {
                var build = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = output,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.AcceptExternalModificationsToPlayer
                };

                BuildSummary summary = BuildPipeline.BuildPlayer(build).summary;
                if (summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError($"[OrangePulseBuild] Android export failed: {summary.result}, errors={summary.totalErrors}");
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log($"[OrangePulseBuild] Android export ready: {output}, bytes={summary.totalSize}");
            }
            finally
            {
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            }

            EditorApplication.Exit(0);
        }

        private static void Build(string relativeOutput, BuildOptions options)
        {
            string output = Path.GetFullPath(relativeOutput);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var build = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = options
            };

            BuildSummary summary = BuildPipeline.BuildPlayer(build).summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[OrangePulseBuild] Failed: {summary.result}, errors={summary.totalErrors}");
                EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"[OrangePulseBuild] Success: {output}, bytes={summary.totalSize}");
            EditorApplication.Exit(0);
        }
    }
}
