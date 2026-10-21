using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TremMasmorra.Game;

public static class SceneBuilder
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("Trem Masmorra/Build Main Scene")]
    public static void BuildMainScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Bootstrap").AddComponent<GameBootstrap>();
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "Trem Masmorra";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        AssetDatabase.SaveAssets();
        Debug.Log("Cena criada: " + ScenePath);
    }
}
