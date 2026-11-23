using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MacPrototypeBuilder
{
    private const string MainScenePath = MainScenePlayMode.ScenePath;

    [MenuItem("Tools/Time Travel/Build macOS Prototype")]
    public static void BuildMac() => Build(MainScenePath, "macOS", BuildOptions.Development);

    [MenuItem("Tools/Time Travel/Build macOS Projection")]
    public static void BuildMacProjection() => Build(MainScenePath, "macOS Projection", BuildOptions.None);

    private static void Build(string scenePath, string outputDirectory, BuildOptions buildOptions)
    {
#if !UNITY_EDITOR_OSX
        Debug.LogError("macOS build: Run this command in Unity Editor on a Mac.");
#else
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("macOS build: Exit Play mode before building.");
            return;
        }

        bool macSupportInstalled = BuildPipeline.IsBuildTargetSupported(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);

        if (!macSupportInstalled)
        {
            Debug.LogError("macOS build: Install Mac Build Support (Mono) for this Editor through Unity Hub.");
            return;
        }

        if (!File.Exists(scenePath))
        {
            Debug.LogError("macOS build: Scene was not found at " + scenePath);
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);

        if (!switched)
        {
            Debug.LogError("macOS build: Unity could not switch to the macOS target.");
            return;
        }

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);

        // Build both CPU versions so the app supports Intel and Apple silicon Macs.
        UnityEditor.OSXStandalone.UserBuildSettings.architecture = OSArchitecture.x64ARM64;
        UnityEditor.OSXStandalone.UserBuildSettings.createXcodeProject = false;

        string projectFolder = Directory.GetParent(Application.dataPath).FullName;
        string outputFolder = Path.Combine(projectFolder, "Builds", outputDirectory);
        Directory.CreateDirectory(outputFolder);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new[] { scenePath };
        options.locationPathName = Path.Combine(outputFolder, "TimeTravelProjection.app");
        options.target = BuildTarget.StandaloneOSX;
        options.options = buildOptions;

        Debug.Log($"macOS build: Building {scenePath}. Output: {options.locationPathName}");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("macOS build succeeded. Open " + options.locationPathName + " to test.");
            EditorUtility.RevealInFinder(options.locationPathName);
        }
        else
        {
            Debug.LogError("macOS build failed: " + report.summary.result +
                ". Errors: " + report.summary.totalErrors + ". Check the Console for details.");
        }
#endif
    }
}
