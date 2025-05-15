using System.IO;
using UnityEditor;
using UnityEngine;

public class FurnitureDataExcelWindow : EditorWindow
{
    private string exportPath = "Assets/FurnitureData.csv";
    private string importPath = "Assets/FurnitureData.csv";
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Furniture Data Excel Tool")]
    public static void ShowWindow()
    {
        GetWindow<FurnitureDataExcelWindow>("Furniture Data Excel Tool");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Furniture Data Excel Tool", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Export Settings", EditorStyles.boldLabel);
        
        exportPath = EditorGUILayout.TextField("Export Path:", exportPath);
        
        if (GUILayout.Button("Browse Export Path"))
        {
            string path = EditorUtility.SaveFilePanel("Save CSV File", Path.GetDirectoryName(exportPath), Path.GetFileName(exportPath), "csv");
            if (!string.IsNullOrEmpty(path))
            {
                exportPath = GetRelativePath(path);
            }
        }
        
        if (GUILayout.Button("Export to CSV"))
        {
            FurnitureDataExcelUtility.ExportToCSV(exportPath);
        }
        
        EditorGUILayout.Space(20);
        GUILayout.Label("Import Settings", EditorStyles.boldLabel);
        
        importPath = EditorGUILayout.TextField("Import Path:", importPath);
        
        if (GUILayout.Button("Browse Import Path"))
        {
            string path = EditorUtility.OpenFilePanel("Open CSV File", Path.GetDirectoryName(importPath), "csv");
            if (!string.IsNullOrEmpty(path))
            {
                importPath = GetRelativePath(path);
            }
        }
        
        if (GUILayout.Button("Import from CSV"))
        {
            if (EditorUtility.DisplayDialog("Import Confirmation", 
                "This will overwrite existing furniture data. Are you sure you want to continue?", 
                "Yes", "No"))
            {
                FurnitureDataExcelUtility.ImportFromCSV(importPath);
            }
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "idk man",
            MessageType.Info);
            
        EditorGUILayout.EndScrollView();
    }
    
    private string GetRelativePath(string fullPath)
    {
        string projectPath = Application.dataPath;
        projectPath = projectPath.Substring(0, projectPath.Length - 6); // Remove "Assets"
        
        if (fullPath.StartsWith(projectPath))
        {
            return fullPath.Substring(projectPath.Length);
        }
        
        return fullPath;
    }
}