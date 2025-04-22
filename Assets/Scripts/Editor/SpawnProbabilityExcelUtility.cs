using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SpawnProbabilityExcelUtility
{
    private const string DefaultExportPath = "Assets/SpawnProbabilityConfig.csv";
    
    public static void ExportToCSV(SpawnProbabilityConfig config, string filePath = null)
    {
        if (config == null)
        {
            Debug.LogError("No SpawnProbabilityConfig provided for export");
            return;
        }
        
        if (string.IsNullOrEmpty(filePath))
            filePath = DefaultExportPath;
            
        // Create CSV content
        StringBuilder csv = new StringBuilder();
        
        // Write config info
        csv.AppendLine("# SpawnProbabilityConfig");
        csv.AppendLine($"ConfigName,{EscapeCSV(config.name)}");
        csv.AppendLine($"DefaultTagProbability,{config.defaultTagProbability}");
        csv.AppendLine();
        
        // Write tag probabilities
        csv.AppendLine("# Tag Probabilities");
        csv.AppendLine("TagName,Probability");
        
        foreach (var tagProb in config.tagProbabilities)
        {
            csv.AppendLine($"{tagProb.tag},{tagProb.spawnProbability}");
        }
        
        csv.AppendLine();
        
        // Write furniture specific probabilities
        csv.AppendLine("# Furniture Specific Probabilities");
        csv.AppendLine("FurniturePath,Probability");
        
        foreach (var furnitureProb in config.furnitureSpecificProbabilities)
        {
            string furniturePath = furnitureProb.furniture != null ? 
                AssetDatabase.GetAssetPath(furnitureProb.furniture) : "";
                
            csv.AppendLine($"{EscapeCSV(furniturePath)},{furnitureProb.spawnProbability}");
        }
        
        // Write to file
        File.WriteAllText(filePath, csv.ToString());
        Debug.Log($"Exported spawn probability config to {filePath}");
        
        // Open the file
        EditorUtility.RevealInFinder(filePath);
    }
    
    public static void ImportFromCSV(SpawnProbabilityConfig config, string filePath = null)
    {
        if (config == null)
        {
            Debug.LogError("No SpawnProbabilityConfig provided for import");
            return;
        }
        
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
        
        // Clear existing data
        config.tagProbabilities.Clear();
        config.furnitureSpecificProbabilities.Clear();
        
        // Parse mode
        string currentSection = "";
        
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
                
            // Check for section headers
            if (line.StartsWith("#"))
            {
                currentSection = line.Substring(1).Trim();
                continue;
            }
            
            // Skip column headers
            if (line.Contains("TagName,Probability") || 
                line.Contains("FurniturePath,Probability") ||
                line.Contains("ConfigName,"))
                continue;
                
            string[] values = ParseCSVLine(line);
            
            // Process based on current section
            if (line.StartsWith("DefaultTagProbability,"))
            {
                if (values.Length >= 2)
                {
                    float defaultProb;
                    if (float.TryParse(values[1], out defaultProb))
                    {
                        config.defaultTagProbability = defaultProb;
                    }
                }
                continue;
            }
            
            if (currentSection.Contains("Tag Probabilities"))
            {
                if (values.Length >= 2)
                {
                    try
                    {
                        RoomTag tag = (RoomTag)Enum.Parse(typeof(RoomTag), values[0]);
                        float probability = float.Parse(values[1]);
                        
                        var tagProb = new SpawnProbabilityConfig.TagProbability
                        {
                            tag = tag,
                            spawnProbability = probability
                        };
                        
                        config.tagProbabilities.Add(tagProb);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error parsing tag probability: {line}. Error: {e.Message}");
                    }
                }
            }
            else if (currentSection.Contains("Furniture Specific Probabilities"))
            {
                if (values.Length >= 2)
                {
                    string furniturePath = values[0];
                    float probability;
                    
                    if (float.TryParse(values[1], out probability))
                    {
                        FurnitureOriginalData furniture = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(furniturePath);
                        
                        if (furniture != null)
                        {
                            var furnitureProb = new SpawnProbabilityConfig.FurnitureSpecificProbability
                            {
                                furniture = furniture,
                                spawnProbability = probability
                            };
                            
                            config.furnitureSpecificProbabilities.Add(furnitureProb);
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find furniture at path: {furniturePath}");
                        }
                    }
                }
            }
        }
        
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        Debug.Log($"Imported spawn probability config from {filePath}");
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