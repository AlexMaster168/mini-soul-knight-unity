using UnityEngine;
using UnityEditor;

public class GameMenu
{
    [MenuItem("Soul Knight/Open Project")]
    static void OpenProject()
    {
        string projectPath = Application.dataPath + "/..";
        EditorUtility.RevealInFinder(projectPath);
    }

    [MenuItem("Soul Knight/Run Game %#r")]
    static void RunGame()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = true;
        }
    }

    [MenuItem("Soul Knight/Stop Game")]
    static void StopGame()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
    }

    [MenuItem("Soul Knight/Create Scene")]
    static void CreateScene()
    {
        string scenePath = "Assets/Scenes/MainScene.unity";

        // Create scene if it doesn't exist
        if (!System.IO.File.Exists(scenePath))
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single
            );
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("Created scene: " + scenePath);
        }

        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
    }
}