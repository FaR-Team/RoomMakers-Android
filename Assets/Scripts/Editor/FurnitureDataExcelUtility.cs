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
        
        // Write header
        csv.AppendLine("AssetPath,Name,es_Name,Price,SizeX,SizeY,TypeOfSize,PrefabPath,FurnitureTag,TagMatchBonusPoints,IsLabeler,HasComboSprite,ComboTriggerFurniturePath");
        
        // Write data rows
        foreach (FurnitureOriginalData data in furnitureData)
        {
            string assetPath = AssetDatabase.GetAssetPath(data);
            string prefabPath = data.prefab != null ? AssetDatabase.GetAssetPath(data.prefab) : "";
            string comboTriggerPath = data.comboTriggerFurniture != null ? AssetDatabase.GetAssetPath(data.comboTriggerFurniture) : "";
            
            csv.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}",
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
                data.isLabeler,
                data.hasComboSprite,
                EscapeCSV(comboTriggerPath)
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
            if (values.Length < 13)
            {
                Debug.LogError($"Line {i} has incorrect format: {line}");
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
            data.typeOfSize = (TypeOfSize)Enum.Parse(typeof(TypeOfSize), values[6]);
            
            // Load prefab
            string prefabPath = values[7];
            if (!string.IsNullOrEmpty(prefabPath))
                data.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
            // Parse enum
            data.furnitureTag = (RoomTag)Enum.Parse(typeof(RoomTag), values[8]);
            data.tagMatchBonusPoints = int.Parse(values[9]);
            data.isLabeler = bool.Parse(values[10]);
            data.hasComboSprite = bool.Parse(values[11]);
            
            // Load combo trigger furniture
            string comboTriggerPath = values[12];
            if (!string.IsNullOrEmpty(comboTriggerPath))
                data.comboTriggerFurniture = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(comboTriggerPath);
                
            EditorUtility.SetDirty(data);
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Imported furniture data from {filePath}");
    }
    
    private static string EscapeCSV(string value)
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