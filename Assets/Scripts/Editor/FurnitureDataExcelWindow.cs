using System.IO;
using UnityEditor;
using UnityEngine;

public class FurnitureDataExcelWindow : EditorWindow
{
    private string exportPath = "Assets/FurnitureData.csv";
    private string importPath = "Assets/FurnitureData.csv";
    private Vector2 scrollPosition;
    
    [MenuItem("DevTools/Furniture CSV")]
    public static void ShowWindow()
    {
        GetWindow<FurnitureDataExcelWindow>("Furniture CSV");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("Furniture CSV", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(20);
        
        DrawSeparator();
        
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Export Settings", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("Export Path:", EditorStyles.miniLabel);
        exportPath = EditorGUILayout.TextField(exportPath);
        
        EditorGUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("📁 Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.SaveFilePanel("Save CSV File", Path.GetDirectoryName(exportPath), Path.GetFileName(exportPath), "csv");
            if (!string.IsNullOrEmpty(path))
            {
                exportPath = GetRelativePath(path);
            }
        }
        
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 0.7f);
        if (GUILayout.Button("Export to CSV", GUILayout.Height(30), GUILayout.Width(150)))
        {
            FurnitureDataExcelUtility.ExportToCSV(exportPath);
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(20);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Import Settings", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("Import Path:", EditorStyles.miniLabel);
        importPath = EditorGUILayout.TextField(importPath);
        
        EditorGUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("📁 Browse", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("Open CSV File", Path.GetDirectoryName(importPath), "csv");
            if (!string.IsNullOrEmpty(path))
            {
                importPath = GetRelativePath(path);
            }
        }
        
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f);
        if (GUILayout.Button("Import from CSV", GUILayout.Height(30), GUILayout.Width(150)))
        {
            if (EditorUtility.DisplayDialog("⚠️ Import Confirmation", 
                "This will overwrite existing furniture data.\n\nAre you sure you want to continue?", 
                "Yes, Import", "Cancel"))
            {
                FurnitureDataExcelUtility.ImportFromCSV(importPath);
            }
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(20);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "💡 Tips:\n" +
            "• Export creates a CSV file with all furniture data\n" +
            "• Import overwrites existing ScriptableObject data\n" +
            "• Always backup your project before importing\n" +
            "• CSV format: AssetPath, Name, es_Name, Price, Size, etc.",
            MessageType.Info);
            
        EditorGUILayout.Space(10);
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawSeparator()
    {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(5);
    }
    
    private string GetRelativePath(string fullPath)
    {
        string projectPath = Application.dataPath;
        projectPath = projectPath.Substring(0, projectPath.Length - 6);
        
        if (fullPath.StartsWith(projectPath))
        {
            return fullPath.Substring(projectPath.Length);
        }
        
        return fullPath;
    }
}