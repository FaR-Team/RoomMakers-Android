#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FurnitureRelationshipVisualizer : EditorWindow
{
    private Vector2 scrollPosition;
    private Vector2 graphScrollPosition;
    private int selectedTab = 0;
    private string[] tabNames = { "Graph View", "Compatibility", "Combos", "Dependencies", "Analysis" };
    
    private List<FurnitureOriginalData> allFurniture = new List<FurnitureOriginalData>();
    private Dictionary<FurnitureOriginalData, Vector2> nodePositions = new Dictionary<FurnitureOriginalData, Vector2>();
    private Dictionary<FurnitureOriginalData, List<FurnitureOriginalData>> compatibilityMap = new Dictionary<FurnitureOriginalData, List<FurnitureOriginalData>>();
    private Dictionary<FurnitureOriginalData, List<FurnitureOriginalData>> comboMap = new Dictionary<FurnitureOriginalData, List<FurnitureOriginalData>>();
    private Dictionary<FurnitureOriginalData, List<FurnitureOriginalData>> dependencyMap = new Dictionary<FurnitureOriginalData, List<FurnitureOriginalData>>();
    
    private bool dataLoaded = false;
    private FurnitureOriginalData selectedFurniture = null;
    private FurnitureOriginalData hoveredFurniture = null;
    private bool showCompatibilityLines = true;
    private bool showComboLines = true;
    private bool showDependencyLines = true;
    private bool autoLayout = true;
    private float nodeSize = 60f;
    private float graphZoom = 1f;
    private Vector2 graphOffset = Vector2.zero;
    
    // Colors
    private Color compatibilityColor = new Color(0.2f, 0.8f, 0.2f, 0.6f);
    private Color comboColor = new Color(0.8f, 0.2f, 0.8f, 0.6f);
    private Color dependencyColor = new Color(0.8f, 0.6f, 0.2f, 0.6f);
    private Color selectedColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    private Color hoveredColor = new Color(1f, 1f, 0.2f, 0.8f);
    
    [MenuItem("DevTools/Furniture Relationship Visualizer")]
    public static void ShowWindow()
    {
        GetWindow<FurnitureRelationshipVisualizer>("Furniture Relationships");
    }
    
    private void OnEnable()
    {
        LoadFurnitureData();
    }
    
    private void OnGUI()
    {
        DrawHeader();
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.Height(25));
        
        EditorGUILayout.Space(10);
        DrawSeparator();
        
        if (!dataLoaded)
        {
            DrawNoDataMessage();
            return;
        }
        
        switch (selectedTab)
        {
            case 0: DrawGraphViewTab(); break;
            case 1: DrawCompatibilityTab(); break;
            case 2: DrawCombosTab(); break;
            case 3: DrawDependenciesTab(); break;
            case 4: DrawAnalysisTab(); break;
        }
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 16;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("🕸️ Furniture Relationship Visualizer", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("🔄 Refresh Data", GUILayout.Width(100)))
        {
            LoadFurnitureData();
        }
        
        if (GUILayout.Button("🎯 Auto Layout", GUILayout.Width(100)))
        {
            GenerateAutoLayout();
        }
        
        GUILayout.FlexibleSpace();
        
        if (dataLoaded)
        {
            GUIStyle dataStyle = new GUIStyle(EditorStyles.miniLabel);
            dataStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.Label($"📊 {allFurniture.Count} items • {GetTotalConnections()} connections", dataStyle);
        }
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
    }
    
    private void DrawGraphViewTab()
    {
        DrawGraphControls();
        EditorGUILayout.Space(10);
        DrawSeparator();
        EditorGUILayout.Space(10);
        
        // Graph area
        Rect graphRect = EditorGUILayout.GetControlRect(GUILayout.Height(400));
        DrawGraph(graphRect);
        
        EditorGUILayout.Space(10);
        DrawSeparator();
        
        // Selected item info
        if (selectedFurniture != null)
        {
            DrawSelectedFurnitureInfo();
        }
        else
        {
            EditorGUILayout.HelpBox("💡 Click on a node in the graph to see detailed information", MessageType.Info);
        }
    }
    
    private void DrawGraphControls()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        
        GUILayout.Label("🎛️ Graph Controls", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Show Connections:", GUILayout.Width(120));
        
        GUI.backgroundColor = showCompatibilityLines ? new Color(0.7f, 1f, 0.7f) : Color.white;
        showCompatibilityLines = GUILayout.Toggle(showCompatibilityLines, "🤝 Compatible", EditorStyles.miniButton, GUILayout.Width(80));
        
        GUI.backgroundColor = showComboLines ? new Color(1f, 0.7f, 1f) : Color.white;
        showComboLines = GUILayout.Toggle(showComboLines, "✨ Combos", EditorStyles.miniButton, GUILayout.Width(70));
        
        GUI.backgroundColor = showDependencyLines ? new Color(1f, 0.9f, 0.7f) : Color.white;
        showDependencyLines = GUILayout.Toggle(showDependencyLines, "🔗 Dependencies", EditorStyles.miniButton, GUILayout.Width(90));
        
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Node Size:", GUILayout.Width(70));
        nodeSize = EditorGUILayout.Slider(nodeSize, 30f, 100f, GUILayout.Width(150));
        
        GUILayout.Space(20);
        
        GUILayout.Label("Zoom:", GUILayout.Width(40));
        graphZoom = EditorGUILayout.Slider(graphZoom, 0.5f, 2f, GUILayout.Width(100));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawGraph(Rect graphRect)
    {
        // Background
        EditorGUI.DrawRect(graphRect, new Color(0.2f, 0.2f, 0.2f, 1f));
        
        GUI.BeginGroup(graphRect);
        
        // Handle mouse events
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;
        
        // Draw connections first (so they appear behind nodes)
        if (showCompatibilityLines)
            DrawConnections(compatibilityMap, compatibilityColor, graphRect);
        if (showComboLines)
            DrawConnections(comboMap, comboColor, graphRect);
        if (showDependencyLines)
            DrawConnections(dependencyMap, dependencyColor, graphRect);
        
        // Draw nodes
        hoveredFurniture = null;
        foreach (var furniture in allFurniture)
        {
            Vector2 nodePos = GetNodeScreenPosition(furniture, graphRect);
            Rect nodeRect = new Rect(nodePos.x - nodeSize/2, nodePos.y - nodeSize/2, nodeSize, nodeSize);
            
            // Check if mouse is over this node
            bool isHovered = nodeRect.Contains(mousePos);
            if (isHovered)
                hoveredFurniture = furniture;
            
            // Determine node color
            Color nodeColor = Color.white;
            if (furniture == selectedFurniture)
                nodeColor = selectedColor;
            else if (isHovered)
                nodeColor = hoveredColor;
            else
                nodeColor = GetFurnitureColor(furniture);
            
            // Draw node
            EditorGUI.DrawRect(nodeRect, nodeColor);
            EditorGUI.DrawRect(new Rect(nodeRect.x, nodeRect.y, nodeRect.width, 1), Color.black);
            EditorGUI.DrawRect(new Rect(nodeRect.x, nodeRect.y + nodeRect.height - 1, nodeRect.width, 1), Color.black);
            EditorGUI.DrawRect(new Rect(nodeRect.x, nodeRect.y, 1, nodeRect.height), Color.black);
            EditorGUI.DrawRect(new Rect(nodeRect.x + nodeRect.width - 1, nodeRect.y, 1, nodeRect.height), Color.black);
            
            // Draw furniture name
            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontSize = Mathf.RoundToInt(10 * graphZoom);
            labelStyle.normal.textColor = Color.black;
            
            string displayName = furniture.Name.Length > 8 ? furniture.Name.Substring(0, 8) + "..." : furniture.Name;
            GUI.Label(nodeRect, displayName, labelStyle);
            
            // Handle node click
            if (e.type == EventType.MouseDown && e.button == 0 && nodeRect.Contains(mousePos))
            {
                selectedFurniture = furniture;
                e.Use();
            }
        }
        
        // Handle graph panning
        if (e.type == EventType.MouseDrag && e.button == 1)
        {
            graphOffset += e.delta;
            Repaint();
        }
        
        GUI.EndGroup();
        
        // Draw legend
        DrawGraphLegend(graphRect);
    }
    
    private void DrawConnections(Dictionary<FurnitureOriginalData, List<FurnitureOriginalData>> connectionMap, Color lineColor, Rect graphRect)
    {
        foreach (var kvp in connectionMap)
        {
            Vector2 fromPos = GetNodeScreenPosition(kvp.Key, graphRect);
            
            foreach (var target in kvp.Value)
            {
                if (target != null)
                {
                    Vector2 toPos = GetNodeScreenPosition(target, graphRect);
                    DrawLine(fromPos, toPos, lineColor);
                }
            }
        }
    }
    
    private void DrawLine(Vector2 from, Vector2 to, Color color)
    {
        Vector3[] points = { new Vector3(from.x, from.y, 0), new Vector3(to.x, to.y, 0) };
        Handles.color = color;
        Handles.DrawAAPolyLine(2f, points);
    }
    
    private void DrawGraphLegend(Rect graphRect)
    {
        Rect legendRect = new Rect(graphRect.x + 10, graphRect.y + 10, 200, 80);
        EditorGUI.DrawRect(legendRect, new Color(0f, 0f, 0f, 0.7f));
        
        GUIStyle legendStyle = new GUIStyle(EditorStyles.miniLabel);
        legendStyle.normal.textColor = Color.white;
        
        GUI.Label(new Rect(legendRect.x + 5, legendRect.y + 5, 190, 15), "🕸️ Connection Types:", legendStyle);
        
        if (showCompatibilityLines)
        {
            EditorGUI.DrawRect(new Rect(legendRect.x + 5, legendRect.y + 20, 15, 2), compatibilityColor);
            GUI.Label(new Rect(legendRect.x + 25, legendRect.y + 17, 100, 15), "🤝 Compatible", legendStyle);
        }
        
        if (showComboLines)
        {
            EditorGUI.DrawRect(new Rect(legendRect.x + 5, legendRect.y + 35, 15, 2), comboColor);
            GUI.Label(new Rect(legendRect.x + 25, legendRect.y + 32, 100, 15), "✨ Combo Trigger", legendStyle);
        }
        
        if (showDependencyLines)
        {
            EditorGUI.DrawRect(new Rect(legendRect.x + 5, legendRect.y + 50, 15, 2), dependencyColor);
            GUI.Label(new Rect(legendRect.x + 25, legendRect.y + 47, 100, 15), "🔗 Dependency", legendStyle);
        }
        
        GUI.Label(new Rect(legendRect.x + 5, legendRect.y + 65, 190, 15), "💡 Right-click drag to pan", legendStyle);
    }
    
    private Vector2 GetNodeScreenPosition(FurnitureOriginalData furniture, Rect graphRect)
    {
        if (!nodePositions.ContainsKey(furniture))
            return Vector2.zero;
        
        Vector2 worldPos = nodePositions[furniture];
        Vector2 screenPos = (worldPos * graphZoom) + graphOffset + new Vector2(graphRect.width/2, graphRect.height/2);
        return screenPos;
    }
    
    private void DrawSelectedFurnitureInfo()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 1f);
        
        GUILayout.Label($"🔍 Selected: {selectedFurniture.Name}", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🔍 Focus in Project", EditorStyles.miniButton, GUILayout.Width(120)))
        {
            Selection.activeObject = selectedFurniture;
            EditorGUIUtility.PingObject(selectedFurniture);
        }
        
        if (GUILayout.Button("🎯 Center in Graph", EditorStyles.miniButton, GUILayout.Width(120)))
        {
            CenterNodeInGraph(selectedFurniture);
        }
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Basic info
        DrawInfoRow("📝 Name", selectedFurniture.Name);
        DrawInfoRow("🌍 Spanish", selectedFurniture.es_Name);
        DrawInfoRow("💰 Price", selectedFurniture.price.ToString());
        DrawInfoRow("📐 Size", $"{selectedFurniture.size.x}x{selectedFurniture.size.y}");
        DrawInfoRow("🏷️ Tag", selectedFurniture.furnitureTag.ToString());
        
        EditorGUILayout.Space(10);
        DrawSeparator();
        EditorGUILayout.Space(5);
        
        // Relationships
        GUILayout.Label("🔗 Relationships:", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // Compatible furniture
        if (compatibilityMap.ContainsKey(selectedFurniture) && compatibilityMap[selectedFurniture].Count > 0)
        {
            GUILayout.Label($"🤝 Compatible with ({compatibilityMap[selectedFurniture].Count}):", EditorStyles.miniLabel);
            foreach (var compatible in compatibilityMap[selectedFurniture].Take(5))
            {
                if (compatible != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button(compatible.Name, EditorStyles.linkLabel))
                    {
                        selectedFurniture = compatible;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            if (compatibilityMap[selectedFurniture].Count > 5)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label($"... and {compatibilityMap[selectedFurniture].Count - 5} more", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }
        
        // Combo relationships
        if (comboMap.ContainsKey(selectedFurniture) && comboMap[selectedFurniture].Count > 0)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label($"✨ Triggers combos with ({comboMap[selectedFurniture].Count}):", EditorStyles.miniLabel);
            foreach (var combo in comboMap[selectedFurniture])
            {
                if (combo != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button(combo.Name, EditorStyles.linkLabel))
                    {
                        selectedFurniture = combo;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        
        // Dependency relationships
        if (dependencyMap.ContainsKey(selectedFurniture) && dependencyMap[selectedFurniture].Count > 0)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label($"🔗 Depends on ({dependencyMap[selectedFurniture].Count}):", EditorStyles.miniLabel);
            foreach (var dependency in dependencyMap[selectedFurniture])
            {
                if (dependency != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    if (GUILayout.Button(dependency.Name, EditorStyles.linkLabel))
                    {
                        selectedFurniture = dependency;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        
        // Items that depend on this one
        var dependents = dependencyMap.Where(kvp => kvp.Value.Contains(selectedFurniture)).Select(kvp => kvp.Key).ToList();
        if (dependents.Count > 0)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label($"🔗 Required by ({dependents.Count}):", EditorStyles.miniLabel);
            foreach (var dependent in dependents.Take(3))
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                if (GUILayout.Button(dependent.Name, EditorStyles.linkLabel))
                {
                    selectedFurniture = dependent;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (dependents.Count > 3)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label($"... and {dependents.Count - 3} more", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawCompatibilityTab()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.8f, 0.2f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🤝 Compatibility Matrix", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Items that can be placed on other furniture", MessageType.Info);
        EditorGUILayout.Space(10);
        
        foreach (var kvp in compatibilityMap.OrderBy(x => x.Key.Name))
        {
            if (kvp.Value.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    Selection.activeObject = kvp.Key;
                    EditorGUIUtility.PingObject(kvp.Key);
                }
                
                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
                GUILayout.Label(kvp.Key.Name, nameStyle);
                
                GUILayout.FlexibleSpace();
                GUILayout.Label($"({kvp.Value.Count} compatible)", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                // Show compatible items in a grid
                int itemsPerRow = 4;
                for (int i = 0; i < kvp.Value.Count; i += itemsPerRow)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (int j = 0; j < itemsPerRow && i + j < kvp.Value.Count; j++)
                    {
                        var compatible = kvp.Value[i + j];
                        if (compatible != null)
                        {
                            if (GUILayout.Button(compatible.Name, EditorStyles.miniButton))
                            {
                                selectedFurniture = compatible;
                                selectedTab = 0; // Switch to graph view
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawCombosTab()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.8f, 0.2f, 0.8f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("✨ Combo Relationships", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Items that trigger special combo sprites when placed together", MessageType.Info);
        EditorGUILayout.Space(10);
        
        var comboItems = allFurniture.Where(f => f.hasComboSprite).ToList();
        
        if (comboItems.Count == 0)
        {
            EditorGUILayout.HelpBox("No furniture items have combo sprites configured.", MessageType.Warning);
        }
        else
        {
            foreach (var furniture in comboItems.OrderBy(f => f.Name))
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    Selection.activeObject = furniture;
                    EditorGUIUtility.PingObject(furniture);
                }
                
                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.normal.textColor = new Color(0.8f, 0.2f, 0.8f);
                GUILayout.Label($"✨ {furniture.Name}", nameStyle);
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                if (furniture.comboTriggerFurniture != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Triggered by:", EditorStyles.miniLabel, GUILayout.Width(80));
                    if (GUILayout.Button(furniture.comboTriggerFurniture.Name, EditorStyles.linkLabel))
                    {
                        selectedFurniture = furniture.comboTriggerFurniture;
                        selectedTab = 0;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("⚠️ No combo trigger set!", EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawDependenciesTab()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.8f, 0.6f, 0.2f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("🔗 Dependency Relationships", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Items that require other furniture or infrastructure to function", MessageType.Info);
        EditorGUILayout.Space(10);
        
        foreach (var kvp in dependencyMap.OrderBy(x => x.Key.Name))
        {
            if (kvp.Value.Count > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    Selection.activeObject = kvp.Key;
                    EditorGUIUtility.PingObject(kvp.Key);
                }
                
                GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel);
                nameStyle.normal.textColor = new Color(0.8f, 0.6f, 0.2f);
                GUILayout.Label($"🔗 {kvp.Key.Name}", nameStyle);
                
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
                
                foreach (var dependency in kvp.Value)
                {
                    if (dependency != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20);
                        GUILayout.Label("Requires:", EditorStyles.miniLabel, GUILayout.Width(60));
                        if (GUILayout.Button(dependency.Name, EditorStyles.linkLabel))
                        {
                            selectedFurniture = dependency;
                            selectedTab = 0;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawAnalysisTab()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.2f, 0.6f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📊 Relationship Analysis", sectionStyle);
        EditorGUILayout.Space(5);
        
        // Connection statistics
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("📈 Connection Statistics", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        int totalCompatible = compatibilityMap.Values.Sum(list => list.Count);
        int totalCombos = comboMap.Values.Sum(list => list.Count);
        int totalDependencies = dependencyMap.Values.Sum(list => list.Count);
        DrawStatRow("Total Compatibility Connections", totalCompatible.ToString());
        DrawStatRow("Total Combo Connections", totalCombos.ToString());
        DrawStatRow("Total Dependency Connections", totalDependencies.ToString());
        DrawStatRow("Total Connections", GetTotalConnections().ToString());
        
        float avgCompatible = allFurniture.Count > 0 ? (float)totalCompatible / allFurniture.Count : 0;
        DrawStatRow("Avg Compatible per Item", avgCompatible.ToString("F1"));
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        // Most connected items
        EditorGUILayout.Space(10);
        GUILayout.Label("🌟 Most Connected Items", sectionStyle);
        EditorGUILayout.Space(5);
        
        var mostConnected = allFurniture
            .Select(f => new { 
                Furniture = f, 
                Connections = GetConnectionCount(f) 
            })
            .OrderByDescending(x => x.Connections)
            .Take(10)
            .ToList();
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        foreach (var item in mostConnected)
        {
            if (item.Connections > 0)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    selectedFurniture = item.Furniture;
                    selectedTab = 0;
                }
                
                GUILayout.Label(item.Furniture.Name, GUILayout.Width(150));
                GUILayout.Label($"{item.Connections} connections", GUILayout.Width(100));
                
                // Connection breakdown
                int compatible = compatibilityMap.ContainsKey(item.Furniture) ? compatibilityMap[item.Furniture].Count : 0;
                int combos = comboMap.ContainsKey(item.Furniture) ? comboMap[item.Furniture].Count : 0;
                int dependencies = dependencyMap.ContainsKey(item.Furniture) ? dependencyMap[item.Furniture].Count : 0;
                
                GUILayout.Label($"({compatible}🤝 {combos}✨ {dependencies}🔗)", EditorStyles.miniLabel);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        // Orphaned items
        EditorGUILayout.Space(10);
        GUILayout.Label("🏝️ Isolated Items", sectionStyle);
        EditorGUILayout.Space(5);
        
        var orphanedItems = allFurniture.Where(f => GetConnectionCount(f) == 0).ToList();
        
        if (orphanedItems.Count > 0)
        {
            EditorGUILayout.HelpBox($"⚠️ {orphanedItems.Count} items have no relationships with other furniture", MessageType.Warning);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            foreach (var orphan in orphanedItems.Take(10))
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    Selection.activeObject = orphan;
                    EditorGUIUtility.PingObject(orphan);
                }
                
                GUILayout.Label(orphan.Name, GUILayout.Width(150));
                GUILayout.Label($"Tag: {orphan.furnitureTag}", GUILayout.Width(100));
                GUILayout.Label($"Size: {orphan.typeOfSize}", EditorStyles.miniLabel);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
            
            if (orphanedItems.Count > 10)
            {
                GUILayout.Label($"... and {orphanedItems.Count - 10} more", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("✅ All items have at least one relationship!", MessageType.Info);
        }
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        // Broken relationships
        EditorGUILayout.Space(10);
        GUILayout.Label("🔧 Broken Relationships", sectionStyle);
        EditorGUILayout.Space(5);
        
        var brokenRelationships = FindBrokenRelationships();
        
        if (brokenRelationships.Count > 0)
        {
            EditorGUILayout.HelpBox($"⚠️ {brokenRelationships.Count} broken relationships found", MessageType.Error);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            foreach (var broken in brokenRelationships.Take(10))
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("🔍", EditorStyles.miniButton, GUILayout.Width(25)))
                {
                    Selection.activeObject = broken.Source;
                    EditorGUIUtility.PingObject(broken.Source);
                }
                
                GUILayout.Label(broken.Source.Name, GUILayout.Width(120));
                GUILayout.Label("→", GUILayout.Width(20));
                GUILayout.Label($"Missing {broken.Type}", EditorStyles.miniLabel);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }
            
            if (brokenRelationships.Count > 10)
            {
                GUILayout.Label($"... and {brokenRelationships.Count - 10} more", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("✅ No broken relationships found!", MessageType.Info);
        }
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        // Recommendations
        EditorGUILayout.Space(10);
        GUILayout.Label("💡 Recommendations", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        // Check for unbalanced connections
        if (avgCompatible < 2f)
        {
            EditorGUILayout.HelpBox("🤝 Consider adding more compatibility relationships to increase furniture synergy", MessageType.Info);
        }
        
        if (totalCombos == 0)
        {
            EditorGUILayout.HelpBox("✨ No combo relationships found. Consider adding special interactions between furniture", MessageType.Info);
        }
        
        if (orphanedItems.Count > allFurniture.Count * 0.3f)
        {
            EditorGUILayout.HelpBox("🏝️ Many items are isolated. Consider creating more relationships to improve gameplay depth", MessageType.Warning);
        }
        
        // Tag-based recommendations
        var tagGroups = allFurniture.GroupBy(f => f.furnitureTag).ToList();
        foreach (var tagGroup in tagGroups)
        {
            var tagItems = tagGroup.ToList();
            var connectedInTag = tagItems.Where(f => GetConnectionCount(f) > 0).Count();
            
            if (connectedInTag < tagItems.Count * 0.5f && tagItems.Count > 2)
            {
                EditorGUILayout.HelpBox($"🏷️ {tagGroup.Key} tag has many unconnected items. Consider adding intra-tag relationships", MessageType.Info);
            }
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
    }
    
    // Helper Methods
    private void LoadFurnitureData()
    {
        allFurniture.Clear();
        nodePositions.Clear();
        compatibilityMap.Clear();
        comboMap.Clear();
        dependencyMap.Clear();
        
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
        
        BuildRelationshipMaps();
        GenerateAutoLayout();
        dataLoaded = allFurniture.Count > 0;
    }
    
    private void BuildRelationshipMaps()
    {
        foreach (var furniture in allFurniture)
        {
            // Compatibility relationships
            if (furniture.compatibles != null && furniture.compatibles.Length > 0)
            {
                compatibilityMap[furniture] = furniture.compatibles.Where(c => c != null).ToList();
            }
            
            // Combo relationships
            if (furniture.hasComboSprite && furniture.comboTriggerFurniture != null)
            {
                if (!comboMap.ContainsKey(furniture.comboTriggerFurniture))
                    comboMap[furniture.comboTriggerFurniture] = new List<FurnitureOriginalData>();
                comboMap[furniture.comboTriggerFurniture].Add(furniture);
            }
            
            // Dependency relationships
            if (furniture.requiredBase != null)
            {
                if (!dependencyMap.ContainsKey(furniture))
                    dependencyMap[furniture] = new List<FurnitureOriginalData>();
                dependencyMap[furniture].Add(furniture.requiredBase);
            }
        }
    }
    
    private void GenerateAutoLayout()
    {
        nodePositions.Clear();
        
        if (allFurniture.Count == 0) return;
        
        // Simple circular layout
        float radius = 200f;
        float angleStep = 360f / allFurniture.Count;
        
        for (int i = 0; i < allFurniture.Count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 position = new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
            nodePositions[allFurniture[i]] = position;
        }
        
        // Improve layout using force-directed algorithm
        for (int iteration = 0; iteration < 50; iteration++)
        {
            Dictionary<FurnitureOriginalData, Vector2> forces = new Dictionary<FurnitureOriginalData, Vector2>();
            
            // Initialize forces
            foreach (var furniture in allFurniture)
            {
                forces[furniture] = Vector2.zero;
            }
            
            // Repulsion forces (keep nodes apart)
            for (int i = 0; i < allFurniture.Count; i++)
            {
                for (int j = i + 1; j < allFurniture.Count; j++)
                {
                    var nodeA = allFurniture[i];
                    var nodeB = allFurniture[j];
                    
                    Vector2 diff = nodePositions[nodeA] - nodePositions[nodeB];
                    float distance = diff.magnitude;
                    
                    if (distance > 0)
                    {
                        Vector2 repulsion = diff.normalized * (100f / (distance * distance));
                        forces[nodeA] += repulsion;
                        forces[nodeB] -= repulsion;
                    }
                }
            }
            
            // Attraction forces (pull connected nodes together)
            foreach (var kvp in compatibilityMap)
            {
                foreach (var target in kvp.Value)
                {
                    if (target != null && nodePositions.ContainsKey(target))
                    {
                        Vector2 diff = nodePositions[target] - nodePositions[kvp.Key];
                        Vector2 attraction = diff * 0.01f;
                        forces[kvp.Key] += attraction;
                        forces[target] -= attraction;
                    }
                }
            }
            
            // Apply forces
            foreach (var furniture in allFurniture)
            {
                nodePositions[furniture] += forces[furniture] * 0.1f;
            }
        }
    }
    
    private void CenterNodeInGraph(FurnitureOriginalData furniture)
    {
        if (nodePositions.ContainsKey(furniture))
        {
            graphOffset = -nodePositions[furniture] * graphZoom;
        }
    }
    
    private int GetTotalConnections()
    {
        return compatibilityMap.Values.Sum(list => list.Count) +
               comboMap.Values.Sum(list => list.Count) +
               dependencyMap.Values.Sum(list => list.Count);
    }
    
    private int GetConnectionCount(FurnitureOriginalData furniture)
    {
        int count = 0;
        
        if (compatibilityMap.ContainsKey(furniture))
            count += compatibilityMap[furniture].Count;
        
        if (comboMap.ContainsKey(furniture))
            count += comboMap[furniture].Count;
        
        if (dependencyMap.ContainsKey(furniture))
            count += dependencyMap[furniture].Count;
        
        // Also count reverse relationships
        count += compatibilityMap.Values.Sum(list => list.Count(f => f == furniture));
        count += comboMap.Values.Sum(list => list.Count(f => f == furniture));
        count += dependencyMap.Values.Sum(list => list.Count(f => f == furniture));
        
        return count;
    }
    
    private Color GetFurnitureColor(FurnitureOriginalData furniture)
    {
        // Color based on room tag
        switch (furniture.furnitureTag)
        {
            case RoomTag.Kitchen: return new Color(1f, 0.8f, 0.6f);
            case RoomTag.LivingRoom: return new Color(0.8f, 1f, 0.6f);
            case RoomTag.Bedroom: return new Color(0.8f, 0.6f, 1f);
            case RoomTag.Bathroom: return new Color(0.6f, 0.8f, 1f);
            case RoomTag.Office: return new Color(1f, 0.6f, 0.8f);
                        case RoomTag.DiningRoom: return new Color(0.6f, 1f, 0.8f);
            case RoomTag.Gym: return new Color(1f, 0.6f, 0.6f);
            case RoomTag.Lab: return new Color(0.6f, 0.6f, 1f);
            case RoomTag.None: return new Color(0.8f, 0.8f, 0.8f);
            default: return new Color(0.9f, 0.9f, 0.9f);
        }
    }
    
    private List<BrokenRelationship> FindBrokenRelationships()
    {
        var broken = new List<BrokenRelationship>();
        
        foreach (var furniture in allFurniture)
        {
            // Check for null compatibles
            if (furniture.compatibles != null)
            {
                foreach (var compatible in furniture.compatibles)
                {
                    if (compatible == null)
                    {
                        broken.Add(new BrokenRelationship 
                        { 
                            Source = furniture, 
                            Type = "Compatible" 
                        });
                    }
                }
            }
            
            // Check for missing combo trigger
            if (furniture.hasComboSprite && furniture.comboTriggerFurniture == null)
            {
                broken.Add(new BrokenRelationship 
                { 
                    Source = furniture, 
                    Type = "Combo Trigger" 
                });
            }
            
            // Check for null required base when it should exist
            if (furniture.requiredBase == null && furniture.Name.ToLower().Contains("kit"))
            {
                // Kits usually don't require bases, so this might be normal
            }
        }
        
        return broken;
    }
    
    private void DrawInfoRow(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(100));
        GUILayout.Label(value, EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }
    
    private void DrawStatRow(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(200));
        GUILayout.FlexibleSpace();
        GUIStyle valueStyle = new GUIStyle(EditorStyles.boldLabel);
        valueStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        GUILayout.Label(value, valueStyle, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }
    
    private void DrawSeparator()
    {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(5);
    }
    
    private void DrawNoDataMessage()
    {
        EditorGUILayout.Space(50);
        
        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        
        GUIStyle messageStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        messageStyle.fontSize = 16;
        messageStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("🕸️", GUILayout.Width(40), GUILayout.Height(40));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("No furniture data found", messageStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("🔄 Refresh Data", GUILayout.Width(120), GUILayout.Height(30)))
        {
            LoadFurnitureData();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }
    
    // Data structures
    private class BrokenRelationship
    {
        public FurnitureOriginalData Source;
        public string Type;
    }
}

// Extension methods for better usability
public static class FurnitureVisualizerExtensions
{
    public static void ShowInRelationshipVisualizer(this FurnitureOriginalData furniture)
    {
        var window = EditorWindow.GetWindow<FurnitureRelationshipVisualizer>("Furniture Relationships");
        // This would require making selectedFurniture public or adding a method to set it
        window.Show();
    }
}
#endif
