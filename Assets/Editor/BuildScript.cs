using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build Windows")]
    public static void Build()
    {
        string[] scenes = { "Assets/Scenes/MainScene.unity" };
        string buildPath = "Builds/SoulKnight.exe";

        // Ensure build directory exists
        string directory = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + buildPath);
            Debug.Log("Build size: " + report.summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("Build failed");
        }
    }

    [MenuItem("Build/Build and Run")]
    public static void BuildAndRun()
    {
        Build();

        string buildPath = "Builds/SoulKnight.exe";
        if (File.Exists(buildPath))
        {
            System.Diagnostics.Process.Start(buildPath);
        }
    }
}