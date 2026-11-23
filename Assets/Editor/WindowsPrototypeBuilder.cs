using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

// Editor-only tool. This script is not included in the finished application.
public static class WindowsPrototypeBuilder
{
    private const string MainScenePath = MainScenePlayMode.ScenePath;

    [MenuItem("Tools/Time Travel/Build Windows Prototype")]
    public static void BuildWindows() => Build(MainScenePath, "Windows", BuildOptions.Development);

    [MenuItem("Tools/Time Travel/Build Windows Projection")]
    public static void BuildWindowsProjection() => Build(MainScenePath, "Windows Projection", BuildOptions.None);

    private static void Build(string scenePath, string outputDirectory, BuildOptions buildOptions)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("Windows build: Exit Play mode before building.");
            return;
        }

        // StandaloneWindows64 means a Windows application for an x86-64 computer.
        bool windowsSupportInstalled = BuildPipeline.IsBuildTargetSupported(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        if (!windowsSupportInstalled)
        {
            Debug.LogError("Windows build: Install Windows Build Support (Mono) through " +
                "Unity Hub > Installs > 6000.6.8f1 > Add modules, then reopen the project.");
            return;
        }

        if (!File.Exists(scenePath))
        {
            Debug.LogError("Windows build: Scene was not found at " + scenePath);
            return;
        }

        // Give the student a chance to save scene edits before building from disk.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        // Application.dataPath is the project's Assets folder.
        string projectFolder = Directory.GetParent(Application.dataPath).FullName;
        string outputFolder = Path.Combine(projectFolder, "Builds", outputDirectory);
        Directory.CreateDirectory(outputFolder);

        // Mono supports building this Windows prototype from the current Mac.
        // The Windows IL2CPP backend would need a different toolchain.
        bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        if (!switched)
        {
            Debug.LogError("Windows build: Unity could not switch to the Windows target.");
            return;
        }

        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new[] { scenePath };
        options.locationPathName = Path.Combine(outputFolder, "TimeTravelProjection.exe");
        options.target = BuildTarget.StandaloneWindows64;
        options.options = buildOptions;

        Debug.Log($"Windows build: Building {scenePath}. Output: {options.locationPathName}");
        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Windows build succeeded. Copy the entire " + outputFolder + " folder " +
                "to a Windows computer, then run TimeTravelProjection.exe.");
            EditorUtility.RevealInFinder(options.locationPathName);
        }
        else
        {
            Debug.LogError("Windows build failed: " + report.summary.result +
                ". Errors: " + report.summary.totalErrors + ". Check the Console for details.");
        }
    }
}
