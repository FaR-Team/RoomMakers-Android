#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameBalanceAnalyzer : EditorWindow
{
    private Vector2 scrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "Overview", "Price Analysis", "Tag Distribution", "Size Analysis", "Outliers" };
    
    private List<FurnitureOriginalData> allFurniture = new List<FurnitureOriginalData>();
    private bool dataLoaded = false;
    
    // Analysis data
    private Dictionary<RoomTag, List<FurnitureOriginalData>> furnitureByTag = new Dictionary<RoomTag, List<FurnitureOriginalData>>();
    private Dictionary<TypeOfSize, List<FurnitureOriginalData>> furnitureBySize = new Dictionary<TypeOfSize, List<FurnitureOriginalData>>();
    private List<FurnitureOriginalData> priceOutliers = new List<FurnitureOriginalData>();
    private List<FurnitureOriginalData> bonusOutliers = new List<FurnitureOriginalData>();
    
    [MenuItem("DevTools/Game Balance Analyzer")]
    public static void ShowWindow()
    {
        GetWindow<GameBalanceAnalyzer>("Game Balance Analyzer");
    }
    
    private void OnEnable()
    {
        LoadFurnitureData();
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
        GUILayout.Label("⚖️ Game Balance Analyzer", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(15);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("🔄 Refresh Data", GUILayout.Width(100)))
        {
            LoadFurnitureData();
        }
        GUILayout.FlexibleSpace();
        if (dataLoaded)
        {
            GUIStyle dataStyle = new GUIStyle(EditorStyles.miniLabel);
            dataStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label($"📊 {allFurniture.Count} furniture items loaded", dataStyle);
        }
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.Height(25));
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        if (!dataLoaded)
        {
            DrawNoDataMessage();
        }
        else
        {
            switch (selectedTab)
            {
                case 0: DrawOverviewTab(); break;
                case 1: DrawPriceAnalysisTab(); break;
                case 2: DrawTagDistributionTab(); break;
                case 3: DrawSizeAnalysisTab(); break;
                case 4: DrawOutliersTab(); break;
            }
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.EndScrollView();
    }
    
    private void LoadFurnitureData()
    {
        allFurniture.Clear();
        furnitureByTag.Clear();
        furnitureBySize.Clear();
        priceOutliers.Clear();
        bonusOutliers.Clear();
        
        string[] guids = AssetDatabase.FindAssets("t:FurnitureOriginalData");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            FurnitureOriginalData furniture = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(path);
            if (furniture != null)
            {
                allFurniture.Add(furniture);
            }
        }
        
        AnalyzeData();
        dataLoaded = allFurniture.Count > 0;
    }
    
    private void AnalyzeData()
    {
        // Group by tags
        foreach (var furniture in allFurniture)
        {
            if (!furnitureByTag.ContainsKey(furniture.furnitureTag))
                furnitureByTag[furniture.furnitureTag] = new List<FurnitureOriginalData>();
            furnitureByTag[furniture.furnitureTag].Add(furniture);
        }
        
        // Group by size
        foreach (var furniture in allFurniture)
        {
            if (!furnitureBySize.ContainsKey(furniture.typeOfSize))
                furnitureBySize[furniture.typeOfSize] = new List<FurnitureOriginalData>();
            furnitureBySize[furniture.typeOfSize].Add(furniture);
        }
        
        // Find price outliers
        if (allFurniture.Count > 0)
        {
            float avgPrice = (float)allFurniture.Average(f => f.price);
            float priceStdDev = Mathf.Sqrt(allFurniture.Average(f => Mathf.Pow(f.price - avgPrice, 2)));
            
            priceOutliers = allFurniture.Where(f => 
                Mathf.Abs(f.price - avgPrice) > priceStdDev * 2).ToList();
            
            // Find bonus point outliers
            float avgBonus = (float)allFurniture.Average(f => f.tagMatchBonusPoints);
            float bonusStdDev = Mathf.Sqrt(allFurniture.Average(f => Mathf.Pow(f.tagMatchBonusPoints - avgBonus, 2)));
            
            bonusOutliers = allFurniture.Where(f => 
                Mathf.Abs(f.tagMatchBonusPoints - avgBonus) > bonusStdDev * 1.5f).ToList();
        }
    }
    
    private void DrawNoDataMessage()
    {
        EditorGUILayout.Space(50);
        
        GUIStyle messageStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        messageStyle.fontSize = 14;
        messageStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("📊", GUILayout.Width(30), GUILayout.Height(30));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("No furniture data found. Click 'Refresh Data' to load.", messageStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    
    private void DrawOverviewTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📈 Quick Stats", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        DrawStatRow("Total Furniture Items", allFurniture.Count.ToString());
        DrawStatRow("Average Price", $"{allFurniture.Average(f => f.price):F1}");
        DrawStatRow("Price Range", $"{allFurniture.Min(f => f.price)} - {allFurniture.Max(f => f.price)}");
        DrawStatRow("Average Bonus Points", $"{allFurniture.Average(f => f.tagMatchBonusPoints):F1}");
        DrawStatRow("Unique Room Tags", furnitureByTag.Keys.Count.ToString());
        DrawStatRow("Unique Sizes", furnitureBySize.Keys.Count.ToString());
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🏷️ Tag Summary", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        var sortedTags = furnitureByTag.OrderByDescending(kvp => kvp.Value.Count);
        foreach (var tagGroup in sortedTags)
        {
            float percentage = (tagGroup.Value.Count / (float)allFurniture.Count) * 100f;
            DrawStatRow($"{tagGroup.Key}", $"{tagGroup.Value.Count} items ({percentage:F1}%)");
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📐 Size Summary", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        var sortedSizes = furnitureBySize.OrderByDescending(kvp => kvp.Value.Count);
        foreach (var sizeGroup in sortedSizes)
        {
            float percentage = (sizeGroup.Value.Count / (float)allFurniture.Count) * 100f;
            DrawStatRow($"{sizeGroup.Key}", $"{sizeGroup.Value.Count} items ({percentage:F1}%)");
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPriceAnalysisTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.6f, 0.2f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("💰 Price Analysis", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        float avgPrice = (float)allFurniture.Average(f => f.price);
        float minPrice = allFurniture.Min(f => f.price);
        float maxPrice = allFurniture.Max(f => f.price);
        
        DrawStatRow("Average Price", $"{avgPrice:F1}");
        DrawStatRow("Minimum Price", minPrice.ToString());
        DrawStatRow("Maximum Price", maxPrice.ToString());
        DrawStatRow("Price Range", $"{maxPrice - minPrice}");
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📊 Price by Size", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        foreach (var sizeGroup in furnitureBySize.OrderBy(kvp => kvp.Key.ToString()))
        {
            if (sizeGroup.Value.Count > 0)
            {
                float avgPriceForSize = (float)sizeGroup.Value.Average(f => f.price);
                float pricePerUnit = avgPriceForSize / GetSizeMultiplier(sizeGroup.Key);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{sizeGroup.Key}:", GUILayout.Width(80));
                GUILayout.Label($"Avg: {avgPriceForSize:F1}", GUILayout.Width(80));
                GUILayout.Label($"Per Unit: {pricePerUnit:F1}", GUILayout.Width(80));
                
                Color barColor = GetColorForValue(pricePerUnit, 5f, 15f);
                DrawProgressBar(pricePerUnit / 20f, barColor);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🏷️ Price by Tag", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        foreach (var tagGroup in furnitureByTag.OrderBy(kvp => kvp.Key.ToString()))
        {
            if (tagGroup.Value.Count > 0)
            {
                float avgPriceForTag = (float)tagGroup.Value.Average(f => f.price);
                float minPriceForTag = tagGroup.Value.Min(f => f.price);
                float maxPriceForTag = tagGroup.Value.Max(f => f.price);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{tagGroup.Key}:", GUILayout.Width(100));
                GUILayout.Label($"Avg: {avgPriceForTag:F1}", GUILayout.Width(80));
                GUILayout.Label($"Range: {minPriceForTag}-{maxPriceForTag}", GUILayout.Width(100));
                
                Color barColor = GetColorForValue(avgPriceForTag, minPrice, maxPrice);
                DrawProgressBar((avgPriceForTag - minPrice) / (maxPrice - minPrice), barColor);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawTagDistributionTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.6f, 0.2f, 0.9f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🏷️ Tag Distribution", sectionStyle);
                EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        var sortedTags = furnitureByTag.OrderByDescending(kvp => kvp.Value.Count);
        int totalItems = allFurniture.Count;
        
        foreach (var tagGroup in sortedTags)
        {
            float percentage = (tagGroup.Value.Count / (float)totalItems) * 100f;
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{tagGroup.Key}:", GUILayout.Width(100));
            GUILayout.Label($"{tagGroup.Value.Count} items", GUILayout.Width(80));
            GUILayout.Label($"{percentage:F1}%", GUILayout.Width(50));
            
            Color barColor = GetTagColor(tagGroup.Key);
            DrawProgressBar(percentage / 100f, barColor);
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🎯 Bonus Points by Tag", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        foreach (var tagGroup in furnitureByTag.OrderBy(kvp => kvp.Key.ToString()))
        {
            if (tagGroup.Value.Count > 0)
            {
                float avgBonus = (float)tagGroup.Value.Average(f => f.tagMatchBonusPoints);
                float minBonus = tagGroup.Value.Min(f => f.tagMatchBonusPoints);
                float maxBonus = tagGroup.Value.Max(f => f.tagMatchBonusPoints);
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{tagGroup.Key}:", GUILayout.Width(100));
                GUILayout.Label($"Avg: {avgBonus:F1}", GUILayout.Width(80));
                GUILayout.Label($"Range: {minBonus}-{maxBonus}", GUILayout.Width(100));
                
                Color barColor = GetColorForValue(avgBonus, 20f, 100f);
                DrawProgressBar(avgBonus / 100f, barColor);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        // Tag balance recommendations
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("💡 Tag Balance Recommendations", sectionStyle);
        EditorGUILayout.Space(5);
        
        var underrepresentedTags = furnitureByTag.Where(kvp => 
            (kvp.Value.Count / (float)totalItems) < 0.05f && kvp.Key != RoomTag.None).ToList();
        
        var overrepresentedTags = furnitureByTag.Where(kvp => 
            (kvp.Value.Count / (float)totalItems) > 0.25f).ToList();
        
        if (underrepresentedTags.Any() || overrepresentedTags.Any())
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            if (underrepresentedTags.Any())
            {
                EditorGUILayout.HelpBox(
                    $"⚠️ Underrepresented tags (< 5%): {string.Join(", ", underrepresentedTags.Select(t => t.Key.ToString()))}", 
                    MessageType.Warning);
            }
            
            if (overrepresentedTags.Any())
            {
                EditorGUILayout.HelpBox(
                    $"📈 Overrepresented tags (> 25%): {string.Join(", ", overrepresentedTags.Select(t => t.Key.ToString()))}", 
                    MessageType.Info);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("✅ Tag distribution looks balanced!", MessageType.Info);
        }
    }
    
    private void DrawSizeAnalysisTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.9f, 0.6f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📐 Size Analysis", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        var sortedSizes = furnitureBySize.OrderBy(kvp => GetSizeMultiplier(kvp.Key));
        int totalItems = allFurniture.Count;
        
        foreach (var sizeGroup in sortedSizes)
        {
            float percentage = (sizeGroup.Value.Count / (float)totalItems) * 100f;
            int sizeMultiplier = GetSizeMultiplier(sizeGroup.Key);
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{sizeGroup.Key}:", GUILayout.Width(80));
            GUILayout.Label($"{sizeGroup.Value.Count} items", GUILayout.Width(80));
            GUILayout.Label($"{percentage:F1}%", GUILayout.Width(50));
            GUILayout.Label($"({sizeMultiplier} units)", GUILayout.Width(80));
            
            Color barColor = GetSizeColor(sizeGroup.Key);
            DrawProgressBar(percentage / 100f, barColor);
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(2);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("💰 Price Efficiency by Size", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        foreach (var sizeGroup in sortedSizes)
        {
            if (sizeGroup.Value.Count > 0)
            {
                float avgPrice = (float)sizeGroup.Value.Average(f => f.price);
                int sizeMultiplier = GetSizeMultiplier(sizeGroup.Key);
                float pricePerUnit = avgPrice / sizeMultiplier;
                float efficiency = 10f / pricePerUnit; // Higher is better
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{sizeGroup.Key}:", GUILayout.Width(80));
                GUILayout.Label($"Avg Price: {avgPrice:F1}", GUILayout.Width(100));
                GUILayout.Label($"Per Unit: {pricePerUnit:F1}", GUILayout.Width(80));
                GUILayout.Label($"Efficiency: {efficiency:F2}", GUILayout.Width(80));
                
                Color efficiencyColor = GetColorForValue(efficiency, 0.5f, 2f);
                DrawProgressBar(Mathf.Clamp01(efficiency / 2f), efficiencyColor);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🎯 Bonus Points by Size", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        foreach (var sizeGroup in sortedSizes)
        {
            if (sizeGroup.Value.Count > 0)
            {
                float avgBonus = (float)sizeGroup.Value.Average(f => f.tagMatchBonusPoints);
                int sizeMultiplier = GetSizeMultiplier(sizeGroup.Key);
                float bonusPerUnit = avgBonus / sizeMultiplier;
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"{sizeGroup.Key}:", GUILayout.Width(80));
                GUILayout.Label($"Avg Bonus: {avgBonus:F1}", GUILayout.Width(100));
                GUILayout.Label($"Per Unit: {bonusPerUnit:F1}", GUILayout.Width(80));
                
                Color bonusColor = GetColorForValue(bonusPerUnit, 20f, 60f);
                DrawProgressBar(bonusPerUnit / 60f, bonusColor);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawOutliersTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.2f, 0.6f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("⚠️ Price Outliers", sectionStyle);
        EditorGUILayout.Space(5);
        
        if (priceOutliers.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            float avgPrice = (float)allFurniture.Average(f => f.price);
            
            foreach (var outlier in priceOutliers.OrderByDescending(f => Mathf.Abs(f.price - avgPrice)))
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    Selection.activeObject = outlier;
                    EditorGUIUtility.PingObject(outlier);
                }
                
                GUILayout.Label(outlier.Name, GUILayout.Width(120));
                GUILayout.Label($"Price: {outlier.price}", GUILayout.Width(80));
                GUILayout.Label($"Size: {outlier.typeOfSize}", GUILayout.Width(80));
                
                float deviation = ((outlier.price - avgPrice) / avgPrice) * 100f;
                string deviationText = deviation > 0 ? $"+{deviation:F0}%" : $"{deviation:F0}%";
                Color deviationColor = deviation > 0 ? new Color(1f, 0.7f, 0.7f) : new Color(0.7f, 0.7f, 1f);
                
                GUI.backgroundColor = deviationColor;
                GUILayout.Label(deviationText, EditorStyles.miniButton, GUILayout.Width(60));
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("✅ No significant price outliers found!", MessageType.Info);
        }
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🎯 Bonus Point Outliers", sectionStyle);
        EditorGUILayout.Space(5);
        
        if (bonusOutliers.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            float avgBonus = (float)allFurniture.Average(f => f.tagMatchBonusPoints);
            
            foreach (var outlier in bonusOutliers.OrderByDescending(f => Mathf.Abs(f.tagMatchBonusPoints - avgBonus)))
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    Selection.activeObject = outlier;
                    EditorGUIUtility.PingObject(outlier);
                }
                
                GUILayout.Label(outlier.Name, GUILayout.Width(120));
                GUILayout.Label($"Bonus: {outlier.tagMatchBonusPoints}", GUILayout.Width(80));
                GUILayout.Label($"Tag: {outlier.furnitureTag}", GUILayout.Width(80));
                
                float deviation = ((outlier.tagMatchBonusPoints - avgBonus) / avgBonus) * 100f;
                string deviationText = deviation > 0 ? $"+{deviation:F0}%" : $"{deviation:F0}%";
                Color deviationColor = deviation > 0 ? new Color(1f, 0.7f, 0.7f) : new Color(0.7f, 0.7f, 1f);
                
                GUI.backgroundColor = deviationColor;
                GUILayout.Label(deviationText, EditorStyles.miniButton, GUILayout.Width(60));
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("✅ No significant bonus point outliers found!", MessageType.Info);
        }
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🔍 Potential Issues", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        // Check for free items
        var freeItems = allFurniture.Where(f => f.price == 0).ToList();
        if (freeItems.Count > 0)
        {
            EditorGUILayout.HelpBox($"💰 {freeItems.Count} items have 0 price: {string.Join(", ", freeItems.Take(5).Select(f => f.Name))}{(freeItems.Count > 5 ? "..." : "")}", MessageType.Warning);
        }
        
        // Check for items with no compatibles
        var isolatedItems = allFurniture.Where(f => f.compatibles == null || f.compatibles.Length == 0).ToList();
        if (isolatedItems.Count > 0)
        {
            float isolatedPercentage = (isolatedItems.Count / (float)allFurniture.Count) * 100f;
            EditorGUILayout.HelpBox($"🔗 {isolatedItems.Count} items ({isolatedPercentage:F1}%) have no compatible furniture", MessageType.Info);
        }
        
        // Check for items with missing sprites
        var noSpriteItems = allFurniture.Where(f => f.sprites == null || f.sprites.Length == 0).ToList();
        if (noSpriteItems.Count > 0)
        {
            EditorGUILayout.HelpBox($"🖼️ {noSpriteItems.Count} items have no sprites: {string.Join(", ", noSpriteItems.Take(3).Select(f => f.Name))}{(noSpriteItems.Count > 3 ? "..." : "")}", MessageType.Error);
        }
        
        // Check for items with missing prefabs
        var noPrefabItems = allFurniture.Where(f => f.prefab == null).ToList();
        if (noPrefabItems.Count > 0)
        {
            EditorGUILayout.HelpBox($"🎮 {noPrefabItems.Count} items have no prefab: {string.Join(", ", noPrefabItems.Take(3).Select(f => f.Name))}{(noPrefabItems.Count > 3 ? "..." : "")}", MessageType.Warning);
        }
        
        // Check for duplicate names
        var duplicateNames = allFurniture.GroupBy(f => f.Name).Where(g => g.Count() > 1).ToList();
        if (duplicateNames.Count > 0)
        {
            EditorGUILayout.HelpBox($"📝 {duplicateNames.Count} duplicate names found: {string.Join(", ", duplicateNames.Take(3).Select(g => g.Key))}{(duplicateNames.Count > 3 ? "..." : "")}", MessageType.Warning);
        }
        
        // Check for missing translations
        var missingSpanish = allFurniture.Where(f => string.IsNullOrEmpty(f.es_Name)).ToList();
        if (missingSpanish.Count > 0)
        {
            EditorGUILayout.HelpBox($"🌍 {missingSpanish.Count} items missing Spanish translation", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📊 Balance Recommendations", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        // Price recommendations
        float priceRange = allFurniture.Max(f => f.price) - allFurniture.Min(f => f.price);
        if (priceRange > 100)
        {
            EditorGUILayout.HelpBox("💰 Large price range detected. Consider creating price tiers or categories.", MessageType.Info);
        }
        
        // Size distribution recommendations
        var sizeDistribution = furnitureBySize.Values.Select(list => list.Count).ToList();
        float sizeVariance = sizeDistribution.Count > 1 ? sizeDistribution.Select(x => (float)x).ToList().Aggregate(0f, (acc, x) => acc + Mathf.Pow((float)(x - sizeDistribution.Average()), 2)) / sizeDistribution.Count : 0;
        
        if (sizeVariance > 25)
        {
            EditorGUILayout.HelpBox("📐 Uneven size distribution. Consider balancing furniture sizes.", MessageType.Info);
        }
        
        // Tag recommendations
        if (furnitureByTag.ContainsKey(RoomTag.None))
        {
            float nonePercentage = (furnitureByTag[RoomTag.None].Count / (float)allFurniture.Count) * 100f;
            if (nonePercentage > 20f)
            {
                EditorGUILayout.HelpBox($"🏷️ {nonePercentage:F1}% of items have no room tag. Consider categorizing them.", MessageType.Warning);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    // Helper Methods
    private void DrawStatRow(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(150));
        GUILayout.FlexibleSpace();
        GUIStyle valueStyle = new GUIStyle(EditorStyles.boldLabel);
        valueStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        GUILayout.Label(value, valueStyle, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }
    
    private void DrawProgressBar(float progress, Color color)
    {
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(16));
        rect.width = Mathf.Max(rect.width - 10, 100);
        
        // Background
        EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
        
        // Progress
        Rect progressRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(progress), rect.height);
        EditorGUI.DrawRect(progressRect, color);
        
        // Border
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height - 1, rect.width, 1), Color.gray);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.gray);
        EditorGUI.DrawRect(new Rect(rect.x + rect.width - 1, rect.y, 1, rect.height), Color.gray);
    }
    
    private void DrawSeparator()
    {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(5);
    }
    
    private int GetSizeMultiplier(TypeOfSize sizeType)
    {
        switch (sizeType)
        {
            case TypeOfSize.one_one: return 1;
            case TypeOfSize.two_one: return 2;
            case TypeOfSize.two_two: return 4;
            case TypeOfSize.three_one: return 3;
            default: return 1;
        }
    }
    
    private Color GetColorForValue(float value, float min, float max)
    {
        float normalized = Mathf.Clamp01((value - min) / (max - min));
        return Color.Lerp(new Color(0.7f, 0.9f, 0.7f), new Color(0.9f, 0.7f, 0.7f), normalized);
    }
    
    private Color GetTagColor(RoomTag tag)
    {
        switch (tag)
        {
            case RoomTag.Kitchen: return new Color(1f, 0.8f, 0.6f);
            case RoomTag.LivingRoom: return new Color(0.8f, 1f, 0.6f);
            case RoomTag.Bedroom: return new Color(0.8f, 0.6f, 1f);
            case RoomTag.Bathroom: return new Color(0.6f, 0.8f, 1f);
            case RoomTag.Office: return new Color(1f, 0.6f, 0.8f);
            case RoomTag.DiningRoom: return new Color(0.6f, 1f, 0.8f);
            case RoomTag.Gym: return new Color(1f, 0.6f, 0.6f);
            case RoomTag.Lab: return new Color(0.6f, 0.6f, 1f);
            case RoomTag.None: return new Color(0.7f, 0.7f, 0.7f);
            default: return new Color(0.8f, 0.8f, 0.8f);
        }
    }
    
    private Color GetSizeColor(TypeOfSize sizeType)
    {
        switch (sizeType)
        {
            case TypeOfSize.one_one: return new Color(0.7f, 1f, 0.7f);
            case TypeOfSize.two_one: return new Color(1f, 0.9f, 0.6f);
            case TypeOfSize.two_two: return new Color(1f, 0.7f, 0.7f);
            case TypeOfSize.three_one: return new Color(0.9f, 0.6f, 1f);
            default: return new Color(0.8f, 0.8f, 0.8f);
        }
    }
}
#endif