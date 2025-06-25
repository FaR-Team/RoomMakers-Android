#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RoomLayoutDesigner : EditorWindow
{
    // Core Data Structures
    [System.Serializable]
    public class PlacedFurniture
    {
        public FurnitureOriginalData furnitureData;
        public Vector2Int gridPosition;
        public bool isWallMounted;
        public int stackLevel = 1;
        public List<PlacedFurniture> stackedItems = new List<PlacedFurniture>();
    }
    
    [System.Serializable]
    public class RoomLayout
    {
        public string roomName;
        public Vector2Int roomSize;
        public RoomTag roomType;
        public List<PlacedFurniture> placedFurniture = new List<PlacedFurniture>();
        public int totalCost;
        public int compatibilityScore;
    }
    
    // UI State
    private RoomLayout currentRoom;
    private List<FurnitureOriginalData> availableFurniture = new List<FurnitureOriginalData>();
    private Vector2 scrollPosition;
    private Vector2 furnitureScrollPosition;
    private int selectedTab = 0;
    private string[] tabs = { "🏠 Design", "📊 Analysis", "💾 Templates" };
    
    // Design Settings
    private Vector2Int defaultRoomSize = new Vector2Int(8, 8);
    private bool showGrid = true;
    private bool showCompatibility = true;
    private bool snapToGrid = true;
    private RoomTag filterByTag = RoomTag.None;
    
    // Selection
    private PlacedFurniture selectedFurniture;
    private FurnitureOriginalData draggedFurniture;
    private Vector2Int dragPosition;
    
    [MenuItem("DevTools/Room Layout Designer")]
    public static void ShowWindow()
    {
        var window = GetWindow<RoomLayoutDesigner>("Room Layout Designer");
        window.minSize = new Vector2(1200, 700);
    }
    
    private void OnEnable()
    {
        LoadAvailableFurniture();
        CreateNewRoom();
    }
    
    private void OnGUI()
    {
        DrawHeader();
        selectedTab = GUILayout.Toolbar(selectedTab, tabs);
        
        EditorGUILayout.BeginHorizontal();
        
        // Left Panel - Furniture Library & Settings
        EditorGUILayout.BeginVertical(GUILayout.Width(300));
        DrawLeftPanel();
        EditorGUILayout.EndVertical();
        
        // Main Canvas - Room Grid
        EditorGUILayout.BeginVertical();
        DrawRoomCanvas();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        HandleInput();
    }
    
    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        GUILayout.Label("🏠 Room Layout Designer", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        
        // Room Info
        GUILayout.Label($"💰 ${currentRoom.totalCost}", EditorStyles.miniLabel);
        GUILayout.Label($"⭐ {currentRoom.compatibilityScore}", EditorStyles.miniLabel);
        
        // Quick Actions
        if (GUILayout.Button("🆕 New", EditorStyles.toolbarButton))
            CreateNewRoom();
        if (GUILayout.Button("💾 Save", EditorStyles.toolbarButton))
            SaveRoomTemplate();
        if (GUILayout.Button("📂 Load", EditorStyles.toolbarButton))
            LoadRoomTemplate();
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLeftPanel()
    {
        switch (selectedTab)
        {
            case 0: DrawDesignPanel(); break;
            case 1: DrawAnalysisPanel(); break;
            case 2: DrawTemplatesPanel(); break;
        }
    }
    
    private void DrawDesignPanel()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Room Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("🏠 Room Settings", EditorStyles.boldLabel);
        
        currentRoom.roomName = EditorGUILayout.TextField("Name", currentRoom.roomName);
        currentRoom.roomType = (RoomTag)EditorGUILayout.EnumPopup("Type", currentRoom.roomType);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Size");
        currentRoom.roomSize.x = EditorGUILayout.IntField(currentRoom.roomSize.x, GUILayout.Width(50));
        GUILayout.Label("×");
        currentRoom.roomSize.y = EditorGUILayout.IntField(currentRoom.roomSize.y, GUILayout.Width(50));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // Visual Settings
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("👁️ Display", EditorStyles.boldLabel);
        
        showGrid = EditorGUILayout.Toggle("Show Grid", showGrid);
        showCompatibility = EditorGUILayout.Toggle("Show Compatibility", showCompatibility);
        snapToGrid = EditorGUILayout.Toggle("Snap to Grid", snapToGrid);
        
        EditorGUILayout.EndVertical();
        
        // Furniture Library
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("🪑 Furniture Library", EditorStyles.boldLabel);
        if (GUILayout.Button("🔄", GUILayout.Width(25)))
            LoadAvailableFurniture();
        EditorGUILayout.EndHorizontal();
        
        // Filter
        filterByTag = (RoomTag)EditorGUILayout.EnumPopup("Filter by Tag", filterByTag);
        
        var filteredFurniture = filterByTag == RoomTag.None 
            ? availableFurniture 
            : availableFurniture.Where(f => f.furnitureTag == filterByTag || f.furnitureTag == RoomTag.None).ToList();
        
        furnitureScrollPosition = EditorGUILayout.BeginScrollView(furnitureScrollPosition, GUILayout.Height(400));
        
        foreach (var furniture in filteredFurniture)
        {
            DrawFurnitureItem(furniture);
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawFurnitureItem(FurnitureOriginalData furniture)
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        
        // Sprite preview
        if (furniture.sprites?.Length > 0 && furniture.sprites[0] != null)
        {
            var sprite = furniture.sprites[0];
            var texture = sprite.texture;
            var rect = sprite.rect;
            
            GUILayout.Label("", GUILayout.Width(32), GUILayout.Height(32));
            var iconRect = GUILayoutUtility.GetLastRect();
            
            var normalizedRect = new Rect(
                rect.x / texture.width,
                rect.y / texture.height,
                rect.width / texture.width,
                rect.height / texture.height
            );
            
            GUI.DrawTextureWithTexCoords(iconRect, texture, normalizedRect);
        }
        else
        {
            GUILayout.Label("📦", GUILayout.Width(32), GUILayout.Height(32));
        }
        
        // Info
        EditorGUILayout.BeginVertical();
        GUILayout.Label(furniture.Name, EditorStyles.boldLabel);
        GUILayout.Label($"{furniture.size.x}×{furniture.size.y} | ${furniture.price}", EditorStyles.miniLabel);
        GUILayout.Label($"{GetTagIcon(furniture.furnitureTag)} {furniture.furnitureTag}", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
        
        // Actions
        if (GUILayout.Button("➕", GUILayout.Width(30)))
        {
            AddFurnitureToRoom(furniture);
        }
        
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawRoomCanvas()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label($"🏠 {currentRoom.roomName} ({currentRoom.roomSize.x}×{currentRoom.roomSize.y})", EditorStyles.boldLabel);
        
        // Canvas area
        var canvasRect = GUILayoutUtility.GetRect(600, 400, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        
        DrawRoomGrid(canvasRect);
        DrawPlacedFurniture(canvasRect);
        DrawDragPreview(canvasRect);
        
        EditorGUILayout.EndVertical();
        
        // Selected furniture info
        if (selectedFurniture != null)
        {
            DrawSelectedFurnitureInfo();
        }
    }
    
    private void DrawRoomGrid(Rect canvasRect)
    {
        if (!showGrid) return;
        
        var cellSize = Mathf.Min(canvasRect.width / currentRoom.roomSize.x, canvasRect.height / currentRoom.roomSize.y);
        
        Handles.BeginGUI();
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
        // Vertical lines
        for (int x = 0; x <= currentRoom.roomSize.x; x++)
        {
            var startPos = new Vector3(canvasRect.x + x * cellSize, canvasRect.y);
            var endPos = new Vector3(canvasRect.x + x * cellSize, canvasRect.y + currentRoom.roomSize.y * cellSize);
            Handles.DrawLine(startPos, endPos);
        }
        
        // Horizontal lines
        for (int y = 0; y <= currentRoom.roomSize.y; y++)
        {
            var startPos = new Vector3(canvasRect.x, canvasRect.y + y * cellSize);
            var endPos = new Vector3(canvasRect.x + currentRoom.roomSize.x * cellSize, canvasRect.y + y * cellSize);
            Handles.DrawLine(startPos, endPos);
        }
        
        Handles.EndGUI();
    }
    
    private void DrawPlacedFurniture(Rect canvasRect)
    {
        var cellSize = Mathf.Min(canvasRect.width / currentRoom.roomSize.x, canvasRect.height / currentRoom.roomSize.y);
        
        foreach (var placed in currentRoom.placedFurniture)
        {
            var furnitureRect = new Rect(
                canvasRect.x + placed.gridPosition.x * cellSize,
                canvasRect.y + placed.gridPosition.y * cellSize,
                placed.furnitureData.size.x * cellSize,
                placed.furnitureData.size.y * cellSize
            );
            
            // Background
            var bgColor = selectedFurniture == placed 
                ? new Color(0.2f, 0.8f, 1f, 0.3f)
                : new Color(0.8f, 0.8f, 0.8f, 0.2f);
            
            EditorGUI.DrawRect(furnitureRect, bgColor);
            
            // Sprite
            if (placed.furnitureData.sprites?.Length > 0 && placed.furnitureData.sprites[0] != null)
            {
                var sprite = placed.furnitureData.sprites[0];
                var texture = sprite.texture;
                var rect = sprite.rect;
                
                var normalizedRect = new Rect(
                    rect.x / texture.width,
                    rect.y / texture.height,
                    rect.width / texture.width,
                    rect.height / texture.height
                );
                
                GUI.DrawTextureWithTexCoords(furnitureRect, texture, normalizedRect);
            }
            
            // Name label
            var labelRect = new Rect(furnitureRect.x, furnitureRect.y - 15, furnitureRect.width, 15);
            GUI.Label(labelRect, placed.furnitureData.Name, EditorStyles.miniLabel);
            
            // Compatibility indicators
            if (showCompatibility)
            {
                DrawCompatibilityIndicators(placed, furnitureRect);
            }
        }
    }
    
    private void DrawCompatibilityIndicators(PlacedFurniture placed, Rect furnitureRect)
    {
        // Check for compatible furniture nearby
        var hasCompatible = currentRoom.placedFurniture.Any(other => 
            other != placed && 
            placed.furnitureData.compatibles != null &&
            placed.furnitureData.compatibles.Contains(other.furnitureData) &&
            IsAdjacent(placed.gridPosition, other.gridPosition, other.furnitureData.size));
        
        if (hasCompatible)
        {
            // Draw green border for compatible furniture
            var borderRect = new Rect(furnitureRect.x - 2, furnitureRect.y - 2, furnitureRect.width + 4, furnitureRect.height + 4);
            EditorGUI.DrawRect(borderRect, Color.green);
            EditorGUI.DrawRect(furnitureRect, Color.clear);
        }
    }
    
    private void DrawDragPreview(Rect canvasRect)
    {
        if (draggedFurniture == null) return;
        
        var cellSize = Mathf.Min(canvasRect.width / currentRoom.roomSize.x, canvasRect.height / currentRoom.roomSize.y);
        var mousePos = Event.current.mousePosition;
        
        // Convert mouse position to grid coordinates
        var gridX = Mathf.FloorToInt((mousePos.x - canvasRect.x) / cellSize);
        var gridY = Mathf.FloorToInt((mousePos.y - canvasRect.y) / cellSize);
        
        if (snapToGrid)
        {
            dragPosition = new Vector2Int(gridX, gridY);
        }
        
        // Check if placement is valid
        bool isValidPlacement = IsValidPlacement(draggedFurniture, dragPosition);
        
        var previewRect = new Rect(
            canvasRect.x + dragPosition.x * cellSize,
            canvasRect.y + dragPosition.y * cellSize,
            draggedFurniture.size.x * cellSize,
            draggedFurniture.size.y * cellSize
        );
        
        // Draw preview with color indicating validity
        var previewColor = isValidPlacement 
            ? new Color(0.2f, 1f, 0.2f, 0.5f) 
            : new Color(1f, 0.2f, 0.2f, 0.5f);
        
        EditorGUI.DrawRect(previewRect, previewColor);
        
        // Draw sprite preview
        if (draggedFurniture.sprites?.Length > 0 && draggedFurniture.sprites[0] != null)
        {
            var sprite = draggedFurniture.sprites[0];
            var texture = sprite.texture;
            var rect = sprite.rect;
            
            var normalizedRect = new Rect(
                rect.x / texture.width,
                rect.y / texture.height,
                rect.width / texture.width,
                rect.height / texture.height
            );
            
            var tempColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            GUI.DrawTextureWithTexCoords(previewRect, texture, normalizedRect);
            GUI.color = tempColor;
        }
        
        Repaint();
    }
    
    private void DrawSelectedFurnitureInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("🔍 Selected Furniture", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"📦 {selectedFurniture.furnitureData.Name}", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
        if (GUILayout.Button("🗑️ Remove", GUILayout.Width(80)))
        {
            RemoveFurniture(selectedFurniture);
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Label($"Position: ({selectedFurniture.gridPosition.x}, {selectedFurniture.gridPosition.y})");
        GUILayout.Label($"Size: {selectedFurniture.furnitureData.size.x}×{selectedFurniture.furnitureData.size.y}");
        GUILayout.Label($"Price: ${selectedFurniture.furnitureData.price}");
        GUILayout.Label($"Tag: {GetTagIcon(selectedFurniture.furnitureData.furnitureTag)} {selectedFurniture.furnitureData.furnitureTag}");
        
        // Wall mounting toggle
        if (selectedFurniture.furnitureData.wallObject)
        {
            selectedFurniture.isWallMounted = EditorGUILayout.Toggle("Wall Mounted", selectedFurniture.isWallMounted);
        }
        
        // Stacking info
        if (selectedFurniture.furnitureData.isStackReceiver || selectedFurniture.furnitureData.isStackable)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("📚 Stacking", EditorStyles.boldLabel);
            
            if (selectedFurniture.furnitureData.isStackReceiver)
            {
                GUILayout.Label($"Can receive stacks (Current: {selectedFurniture.stackedItems.Count})");
            }
            
            if (selectedFurniture.furnitureData.isStackable)
            {
                GUILayout.Label($"Stackable (Max: {selectedFurniture.furnitureData.maxStackLevel})");
            }
        }
        
        // Compatibility info
        if (selectedFurniture.furnitureData.compatibles?.Length > 0)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("🔗 Compatible With", EditorStyles.boldLabel);
            
            foreach (var compatible in selectedFurniture.furnitureData.compatibles)
            {
                if (compatible != null)
                {
                    var isPlaced = currentRoom.placedFurniture.Any(p => p.furnitureData == compatible);
                    var icon = isPlaced ? "✅" : "⭕";
                    GUILayout.Label($"{icon} {compatible.Name}");
                }
            }
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawAnalysisPanel()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Room Statistics
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("📊 Room Statistics", EditorStyles.boldLabel);
        
        GUILayout.Label($"Total Furniture: {currentRoom.placedFurniture.Count}");
        GUILayout.Label($"Total Cost: ${currentRoom.totalCost}");
        GUILayout.Label($"Compatibility Score: {currentRoom.compatibilityScore}");
        GUILayout.Label($"Room Coverage: {CalculateRoomCoverage():F1}%");
        
        EditorGUILayout.EndVertical();
        
        // Tag Analysis
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("🏷️ Furniture Tags", EditorStyles.boldLabel);
        
        var tagCounts = currentRoom.placedFurniture
            .GroupBy(f => f.furnitureData.furnitureTag)
            .ToDictionary(g => g.Key, g => g.Count());
        
        foreach (var tag in System.Enum.GetValues(typeof(RoomTag)).Cast<RoomTag>())
        {
            if (tagCounts.ContainsKey(tag))
            {
                var matchBonus = tag == currentRoom.roomType ? " (+Bonus)" : "";
                GUILayout.Label($"{GetTagIcon(tag)} {tag}: {tagCounts[tag]}{matchBonus}");
            }
        }
        
        EditorGUILayout.EndVertical();
        
        // Validation Issues
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("⚠️ Validation", EditorStyles.boldLabel);
        
        var issues = ValidateRoom();
        if (issues.Count == 0)
        {
            EditorGUILayout.HelpBox("✅ No issues found!", MessageType.Info);
        }
        else
        {
            foreach (var issue in issues)
            {
                EditorGUILayout.HelpBox(issue, MessageType.Warning);
            }
        }
        
        if (GUILayout.Button("🔍 Run Full Validation"))
        {
            var allIssues = ValidateRoom();
            var message = allIssues.Count == 0 
                ? "✅ Room layout is valid!" 
                : $"Found {allIssues.Count} issues:\n" + string.Join("\n", allIssues);
            
            EditorUtility.DisplayDialog("Validation Results", message, "OK");
        }
        
        EditorGUILayout.EndVertical();
        
        // Optimization Suggestions
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("💡 Suggestions", EditorStyles.boldLabel);
        
        var suggestions = GenerateSuggestions();
        foreach (var suggestion in suggestions)
        {
            EditorGUILayout.HelpBox(suggestion, MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawTemplatesPanel()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        // Save Current Layout
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("💾 Save Template", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Save Current Layout as Template"))
        {
            SaveRoomTemplate();
        }
        
        EditorGUILayout.EndVertical();
        
        // Load Templates
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("📂 Load Template", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Browse Templates"))
        {
            LoadRoomTemplate();
        }
        
        EditorGUILayout.EndVertical();
        
        // Quick Templates
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("⚡ Quick Templates", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🛏️ Basic Bedroom"))
        {
            CreateBedroomTemplate();
        }
        if (GUILayout.Button("🍳 Basic Kitchen"))
        {
            CreateKitchenTemplate();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("🛋️ Living Room"))
        {
            CreateLivingRoomTemplate();
        }
        if (GUILayout.Button("🚿 Bathroom"))
        {
            CreateBathroomTemplate();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
        
        // Export Options
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("📤 Export", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Export to CSV"))
        {
            ExportRoomToCSV();
        }
        
        if (GUILayout.Button("Export as Prefab"))
        {
            ExportRoomAsPrefab();
        }
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void HandleInput()
    {
        var e = Event.current;
        var canvasRect = GUILayoutUtility.GetLastRect();
        
        if (canvasRect.Contains(e.mousePosition))
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    HandleMouseDown(e, canvasRect);
                    break;
                    
                case EventType.MouseDrag:
                    HandleMouseDrag(e, canvasRect);
                    break;
                    
                case EventType.MouseUp:
                    HandleMouseUp(e, canvasRect);
                    break;
                    
                case EventType.KeyDown:
                    HandleKeyDown(e);
                    break;
            }
        }
    }
    
    private void HandleMouseDown(Event e, Rect canvasRect)
    {
        if (e.button == 0) // Left click
        {
            var cellSize = Mathf.Min(canvasRect.width / currentRoom.roomSize.x, canvasRect.height / currentRoom.roomSize.y);
            var gridPos = new Vector2Int(
                Mathf.FloorToInt((e.mousePosition.x - canvasRect.x) / cellSize),
                Mathf.FloorToInt((e.mousePosition.y - canvasRect.y) / cellSize)
            );
            
            // Check if clicking on existing furniture
            var clickedFurniture = GetFurnitureAtPosition(gridPos);
            if (clickedFurniture != null)
            {
                selectedFurniture = clickedFurniture;
                e.Use();
            }
            else
            {
                selectedFurniture = null;
            }
            
            Repaint();
        }
    }
    
    private void HandleMouseDrag(Event e, Rect canvasRect)
    {
        if (draggedFurniture != null)
        {
            var cellSize = Mathf.Min(canvasRect.width / currentRoom.roomSize.x, canvasRect.height / currentRoom.roomSize.y);
            dragPosition = new Vector2Int(
                Mathf.FloorToInt((e.mousePosition.x - canvasRect.x) / cellSize),
                Mathf.FloorToInt((e.mousePosition.y - canvasRect.y) / cellSize)
            );
            
            e.Use();
            Repaint();
        }
    }
    
    private void HandleMouseUp(Event e, Rect canvasRect)
    {
        if (draggedFurniture != null && e.button == 0)
        {
            if (IsValidPlacement(draggedFurniture, dragPosition))
            {
                var placed = new PlacedFurniture
                {
                    furnitureData = draggedFurniture,
                    gridPosition = dragPosition,
                    isWallMounted = draggedFurniture.wallObject && IsAgainstWall(dragPosition, draggedFurniture.size)
                };
                
                currentRoom.placedFurniture.Add(placed);
                UpdateRoomStats();
            }
            
            draggedFurniture = null;
            e.Use();
            Repaint();
        }
    }
    
    private void HandleKeyDown(Event e)
    {
        switch (e.keyCode)
        {
            case KeyCode.Delete:
                if (selectedFurniture != null)
                {
                    RemoveFurniture(selectedFurniture);
                    e.Use();
                }
                break;
                
            case KeyCode.Escape:
                selectedFurniture = null;
                draggedFurniture = null;
                e.Use();
                Repaint();
                break;
        }
    }
    
    // Core Logic Methods
    private void LoadAvailableFurniture()
    {
        availableFurniture.Clear();
        
        string[] guids = AssetDatabase.FindAssets("t:FurnitureOriginalData");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var furniture = AssetDatabase.LoadAssetAtPath<FurnitureOriginalData>(path);
            if (furniture != null)
            {
                availableFurniture.Add(furniture);
            }
        }
        
        // Sort by name
        // Sort by name
        availableFurniture = availableFurniture.OrderBy(f => f.Name).ToList();
    }
    
    private void CreateNewRoom()
    {
        currentRoom = new RoomLayout
        {
            roomName = "New Room",
            roomSize = defaultRoomSize,
            roomType = RoomTag.LivingRoom,
            placedFurniture = new List<PlacedFurniture>()
        };
        
        selectedFurniture = null;
        draggedFurniture = null;
        UpdateRoomStats();
    }
    
    private void AddFurnitureToRoom(FurnitureOriginalData furniture)
    {
        draggedFurniture = furniture;
        dragPosition = Vector2Int.zero;
    }
    
    private void RemoveFurniture(PlacedFurniture furniture)
    {
        currentRoom.placedFurniture.Remove(furniture);
        if (selectedFurniture == furniture)
        {
            selectedFurniture = null;
        }
        UpdateRoomStats();
    }
    
    private bool IsValidPlacement(FurnitureOriginalData furniture, Vector2Int position)
    {
        // Check bounds
        if (position.x < 0 || position.y < 0 || 
            position.x + furniture.size.x > currentRoom.roomSize.x ||
            position.y + furniture.size.y > currentRoom.roomSize.y)
        {
            return false;
        }
        
        // Check for overlaps
        var furnitureRect = new RectInt(position.x, position.y, furniture.size.x, furniture.size.y);
        
        foreach (var placed in currentRoom.placedFurniture)
        {
            var placedRect = new RectInt(placed.gridPosition.x, placed.gridPosition.y, 
                placed.furnitureData.size.x, placed.furnitureData.size.y);
            
            if (furnitureRect.Overlaps(placedRect))
            {
                // Check if it's stackable
                if (furniture.isStackable && placed.furnitureData.isStackReceiver)
                {
                    if (placed.stackedItems.Count < placed.furnitureData.maxStackLevel)
                    {
                        continue; // Allow stacking
                    }
                }
                return false;
            }
        }
        
        // Check base requirements
        if (furniture.requiredBase != null)
        {
            bool hasValidBase = currentRoom.placedFurniture.Any(placed => 
                placed.furnitureData == furniture.requiredBase &&
                IsOnTopOf(position, furniture.size, placed.gridPosition, placed.furnitureData.size));
            
            if (!hasValidBase)
            {
                return false;
            }
        }
        
        // Check wall requirements
        if (furniture.wallObject && !IsAgainstWall(position, furniture.size))
        {
            return false;
        }
        
        return true;
    }
    
    private bool IsAgainstWall(Vector2Int position, Vector2Int size)
    {
        // Check if furniture is against any wall
        return position.x == 0 || // Left wall
               position.y == 0 || // Bottom wall
               position.x + size.x == currentRoom.roomSize.x || // Right wall
               position.y + size.y == currentRoom.roomSize.y;   // Top wall
    }
    
    private bool IsOnTopOf(Vector2Int topPos, Vector2Int topSize, Vector2Int basePos, Vector2Int baseSize)
    {
        var topRect = new RectInt(topPos.x, topPos.y, topSize.x, topSize.y);
        var baseRect = new RectInt(basePos.x, basePos.y, baseSize.x, baseSize.y);
        
        return baseRect.Contains(topRect.min) && baseRect.Contains(topRect.max - Vector2Int.one);
    }
    
    private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2, Vector2Int size2)
    {
        var rect1 = new RectInt(pos1.x, pos1.y, 1, 1);
        var rect2 = new RectInt(pos2.x, pos2.y, size2.x, size2.y);
        
        // Expand rect1 by 1 in all directions to check adjacency
        var expandedRect1 = new RectInt(rect1.x - 1, rect1.y - 1, rect1.width + 2, rect1.height + 2);
        
        return expandedRect1.Overlaps(rect2) && !rect1.Overlaps(rect2);
    }
    
    private PlacedFurniture GetFurnitureAtPosition(Vector2Int gridPos)
    {
        return currentRoom.placedFurniture.FirstOrDefault(placed =>
        {
            var rect = new RectInt(placed.gridPosition.x, placed.gridPosition.y,
                placed.furnitureData.size.x, placed.furnitureData.size.y);
            return rect.Contains(gridPos);
        });
    }
    
    private void UpdateRoomStats()
    {
        currentRoom.totalCost = currentRoom.placedFurniture.Sum(f => f.furnitureData.price);
        currentRoom.compatibilityScore = CalculateCompatibilityScore();
    }
    
    private int CalculateCompatibilityScore()
    {
        int score = 0;
        
        foreach (var placed in currentRoom.placedFurniture)
        {
            // Room type matching bonus
            if (placed.furnitureData.furnitureTag == currentRoom.roomType)
            {
                score += placed.furnitureData.tagMatchBonusPoints;
            }
            
            // Compatibility bonuses
            if (placed.furnitureData.compatibles != null)
            {
                foreach (var compatible in placed.furnitureData.compatibles)
                {
                    if (currentRoom.placedFurniture.Any(other => 
                        other.furnitureData == compatible &&
                        IsAdjacent(placed.gridPosition, other.gridPosition, other.furnitureData.size)))
                    {
                        score += 25; // Compatibility bonus
                    }
                }
            }
            
            // Combo bonuses
            if (placed.furnitureData.hasComboSprite && placed.furnitureData.comboTriggerFurniture != null)
            {
                if (currentRoom.placedFurniture.Any(other => 
                    other.furnitureData == placed.furnitureData.comboTriggerFurniture))
                {
                    score += 50; // Combo bonus
                }
            }
        }
        
        return score;
    }
    
    private float CalculateRoomCoverage()
    {
        int totalCells = currentRoom.roomSize.x * currentRoom.roomSize.y;
        int occupiedCells = currentRoom.placedFurniture.Sum(f => f.furnitureData.size.x * f.furnitureData.size.y);
        
        return (float)occupiedCells / totalCells * 100f;
    }
    
    private List<string> ValidateRoom()
    {
        var issues = new List<string>();
        
        // Check for furniture without required bases
        foreach (var placed in currentRoom.placedFurniture)
        {
            if (placed.furnitureData.requiredBase != null)
            {
                bool hasBase = currentRoom.placedFurniture.Any(other =>
                    other.furnitureData == placed.furnitureData.requiredBase &&
                    IsOnTopOf(placed.gridPosition, placed.furnitureData.size, 
                             other.gridPosition, other.furnitureData.size));
                
                if (!hasBase)
                {
                    issues.Add($"{placed.furnitureData.Name} requires {placed.furnitureData.requiredBase.Name} as base");
                }
            }
            
            // Check wall objects
            if (placed.furnitureData.wallObject && !placed.isWallMounted)
            {
                if (!IsAgainstWall(placed.gridPosition, placed.furnitureData.size))
                {
                    issues.Add($"{placed.furnitureData.Name} should be against a wall");
                }
            }
        }
        
        // Check for isolated furniture (no compatible neighbors)
        foreach (var placed in currentRoom.placedFurniture)
        {
            if (placed.furnitureData.compatibles?.Length > 0)
            {
                bool hasCompatibleNeighbor = placed.furnitureData.compatibles.Any(compatible =>
                    currentRoom.placedFurniture.Any(other =>
                        other.furnitureData == compatible &&
                        IsAdjacent(placed.gridPosition, other.gridPosition, other.furnitureData.size)));
                
                if (!hasCompatibleNeighbor)
                {
                    issues.Add($"{placed.furnitureData.Name} has no compatible neighbors nearby");
                }
            }
        }
        
        return issues;
    }
    
    private List<string> GenerateSuggestions()
    {
        var suggestions = new List<string>();
        
        // Room type suggestions
        var roomTypeFurniture = currentRoom.placedFurniture.Count(f => f.furnitureData.furnitureTag == currentRoom.roomType);
        var totalFurniture = currentRoom.placedFurniture.Count;
        
        if (totalFurniture > 0 && (float)roomTypeFurniture / totalFurniture < 0.5f)
        {
            suggestions.Add($"Consider adding more {currentRoom.roomType} furniture for better room theme matching");
        }
        
        // Coverage suggestions
        var coverage = CalculateRoomCoverage();
        if (coverage < 20f)
        {
            suggestions.Add("Room seems empty. Consider adding more furniture");
        }
        else if (coverage > 80f)
        {
            suggestions.Add("Room might be overcrowded. Consider removing some furniture");
        }
        
        // Compatibility suggestions
        var uncompatibleFurniture = currentRoom.placedFurniture.Where(placed =>
            placed.furnitureData.compatibles?.Length > 0 &&
            !placed.furnitureData.compatibles.Any(compatible =>
                currentRoom.placedFurniture.Any(other =>
                    other.furnitureData == compatible &&
                    IsAdjacent(placed.gridPosition, other.gridPosition, other.furnitureData.size)))).ToList();
        
        if (uncompatibleFurniture.Count > 0)
        {
            suggestions.Add($"Consider repositioning {uncompatibleFurniture.First().furnitureData.Name} near compatible furniture");
        }
        
        // Missing essential furniture suggestions
        if (currentRoom.roomType == RoomTag.Kitchen && !currentRoom.placedFurniture.Any(f => f.furnitureData.Name.Contains("Fridge")))
        {
            suggestions.Add("Kitchen could use a refrigerator");
        }
        
        if (currentRoom.roomType == RoomTag.Bedroom && !currentRoom.placedFurniture.Any(f => f.furnitureData.Name.Contains("Bed")))
        {
            suggestions.Add("Bedroom needs a bed");
        }
        
        return suggestions;
    }
    
    // Template Methods
    private void SaveRoomTemplate()
    {
        string path = EditorUtility.SaveFilePanel("Save Room Template", "Assets/RoomTemplates", currentRoom.roomName, "json");
        if (!string.IsNullOrEmpty(path))
        {
            string json = JsonUtility.ToJson(currentRoom, true);
            File.WriteAllText(path, json);
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Template Saved", $"Room template saved to {path}", "OK");
        }
    }
    
    private void LoadRoomTemplate()
    {
        string path = EditorUtility.OpenFilePanel("Load Room Template", "Assets/RoomTemplates", "json");
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                var loadedRoom = JsonUtility.FromJson<RoomLayout>(json);
                
                // Validate that all furniture assets still exist
                bool allFurnitureValid = true;
                foreach (var placed in loadedRoom.placedFurniture)
                {
                    if (placed.furnitureData == null)
                    {
                        allFurnitureValid = false;
                        break;
                    }
                }
                
                if (allFurnitureValid)
                {
                    currentRoom = loadedRoom;
                    selectedFurniture = null;
                    UpdateRoomStats();
                    EditorUtility.DisplayDialog("Template Loaded", "Room template loaded successfully", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Load Error", "Some furniture in the template no longer exists", "OK");
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("Load Error", $"Failed to load template: {e.Message}", "OK");
            }
        }
    }
    
    private void CreateBedroomTemplate()
    {
        CreateNewRoom();
        currentRoom.roomName = "Basic Bedroom";
        currentRoom.roomType = RoomTag.Bedroom;
        currentRoom.roomSize = new Vector2Int(6, 6);
        
        // Add basic bedroom furniture
        var bed = availableFurniture.FirstOrDefault(f => f.Name.Contains("Bed"));
        if (bed != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = bed,
                gridPosition = new Vector2Int(1, 1)
            });
        }
        
        UpdateRoomStats();
    }
    
    private void CreateKitchenTemplate()
    {
        CreateNewRoom();
        currentRoom.roomName = "Basic Kitchen";
        currentRoom.roomType = RoomTag.Kitchen;
        currentRoom.roomSize = new Vector2Int(8, 6);
        
        // Add basic kitchen furniture
        var fridge = availableFurniture.FirstOrDefault(f => f.Name.Contains("Fridge"));
        var counter = availableFurniture.FirstOrDefault(f => f.Name.Contains("Counter"));
        var oven = availableFurniture.FirstOrDefault(f => f.Name.Contains("Oven"));
        
        if (fridge != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = fridge,
                gridPosition = new Vector2Int(0, 0),
                isWallMounted = true
            });
        }
        
        if (counter != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = counter,
                gridPosition = new Vector2Int(2, 0)
            });
        }
        
        if (oven != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = oven,
                gridPosition = new Vector2Int(4, 0)
            });
        }
        
        UpdateRoomStats();
    }
    
    private void CreateLivingRoomTemplate()
    {
        CreateNewRoom();
        currentRoom.roomName = "Basic Living Room";
        currentRoom.roomType = RoomTag.LivingRoom;
        currentRoom.roomSize = new Vector2Int(8, 8);
        
        // Add basic living room furniture
        var couch = availableFurniture.FirstOrDefault(f => f.Name.Contains("Couch"));
        var tv = availableFurniture.FirstOrDefault(f => f.Name.Contains("Tv"));
        var tvTable = availableFurniture.FirstOrDefault(f => f.Name.Contains("TvTable"));
        var plant = availableFurniture.FirstOrDefault(f => f.Name.Contains("Plant"));
        
        if (couch != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = couch,
                gridPosition = new Vector2Int(3, 2)
            });
        }
        
        if (tvTable != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = tvTable,
                gridPosition = new Vector2Int(1, 0),
                isWallMounted = true
            });
        }
        
        if (tv != null && tvTable != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = tv,
                gridPosition = new Vector2Int(1, 0)
            });
        }
        
        if (plant != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = plant,
                gridPosition = new Vector2Int(6, 6)
            });
        }
        
        UpdateRoomStats();
    }
    
    private void CreateBathroomTemplate()
    {
        CreateNewRoom();
        currentRoom.roomName = "Basic Bathroom";
        currentRoom.roomType = RoomTag.Bathroom;
        currentRoom.roomSize = new Vector2Int(4, 4);
        
        // Add basic bathroom furniture
        var toilet = availableFurniture.FirstOrDefault(f => f.Name.Contains("Toilette"));
        var bathtub = availableFurniture.FirstOrDefault(f => f.Name.Contains("BathTub"));
        var washingMachine = availableFurniture.FirstOrDefault(f => f.Name.Contains("WashingMachine"));
        
        if (toilet != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = toilet,
                gridPosition = new Vector2Int(0, 0),
                isWallMounted = true
            });
        }
        
        if (bathtub != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = bathtub,
                gridPosition = new Vector2Int(0, 2),
                isWallMounted = true
            });
        }
        
        if (washingMachine != null)
        {
            currentRoom.placedFurniture.Add(new PlacedFurniture
            {
                furnitureData = washingMachine,
                gridPosition = new Vector2Int(2, 0)
            });
        }
        
        UpdateRoomStats();
    }
    
    private void ExportRoomToCSV()
    {
        string path = EditorUtility.SaveFilePanel("Export Room to CSV", "Assets", currentRoom.roomName, "csv");
        if (!string.IsNullOrEmpty(path))
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("FurnitureName,PositionX,PositionY,SizeX,SizeY,Price,Tag,IsWallMounted,StackLevel");
            
            foreach (var placed in currentRoom.placedFurniture)
            {
                csv.AppendLine($"{placed.furnitureData.Name},{placed.gridPosition.x},{placed.gridPosition.y}," +
                             $"{placed.furnitureData.size.x},{placed.furnitureData.size.y}," +
                             $"{placed.furnitureData.price},{placed.furnitureData.furnitureTag}," +
                             $"{placed.isWallMounted},{placed.stackLevel}");
            }
            
            File.WriteAllText(path, csv.ToString());
            EditorUtility.DisplayDialog("Export Complete", $"Room exported to {path}", "OK");
        }
    }
    
    private void ExportRoomAsPrefab()
    {
        string path = EditorUtility.SaveFilePanel("Export Room as Prefab", "Assets/Prefabs/Rooms", currentRoom.roomName, "prefab");
        if (!string.IsNullOrEmpty(path))
        {
            // Create a temporary GameObject to hold the room
            GameObject roomObject = new GameObject(currentRoom.roomName);
            
            // Add room info component
            var roomInfo = roomObject.AddComponent<RoomInfo>();
            roomInfo.roomName = currentRoom.roomName;
            roomInfo.roomType = currentRoom.roomType;
            roomInfo.roomSize = currentRoom.roomSize;
            roomInfo.totalCost = currentRoom.totalCost;
            roomInfo.compatibilityScore = currentRoom.compatibilityScore;
            
            // Create furniture instances
            foreach (var placed in currentRoom.placedFurniture)
            {
                if (placed.furnitureData.prefab != null)
                {
                    GameObject furnitureInstance = PrefabUtility.InstantiatePrefab(placed.furnitureData.prefab) as GameObject;
                    furnitureInstance.transform.SetParent(roomObject.transform);
                    furnitureInstance.transform.localPosition = new Vector3(placed.gridPosition.x, 0, placed.gridPosition.y);
                    
                    // Add furniture info component
                    var furnitureInfo = furnitureInstance.AddComponent<FurnitureInfo>();
                    furnitureInfo.originalData = placed.furnitureData;
                    furnitureInfo.gridPosition = placed.gridPosition;
                    furnitureInfo.isWallMounted = placed.isWallMounted;
                    furnitureInfo.stackLevel = placed.stackLevel;
                }
            }
            
            // Save as prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(roomObject, path);
            DestroyImmediate(roomObject);
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Export Complete", $"Room prefab created at {path}", "OK");
            
            // Focus on the created prefab
            EditorGUIUtility.PingObject(prefab);
        }
    }
    
    private void RandomFillRoom()
    {
        if (EditorUtility.DisplayDialog("Random Fill", "This will clear the current room and fill it randomly. Continue?", "Yes", "Cancel"))
        {
            currentRoom.placedFurniture.Clear();
            
            // Filter furniture by room type
            var suitableFurniture = availableFurniture.Where(f => 
                f.furnitureTag == currentRoom.roomType || f.furnitureTag == RoomTag.None).ToList();
            
            if (suitableFurniture.Count == 0)
                suitableFurniture = availableFurniture;
            
            // Try to place random furniture
            int attempts = 0;
            int maxAttempts = 100;
            
            while (attempts < maxAttempts && currentRoom.placedFurniture.Count < 10)
            {
                var randomFurniture = suitableFurniture[UnityEngine.Random.Range(0, suitableFurniture.Count)];
                var randomPosition = new Vector2Int(
                    UnityEngine.Random.Range(0, currentRoom.roomSize.x - randomFurniture.size.x + 1),
                    UnityEngine.Random.Range(0, currentRoom.roomSize.y - randomFurniture.size.y + 1)
                );
                
                if (IsValidPlacement(randomFurniture, randomPosition))
                {
                    currentRoom.placedFurniture.Add(new PlacedFurniture
                    {
                        furnitureData = randomFurniture,
                        gridPosition = randomPosition,
                        isWallMounted = randomFurniture.wallObject && IsAgainstWall(randomPosition, randomFurniture.size)
                    });
                }
                
                attempts++;
            }
            
            UpdateRoomStats();
        }
    }
    
    // Utility Methods
    private string GetTagIcon(RoomTag tag)
    {
        return tag switch
        {
            RoomTag.Kitchen => "🍳",
            RoomTag.LivingRoom => "🛋️",
            RoomTag.Bedroom => "🛏️",
            RoomTag.Bathroom => "🚿",
            RoomTag.Office => "💼",
            RoomTag.Lab => "🔬",
            RoomTag.Gym => "💪",
            RoomTag.DiningRoom => "🍽️",
            _ => "📦"
        };
    }
    
    private void DrawVerticalSeparator()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(2));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
        
        Rect rect = GUILayoutUtility.GetLastRect();
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }
}

// Supporting Components for Prefab Export
public class RoomInfo : MonoBehaviour
{
    [Header("Room Data")]
    public string roomName;
    public RoomTag roomType;
    public Vector2Int roomSize;
    public int totalCost;
    public int compatibilityScore;
    
    [Header("Runtime Data")]
    public List<FurnitureInfo> furnitureList = new List<FurnitureInfo>();
    
    private void Awake()
    {
        // Collect all furniture info components
        furnitureList.Clear();
        furnitureList.AddRange(GetComponentsInChildren<FurnitureInfo>());
    }
    
    public void RecalculateStats()
    {
        totalCost = furnitureList.Sum(f => f.originalData.price);
        // Add compatibility score calculation here if needed
    }
}

public class FurnitureInfo : MonoBehaviour
{
    [Header("Furniture Data")]
    public FurnitureOriginalData originalData;
    public Vector2Int gridPosition;
    public bool isWallMounted;
    public int stackLevel = 1;
    
    [Header("Runtime State")]
    public bool isSelected;
    public bool isHighlighted;
    
    private void Start()
    {
        // Set up any runtime behavior
        if (originalData != null && originalData.sprites?.Length > 0)
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = originalData.sprites[0];
            }
        }
    }
    
    public void OnFurnitureClicked()
    {
        // Handle furniture interaction in game
        Debug.Log($"Clicked on {originalData.Name}");
    }
    
    public void SetHighlight(bool highlight)
    {
        isHighlighted = highlight;
        
        // Visual feedback
        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlight ? Color.yellow : Color.white;
        }
    }
}

#endif