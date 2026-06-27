using UnityEditor;
using UnityEditor.SceneManagement;

public class OpenSceneOnLaunch
{
    public static void Open()
    {
        string path = "Assets/Scenes/MainScene.unity";
        if (!System.IO.File.Exists(path)) return;
        EditorSceneManager.OpenScene(path);
    }

    [InitializeOnLoadMethod]
    static void OnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.isLoaded && scene.name == "MainScene") return;
            Open();
        };
    }
}