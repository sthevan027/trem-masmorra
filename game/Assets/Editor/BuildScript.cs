using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    [MenuItem("Trem Masmorra/Build Windows")]
    public static void BuildWindows()
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            locationPathName = "D:/Projetos/trem-masmorra/build/TremMasmorra.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };
        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log("Build: " + report.summary.result + " " + report.summary.totalSize + " bytes");
        if (report.summary.result != BuildResult.Succeeded) EditorApplication.Exit(1);
    }
}
