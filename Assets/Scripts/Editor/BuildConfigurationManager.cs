#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildConfigurationManager : EditorWindow
{
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "Build Profiles", "Platform Settings", "Asset Optimization", "Version Control", "Quick Build" };
    
    // Build Profiles
    private List<BuildProfile> buildProfiles = new List<BuildProfile>();
    private BuildProfile selectedProfile = null;
    private string newProfileName = "";
    private bool showCreateProfile = false;
    
    // Platform Settings
    private BuildTarget currentTarget = BuildTarget.WebGL;
    private Dictionary<BuildTarget, PlatformConfig> platformConfigs = new Dictionary<BuildTarget, PlatformConfig>();
    
    // Asset Optimization
    private bool optimizeTextures = true;
    private bool optimizeAudio = true;
    private bool stripUnusedAssets = true;
    private TextureImporterFormat textureFormat = TextureImporterFormat.Automatic;
    private AudioCompressionFormat audioFormat = AudioCompressionFormat.Vorbis;
    
    // Version Management
    private string versionNumber = "1.0.0";
    private int buildNumber = 1;
    private bool autoIncrementBuild = true;
    private string versionSuffix = "";
    
    // Quick Build
    private bool developmentBuild = true;
    private bool scriptDebugging = true;
    private bool deepProfiling = false;
    private string lastBuildPath = "";
    
    [MenuItem("DevTools/Build Configuration Manager")]
    public static void ShowWindow()
    {
        GetWindow<BuildConfigurationManager>("Build Configuration Manager");
    }
    
    private void OnEnable()
    {
        LoadBuildProfiles();
        LoadPlatformConfigs();
        LoadVersionInfo();
    }
    
    private void OnDisable()
    {
        SaveBuildProfiles();
        SavePlatformConfigs();
        SaveVersionInfo();
    }
    
    private void OnGUI()
    {
        DrawHeader();
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.Height(25));
        
        EditorGUILayout.Space(10);
        DrawSeparator();
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        switch (selectedTab)
        {
            case 0: DrawBuildProfilesTab(); break;
            case 1: DrawPlatformSettingsTab(); break;
            case 2: DrawAssetOptimizationTab(); break;
            case 3: DrawVersionControlTab(); break;
            case 4: DrawQuickBuildTab(); break;
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("🔧 Build Configuration Manager", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        
        // Current platform indicator
        GUIStyle platformStyle = new GUIStyle(EditorStyles.helpBox);
        platformStyle.normal.textColor = GetPlatformColor(EditorUserBuildSettings.activeBuildTarget);
        
        GUILayout.BeginVertical(platformStyle, GUILayout.Width(150));
        GUILayout.Label("🎯 Current Platform:", EditorStyles.miniLabel);
        GUILayout.Label($"{GetPlatformIcon(EditorUserBuildSettings.activeBuildTarget)} {EditorUserBuildSettings.activeBuildTarget}", EditorStyles.boldLabel);
        GUILayout.EndVertical();
        
        GUILayout.Space(10);
        
        // Version info
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(120));
        GUILayout.Label("📦 Version:", EditorStyles.miniLabel);
        GUILayout.Label($"v{PlayerSettings.bundleVersion}", EditorStyles.boldLabel);
        GUILayout.EndVertical();
        
        GUILayout.FlexibleSpace();
        
        // Quick actions
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("🚀 Quick Build", GUILayout.Width(100), GUILayout.Height(35)))
        {
            selectedTab = 4;
        }
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("⚙️ Settings", GUILayout.Width(80), GUILayout.Height(35)))
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
        
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
    }
    
    private void DrawBuildProfilesTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📋 Build Profiles", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Create and manage different build configurations for various scenarios", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Create new profile section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("➕ Create New Profile", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        showCreateProfile = EditorGUILayout.Toggle("Show", showCreateProfile, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
        
        if (showCreateProfile)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name:", GUILayout.Width(50));
            newProfileName = EditorGUILayout.TextField(newProfileName);
            
            GUI.enabled = !string.IsNullOrEmpty(newProfileName) && !buildProfiles.Any(p => p.name == newProfileName);
            if (GUILayout.Button("Create", GUILayout.Width(60)))
            {
                CreateNewProfile(newProfileName);
                newProfileName = "";
                showCreateProfile = false;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Existing profiles
        if (buildProfiles.Count == 0)
        {
            EditorGUILayout.HelpBox("No build profiles created yet. Create your first profile above!", MessageType.Info);
        }
        else
        {
            foreach (var profile in buildProfiles.ToList())
            {
                DrawBuildProfile(profile);
                EditorGUILayout.Space(5);
            }
        }
    }
    
    private void DrawBuildProfile(BuildProfile profile)
    {
        bool isSelected = selectedProfile == profile;
        
        EditorGUILayout.BeginVertical(isSelected ? EditorStyles.helpBox : EditorStyles.textArea);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        // Profile icon and name
        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
        nameStyle.normal.textColor = isSelected ? new Color(0.2f, 0.6f, 1f) : Color.white;
        
        GUILayout.Label($"{GetPlatformIcon(profile.buildTarget)} {profile.name}", nameStyle);
        
        GUILayout.FlexibleSpace();
        
        // Profile actions
        if (GUILayout.Button("📝", EditorStyles.miniButton, GUILayout.Width(25)))
        {
            selectedProfile = isSelected ? null : profile;
        }
        
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("🚀", EditorStyles.miniButton, GUILayout.Width(25)))
        {
            BuildWithProfile(profile);
        }
        
        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
        if (GUILayout.Button("🗑️", EditorStyles.miniButton, GUILayout.Width(25)))
        {
            if (EditorUtility.DisplayDialog("Delete Profile", $"Are you sure you want to delete '{profile.name}'?", "Delete", "Cancel"))
            {
                buildProfiles.Remove(profile);
                if (selectedProfile == profile) selectedProfile = null;
            }
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        // Profile details
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        GUILayout.Label($"Platform: {profile.buildTarget}", EditorStyles.miniLabel, GUILayout.Width(150));
        GUILayout.Label($"Development: {(profile.developmentBuild ? "Yes" : "No")}", EditorStyles.miniLabel, GUILayout.Width(100));
        GUILayout.Label($"Version: {profile.version}", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
        
        if (isSelected)
        {
            EditorGUILayout.Space(10);
            DrawProfileEditor(profile);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawProfileEditor(BuildProfile profile)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📝 Profile Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Basic settings
        profile.name = EditorGUILayout.TextField("Profile Name", profile.name);
        profile.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", profile.buildTarget);
        profile.developmentBuild = EditorGUILayout.Toggle("Development Build", profile.developmentBuild);
        
        EditorGUILayout.Space(5);
        
        // Advanced settings
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Advanced Options:", EditorStyles.boldLabel);
        profile.showAdvanced = EditorGUILayout.Toggle(profile.showAdvanced, GUILayout.Width(20));
        EditorGUILayout.EndHorizontal();
        
        if (profile.showAdvanced)
        {
            EditorGUI.indentLevel++;
            profile.scriptDebugging = EditorGUILayout.Toggle("Script Debugging", profile.scriptDebugging);
            profile.deepProfiling = EditorGUILayout.Toggle("Deep Profiling", profile.deepProfiling);
            profile.version = EditorGUILayout.TextField("Version Override", profile.version);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(5);
        
        // Build path
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Build Path:", GUILayout.Width(80));
        profile.buildPath = EditorGUILayout.TextField(profile.buildPath);
        if (GUILayout.Button("📁", GUILayout.Width(30)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Build Folder", profile.buildPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                profile.buildPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Profile actions
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("💾 Save Profile", GUILayout.Width(100)))
        {
            SaveBuildProfiles();
            EditorUtility.DisplayDialog("Profile Saved", $"Profile '{profile.name}' has been saved.", "OK");
        }
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("🚀 Build Now", GUILayout.Width(100)))
        {
            BuildWithProfile(profile);
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }    
    private void DrawPlatformSettingsTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.6f, 0.2f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🎯 Platform Settings", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Configure platform-specific build settings", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Platform selector
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("Select Platform:", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        BuildTarget[] supportedPlatforms = { BuildTarget.WebGL, BuildTarget.Android, BuildTarget.StandaloneWindows64, BuildTarget.iOS };
        
        foreach (var platform in supportedPlatforms)
        {
            bool isSelected = currentTarget == platform;
            bool isActive = EditorUserBuildSettings.activeBuildTarget == platform;
            
            GUI.backgroundColor = isSelected ? new Color(0.7f, 0.9f, 1f) : (isActive ? new Color(0.7f, 1f, 0.7f) : Color.white);
            
            if (GUILayout.Button($"{GetPlatformIcon(platform)}\n{platform}", GUILayout.Height(50), GUILayout.Width(80)))
            {
                currentTarget = platform;
            }
        }
        
        GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Platform-specific settings
        if (!platformConfigs.ContainsKey(currentTarget))
        {
            platformConfigs[currentTarget] = new PlatformConfig();
        }
        
        var config = platformConfigs[currentTarget];
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label($"{GetPlatformIcon(currentTarget)} {currentTarget} Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        switch (currentTarget)
        {
            case BuildTarget.WebGL:
                DrawWebGLSettings(config);
                break;
            case BuildTarget.Android:
                DrawAndroidSettings(config);
                break;
            case BuildTarget.StandaloneWindows64:
                DrawWindowsSettings(config);
                break;
        }
        
        EditorGUILayout.Space(5);
        
        // Apply settings button
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("✅ Apply Settings", GUILayout.Width(120), GUILayout.Height(30)))
        {
            ApplyPlatformSettings(currentTarget, config);
            EditorUtility.DisplayDialog("Settings Applied", $"Platform settings for {currentTarget} have been applied.", "OK");
        }
        
        GUI.backgroundColor = new Color(0.9f, 0.7f, 0.2f);
        if (GUILayout.Button("🔄 Switch Platform", GUILayout.Width(120), GUILayout.Height(30)))
        {
            if (EditorUserBuildSettings.activeBuildTarget != currentTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(currentTarget), currentTarget);
            }
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawWebGLSettings(PlatformConfig config)
    {
        GUILayout.Label("🌐 WebGL Specific Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        config.webGLTemplate = EditorGUILayout.TextField("Template", config.webGLTemplate);
        config.webGLCompressionFormat = (WebGLCompressionFormat)EditorGUILayout.EnumPopup("Compression Format", config.webGLCompressionFormat);
        config.webGLLinkerTarget = (WebGLLinkerTarget)EditorGUILayout.EnumPopup("Linker Target", config.webGLLinkerTarget);
        config.webGLOptimizationLevel = EditorGUILayout.IntSlider("Optimization Level", config.webGLOptimizationLevel, 0, 3);
        
        EditorGUILayout.Space(10);
        
        GUILayout.Label("🚀 Development Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        config.webGLDevelopmentBuild = EditorGUILayout.Toggle("Development Build", config.webGLDevelopmentBuild);
        config.webGLAutoconnectProfiler = EditorGUILayout.Toggle("Autoconnect Profiler", config.webGLAutoconnectProfiler);
        config.webGLScriptDebugging = EditorGUILayout.Toggle("Script Debugging", config.webGLScriptDebugging);
        
        if (config.webGLDevelopmentBuild)
        {
            EditorGUILayout.HelpBox("💡 Development builds are larger but include debugging symbols and profiler support", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        GUILayout.Label("⚡ Performance Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        config.webGLDataCaching = EditorGUILayout.Toggle("Data Caching", config.webGLDataCaching);
        config.webGLDecompressionFallback = EditorGUILayout.Toggle("Decompression Fallback", config.webGLDecompressionFallback);
        config.webGLMemorySize = EditorGUILayout.IntField("Memory Size (MB)", config.webGLMemorySize);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(3);
        GUILayout.Label("📊 Recommended Settings for Development:", EditorStyles.miniLabel);
        GUILayout.Label("• Development Build: ON", EditorStyles.miniLabel);
        GUILayout.Label("• Script Debugging: ON", EditorStyles.miniLabel);
        GUILayout.Label("• Compression: Disabled (faster builds)", EditorStyles.miniLabel);
        GUILayout.Label("• Memory Size: 512MB+", EditorStyles.miniLabel);
        EditorGUILayout.Space(3);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAndroidSettings(PlatformConfig config)
    {
        GUILayout.Label("🤖 Android Specific Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        config.androidTargetArchitecture = (AndroidArchitecture)EditorGUILayout.EnumFlagsField("Target Architecture", config.androidTargetArchitecture);
        config.androidMinSdkVersion = (AndroidSdkVersions)EditorGUILayout.EnumPopup("Min SDK Version", config.androidMinSdkVersion);
        config.androidTargetSdkVersion = (AndroidSdkVersions)EditorGUILayout.EnumPopup("Target SDK Version", config.androidTargetSdkVersion);
        
        EditorGUILayout.Space(10);
        
        GUILayout.Label("📦 Build Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        config.androidBuildSystem = (AndroidBuildSystem)EditorGUILayout.EnumPopup("Build System", config.androidBuildSystem);
        config.androidScriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", config.androidScriptingBackend);
        config.androidTargetDevice = (AndroidArchitecture)EditorGUILayout.EnumPopup("Target Device", config.androidTargetDevice);
        
        EditorGUILayout.Space(10);
        
        GUILayout.Label("🔐 Security & Signing", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        config.androidKeystoreName = EditorGUILayout.TextField("Keystore Name", config.androidKeystoreName);
        config.androidKeystorePass = EditorGUILayout.PasswordField("Keystore Pass", config.androidKeystorePass);
        config.androidKeyaliasName = EditorGUILayout.TextField("Key Alias Name", config.androidKeyaliasName);
        config.androidKeyaliasPass = EditorGUILayout.PasswordField("Key Alias Pass", config.androidKeyaliasPass);
    }
    
    private void DrawWindowsSettings(PlatformConfig config)
    {
        GUILayout.Label("🖥️ Windows Specific Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        config.windowsArchitecture = (BuildTarget)EditorGUILayout.EnumPopup("Architecture", config.windowsArchitecture);
        config.windowsScriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", config.windowsScriptingBackend);
        config.windowsFullscreenMode = (FullScreenMode)EditorGUILayout.EnumPopup("Fullscreen Mode", config.windowsFullscreenMode);
        
        EditorGUILayout.Space(10);
        
        GUILayout.Label("🎮 Player Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        config.windowsResizableWindow = EditorGUILayout.Toggle("Resizable Window", config.windowsResizableWindow);
        config.windowsVisibleInBackground = EditorGUILayout.Toggle("Visible in Background", config.windowsVisibleInBackground);
        config.windowsAllowFullscreenSwitch = EditorGUILayout.Toggle("Allow Fullscreen Switch", config.windowsAllowFullscreenSwitch);
    }    
    private void DrawAssetOptimizationTab()    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.6f, 0.9f, 0.2f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("⚡ Asset Optimization", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Optimize assets for different build targets and configurations", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Texture Optimization
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🖼️ Texture Optimization", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        optimizeTextures = EditorGUILayout.Toggle("Enable Texture Optimization", optimizeTextures);
        
        if (optimizeTextures)
        {
            EditorGUI.indentLevel++;
            textureFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Default Format", textureFormat);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Analyze Textures", GUILayout.Width(130)))
            {
                AnalyzeTextures();
            }
            if (GUILayout.Button("⚡ Optimize All", GUILayout.Width(100)))
            {
                OptimizeAllTextures();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Audio Optimization
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🔊 Audio Optimization", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        optimizeAudio = EditorGUILayout.Toggle("Enable Audio Optimization", optimizeAudio);
        
        if (optimizeAudio)
        {
            EditorGUI.indentLevel++;
            audioFormat = (AudioCompressionFormat)EditorGUILayout.EnumPopup("Compression Format", audioFormat);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔍 Analyze Audio", GUILayout.Width(130)))
            {
                AnalyzeAudio();
            }
            if (GUILayout.Button("⚡ Optimize All", GUILayout.Width(100)))
            {
                OptimizeAllAudio();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Build Size Analysis
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📊 Build Size Analysis", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        stripUnusedAssets = EditorGUILayout.Toggle("Strip Unused Assets", stripUnusedAssets);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📈 Analyze Build Size", GUILayout.Width(150)))
        {
            AnalyzeBuildSize();
        }
        if (GUILayout.Button("🧹 Clean Unused Assets", GUILayout.Width(150)))
        {
            CleanUnusedAssets();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Platform-specific optimizations
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🎯 Platform-Specific Optimizations", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🌐 Optimize for WebGL"))
        {
            OptimizeForWebGL();
        }
        if (GUILayout.Button("🤖 Optimize for Android"))
        {
            OptimizeForAndroid();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawVersionControlTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.8f, 0.2f, 0.6f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📦 Version Management", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Manage version numbers and build increments", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Current Version Info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📋 Current Version Information", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Bundle Version:", GUILayout.Width(100));
        GUIStyle versionStyle = new GUIStyle(EditorStyles.boldLabel);
        versionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        GUILayout.Label(PlayerSettings.bundleVersion, versionStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Build Number:", GUILayout.Width(100));
        GUILayout.Label(PlayerSettings.Android.bundleVersionCode.ToString(), versionStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Product Name:", GUILayout.Width(100));
        GUILayout.Label(PlayerSettings.productName, versionStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Version Editor
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("✏️ Edit Version", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        versionNumber = EditorGUILayout.TextField("Version Number", versionNumber);
        buildNumber = EditorGUILayout.IntField("Build Number", buildNumber);
        versionSuffix = EditorGUILayout.TextField("Version Suffix", versionSuffix);
        
        EditorGUILayout.Space(5);
        
        autoIncrementBuild = EditorGUILayout.Toggle("Auto Increment Build", autoIncrementBuild);
        
        if (autoIncrementBuild)
        {
            EditorGUILayout.HelpBox("💡 Build number will automatically increment on each build", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        
        // Preview
        string previewVersion = versionNumber;
        if (!string.IsNullOrEmpty(versionSuffix))
        {
            previewVersion += $"-{versionSuffix}";
        }
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Preview:", GUILayout.Width(60));
        GUIStyle previewStyle = new GUIStyle(EditorStyles.boldLabel);
        previewStyle.normal.textColor = new Color(0.6f, 0.9f, 0.2f);
        GUILayout.Label($"v{previewVersion} (Build {buildNumber})", previewStyle);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Version Actions
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("💾 Apply Version", GUILayout.Width(120)))
        {
            ApplyVersionSettings();
        }
        
        GUI.backgroundColor = new Color(0.9f, 0.7f, 0.2f);
        if (GUILayout.Button("🔄 Reset to Current", GUILayout.Width(120)))
        {
            LoadVersionInfo();
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Version History
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📚 Version History", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // This would require implementing version history storage
        EditorGUILayout.HelpBox("Version history tracking coming soon!", MessageType.Info);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("📝 Add Release Notes"))
        {
            // Open release notes editor
            EditorUtility.DisplayDialog("Release Notes", "Release notes editor coming soon!", "OK");
        }
        if (GUILayout.Button("🏷️ Create Version Tag"))
        {
            // Create git tag if available
            CreateVersionTag();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Semantic Versioning Helper
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🔢 Semantic Versioning Helper", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔧 Patch\n(Bug fixes)", GUILayout.Height(40)))
        {
            IncrementVersion(VersionType.Patch);
        }
        if (GUILayout.Button("✨ Minor\n(New features)", GUILayout.Height(40)))
        {
            IncrementVersion(VersionType.Minor);
        }
        if (GUILayout.Button("🚀 Major\n(Breaking changes)", GUILayout.Height(40)))
        {
            IncrementVersion(VersionType.Major);
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawQuickBuildTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🚀 Quick Build", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Fast build configuration for development and testing", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Current Platform Status
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🎯 Current Build Target", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"{GetPlatformIcon(EditorUserBuildSettings.activeBuildTarget)} {EditorUserBuildSettings.activeBuildTarget}", 
                       new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 });
        GUILayout.FlexibleSpace();
        
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
        {
            GUI.backgroundColor = new Color(0.9f, 0.7f, 0.2f);
            if (GUILayout.Button("🌐 Switch to WebGL", GUILayout.Width(130)))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }
            GUI.backgroundColor = Color.white;
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Quick Build Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("⚙️ Build Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        developmentBuild = EditorGUILayout.Toggle("🔧 Development Build", developmentBuild);
        
        if (developmentBuild)
        {
            EditorGUI.indentLevel++;
            scriptDebugging = EditorGUILayout.Toggle("🐛 Script Debugging", scriptDebugging);
            deepProfiling = EditorGUILayout.Toggle("📊 Deep Profiling", deepProfiling);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("💡 Development builds are larger but include debugging capabilities", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Build Path
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📁 Build Location", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        lastBuildPath = EditorGUILayout.TextField("Build Path", lastBuildPath);
        if (GUILayout.Button("📁", GUILayout.Width(30)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Build Folder", lastBuildPath, "Build");
            if (!string.IsNullOrEmpty(path))
            {
                lastBuildPath = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        if (string.IsNullOrEmpty(lastBuildPath))
        {
            lastBuildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // Build Actions
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(10);
        
        GUILayout.Label("🚀 Build Actions", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);
        
        // Main build button
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("🚀 BUILD NOW", GUILayout.Height(50)))
        {
            PerformQuickBuild();
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space(10);
        
        // Additional actions
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("🧹 Clean Build", GUILayout.Height(35)))
        {
            CleanBuildFolder();
        }
        
        GUI.backgroundColor = new Color(1f, 0.9f, 0.7f);
        if (GUILayout.Button("📂 Open Build Folder", GUILayout.Height(35)))
        {
            if (Directory.Exists(lastBuildPath))
            {
                EditorUtility.RevealInFinder(lastBuildPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Folder Not Found", "Build folder doesn't exist yet. Build first!", "OK");
            }
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Build Statistics
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📊 Build Statistics", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        if (Directory.Exists(lastBuildPath))
        {
            var buildInfo = GetBuildInfo(lastBuildPath);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Last Build:", GUILayout.Width(80));
            GUILayout.Label(buildInfo.lastBuildTime, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Build Size:", GUILayout.Width(80));
            GUILayout.Label(buildInfo.buildSize, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("File Count:", GUILayout.Width(80));
            GUILayout.Label(buildInfo.fileCount.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("No build found. Build first to see statistics!", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    // Helper Methods
    private void CreateNewProfile(string name)
    {
        var profile = new BuildProfile
        {
            name = name,
            buildTarget = EditorUserBuildSettings.activeBuildTarget,
            developmentBuild = EditorUserBuildSettings.development,
            scriptDebugging = EditorUserBuildSettings.allowDebugging,
            version = PlayerSettings.bundleVersion,
            buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds", name)
        };
        
        buildProfiles.Add(profile);
        selectedProfile = profile;
    }
    
    private void BuildWithProfile(BuildProfile profile)
    {
        if (EditorUtility.DisplayDialog("Build Confirmation", 
            $"Build with profile '{profile.name}'?\n\nTarget: {profile.buildTarget}\nDevelopment: {profile.developmentBuild}", 
            "Build", "Cancel"))
        {
            ApplyProfileSettings(profile);
            PerformBuild(profile.buildPath, profile.buildTarget);
        }
        }
    
    private void ApplyProfileSettings(BuildProfile profile)
    {
        EditorUserBuildSettings.development = profile.developmentBuild;
        EditorUserBuildSettings.allowDebugging = profile.scriptDebugging;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = profile.deepProfiling;
        
        if (!string.IsNullOrEmpty(profile.version))
        {
            PlayerSettings.bundleVersion = profile.version;
        }
        
        if (autoIncrementBuild)
        {
            PlayerSettings.Android.bundleVersionCode++;
            PlayerSettings.iOS.buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
        }
    }
    
    private void ApplyPlatformSettings(BuildTarget target, PlatformConfig config)
    {
        switch (target)
        {
            case BuildTarget.WebGL:
                PlayerSettings.WebGL.template = config.webGLTemplate;
                PlayerSettings.WebGL.compressionFormat = config.webGLCompressionFormat;
                PlayerSettings.WebGL.linkerTarget = config.webGLLinkerTarget;
                PlayerSettings.WebGL.dataCaching = config.webGLDataCaching;
                PlayerSettings.WebGL.decompressionFallback = config.webGLDecompressionFallback;
                PlayerSettings.WebGL.memorySize = config.webGLMemorySize;
                break;
                
            case BuildTarget.Android:
                PlayerSettings.Android.targetArchitectures = config.androidTargetArchitecture;
                PlayerSettings.Android.minSdkVersion = config.androidMinSdkVersion;
                PlayerSettings.Android.targetSdkVersion = config.androidTargetSdkVersion;
                EditorUserBuildSettings.androidBuildSystem = config.androidBuildSystem;
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, config.androidScriptingBackend);
                PlayerSettings.Android.targetArchitectures = config.androidTargetDevice;
                
                if (!string.IsNullOrEmpty(config.androidKeystoreName))
                {
                    PlayerSettings.Android.keystoreName = config.androidKeystoreName;
                    PlayerSettings.Android.keystorePass = config.androidKeystorePass;
                    PlayerSettings.Android.keyaliasName = config.androidKeyaliasName;
                    PlayerSettings.Android.keyaliasPass = config.androidKeyaliasPass;
                }
                break;
                
            case BuildTarget.StandaloneWindows64:
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, config.windowsScriptingBackend);
                PlayerSettings.fullScreenMode = config.windowsFullscreenMode;
                PlayerSettings.resizableWindow = config.windowsResizableWindow;
                PlayerSettings.visibleInBackground = config.windowsVisibleInBackground;
                PlayerSettings.allowFullscreenSwitch = config.windowsAllowFullscreenSwitch;
                break;
                
            case BuildTarget.iOS:
                PlayerSettings.iOS.targetDevice = config.iOSTargetDevice;
                PlayerSettings.iOS.targetOSVersionString = config.iOSTargetOSVersion.ToString();
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, config.iOSScriptingBackend);
                PlayerSettings.iOS.appleDeveloperTeamID = config.iOSAppleDeveloperTeamID;
                PlayerSettings.iOS.iOSManualProvisioningProfileID = config.iOSProvisioningProfileID;
                break;
        }
    }
    
    private void PerformQuickBuild()
    {
        EditorUserBuildSettings.development = developmentBuild;
        EditorUserBuildSettings.allowDebugging = scriptDebugging;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = deepProfiling;
        
        if (autoIncrementBuild)
        {
            PlayerSettings.Android.bundleVersionCode++;
            PlayerSettings.iOS.buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
        }
        
        PerformBuild(lastBuildPath, EditorUserBuildSettings.activeBuildTarget);
    }
    
    private void PerformBuild(string buildPath, BuildTarget target)
    {
        if (string.IsNullOrEmpty(buildPath))
        {
            EditorUtility.DisplayDialog("Error", "Build path cannot be empty!", "OK");
            return;
        }
        
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();
        
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No scenes found in build settings!", "OK");
            return;
        }
        
        string fileName = PlayerSettings.productName;
        string fullPath = Path.Combine(buildPath, fileName);
        
        // Add platform-specific extensions
        switch (target)
        {
            case BuildTarget.StandaloneWindows64:
                fullPath += ".exe";
                break;
            case BuildTarget.Android:
                fullPath += ".apk";
                break;
        }
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = fullPath,
            target = target,
            targetGroup = BuildPipeline.GetBuildTargetGroup(target)
        };
        
        // Set build options based on current settings
        if (EditorUserBuildSettings.development)
        {
            buildOptions.options |= BuildOptions.Development;
        }
        
        if (EditorUserBuildSettings.allowDebugging)
        {
            buildOptions.options |= BuildOptions.AllowDebugging;
        }
        
        if (EditorUserBuildSettings.buildWithDeepProfilingSupport)
        {
            buildOptions.options |= BuildOptions.EnableDeepProfilingSupport;
        }
        
        Debug.Log($"Starting build for {target} at {fullPath}");
        
        var report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded! Size: {GetFileSizeString((long)report.summary.totalSize)}");
            
            if (EditorUtility.DisplayDialog("Build Complete", 
                $"Build completed successfully!\n\nSize: {GetFileSizeString((long)report.summary.totalSize)}\nTime: {report.summary.totalTime:mm\\:ss}\n\nOpen build folder?", 
                "Open Folder", "Close"))
            {
                EditorUtility.RevealInFinder(fullPath);
            }
        }
        else
        {
            Debug.LogError($"Build failed: {report.summary.result}");
            EditorUtility.DisplayDialog("Build Failed", $"Build failed with result: {report.summary.result}", "OK");
        }
    }    
    private void CleanBuildFolder()
    {
        if (string.IsNullOrEmpty(lastBuildPath) || !Directory.Exists(lastBuildPath))
        {
            EditorUtility.DisplayDialog("Nothing to Clean", "Build folder doesn't exist!", "OK");
            return;
        }
        
        if (EditorUtility.DisplayDialog("Clean Build Folder", 
            $"Delete all files in:\n{lastBuildPath}\n\nThis cannot be undone!", 
            "Delete", "Cancel"))
        {
            try
            {
                Directory.Delete(lastBuildPath, true);
                Directory.CreateDirectory(lastBuildPath);
                EditorUtility.DisplayDialog("Clean Complete", "Build folder has been cleaned!", "OK");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Clean Failed", $"Failed to clean build folder:\n{e.Message}", "OK");
            }
        }
    }
    
    private void ApplyVersionSettings()
    {
        PlayerSettings.bundleVersion = versionNumber + (string.IsNullOrEmpty(versionSuffix) ? "" : $"-{versionSuffix}");
        PlayerSettings.Android.bundleVersionCode = buildNumber;
        PlayerSettings.iOS.buildNumber = buildNumber.ToString();
        
        EditorUtility.DisplayDialog("Version Applied", 
            $"Version updated to: {PlayerSettings.bundleVersion}\nBuild number: {buildNumber}", "OK");
    }
    
    private void IncrementVersion(VersionType type)
    {
        var parts = versionNumber.Split('.');
        if (parts.Length != 3)
        {
            EditorUtility.DisplayDialog("Invalid Version", "Version must be in format X.Y.Z", "OK");
            return;
        }
        
        int major = int.Parse(parts[0]);
        int minor = int.Parse(parts[1]);
        int patch = int.Parse(parts[2]);
        
        switch (type)
        {
            case VersionType.Major:
                major++;
                minor = 0;
                patch = 0;
                break;
            case VersionType.Minor:
                minor++;
                patch = 0;
                break;
            case VersionType.Patch:
                patch++;
                break;
        }
        
        versionNumber = $"{major}.{minor}.{patch}";
        buildNumber++;
    }
    
    private void CreateVersionTag()
    {
        // This would integrate with version control systems
        EditorUtility.DisplayDialog("Version Tag", "Version control integration coming soon!", "OK");
    }
    
    // Asset Optimization Methods
    private void AnalyzeTextures()
    {
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");
        int totalTextures = textureGuids.Length;
        long totalSize = 0;
        
        foreach (string guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FileInfo fileInfo = new FileInfo(path);
            totalSize += fileInfo.Length;
        }
        
        EditorUtility.DisplayDialog("Texture Analysis", 
            $"Found {totalTextures} textures\nTotal size: {GetFileSizeString(totalSize)}", "OK");
    }
    
    private void OptimizeAllTextures()
    {
        if (EditorUtility.DisplayDialog("Optimize Textures", 
            "This will modify all texture import settings. Continue?", "Optimize", "Cancel"))
        {
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");
            int optimized = 0;
            
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                
                if (importer != null)
                {
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.SaveAndReimport();
                    optimized++;
                }
            }
            
            EditorUtility.DisplayDialog("Optimization Complete", $"Optimized {optimized} textures", "OK");
        }
    }
    
    private void AnalyzeAudio()
    {
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip");
        EditorUtility.DisplayDialog("Audio Analysis", $"Found {audioGuids.Length} audio clips", "OK");
    }
    
    private void OptimizeAllAudio()
    {
        EditorUtility.DisplayDialog("Audio Optimization", "Audio optimization coming soon!", "OK");
    }
    
    private void AnalyzeBuildSize()
    {
        EditorUtility.DisplayDialog("Build Size Analysis", "Build size analysis coming soon!", "OK");
    }
    
    private void CleanUnusedAssets()
    {
        EditorUtility.DisplayDialog("Clean Unused Assets", "Unused asset cleanup coming soon!", "OK");
    }
    
    private void OptimizeForWebGL()
    {
        if (EditorUtility.DisplayDialog("WebGL Optimization", 
            "Apply WebGL-specific optimizations?", "Apply", "Cancel"))
        {
            // Apply WebGL optimizations
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
            PlayerSettings.WebGL.memorySize = 512;
            PlayerSettings.stripEngineCode = true;
            
            EditorUtility.DisplayDialog("Optimization Applied", "WebGL optimizations have been applied!", "OK");
        }
    }
    
    private void OptimizeForAndroid()
    {
        if (EditorUtility.DisplayDialog("Android Optimization", 
            "Apply Android-specific optimizations?", "Apply", "Cancel"))
        {
            // Apply Android optimizations
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.stripEngineCode = true;
            
            EditorUtility.DisplayDialog("Optimization Applied", "Android optimizations have been applied!", "OK");
        }
    }
    
    // Utility Methods
    private string GetPlatformIcon(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.WebGL: return "🌐";
            case BuildTarget.Android: return "🤖";
            case BuildTarget.iOS: return "📱";
            case BuildTarget.StandaloneWindows64: return "🖥️";
            case BuildTarget.StandaloneOSX: return "🍎";
            case BuildTarget.StandaloneLinux64: return "🐧";
            default: return "🎯";
        }
    }
    
    private Color GetPlatformColor(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.WebGL: return new Color(0.2f, 0.6f, 1f);
            case BuildTarget.Android: return new Color(0.6f, 1f, 0.2f);
            case BuildTarget.iOS: return new Color(0.8f, 0.8f, 0.8f);
            case BuildTarget.StandaloneWindows64: return new Color(0.2f, 0.8f, 1f);
            default: return Color.white;
        }
    }
    
    private string GetFileSizeString(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }
    
    private BuildInfo GetBuildInfo(string buildPath)
    {
        var info = new BuildInfo();
        
        if (Directory.Exists(buildPath))
        {
            var directory = new DirectoryInfo(buildPath);
            info.lastBuildTime = directory.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
            
            var files = directory.GetFiles("*", SearchOption.AllDirectories);
            info.fileCount = files.Length;
            info.buildSize = GetFileSizeString(files.Sum(f => f.Length));
        }
        else
        {
            info.lastBuildTime = "Never";
            info.buildSize = "0 B";
            info.fileCount = 0;
        }
        
        return info;
    }
    
    private void DrawSeparator()
    {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(5);
    }
    
    // Data persistence methods
    private void LoadBuildProfiles()
    {
        string json = EditorPrefs.GetString("BuildConfigManager_Profiles", "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var wrapper = JsonUtility.FromJson<BuildProfileWrapper>(json);
                buildProfiles = wrapper.profiles?.ToList() ?? new List<BuildProfile>();
            }
            catch
            {
                buildProfiles = new List<BuildProfile>();
            }
        }
    }
    
    private void SaveBuildProfiles()
    {
        var wrapper = new BuildProfileWrapper { profiles = buildProfiles.ToArray() };
        string json = JsonUtility.ToJson(wrapper);
        EditorPrefs.SetString("BuildConfigManager_Profiles", json);
    }
    
    private void LoadPlatformConfigs()
    {
        string json = EditorPrefs.GetString("BuildConfigManager_PlatformConfigs", "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var wrapper = JsonUtility.FromJson<PlatformConfigWrapper>(json);
                platformConfigs = wrapper.configs?.ToDictionary(c => c.target, c => c.config) ?? new Dictionary<BuildTarget, PlatformConfig>();
            }
            catch
            {
                platformConfigs = new Dictionary<BuildTarget, PlatformConfig>();
            }
        }
    }
    
    private void SavePlatformConfigs()
    {
        var configList = platformConfigs.Select(kvp => new PlatformConfigEntry { target = kvp.Key, config = kvp.Value }).ToArray();
        var wrapper = new PlatformConfigWrapper { configs = configList };
        string json = JsonUtility.ToJson(wrapper);
        EditorPrefs.SetString("BuildConfigManager_PlatformConfigs", json);
    }
    
    private void LoadVersionInfo()
    {
        versionNumber = PlayerSettings.bundleVersion;
        buildNumber = PlayerSettings.Android.bundleVersionCode;
        autoIncrementBuild = EditorPrefs.GetBool("BuildConfigManager_AutoIncrement", true);
        lastBuildPath = EditorPrefs.GetString("BuildConfigManager_LastBuildPath", 
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds"));
    }
    
    private void SaveVersionInfo()
    {
        EditorPrefs.SetBool("BuildConfigManager_AutoIncrement", autoIncrementBuild);
        EditorPrefs.SetString("BuildConfigManager_LastBuildPath", lastBuildPath);
    }
}

// Data classes for serialization and configuration
[System.Serializable]
public class BuildProfile
{
    public string name = "";
    public BuildTarget buildTarget = BuildTarget.WebGL;
    public bool developmentBuild = true;
    public bool scriptDebugging = true;
    public bool deepProfiling = false;
    public BuildCompression compressionType = BuildCompression.LZ4;
    public string version = "";
    public string buildPath = "";
    public bool showAdvanced = false;
}[System.Serializable]
public class BuildProfileWrapper
{
    public BuildProfile[] profiles;
}

[System.Serializable]
public class PlatformConfig
{
    // WebGL Settings
    public string webGLTemplate = "APPLICATION:Default";
    public WebGLCompressionFormat webGLCompressionFormat = WebGLCompressionFormat.Gzip;
    public WebGLLinkerTarget webGLLinkerTarget = WebGLLinkerTarget.Wasm;
    public int webGLOptimizationLevel = 2;
    public bool webGLDevelopmentBuild = true;
    public bool webGLAutoconnectProfiler = false;
    public bool webGLScriptDebugging = true;
    public bool webGLDataCaching = true;
    public bool webGLDecompressionFallback = false;
    public int webGLMemorySize = 512;
    
    // Android Settings
    public AndroidArchitecture androidTargetArchitecture = AndroidArchitecture.ARM64;
    public AndroidSdkVersions androidMinSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
    public AndroidSdkVersions androidTargetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
    public AndroidBuildSystem androidBuildSystem = AndroidBuildSystem.Gradle;
    public ScriptingImplementation androidScriptingBackend = ScriptingImplementation.IL2CPP;
    public AndroidArchitecture androidTargetDevice = AndroidArchitecture.All;
    public string androidKeystoreName = "";
    public string androidKeystorePass = "";
    public string androidKeyaliasName = "";
    public string androidKeyaliasPass = "";
    
    // Windows Settings
    public BuildTarget windowsArchitecture = BuildTarget.StandaloneWindows64;
    public ScriptingImplementation windowsScriptingBackend = ScriptingImplementation.Mono2x;
    public FullScreenMode windowsFullscreenMode = FullScreenMode.Windowed;
    public bool windowsResizableWindow = true;
    public bool windowsVisibleInBackground = false;
    public bool windowsAllowFullscreenSwitch = true;
    
    // iOS Settings
    public iOSTargetDevice iOSTargetDevice = iOSTargetDevice.iPhoneAndiPad;
    public string iOSTargetOSVersion = "11.0";
    public ScriptingImplementation iOSScriptingBackend = ScriptingImplementation.IL2CPP;
    public string iOSAppleDeveloperTeamID = "";
    public string iOSProvisioningProfileID = "";
    public string iOSSigningTeamID = "";
}[System.Serializable]public class PlatformConfigEntry
{
    public BuildTarget target;
    public PlatformConfig config;
}

[System.Serializable]
public class PlatformConfigWrapper
{
    public PlatformConfigEntry[] configs;
}

public class BuildInfo
{
    public string lastBuildTime;
    public string buildSize;
    public int fileCount;
}

public enum VersionType
{
    Major,
    Minor,
    Patch
}

#endif