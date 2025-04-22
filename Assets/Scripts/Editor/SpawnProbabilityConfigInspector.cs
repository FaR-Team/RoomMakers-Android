using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpawnProbabilityConfig))]
public class SpawnProbabilityConfigInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SpawnProbabilityConfig config = (SpawnProbabilityConfig)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("CSV Import/Export", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Export to CSV"))
        {
            string path = EditorUtility.SaveFilePanel("Save CSV File", 
                "Assets", "SpawnProbabilityConfig.csv", "csv");
                
            if (!string.IsNullOrEmpty(path))
            {
                SpawnProbabilityExcelUtility.ExportToCSV(config, path);
            }
        }
        
        if (GUILayout.Button("Import from CSV"))
        {
            string path = EditorUtility.OpenFilePanel("Open CSV File", 
                "Assets", "csv");
                
            if (!string.IsNullOrEmpty(path))
            {
                if (EditorUtility.DisplayDialog("Import Confirmation", 
                    "This will overwrite existing probability settings. Are you sure?", 
                    "Yes", "No"))
                {
                    SpawnProbabilityExcelUtility.ImportFromCSV(config, path);
                }
            }
        }
    }
}