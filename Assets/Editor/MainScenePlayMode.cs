using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
internal static class MainScenePlayMode
{
    internal const string ScenePath = "Assets/Scenes/MainScene.unity";

    static MainScenePlayMode()
    {
        EditorApplication.delayCall += Configure;
    }

    private static void Configure()
    {
        var mainScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (mainScene != null)
        {
            EditorSceneManager.playModeStartScene = mainScene;
        }
    }
}
