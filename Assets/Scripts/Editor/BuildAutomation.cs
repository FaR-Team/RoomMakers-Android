#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BuildAutomation
{
    // Command line build methods for CI/CD
    public static void BuildWebGLDevelopment()
    {
        string[] args = Environment.GetCommandLineArgs();
        string buildPath = GetCommandLineArg("-buildPath") ?? "Builds/WebGL-Dev";
        
        ConfigureWebGLDevelopment();
        PerformBuild(BuildTarget.WebGL, buildPath, true);
    }
    
    public static void BuildWebGLRelease()
    {
        string[] args = Environment.GetCommandLineArgs();
        string buildPath = GetCommandLineArg("-buildPath") ?? "Builds/WebGL-Release";
        
        ConfigureWebGLRelease();
        PerformBuild(BuildTarget.WebGL, buildPath, false);
    }
    
    public static void BuildAndroidDevelopment()
    {
        string buildPath = GetCommandLineArg("-buildPath") ?? "Builds/Android-Dev.apk";
        
        ConfigureAndroidDevelopment();
        PerformBuild(BuildTarget.Android, buildPath, true);
    }
    
    public static void BuildAndroidRelease()
    {
        string buildPath = GetCommandLineArg("-buildPath") ?? "Builds/Android-Release.apk";
        
        ConfigureAndroidRelease();
        PerformBuild(BuildTarget.Android, buildPath, false);
    }
    
    private static void ConfigureWebGLDevelopment()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.memorySize = 512;
        PlayerSettings.WebGL.dataCaching = false;
        PlayerSettings.stripEngineCode = false;
        
        Debug.Log("Configured WebGL for Development");
    }
    
    private static void ConfigureWebGLRelease()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.memorySize = 256;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.stripEngineCode = true;
        
        Debug.Log("Configured WebGL for Release");
    }
    
    private static void ConfigureAndroidDevelopment()
    {
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.stripEngineCode = false;
        
        Debug.Log("Configured Android for Development");
    }
    
    private static void ConfigureAndroidRelease()
    {
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.stripEngineCode = true;
        
        Debug.Log("Configured Android for Release");
    }
    
    private static void PerformBuild(BuildTarget target, string buildPath, bool development)
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
        
        EditorUserBuildSettings.development = development;
        EditorUserBuildSettings.allowDebugging = development;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = development;
        
        string[] scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);
        
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes found in build settings!");
            EditorApplication.Exit(1);
            return;
        }
        
        Directory.CreateDirectory(Path.GetDirectoryName(buildPath));
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = target,
            targetGroup = BuildPipeline.GetBuildTargetGroup(target)
        };
        
        if (development)
        {
            buildOptions.options |= BuildOptions.Development;
            buildOptions.options |= BuildOptions.AllowDebugging;
        }
        
        Debug.Log($"Starting {(development ? "Development" : "Release")} build for {target}");
        Debug.Log($"Build path: {buildPath}");
        
        var report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded! Size: {report.summary.totalSize} bytes, Time: {report.summary.totalTime}");
            
            // Auto-increment build number for successful builds
            PlayerSettings.Android.bundleVersionCode++;
            PlayerSettings.iOS.buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
            
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"Build failed: {report.summary.result}");
            EditorApplication.Exit(1);
        }
    }
    
    private static string GetCommandLineArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
#endif