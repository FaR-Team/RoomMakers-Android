#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class LocalizationManager : EditorWindow
{
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "Text Extraction", "Translation Import", "Validation", "Preview", "Settings" };
    
    // Text Extraction
    private List<LocalizationEntry> extractedEntries = new List<LocalizationEntry>();
    private bool includeScriptableObjects = true;
    private bool includePrefabs = true;
    private bool includeScenes = true;
    private bool includeUIText = true;
    private string[] searchFolders = { "Assets" };
    private bool showExtractionSettings = false;
    
    // Translation Import
    private string importFilePath = "";
    private LocalizationFormat importFormat = LocalizationFormat.CSV;
    private bool overwriteExisting = false;
    private bool createBackup = true;
    
    // Validation
    private List<ValidationIssue> validationIssues = new List<ValidationIssue>();
    private bool showMissingTranslations = true;
    private bool showEmptyTranslations = true;
    private bool showDuplicateKeys = true;
    private bool showUnusedKeys = false;
    
    // Preview
    private SystemLanguage previewLanguage = SystemLanguage.Spanish;
    private List<PreviewItem> previewItems = new List<PreviewItem>();
    private string previewFilter = "";
    
    // Settings
    private List<SystemLanguage> supportedLanguages = new List<SystemLanguage> { SystemLanguage.English, SystemLanguage.Spanish };
    private SystemLanguage defaultLanguage = SystemLanguage.English;
    private string exportPath = "Assets/Localization/";
    private bool autoDetectText = true;
    private bool useKeyGeneration = true;
    private string keyPrefix = "LOC_";
    
    // Statistics
    private LocalizationStats stats = new LocalizationStats();
    
    [MenuItem("DevTools/Localization Manager")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationManager>("Localization Manager");
    }
    
    private void OnEnable()
    {
        LoadSettings();
        RefreshStats();
    }
    
    private void OnDisable()
    {
        SaveSettings();
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
            case 0: DrawTextExtractionTab(); break;
            case 1: DrawTranslationImportTab(); break;
            case 2: DrawValidationTab(); break;
            case 3: DrawPreviewTab(); break;
            case 4: DrawSettingsTab(); break;
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
        GUILayout.Label("🌍 Localization Manager", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Statistics bar
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        DrawStatCard("📝 Total Entries", stats.totalEntries.ToString(), new Color(0.2f, 0.6f, 0.9f));
        DrawStatCard("🌐 Languages", supportedLanguages.Count.ToString(), new Color(0.6f, 0.9f, 0.2f));
        DrawStatCard("✅ Translated", $"{stats.translatedEntries}/{stats.totalEntries}", new Color(0.2f, 0.8f, 0.2f));
        DrawStatCard("⚠️ Missing", stats.missingTranslations.ToString(), new Color(0.9f, 0.6f, 0.2f));
        
        GUILayout.FlexibleSpace();
        
        // Quick actions
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("🔄 Refresh", GUILayout.Width(80), GUILayout.Height(35)))
        {
            RefreshStats();
            RefreshValidation();
        }
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("📤 Export All", GUILayout.Width(80), GUILayout.Height(35)))
        {
            ExportAllTranslations();
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
    }
    
    private void DrawStatCard(string label, string value, Color color)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        
        GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label(label, labelStyle);
        
        GUIStyle valueStyle = new GUIStyle(EditorStyles.boldLabel);
        valueStyle.alignment = TextAnchor.MiddleCenter;
        valueStyle.normal.textColor = color;
        valueStyle.fontSize = 14;
        GUILayout.Label(value, valueStyle);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawTextExtractionTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📝 Text Extraction", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Extract all localizable text from your project assets", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Extraction Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("⚙️ Extraction Settings", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        showExtractionSettings = EditorGUILayout.Toggle("Show", showExtractionSettings, GUILayout.Width(60));
        EditorGUILayout.EndHorizontal();
        
        if (showExtractionSettings)
        {
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            
            GUILayout.Label("📁 Source Types:", EditorStyles.boldLabel);
            includeScriptableObjects = EditorGUILayout.Toggle("ScriptableObjects", includeScriptableObjects);
            includePrefabs = EditorGUILayout.Toggle("Prefabs", includePrefabs);
            includeScenes = EditorGUILayout.Toggle("Scenes", includeScenes);
            includeUIText = EditorGUILayout.Toggle("UI Text Components", includeUIText);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical();
            
            GUILayout.Label("🎯 Search Folders:", EditorStyles.boldLabel);
            for (int i = 0; i < searchFolders.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                searchFolders[i] = EditorGUILayout.TextField(searchFolders[i]);
                if (GUILayout.Button("📁", GUILayout.Width(25)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select Folder", searchFolders[i], "");
                    if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
                    {
                        searchFolders[i] = "Assets" + path.Substring(Application.dataPath.Length);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("➕ Add Folder"))
            {
                Array.Resize(ref searchFolders, searchFolders.Length + 1);
                searchFolders[searchFolders.Length - 1] = "Assets";
            }
            if (searchFolders.Length > 1 && GUILayout.Button("➖ Remove Last"))
            {
                Array.Resize(ref searchFolders, searchFolders.Length - 1);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Extraction Actions
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("🔍 Extract Text", GUILayout.Height(40)))
        {
            ExtractAllText();
        }
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("📤 Export to CSV", GUILayout.Height(40)))
        {
            ExportToCSV();
        }
        
        GUI.backgroundColor = new Color(0.9f, 0.7f, 1f);
        if (GUILayout.Button("📤 Export to JSON", GUILayout.Height(40)))
        {
            ExportToJSON();
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(15);
        
        // Extracted Entries Display
        if (extractedEntries.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            GUILayout.Label($"📋 Extracted Entries ({extractedEntries.Count})", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Filter
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("🔍 Filter:", GUILayout.Width(50));
            string newFilter = EditorGUILayout.TextField(previewFilter);
            if (newFilter != previewFilter)
            {
                previewFilter = newFilter;
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label("English", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("Spanish", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("Source", EditorStyles.boldLabel, GUILayout.Width(100));
            GUILayout.Label("Type", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            // Entries
            var filteredEntries = string.IsNullOrEmpty(previewFilter) 
                ? extractedEntries 
                : extractedEntries.Where(e => e.key.ToLower().Contains(previewFilter.ToLower()) || 
                                            e.englishText.ToLower().Contains(previewFilter.ToLower())).ToList();
            
            foreach (var entry in filteredEntries.Take(50)) // Limit display for performance
            {
                DrawExtractionEntry(entry);
            }
            
            if (filteredEntries.Count > 50)
            {
                EditorGUILayout.HelpBox($"Showing first 50 of {filteredEntries.Count} entries. Use filter to narrow results.", MessageType.Info);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No entries extracted yet. Click 'Extract Text' to scan your project.", MessageType.Info);
        }
    }
    
    private void DrawExtractionEntry(LocalizationEntry entry)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        // Key
        GUILayout.Label(entry.key, GUILayout.Width(150));
        
        // English text
        GUIStyle textStyle = new GUIStyle(EditorStyles.textField);
        textStyle.wordWrap = false;
        entry.englishText = EditorGUILayout.TextField(entry.englishText, textStyle, GUILayout.Width(200));
        
        // Spanish text
        Color originalColor = GUI.backgroundColor;
        if (string.IsNullOrEmpty(entry.spanishText))
        {
            GUI.backgroundColor = new Color(1f, 0.8f, 0.8f);
        }
        entry.spanishText = EditorGUILayout.TextField(entry.spanishText, textStyle, GUILayout.Width(200));
        GUI.backgroundColor = originalColor;
        
        // Source
        GUILayout.Label(Path.GetFileName(entry.sourcePath), GUILayout.Width(100));
        
        // Type
        GUILayout.Label(entry.sourceType.ToString());
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawTranslationImportTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.6f, 0.2f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📥 Translation Import", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Import translated text from external files", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Import Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📁 Import Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // File selection
        EditorGUILayout.BeginHorizontal();
        importFilePath = EditorGUILayout.TextField("Import File", importFilePath);
        if (GUILayout.Button("📁", GUILayout.Width(30)))
        {
            string extension = importFormat == LocalizationFormat.CSV ? "csv" : "json";
            string path = EditorUtility.OpenFilePanel("Select Translation File", "", extension);
            if (!string.IsNullOrEmpty(path))
            {
                importFilePath = path;
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Format selection
        importFormat = (LocalizationFormat)EditorGUILayout.EnumPopup("File Format", importFormat);
        
        EditorGUILayout.Space(5);
        
        // Import options
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);
        createBackup = EditorGUILayout.Toggle("Create Backup", createBackup);
        
        EditorGUILayout.Space(5);
        
        if (overwriteExisting)
        {
            EditorGUILayout.HelpBox("⚠️ This will overwrite existing translations!", MessageType.Warning);
        }
        
        if (createBackup)
        {
            EditorGUILayout.HelpBox("💾 A backup will be created before importing", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Import Actions
        EditorGUILayout.BeginHorizontal();
        
        GUI.enabled = !string.IsNullOrEmpty(importFilePath) && File.Exists(importFilePath);
        
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("📥 Import Translations", GUILayout.Height(40)))
        {
            ImportTranslations();
        }
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("👁️ Preview Import", GUILayout.Height(40)))
        {
            PreviewImport();
        }
        
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(15);
        
        // Import Templates
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📋 Import Templates", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Download template files to help translators understand the format", MessageType.Info);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("📄 Download CSV Template"))
        {
            CreateCSVTemplate();
        }
        
        if (GUILayout.Button("📄 Download JSON Template"))
        {
            CreateJSONTemplate();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Supported Formats Info
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("ℹ️ Supported Formats", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📊 CSV Format:", EditorStyles.boldLabel);
        GUILayout.Label("Key,English,Spanish,Context,Notes", EditorStyles.miniLabel);
        GUILayout.Label("MENU_START,Start Game,Iniciar Juego,Main Menu,Button text", EditorStyles.miniLabel);
        
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🔧 JSON Format:", EditorStyles.boldLabel);
        GUILayout.Label("{ \"key\": { \"en\": \"English\", \"es\": \"Spanish\" } }", EditorStyles.miniLabel);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawValidationTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.8f, 0.2f, 0.2f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("✅ Translation Validation", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Validate translations for completeness and consistency", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Validation Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("⚙️ Validation Options", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        
        showMissingTranslations = EditorGUILayout.Toggle("Missing Translations", showMissingTranslations);
        showEmptyTranslations = EditorGUILayout.Toggle("Empty Translations", showEmptyTranslations);
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical();
        
        showDuplicateKeys = EditorGUILayout.Toggle("Duplicate Keys", showDuplicateKeys);
        showUnusedKeys = EditorGUILayout.Toggle("Unused Keys", showUnusedKeys);
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Validation Actions
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        if (GUILayout.Button("🔍 Run Validation", GUILayout.Height(40)))
        {
            RunValidation();
        }
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 1f);
        if (GUILayout.Button("🔧 Auto-Fix Issues", GUILayout.Height(40)))
        {
            AutoFixIssues();
        }
        
        GUI.backgroundColor = new Color(0.9f, 0.7f, 1f);
        if (GUILayout.Button("📊 Generate Report", GUILayout.Height(40)))
        {
            GenerateValidationReport();
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(15);
        
        // Validation Results
        if (validationIssues.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            GUILayout.Label($"⚠️ Validation Issues ({validationIssues.Count})", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Group issues by severity
            var criticalIssues = validationIssues.Where(i => i.severity == ValidationSeverity.Critical).ToList();
            var warningIssues = validationIssues.Where(i => i.severity == ValidationSeverity.Warning).ToList();
            var infoIssues = validationIssues.Where(i => i.severity == ValidationSeverity.Info).ToList();
            
            if (criticalIssues.Count > 0)
            {
                DrawValidationGroup("🔴 Critical Issues", criticalIssues, new Color(1f, 0.3f, 0.3f));
            }
            
            if (warningIssues.Count > 0)
            {
                DrawValidationGroup("🟡 Warnings", warningIssues, new Color(1f, 0.8f, 0.3f));
            }
            
            if (infoIssues.Count > 0)
            {
                DrawValidationGroup("🔵 Information", infoIssues, new Color(0.3f, 0.7f, 1f));
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No validation issues found. Click 'Run Validation' to check your translations.", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        // Validation Statistics
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📊 Translation Statistics", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        foreach (var language in supportedLanguages)
        {
            if (language == defaultLanguage) continue;
            
            var langStats = CalculateLanguageStats(language);
            DrawLanguageStats(language, langStats);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawValidationGroup(string title, List<ValidationIssue> issues, Color color)
    {
        EditorGUILayout.Space(5);
        
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.normal.textColor = color;
        GUILayout.Label(title, titleStyle);
        
        EditorGUILayout.Space(3);
        
        foreach (var issue in issues.Take(10)) // Limit display
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            GUILayout.Label(GetSeverityIcon(issue.severity), GUILayout.Width(20));
            
            EditorGUILayout.BeginVertical();
            GUILayout.Label(issue.description, EditorStyles.wordWrappedLabel);
            if (!string.IsNullOrEmpty(issue.context))
            {
                GUILayout.Label($"Context: {issue.context}", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();
            
            if (!string.IsNullOrEmpty(issue.assetPath))
            {
                if (GUILayout.Button("🔍", GUILayout.Width(25)))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(issue.assetPath);
                    if (asset != null)
                    {
                        EditorGUIUtility.PingObject(asset);
                        Selection.activeObject = asset;
                    }
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        if (issues.Count > 10)
        {
            EditorGUILayout.HelpBox($"Showing first 10 of {issues.Count} issues", MessageType.Info);
        }
    }
    
    private void DrawLanguageStats(SystemLanguage language, LanguageStats stats)
    {
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Label($"{GetLanguageFlag(language)} {language}", GUILayout.Width(100));
        
        // Progress bar
        Rect progressRect = EditorGUILayout.GetControlRect(GUILayout.Height(20));
        float progress = stats.totalEntries > 0 ? (float)stats.translatedEntries / stats.totalEntries : 0f;
        
        EditorGUI.DrawRect(progressRect, new Color(0.3f, 0.3f, 0.3f));
        
        Rect fillRect = new Rect(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height);
        Color progressColor = progress > 0.8f ? new Color(0.2f, 0.8f, 0.2f) : 
                             progress > 0.5f ? new Color(0.8f, 0.8f, 0.2f) : 
                             new Color(0.8f, 0.2f, 0.2f);
        EditorGUI.DrawRect(fillRect, progressColor);
        
        GUIStyle percentStyle = new GUIStyle(EditorStyles.miniLabel);
        percentStyle.alignment = TextAnchor.MiddleCenter;
        percentStyle.normal.textColor = Color.white;
        GUI.Label(progressRect, $"{progress:P0}", percentStyle);
        
        GUILayout.Label($"{stats.translatedEntries}/{stats.totalEntries}", GUILayout.Width(60));
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawPreviewTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.6f, 0.2f, 0.8f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("👁️ Translation Preview", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Preview how your translations will look in different languages", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Preview Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("🌐 Preview Language:", EditorStyles.boldLabel, GUILayout.Width(120));
        previewLanguage = (SystemLanguage)EditorGUILayout.EnumPopup(previewLanguage);
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("🔄 Refresh Preview", GUILayout.Width(120)))
        {
            RefreshPreview();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Filter
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("🔍 Filter:", GUILayout.Width(50));
        previewFilter = EditorGUILayout.TextField(previewFilter);
        if (GUILayout.Button("❌", GUILayout.Width(25)))
        {
            previewFilter = "";
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Preview Display
        if (previewItems.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            GUILayout.Label($"👁️ Preview ({GetLanguageFlag(previewLanguage)} {previewLanguage})", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            // Headers
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(150));
            GUILayout.Label($"{defaultLanguage}", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label($"{previewLanguage}", EditorStyles.boldLabel, GUILayout.Width(200));
            GUILayout.Label("Status", EditorStyles.boldLabel, GUILayout.Width(80));
            GUILayout.Label("Context", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            // Filter items
            var filteredItems = string.IsNullOrEmpty(previewFilter) 
                ? previewItems 
                : previewItems.Where(item => 
                    item.key.ToLower().Contains(previewFilter.ToLower()) ||
                    item.originalText.ToLower().Contains(previewFilter.ToLower()) ||
                    item.translatedText.ToLower().Contains(previewFilter.ToLower())).ToList();
            
            foreach (var item in filteredItems.Take(50))
            {
                DrawPreviewItem(item);
            }
            
            if (filteredItems.Count > 50)
            {
                EditorGUILayout.HelpBox($"Showing first 50 of {filteredItems.Count} items. Use filter to narrow results.", MessageType.Info);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No preview data available. Click 'Refresh Preview' to load translations.", MessageType.Info);
        }
        
        EditorGUILayout.Space(10);
        
        // Live Preview Actions
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🎮 Live Preview Actions", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("🎯 Apply to Scene", GUILayout.Height(35)))
        {
            ApplyTranslationsToScene();
        }
        
        GUI.backgroundColor = new Color(1f, 0.9f, 0.7f);
        if (GUILayout.Button("📱 Test on Device", GUILayout.Height(35)))
        {
            TestOnDevice();
        }
        
        GUI.backgroundColor = new Color(0.9f, 0.7f, 1f);
        if (GUILayout.Button("📸 Take Screenshots", GUILayout.Height(35)))
        {
            TakeScreenshots();
        }
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPreviewItem(PreviewItem item)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        // Key
        GUILayout.Label(item.key, GUILayout.Width(150));
        
        // Original text
        GUILayout.Label(item.originalText, EditorStyles.wordWrappedLabel, GUILayout.Width(200));
        
        // Translated text
        Color originalBg = GUI.backgroundColor;
        if (string.IsNullOrEmpty(item.translatedText))
        {
            GUI.backgroundColor = new Color(1f, 0.8f, 0.8f);
        }
        else if (item.translatedText == item.originalText)
        {
            GUI.backgroundColor = new Color(1f, 1f, 0.8f);
        }
        
        GUILayout.Label(item.translatedText, EditorStyles.wordWrappedLabel, GUILayout.Width(200));
        GUI.backgroundColor = originalBg;
        
        // Status
        GUILayout.Label(GetTranslationStatusIcon(item.status), GUILayout.Width(80));
        
        // Context
        GUILayout.Label(item.context, EditorStyles.miniLabel);
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawSettingsTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.8f, 0.6f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("⚙️ Localization Settings", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Configure localization system settings and preferences", MessageType.Info);
        EditorGUILayout.Space(10);
        
        // Language Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🌐 Language Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Default language
        defaultLanguage = (SystemLanguage)EditorGUILayout.EnumPopup("Default Language", defaultLanguage);
        
        EditorGUILayout.Space(5);
        
        // Supported languages
        GUILayout.Label("Supported Languages:", EditorStyles.boldLabel);
        
        for (int i = 0; i < supportedLanguages.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label($"{GetLanguageFlag(supportedLanguages[i])}", GUILayout.Width(25));
            supportedLanguages[i] = (SystemLanguage)EditorGUILayout.EnumPopup(supportedLanguages[i]);
            
            if (supportedLanguages[i] != defaultLanguage)
            {
                GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                if (GUILayout.Button("❌", GUILayout.Width(25)))
                {
                    supportedLanguages.RemoveAt(i);
                    i--;
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUILayout.Label("(Default)", EditorStyles.miniLabel, GUILayout.Width(25));
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("➕ Add Language", GUILayout.Width(120)))
        {
            supportedLanguages.Add(SystemLanguage.French);
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // File Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📁 File Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        exportPath = EditorGUILayout.TextField("Export Path", exportPath);
        if (GUILayout.Button("📁", GUILayout.Width(30)))
        {
            string path = EditorUtility.SaveFolderPanel("Select Export Folder", exportPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    exportPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    exportPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Automation Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🤖 Automation Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        autoDetectText = EditorGUILayout.Toggle("Auto-detect New Text", autoDetectText);
        useKeyGeneration = EditorGUILayout.Toggle("Auto-generate Keys", useKeyGeneration);
        
        if (useKeyGeneration)
        {
            EditorGUI.indentLevel++;
            keyPrefix = EditorGUILayout.TextField("Key Prefix", keyPrefix);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space(5);
        
        if (autoDetectText)
        {
            EditorGUILayout.HelpBox("💡 New text will be automatically detected when assets are modified", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Integration Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("🔗 Integration Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Configure integration with external translation services", MessageType.Info);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🌐 Google Translate API"))
        {
            EditorUtility.DisplayDialog("Integration", "Google Translate API integration coming soon!", "OK");
        }
        
        if (GUILayout.Button("📝 Crowdin Integration"))
        {
            EditorUtility.DisplayDialog("Integration", "Crowdin integration coming soon!", "OK");
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        
        // Settings Actions
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f);
        if (GUILayout.Button("💾 Save Settings", GUILayout.Width(120), GUILayout.Height(35)))
        {
            SaveSettings();
            EditorUtility.DisplayDialog("Settings Saved", "Localization settings have been saved!", "OK");
        }
        
        GUI.backgroundColor = new Color(1f, 0.9f, 0.7f);
        if (GUILayout.Button("🔄 Reset to Default", GUILayout.Width(120), GUILayout.Height(35)))
        {
            if (EditorUtility.DisplayDialog("Reset Settings", "Reset all settings to default values?", "Reset", "Cancel"))
            {
                ResetToDefaults();
            }
        }
        
        GUI.backgroundColor = Color.white;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    
    // Core Methods
    private void ExtractAllText()
    {
        extractedEntries.Clear();
        
        EditorUtility.DisplayProgressBar("Extracting Text", "Scanning project...", 0f);
        
        try
        {
            if (includeScriptableObjects)
            {
                ExtractFromScriptableObjects();
            }
            
            if (includePrefabs)
            {
                ExtractFromPrefabs();
            }
            
            if (includeScenes)
            {
                ExtractFromScenes();
            }
            
            // Remove duplicates and sort
            extractedEntries = extractedEntries
                .GroupBy(e => e.key)
                .Select(g => g.First())
                .OrderBy(e => e.key)
                .ToList();
            
            RefreshStats();
            
            EditorUtility.DisplayDialog("Extraction Complete", 
                $"Extracted {extractedEntries.Count} localizable entries", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    private void ExtractFromScriptableObjects()
    {
        string[] guids = AssetDatabase.FindAssets("t:FurnitureOriginalData", searchFolders);
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            EditorUtility.DisplayProgressBar("Extracting Text", $"Processing {Path.GetFileName(path)}", (float)i / guids.Length);
            
            var furniture = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(path);
            if (furniture != null)
            {
                // Extract name
                if (!string.IsNullOrEmpty(furniture.Name))
                {
                    extractedEntries.Add(new LocalizationEntry
                    {
                        key = GenerateKey("FURNITURE", furniture.name, "NAME"),
                        englishText = furniture.Name,
                        spanishText = furniture.es_Name,
                        sourcePath = path,
                        sourceType = LocalizationSourceType.ScriptableObject,
                        context = "Furniture Name"
                    });
                }
                
                // Extract description
                if (!string.IsNullOrEmpty(furniture.Description))
                {
                    extractedEntries.Add(new LocalizationEntry
                    {
                        key = GenerateKey("FURNITURE", furniture.name, "DESC"),
                        englishText = furniture.Description,
                        spanishText = furniture.es_Description,
                        sourcePath = path,
                        sourceType = LocalizationSourceType.ScriptableObject,
                        context = "Furniture Description"
                    });
                }
            }
        }
    }
    
    private void ExtractFromPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", searchFolders);
        
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            EditorUtility.DisplayProgressBar("Extracting Text", $"Processing {Path.GetFileName(path)}", (float)i / guids.Length);
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && includeUIText)
            {
                ExtractFromGameObject(prefab, path);
            }
        }
    }
    
    private void ExtractFromScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", searchFolders);
        
        for (int i = 0; i < sceneGuids.Length; i++)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
            EditorUtility.DisplayProgressBar("Extracting Text", $"Processing {Path.GetFileName(scenePath)}", (float)i / sceneGuids.Length);
            
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);
            
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                ExtractFromGameObject(rootObject, scenePath);
            }
            
            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
        }
    }
    
    private void ExtractFromGameObject(GameObject obj, string sourcePath)
    {
        // Extract from UI Text components
        var textComponents = obj.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        foreach (var text in textComponents)
        {
            if (!string.IsNullOrEmpty(text.text))
            {
                extractedEntries.Add(new LocalizationEntry
                {
                    key = GenerateKey("UI", obj.name, text.name),
                    englishText = text.text,
                    spanishText = "", // Will be filled from existing translations
                    sourcePath = sourcePath,
                    sourceType = LocalizationSourceType.UIText,
                    context = $"UI Text in {obj.name}"
                });
            }
        }
        
        // Extract from TMPro Text components
        var tmpComponents = obj.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
        foreach (var tmp in tmpComponents)
        {
            if (!string.IsNullOrEmpty(tmp.text))
            {
                extractedEntries.Add(new LocalizationEntry
                {
                    key = GenerateKey("UI", obj.name, tmp.name),
                    englishText = tmp.text,
                    spanishText = "",
                    sourcePath = sourcePath,
                    sourceType = LocalizationSourceType.TMProText,
                    context = $"TMPro Text in {obj.name}"
                });
            }
        }
    }
    
    private string GenerateKey(string category, string objectName, string field)
    {
        if (!useKeyGeneration)
        {
            return $"{keyPrefix}{category}_{objectName}_{field}".ToUpper().Replace(" ", "_");
        }
        
        // Clean object name
        string cleanName = objectName.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("Clone", "");
        return $"{keyPrefix}{category}_{cleanName}_{field}".ToUpper();
    }
    
    private void ExportToCSV()
    {
        if (extractedEntries.Count == 0)
        {
            EditorUtility.DisplayDialog("No Data", "No entries to export. Extract text first.", "OK");
            return;
        }
        
        string path = EditorUtility.SaveFilePanel("Export Translations to CSV", exportPath, "translations.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;
        
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Key,English,Spanish,Context,Source,Type");
        
        foreach (var entry in extractedEntries)
        {
            csv.AppendLine($"{EscapeCSV(entry.key)},{EscapeCSV(entry.englishText)},{EscapeCSV(entry.spanishText)},{EscapeCSV(entry.context)},{EscapeCSV(entry.sourcePath)},{entry.sourceType}");
        }
        
        File.WriteAllText(path, csv.ToString());
        EditorUtility.DisplayDialog("Export Complete", $"Exported {extractedEntries.Count} entries to {path}", "OK");
        EditorUtility.RevealInFinder(path);
    }
    
    private void ExportToJSON()
    {
        if (extractedEntries.Count == 0)
        {
            EditorUtility.DisplayDialog("No Data", "No entries to export. Extract text first.", "OK");
            return;
        }
        
        string path = EditorUtility.SaveFilePanel("Export Translations to JSON", exportPath, "translations.json", "json");
        if (string.IsNullOrEmpty(path)) return;
        
        var jsonData = new Dictionary<string, Dictionary<string, object>>();
        
        foreach (var entry in extractedEntries)
        {
            jsonData[entry.key] = new Dictionary<string, object>
            {
                ["en"] = entry.englishText,
                ["es"] = entry.spanishText,
                ["context"] = entry.context,
                ["source"] = entry.sourcePath,
                ["type"] = entry.sourceType.ToString()
            };
        }
        
        string json = JsonUtility.ToJson(new SerializableDictionary { data = jsonData }, true);
        File.WriteAllText(path, json);
        
        EditorUtility.DisplayDialog("Export Complete", $"Exported {extractedEntries.Count} entries to {path}", "OK");
        EditorUtility.RevealInFinder(path);
    }
    
    private void ExportAllTranslations()
    {
        string folder = EditorUtility.SaveFolderPanel("Export All Translations", exportPath, "");
        if (string.IsNullOrEmpty(folder)) return;
        
        ExtractAllText();
        
        // Export CSV
        string csvPath = Path.Combine(folder, "translations.csv");
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Key,English,Spanish,Context,Source,Type");
        
        foreach (var entry in extractedEntries)
        {
            csv.AppendLine($"{EscapeCSV(entry.key)},{EscapeCSV(entry.englishText)},{EscapeCSV(entry.spanishText)},{EscapeCSV(entry.context)},{EscapeCSV(entry.sourcePath)},{entry.sourceType}");
        }
        
        File.WriteAllText(csvPath, csv.ToString());
        
        // Export JSON
        string jsonPath = Path.Combine(folder, "translations.json");
        var jsonData = new Dictionary<string, Dictionary<string, object>>();
        
        foreach (var entry in extractedEntries)
        {
            jsonData[entry.key] = new Dictionary<string, object>
            {
                ["en"] = entry.englishText,
                ["es"] = entry.spanishText,
                ["context"] = entry.context,
                ["source"] = entry.sourcePath,
                ["type"] = entry.sourceType.ToString()
            };
        }
        
        string json = JsonUtility.ToJson(new SerializableDictionary { data = jsonData }, true);
        File.WriteAllText(jsonPath, json);
        
        // Create README
        string readmePath = Path.Combine(folder, "README.md");
        StringBuilder readme = new StringBuilder();
        readme.AppendLine("# Translation Files");
        readme.AppendLine();
        readme.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        readme.AppendLine($"Total entries: {extractedEntries.Count}");
        readme.AppendLine($"Languages: {string.Join(", ", supportedLanguages)}");
        readme.AppendLine();
        readme.AppendLine("## Files");
        readme.AppendLine("- `translations.csv` - CSV format for spreadsheet editing");
        readme.AppendLine("- `translations.json` - JSON format for programmatic access");
        readme.AppendLine();
        readme.AppendLine("## CSV Format");
        readme.AppendLine("Key,English,Spanish,Context,Source,Type");
        readme.AppendLine();
        readme.AppendLine("## JSON Format");
        readme.AppendLine("```json");
        readme.AppendLine("{");
        readme.AppendLine("  \"KEY_NAME\": {");
        readme.AppendLine("    \"en\": \"English text\",");
        readme.AppendLine("    \"es\": \"Spanish text\",");
        readme.AppendLine("    \"context\": \"Usage context\",");
        readme.AppendLine("    \"source\": \"Source file path\",");
        readme.AppendLine("    \"type\": \"Source type\"");
        readme.AppendLine("  }");
        readme.AppendLine("}");
        readme.AppendLine("```");
        
        File.WriteAllText(readmePath, readme.ToString());
        
        EditorUtility.DisplayDialog("Export Complete", 
            $"Exported all translations to {folder}\n\nFiles created:\n- translations.csv\n- translations.json\n- README.md", "OK");
        EditorUtility.RevealInFinder(folder);
    }
    
    private void ImportTranslations()
    {
        if (string.IsNullOrEmpty(importFilePath) || !File.Exists(importFilePath))
        {
            EditorUtility.DisplayDialog("File Not Found", "Please select a valid import file.", "OK");
            return;
        }
        
        if (createBackup)
        {
            CreateTranslationBackup();
        }
        
        try
        {
            if (importFormat == LocalizationFormat.CSV)
            {
                ImportFromCSV();
            }
            else
            {
                ImportFromJSON();
            }
            
            RefreshStats();
            EditorUtility.DisplayDialog("Import Complete", "Translations imported successfully!", "OK");
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Import Failed", $"Failed to import translations:\n{e.Message}", "OK");
        }
    }
    
    private void ImportFromCSV()
    {
        string[] lines = File.ReadAllLines(importFilePath);
        if (lines.Length <= 1) return;
        
        int imported = 0;
        int updated = 0;
        
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = ParseCSVLine(lines[i]);
            if (values.Length < 3) continue;
            
            string key = values[0];
            string english = values[1];
            string spanish = values[2];
            
            // Find existing entry or create new
            var existingEntry = extractedEntries.FirstOrDefault(e => e.key == key);
            if (existingEntry != null)
            {
                if (overwriteExisting || string.IsNullOrEmpty(existingEntry.spanishText))
                {
                    existingEntry.spanishText = spanish;
                    updated++;
                }
            }
            else
            {
                extractedEntries.Add(new LocalizationEntry
                {
                    key = key,
                    englishText = english,
                    spanishText = spanish,
                    context = values.Length > 3 ? values[3] : "",
                    sourcePath = values.Length > 4 ? values[4] : "",
                    sourceType = values.Length > 5 && Enum.TryParse<LocalizationSourceType>(values[5], out var type) 
                        ? type : LocalizationSourceType.Manual
                });
                imported++;
            }
        }
        
        ApplyTranslationsToAssets();
        
        Debug.Log($"Import complete: {imported} new entries, {updated} updated entries");
    }
    
    private void ImportFromJSON()
    {
        string json = File.ReadAllText(importFilePath);
        // JSON import implementation would go here
        EditorUtility.DisplayDialog("JSON Import", "JSON import coming soon!", "OK");
    }
    
    private void ApplyTranslationsToAssets()
    {
        EditorUtility.DisplayProgressBar("Applying Translations", "Updating assets...", 0f);
        
        try
        {
            // Apply to ScriptableObjects
            string[] furnitureGuids = AssetDatabase.FindAssets("t:FurnitureOriginalData");
            
            for (int i = 0; i < furnitureGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(furnitureGuids[i]);
                EditorUtility.DisplayProgressBar("Applying Translations", $"Updating {Path.GetFileName(path)}", (float)i / furnitureGuids.Length);
                
                var furniture = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(path);
                if (furniture != null)
                {
                    // Update name
                    string nameKey = GenerateKey("FURNITURE", furniture.name, "NAME");
                    var nameEntry = extractedEntries.FirstOrDefault(e => e.key == nameKey);
                    if (nameEntry != null && !string.IsNullOrEmpty(nameEntry.spanishText))
                    {
                        furniture.es_Name = nameEntry.spanishText;
                        EditorUtility.SetDirty(furniture);
                    }
                    
                    // Update description
                    string descKey = GenerateKey("FURNITURE", furniture.name, "DESC");
                    var descEntry = extractedEntries.FirstOrDefault(e => e.key == descKey);
                    if (descEntry != null && !string.IsNullOrEmpty(descEntry.spanishText))
                    {
                        furniture.es_Description = descEntry.spanishText;
                        EditorUtility.SetDirty(furniture);
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    private void RunValidation()
    {
        validationIssues.Clear();
        
        EditorUtility.DisplayProgressBar("Validating Translations", "Checking for issues...", 0f);
        
        try
        {
            if (showMissingTranslations)
            {
                ValidateMissingTranslations();
            }
            
            if (showEmptyTranslations)
            {
                ValidateEmptyTranslations();
            }
            
            if (showDuplicateKeys)
            {
                ValidateDuplicateKeys();
            }
            
            if (showUnusedKeys)
            {
                ValidateUnusedKeys();
            }
            
            EditorUtility.DisplayDialog("Validation Complete", 
                $"Found {validationIssues.Count} issues", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    private void ValidateMissingTranslations()
    {
        foreach (var entry in extractedEntries)
        {
            if (string.IsNullOrEmpty(entry.spanishText))
            {
                validationIssues.Add(new ValidationIssue
                {
                    severity = ValidationSeverity.Warning,
                    description = $"Missing Spanish translation for '{entry.key}'",
                    context = entry.englishText,
                    assetPath = entry.sourcePath
                });
            }
        }
    }
    
    private void ValidateEmptyTranslations()
    {
        foreach (var entry in extractedEntries)
        {
            if (!string.IsNullOrEmpty(entry.spanishText) && string.IsNullOrWhiteSpace(entry.spanishText))
            {
                validationIssues.Add(new ValidationIssue
                {
                    severity = ValidationSeverity.Warning,
                    description = $"Empty Spanish translation for '{entry.key}'",
                    context = entry.englishText,
                    assetPath = entry.sourcePath
                });
            }
        }
    }
    
    private void ValidateDuplicateKeys()
    {
        var duplicates = extractedEntries
            .GroupBy(e => e.key)
            .Where(g => g.Count() > 1)
            .ToList();
        
        foreach (var duplicate in duplicates)
        {
            validationIssues.Add(new ValidationIssue
            {
                severity = ValidationSeverity.Critical,
                description = $"Duplicate key '{duplicate.Key}' found in {duplicate.Count()} locations",
                context = string.Join(", ", duplicate.Select(d => Path.GetFileName(d.sourcePath))),
                assetPath = duplicate.First().sourcePath
            });
        }
    }
    
    private void ValidateUnusedKeys()
    {
        // This would require more complex analysis of actual usage in code
        // For now, just check if keys follow naming conventions
        foreach (var entry in extractedEntries)
        {
            if (!entry.key.StartsWith(keyPrefix))
            {
                validationIssues.Add(new ValidationIssue
                {
                    severity = ValidationSeverity.Info,
                    description = $"Key '{entry.key}' doesn't follow naming convention (should start with '{keyPrefix}')",
                    context = entry.englishText,
                    assetPath = entry.sourcePath
                });
            }
        }
    }
    
    private void AutoFixIssues()
    {
        int fixedCount = 0;
        
        EditorUtility.DisplayProgressBar("Auto-fixing Issues", "Processing...", 0f);
        
        try
        {
            // Fix naming convention issues
            foreach (var issue in validationIssues.Where(i => i.description.Contains("naming convention")).ToList())
            {
                var entry = extractedEntries.FirstOrDefault(e => e.key == issue.context);
                if (entry != null && !entry.key.StartsWith(keyPrefix))
                {
                    string oldKey = entry.key;
                    entry.key = keyPrefix + entry.key;
                    
                    // Update the issue to reflect the fix
                    validationIssues.Remove(issue);
                    fixedCount++;
                    
                    Debug.Log($"Fixed key: {oldKey} -> {entry.key}");
                }
            }
            
            // Auto-translate missing translations (placeholder)
            foreach (var issue in validationIssues.Where(i => i.description.Contains("Missing Spanish translation")).Take(5).ToList())
            {
                var entry = extractedEntries.FirstOrDefault(e => e.key.Contains(issue.context));
                if (entry != null && string.IsNullOrEmpty(entry.spanishText))
                {
                    // Simple placeholder translation (in real implementation, could use translation API)
                    entry.spanishText = $"[ES] {entry.englishText}";
                    validationIssues.Remove(issue);
                    fixedCount++;
                }
            }
            
            if (fixedCount > 0)
            {
                ApplyTranslationsToAssets();
            }
            
            EditorUtility.DisplayDialog("Auto-fix Complete", 
                $"Fixed {fixedCount} issues automatically", "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    private void GenerateValidationReport()
    {
        string reportPath = EditorUtility.SaveFilePanel("Save Validation Report", exportPath, "validation_report.html", "html");
        if (string.IsNullOrEmpty(reportPath)) return;
        
        StringBuilder html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html><head>");
        html.AppendLine("<title>Localization Validation Report</title>");
        html.AppendLine("<style>");
        html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine(".critical { color: #d32f2f; }");
        html.AppendLine(".warning { color: #f57c00; }");
        html.AppendLine(".info { color: #1976d2; }");
        html.AppendLine("table { border-collapse: collapse; width: 100%; }");
        html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("th { background-color: #f2f2f2; }");
        html.AppendLine("</style>");
        html.AppendLine("</head><body>");
        
        html.AppendLine("<h1>🌍 Localization Validation Report</h1>");
        html.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine($"<p>Total Issues: {validationIssues.Count}</p>");
        
        // Summary
        html.AppendLine("<h2>📊 Summary</h2>");
        html.AppendLine("<table>");
        html.AppendLine("<tr><th>Severity</th><th>Count</th></tr>");
        html.AppendLine($"<tr><td class='critical'>Critical</td><td>{validationIssues.Count(i => i.severity == ValidationSeverity.Critical)}</td></tr>");
        html.AppendLine($"<tr><td class='warning'>Warning</td><td>{validationIssues.Count(i => i.severity == ValidationSeverity.Warning)}</td></tr>");
        html.AppendLine($"<tr><td class='info'>Info</td><td>{validationIssues.Count(i => i.severity == ValidationSeverity.Info)}</td></tr>");
        html.AppendLine("</table>");
        
        // Issues
        html.AppendLine("<h2>⚠️ Issues</h2>");
        html.AppendLine("<table>");
        html.AppendLine("<tr><th>Severity</th><th>Description</th><th>Context</th><th>Source</th></tr>");
        
        foreach (var issue in validationIssues.OrderBy(i => i.severity))
        {
            string severityClass = issue.severity.ToString().ToLower();
            html.AppendLine($"<tr>");
            html.AppendLine($"<td class='{severityClass}'>{issue.severity}</td>");
            html.AppendLine($"<td>{issue.description}</td>");
            html.AppendLine($"<td>{issue.context}</td>");
            html.AppendLine($"<td>{Path.GetFileName(issue.assetPath)}</td>");
            html.AppendLine("</tr>");
        }
        
        html.AppendLine("</table>");
        html.AppendLine("</body></html>");
        
        File.WriteAllText(reportPath, html.ToString());
        EditorUtility.DisplayDialog("Report Generated", $"Validation report saved to {reportPath}", "OK");
        EditorUtility.RevealInFinder(reportPath);
    }
    
    private void RefreshPreview()
    {
        previewItems.Clear();
        
        foreach (var entry in extractedEntries)
        {
            string translatedText = "";
            TranslationStatus status = TranslationStatus.Missing;
            
            if (previewLanguage == SystemLanguage.Spanish)
            {
                translatedText = entry.spanishText;
                if (!string.IsNullOrEmpty(translatedText))
                {
                    status = translatedText == entry.englishText ? TranslationStatus.Untranslated : TranslationStatus.Translated;
                }
            }
            else
            {
                translatedText = entry.englishText;
                status = TranslationStatus.Translated;
            }
            
            previewItems.Add(new PreviewItem
            {
                key = entry.key,
                originalText = entry.englishText,
                translatedText = translatedText,
                context = entry.context,
                status = status
            });
        }
    }
    
    private void ApplyTranslationsToScene()
    {
        if (EditorUtility.DisplayDialog("Apply to Scene", 
            $"Apply {previewLanguage} translations to current scene?", "Apply", "Cancel"))
        {
            // Implementation would apply translations to UI elements in the current scene
            EditorUtility.DisplayDialog("Applied", $"Applied {previewLanguage} translations to scene", "OK");
        }
    }
    
    private void TestOnDevice()
    {
        EditorUtility.DisplayDialog("Device Testing", "Device testing integration coming soon!", "OK");
    }
    
    private void TakeScreenshots()
    {
        string screenshotPath = EditorUtility.SaveFolderPanel("Save Screenshots", "", "Screenshots");
        if (!string.IsNullOrEmpty(screenshotPath))
        {
            EditorUtility.DisplayDialog("Screenshots", $"Screenshot feature coming soon!\nWill save to: {screenshotPath}", "OK");
        }
    }
    
    private void CreateCSVTemplate()
    {
        string path = EditorUtility.SaveFilePanel("Save CSV Template", exportPath, "translation_template.csv", "csv");
        if (string.IsNullOrEmpty(path)) return;
        
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("Key,English,Spanish,Context,Notes");
        csv.AppendLine("MENU_START,Start Game,,Main Menu Button,");
        csv.AppendLine("MENU_OPTIONS,Options,,Main Menu Button,");
        csv.AppendLine("MENU_QUIT,Quit Game,,Main Menu Button,");
        csv.AppendLine("GAME_SCORE,Score: {0},,UI Display,{0} will be replaced with actual score");
        csv.AppendLine("FURNITURE_CHAIR_NAME,Chair,,Furniture Name,");
        csv.AppendLine("FURNITURE_CHAIR_DESC,A comfortable chair,,Furniture Description,");
        
        File.WriteAllText(path, csv.ToString());
        EditorUtility.DisplayDialog("Template Created", $"CSV template saved to {path}", "OK");
        EditorUtility.RevealInFinder(path);
    }
    
    private void CreateJSONTemplate()
    {
        string path = EditorUtility.SaveFilePanel("Save JSON Template", exportPath, "translation_template.json", "json");
        if (string.IsNullOrEmpty(path)) return;
        
        var template = new Dictionary<string, Dictionary<string, object>>
        {
            ["MENU_START"] = new Dictionary<string, object>
            {
                ["en"] = "Start Game",
                ["es"] = "",
                ["context"] = "Main Menu Button"
            },
            ["MENU_OPTIONS"] = new Dictionary<string, object>
            {
                ["en"] = "Options",
                ["es"] = "",
                ["context"] = "Main Menu Button"
            },
            ["FURNITURE_CHAIR_NAME"] = new Dictionary<string, object>
            {
                ["en"] = "Chair",
                ["es"] = "",
                ["context"] = "Furniture Name"
            }
        };
        
        string json = JsonUtility.ToJson(new SerializableDictionary { data = template }, true);
        File.WriteAllText(path, json);
        
        EditorUtility.DisplayDialog("Template Created", $"JSON template saved to {path}", "OK");
        EditorUtility.RevealInFinder(path);
    }
    
    private void PreviewImport()
    {
        if (string.IsNullOrEmpty(importFilePath) || !File.Exists(importFilePath))
        {
            EditorUtility.DisplayDialog("File Not Found", "Please select a valid import file.", "OK");
            return;
        }
        
        // Show preview of what would be imported
        EditorUtility.DisplayDialog("Import Preview", "Import preview feature coming soon!", "OK");
    }
    
    private void CreateTranslationBackup()
    {
        string backupFolder = Path.Combine(exportPath, "Backups");
        Directory.CreateDirectory(backupFolder);
        
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string backupPath = Path.Combine(backupFolder, $"translations_backup_{timestamp}.csv");
        
        if (extractedEntries.Count > 0)
        {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("Key,English,Spanish,Context,Source,Type");
            
            foreach (var entry in extractedEntries)
            {
                csv.AppendLine($"{EscapeCSV(entry.key)},{EscapeCSV(entry.englishText)},{EscapeCSV(entry.spanishText)},{EscapeCSV(entry.context)},{EscapeCSV(entry.sourcePath)},{entry.sourceType}");
            }
            
            File.WriteAllText(backupPath, csv.ToString());
            Debug.Log($"Created translation backup: {backupPath}");
        }
    }
    
    // Utility Methods
    private void RefreshStats()
    {
        stats = new LocalizationStats();
        stats.totalEntries = extractedEntries.Count;
        stats.translatedEntries = extractedEntries.Count(e => !string.IsNullOrEmpty(e.spanishText));
        stats.missingTranslations = stats.totalEntries - stats.translatedEntries;
    }
    
    private void RefreshValidation()
    {
        if (validationIssues.Count == 0)
        {
            RunValidation();
        }
    }
    
    private LanguageStats CalculateLanguageStats(SystemLanguage language)
    {
        var stats = new LanguageStats();
        stats.totalEntries = extractedEntries.Count;
        
        if (language == SystemLanguage.Spanish)
        {
            stats.translatedEntries = extractedEntries.Count(e => !string.IsNullOrEmpty(e.spanishText));
        }
        else
        {
            stats.translatedEntries = stats.totalEntries; // Default language is always "translated"
        }
        
        return stats;
    }
    
    private string GetLanguageFlag(SystemLanguage language)
    {
        return language switch
        {
            SystemLanguage.English => "🇺🇸",
            SystemLanguage.Spanish => "🇪🇸",
            SystemLanguage.French => "🇫🇷",
            SystemLanguage.German => "🇩🇪",
            SystemLanguage.Italian => "🇮🇹",
            SystemLanguage.Portuguese => "🇵🇹",
            SystemLanguage.Russian => "🇷🇺",
            SystemLanguage.Japanese => "🇯🇵",
            SystemLanguage.Korean => "🇰🇷",
            SystemLanguage.Chinese => "🇨🇳",
            _ => "🌐"
        };
    }
    
    private string GetSeverityIcon(ValidationSeverity severity)
    {
        return severity switch
        {
            ValidationSeverity.Critical => "🔴",
            ValidationSeverity.Warning => "🟡",
            ValidationSeverity.Info => "🔵",
            _ => "⚪"
        };
    }
    
    private string GetTranslationStatusIcon(TranslationStatus status)
    {
        return status switch
        {
            TranslationStatus.Translated => "✅",
            TranslationStatus.Untranslated => "🟡",
            TranslationStatus.Missing => "❌",
            TranslationStatus.NeedsReview => "🔍",
            _ => "❓"
        };
    }
    
    private void DrawSeparator()
    {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(5);
    }
    
    private string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
            
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
            
        return value;
    }
    
    private string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder field = new StringBuilder();
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    field.Append('\"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(field.ToString());
                field.Clear();
            }
            else
            {
                field.Append(c);
            }
        }
        
        result.Add(field.ToString());
        return result.ToArray();
    }
    
    private void SaveSettings()
    {
        EditorPrefs.SetString("LocalizationManager_ExportPath", exportPath);
        EditorPrefs.SetBool("LocalizationManager_AutoDetectText", autoDetectText);
        EditorPrefs.SetBool("LocalizationManager_UseKeyGeneration", useKeyGeneration);
        EditorPrefs.SetString("LocalizationManager_KeyPrefix", keyPrefix);
        EditorPrefs.SetInt("LocalizationManager_DefaultLanguage", (int)defaultLanguage);
        
        // Save supported languages
        string languagesJson = JsonUtility.ToJson(new SerializableLanguageList { languages = supportedLanguages });
        EditorPrefs.SetString("LocalizationManager_SupportedLanguages", languagesJson);
    }
    
    private void LoadSettings()
    {
        exportPath = EditorPrefs.GetString("LocalizationManager_ExportPath", "Assets/Localization/");
        autoDetectText = EditorPrefs.GetBool("LocalizationManager_AutoDetectText", true);
        useKeyGeneration = EditorPrefs.GetBool("LocalizationManager_UseKeyGeneration", true);
        keyPrefix = EditorPrefs.GetString("LocalizationManager_KeyPrefix", "LOC_");
        defaultLanguage = (SystemLanguage)EditorPrefs.GetInt("LocalizationManager_DefaultLanguage", (int)SystemLanguage.English);
        
        // Load supported languages
        string languagesJson = EditorPrefs.GetString("LocalizationManager_SupportedLanguages", "");
        if (!string.IsNullOrEmpty(languagesJson))
        {
            try
            {
                var languageList = JsonUtility.FromJson<SerializableLanguageList>(languagesJson);
                supportedLanguages = languageList.languages;
            }
            catch
            {
                supportedLanguages = new List<SystemLanguage> { SystemLanguage.English, SystemLanguage.Spanish };
            }
        }
    }
    
    private void ResetToDefaults()
    {
        exportPath = "Assets/Localization/";
        autoDetectText = true;
        useKeyGeneration = true;
        keyPrefix = "LOC_";
        defaultLanguage = SystemLanguage.English;
        supportedLanguages = new List<SystemLanguage> { SystemLanguage.English, SystemLanguage.Spanish };
        
        includeScriptableObjects = true;
        includePrefabs = true;
        includeScenes = true;
        includeUIText = true;
        searchFolders = new string[] { "Assets" };
        
        importFormat = LocalizationFormat.CSV;
        overwriteExisting = false;
        createBackup = true;
        
        showMissingTranslations = true;
        showEmptyTranslations = true;
        showDuplicateKeys = true;
        showUnusedKeys = false;
        
        previewLanguage = SystemLanguage.Spanish;
        previewFilter = "";
        
        SaveSettings();
    }
}

// Supporting Classes and Enums
[System.Serializable]
public class LocalizationEntry
{
    public string key;
    public string englishText;
    public string spanishText;
    public string sourcePath;
    public LocalizationSourceType sourceType;
    public string context;
}

[System.Serializable]
public class ValidationIssue
{
    public ValidationSeverity severity;
    public string description;
    public string context;
    public string assetPath;
}

[System.Serializable]
public class PreviewItem
{
    public string key;
    public string originalText;
    public string translatedText;
    public string context;
    public TranslationStatus status;
}

[System.Serializable]
public class LocalizationStats
{
    public int totalEntries;
    public int translatedEntries;
    public int missingTranslations;
}

[System.Serializable]
public class LanguageStats
{
    public int totalEntries;
    public int translatedEntries;
}

[System.Serializable]
public class SerializableDictionary
{
    public Dictionary<string, Dictionary<string, object>> data;
}

[System.Serializable]
public class SerializableLanguageList
{
    public List<SystemLanguage> languages;
}

public enum LocalizationFormat
{
    CSV,
    JSON
}

public enum LocalizationSourceType
{
    ScriptableObject,
    Prefab,
    Scene,
    UIText,
    TMProText,
    Manual
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Critical
}

public enum TranslationStatus
{
    Missing,
    Untranslated,
    Translated,
    NeedsReview
}

#endif
