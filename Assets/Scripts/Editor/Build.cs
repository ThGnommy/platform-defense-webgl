using UnityEditor;

public static class Build {
    public static void PerformBuild() {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.target = BuildTarget.WebGL;
        options.scenes = new string[] { "Assets/Scenes/Level.unity" };
        options.locationPathName = "~/PD";
        
        BuildPipeline.BuildPlayer(options);
    }
}