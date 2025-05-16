using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class FurnitureDataExcelUtility
{
    private const string DefaultExportPath = "Assets/FurnitureData.csv";
    
    public static void ExportToCSV(string filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
            filePath = DefaultExportPath;
            
        // Find all FurnitureOriginalData scriptable objects
        string[] guids = AssetDatabase.FindAssets("t:FurnitureOriginalData");
        List<FurnitureOriginalData> furnitureData = new List<FurnitureOriginalData>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FurnitureOriginalData data = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(path);
            if (data != null)
                furnitureData.Add(data);
        }
        
        // Create CSV content
        StringBuilder csv = new StringBuilder();
        
        // Make sure header matches the data we're exporting
        csv.AppendLine("AssetPath,Name,es_Name,Price,SizeX,SizeY,TypeOfSize,PrefabPath,FurnitureTag,TagMatchBonusPoints,WallObject,HasComboSprite,ComboTriggerFurniturePath,Compatibles,RequiresBase,RequiredBasePath");
        
        // Write data rows
        foreach (FurnitureOriginalData data in furnitureData)
        {
            string assetPath = AssetDatabase.GetAssetPath(data);
            string prefabPath = data.prefab != null ? AssetDatabase.GetAssetPath(data.prefab) : "";
            string comboTriggerPath = data.comboTriggerFurniture != null ? AssetDatabase.GetAssetPath(data.comboTriggerFurniture) : "";
            
            // Create compatibles list as semicolon-separated paths
            string compatiblesStr = "";
            if (data.compatibles != null && data.compatibles.Length > 0)
            {
                List<string> compatiblePaths = new List<string>();
                foreach (var compatible in data.compatibles)
                {
                    if (compatible != null)
                    {
                        compatiblePaths.Add(AssetDatabase.GetAssetPath(compatible));
                    }
                }
                compatiblesStr = string.Join(";", compatiblePaths);
            }

            // Check if requiredBase is set
            bool requiresBase = data.requiredBase != null;
            string requiredBasePath = requiresBase ? AssetDatabase.GetAssetPath(data.requiredBase) : "";
            
            // Ensure we're writing the correct number of fields in the correct order
            csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15}",
                assetPath,
                EscapeCSV(data.Name),
                EscapeCSV(data.es_Name),
                data.price,
                data.size.x,
                data.size.y,
                data.typeOfSize,
                EscapeCSV(prefabPath),
                data.furnitureTag,
                data.tagMatchBonusPoints,
                data.wallObject,
                data.hasComboSprite,
                EscapeCSV(comboTriggerPath),
                EscapeCSV(compatiblesStr),
                requiresBase.ToString(), // Explicitly convert to string
                EscapeCSV(requiredBasePath)
            ));
        }
        
        // Write to file
        File.WriteAllText(filePath, csv.ToString());
        Debug.Log($"Exported {furnitureData.Count} furniture items to {filePath}");
        
        // Open the file
        EditorUtility.RevealInFinder(filePath);
    }
    
    public static void ImportFromCSV(string filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
            filePath = DefaultExportPath;
            
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }
        
        string[] lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV file is empty or contains only headers");
            return;
        }
        
        // Skip header line
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
                continue;
                
            string[] values = ParseCSVLine(line);
            if (values.Length < 16) // Make sure we have all fields including RequiresBase and RequiredBasePath
            {
                Debug.LogError($"Line {i} has incorrect format: {line}. Expected 16 fields, got {values.Length}");
                continue;
            }
            
            string assetPath = values[0];
            FurnitureOriginalData data = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(assetPath);
            
            if (data == null)
            {
                Debug.LogError($"Could not find furniture data at path: {assetPath}");
                continue;
            }
            
            // Update the data
            data.Name = values[1];
            data.es_Name = values[2];
            data.price = int.Parse(values[3]);
            data.size = new Vector2Int(int.Parse(values[4]), int.Parse(values[5]));
            
            // Handle TypeOfSize safely
            if (!string.IsNullOrEmpty(values[6]))
            {
                try
                {
                    data.typeOfSize = (TypeOfSize)Enum.Parse(typeof(TypeOfSize), values[6]);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to parse TypeOfSize '{values[6]}' for {data.Name}: {ex.Message}");
                    // Keep existing value
                }
            }
            
            // Load prefab
            string prefabPath = values[7];
            if (!string.IsNullOrEmpty(prefabPath))
                data.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
            // Parse enum
            if (!string.IsNullOrEmpty(values[8]))
            {
                try
                {
                    data.furnitureTag = (RoomTag)Enum.Parse(typeof(RoomTag), values[8]);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to parse RoomTag '{values[8]}' for {data.Name}: {ex.Message}");
                    // Keep existing value
                }
            }
            
            data.tagMatchBonusPoints = int.Parse(values[9]);
            data.wallObject = bool.Parse(values[10]);
            data.hasComboSprite = bool.Parse(values[11]);
            
            // Load combo trigger furniture
            string comboTriggerPath = values[12];
            if (!string.IsNullOrEmpty(comboTriggerPath))
                data.comboTriggerFurniture = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(comboTriggerPath);
            else
                data.comboTriggerFurniture = null;
            
            // Load compatibles list
            string compatiblesStr = values[13];
            List<FurnitureOriginalData> compatiblesList = new List<FurnitureOriginalData>();
            if (!string.IsNullOrEmpty(compatiblesStr))
            {
                string[] compatiblePaths = compatiblesStr.Split(';');
                foreach (string path in compatiblePaths)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        FurnitureOriginalData compatible = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(path);
                        if (compatible != null)
                        {
                            compatiblesList.Add(compatible);
                        }
                    }
                }
            }
            data.compatibles = compatiblesList.ToArray();

            // Handle RequiresBase field - index 14
            bool requiresBase = false;
            if (!string.IsNullOrEmpty(values[14]))
            {
                requiresBase = bool.Parse(values[14]);
            }
    
            // Load required base object - index 15
            data.requiredBase = null; // Default to null
            if (requiresBase)
            {
                string requiredBasePath = values[15];
                if (!string.IsNullOrEmpty(requiredBasePath))
                {
                    data.requiredBase = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(requiredBasePath);
                    if (data.requiredBase == null)
                    {
                        Debug.LogWarning($"Could not find required base at path: {requiredBasePath} for furniture: {data.Name}");
                    }
                }
            }
                
            EditorUtility.SetDirty(data);
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Imported furniture data from {filePath}");
    }    private static string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
            
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
            
        return value;
    }
    
    private static string[] ParseCSVLine(string line)
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
}