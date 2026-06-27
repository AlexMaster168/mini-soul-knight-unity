using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class PlayModeLauncher
{
    static bool pendingPlay;

    [InitializeOnLoadMethod]
    static void Init()
    {
        EditorApplication.delayCall += () =>
        {
            Debug.Log("[PlayModeLauncher] Ready. Run EnterPlayMode() to start.");
        };
    }

    public static void EnterPlayMode()
    {
        string scenePath = "Assets/Scenes/MainScene.unity";

        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError("[PlayModeLauncher] Scene not found: " + scenePath);
            return;
        }

        if (EditorSceneManager.GetActiveScene().name != "MainScene")
        {
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        if (!EditorApplication.isPlaying && !EditorApplication.isCompiling)
        {
            EditorApplication.isPlaying = true;
            Debug.Log("[PlayModeLauncher] Entered Play Mode!");
        }
        else if (EditorApplication.isCompiling)
        {
            pendingPlay = true;
            EditorApplication.delayCall += EnterPlayMode;
            Debug.Log("[PlayModeLauncher] Waiting for compile...");
        }
    }

    [InitializeOnLoadMethod]
    static void AutoPlayOnLaunch()
    {
        EditorApplication.delayCall += () =>
        {
            if (!EditorApplication.isPlaying && !EditorApplication.isCompiling)
            {
                string scenePath = "Assets/Scenes/MainScene.unity";
                if (System.IO.File.Exists(scenePath))
                {
                    if (EditorSceneManager.GetActiveScene().name != "MainScene")
                        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    EditorApplication.isPlaying = true;
                    Debug.Log("[PlayModeLauncher] Auto-entered Play Mode on launch!");
                }
            }
        };
    }
}
