using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class FurnitureMaker : EditorWindow
{
    private string furnitureName = "";
    private string spanishName = "";
    private int price = 0;
    private Vector2Int size = new Vector2Int(1, 1);
    private TypeOfSize typeOfSize = TypeOfSize.one_one;
    private List<FurnitureOriginalData> compatibles = new List<FurnitureOriginalData>();
    private List<Sprite> sprites = new List<Sprite>();
    private bool hasComboSprite = false;
    private FurnitureOriginalData comboTriggerFurniture;
    private string savePath = "Assets/Resources/Furniture";
    private Vector2 scrollPosition;
    private bool showCompatibles = false;
    private bool showSprites = false;
    private bool isTopObject = false;

    // Template references
    private GameObject top_template_1x1;
    private GameObject top_template_2x1;
    private GameObject top_template_2x2;
    private GameObject bottom_template_1x1;
    private GameObject bottom_template_2x1;
    private GameObject bottom_template_2x2;

    
    // Template paths - update these to match your actual template locations
    private const string TOP_TEMPLATE_PATH_1x1 = "Assets/Prefabs/Furniture/Templates/Top_Template_1x1.prefab";
    private const string TOP_TEMPLATE_PATH_2x1 = "Assets/Prefabs/Furniture/Templates/Top_Template_2x1.prefab";
    private const string TOP_TEMPLATE_PATH_2x2 = "Assets/Prefabs/Furniture/Templates/Top_Template_2x2.prefab";
    private const string BOTTOM_TEMPLATE_PATH_1x1 = "Assets/Prefabs/Furniture/Templates/Bottom_Template_1x1.prefab";
    private const string BOTTOM_TEMPLATE_PATH_2x1 = "Assets/Prefabs/Furniture/Templates/Bottom_Template_2x1.prefab";
    private const string BOTTOM_TEMPLATE_PATH_2x2 = "Assets/Prefabs/Furniture/Templates/Bottom_Template_2x2.prefab";

    [MenuItem("Tools/Furniture Maker")]
    public static void ShowWindow()
    {
        GetWindow<FurnitureMaker>("Furniture Maker");
    }

    private void OnEnable()
    {
        // Load template prefabs
        top_template_1x1 = AssetDatabase.LoadAssetAtPath<GameObject>(TOP_TEMPLATE_PATH_1x1);
        top_template_2x1 = AssetDatabase.LoadAssetAtPath<GameObject>(TOP_TEMPLATE_PATH_2x1);
        top_template_2x2 = AssetDatabase.LoadAssetAtPath<GameObject>(TOP_TEMPLATE_PATH_2x2);
        bottom_template_1x1 = AssetDatabase.LoadAssetAtPath<GameObject>(BOTTOM_TEMPLATE_PATH_1x1);
        bottom_template_2x1 = AssetDatabase.LoadAssetAtPath<GameObject>(BOTTOM_TEMPLATE_PATH_2x1);
        bottom_template_2x2 = AssetDatabase.LoadAssetAtPath<GameObject>(BOTTOM_TEMPLATE_PATH_2x2);
    }

    private void OnGUI()
    {
        GUILayout.Label("Furniture Creator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Basic Properties
        furnitureName = EditorGUILayout.TextField("Name (English)", furnitureName);
        spanishName = EditorGUILayout.TextField("Name (Spanish)", spanishName);
        price = EditorGUILayout.IntField("Price", price);
        
        // Size and Type Selection
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Size");
        if (GUILayout.Button("1x1", EditorStyles.miniButtonLeft))
        {
            size = new Vector2Int(1, 1);
            typeOfSize = TypeOfSize.one_one;
        }
        if (GUILayout.Button("1x2", EditorStyles.miniButtonMid))
        {
            size = new Vector2Int(2, 1);
            typeOfSize = TypeOfSize.two_one;
        }
        if (GUILayout.Button("2x2", EditorStyles.miniButtonMid))
        {
            size = new Vector2Int(2, 2);
            typeOfSize = TypeOfSize.two_two;
        }
        EditorGUILayout.EndHorizontal();
        
        // Display current size and type
        EditorGUILayout.LabelField("Current Size", $"{size.x}x{size.y} ({typeOfSize})");
        
        // Top/Bottom Object Selection
        isTopObject = EditorGUILayout.Toggle("Is Top Object", isTopObject);
        
        // Sprites
        EditorGUILayout.Space();
        showSprites = EditorGUILayout.Foldout(showSprites, "Sprites");
        if (showSprites)
        {
            EditorGUI.indentLevel++;
            
            int spriteCount = sprites.Count;
            int newSpriteCount = Mathf.Max(0, EditorGUILayout.IntField("Sprite Count", spriteCount));
            
            if (newSpriteCount != spriteCount)
            {
                while (sprites.Count > newSpriteCount)
                    sprites.RemoveAt(sprites.Count - 1);
                
                while (sprites.Count < newSpriteCount)
                    sprites.Add(null);
            }
            
            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite {i + 1}", sprites[i], typeof(Sprite), false);
            }
            
            EditorGUI.indentLevel--;
        }

        // Combo Sprite
        EditorGUILayout.Space();
        hasComboSprite = EditorGUILayout.Toggle("Has Combo Sprite", hasComboSprite);
        
        if (hasComboSprite)
        {
            EditorGUI.indentLevel++;
            comboTriggerFurniture = (FurnitureOriginalData)EditorGUILayout.ObjectField("Combo Trigger Furniture", comboTriggerFurniture, typeof(FurnitureOriginalData), false);
            EditorGUI.indentLevel--;
        }

        // Compatible Furniture
        EditorGUILayout.Space();
        showCompatibles = EditorGUILayout.Foldout(showCompatibles, "Compatible Furniture");
        if (showCompatibles)
        {
            EditorGUI.indentLevel++;
            
            int compatibleCount = compatibles.Count;
            int newCompatibleCount = Mathf.Max(0, EditorGUILayout.IntField("Compatible Count", compatibleCount));
            
            if (newCompatibleCount != compatibleCount)
            {
                while (compatibles.Count > newCompatibleCount)
                    compatibles.RemoveAt(compatibles.Count - 1);
                
                while (compatibles.Count < newCompatibleCount)
                    compatibles.Add(null);
            }
            
            for (int i = 0; i < compatibles.Count; i++)
            {
                compatibles[i] = (FurnitureOriginalData)EditorGUILayout.ObjectField($"Compatible {i + 1}", compatibles[i], typeof(FurnitureOriginalData), false);
            }
            
            EditorGUI.indentLevel--;
        }

        // Save Path
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Save Path");
        savePath = EditorGUILayout.TextField(savePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.SaveFolderPanel("Choose Save Location", savePath, "");
            if (!string.IsNullOrEmpty(path))
            {
                // Convert to project relative path
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                savePath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        // Create Button
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.enabled = !string.IsNullOrEmpty(furnitureName) && sprites.Count > 0;
        if (GUILayout.Button("Create Furniture", GUILayout.Width(150), GUILayout.Height(30)))
        {
            CreateFurniture();
        }
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        
        // Reset Button
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset Form", GUILayout.Width(150)))
        {
            ResetForm();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private GameObject GetTemplateForSize()
    {
        // Select the appropriate template based on size and type
        if (size.x == 1 && size.y == 1)
        {
            if (isTopObject) return top_template_1x1;
            else return bottom_template_1x1;
        }
        else if (size.x == 1 && size.y == 2)
            if (isTopObject) return top_template_2x1;
            else return bottom_template_2x1;
        else if (size.x == 2 && size.y == 2)
            if (isTopObject) return top_template_2x2;
            else return bottom_template_2x2;
        
        // Default to 1x1 if no match
        return bottom_template_1x1;
    }

    private void CreateFurniture()
    {
        // Validate input
        if (string.IsNullOrEmpty(furnitureName))
        {
            EditorUtility.DisplayDialog("Error", "Furniture name cannot be empty.", "OK");
            return;
        }

        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "At least one sprite is required.", "OK");
            return;
        }

        // Create directory if it doesn't exist
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // Create the furniture scriptable object
        FurnitureOriginalData furniture = ScriptableObject.CreateInstance<FurnitureOriginalData>();
        furniture.Name = furnitureName;
        furniture.es_Name = spanishName;
        furniture.price = price;
        furniture.size = size;
        furniture.typeOfSize = typeOfSize;
        furniture.hasComboSprite = hasComboSprite;
        furniture.comboTriggerFurniture = comboTriggerFurniture;
        
        // Set sprites
        furniture.sprites = sprites.ToArray();
        
        // Set compatibles
        furniture.compatibles = compatibles.ToArray();

        // Get the appropriate template based on size
        GameObject templatePrefab = GetTemplateForSize();
        
        if (templatePrefab != null)
        {
            string prefabPath = $"{savePath}/{furnitureName}_Prefab.prefab";
            GameObject prefabInstance = Instantiate(templatePrefab);
            prefabInstance.name = $"{furnitureName}_Prefab";
            
            // Set up the prefab with the correct properties
            // You might want to add a component or tag to indicate if it's a top object
            if (isTopObject)
            {
                // Add a tag or component to indicate it's a top object
                // For example:
                // prefabInstance.tag = "TopObject";
                // Or add a custom component:
                // prefabInstance.AddComponent<TopObjectMarker>();
            }
            
            // Set the sprite in the prefab if it has a SpriteRenderer
            SpriteRenderer spriteRenderer = prefabInstance.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && sprites.Count > 0)
            {
                spriteRenderer.sprite = sprites[0];
            }
            
            // Save the prefab
            GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            DestroyImmediate(prefabInstance);
            
            // Assign the prefab to the furniture
            furniture.prefab = createdPrefab;
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "No template found for the selected size. Creating furniture without a prefab.", "OK");
        }

        // Save the furniture scriptable object
        string assetPath = $"{savePath}/{furnitureName}.asset";
        AssetDatabase.CreateAsset(furniture, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = furniture;
        
        EditorUtility.DisplayDialog("Success", $"Furniture '{furnitureName}' created successfully!", "OK");
    }

    private void ResetForm()
    {
        furnitureName = "";
        spanishName = "";
        price = 0;
        size = new Vector2Int(1, 1);
        typeOfSize = TypeOfSize.one_one;
        compatibles.Clear();
        sprites.Clear();
        hasComboSprite = false;
        comboTriggerFurniture = null;
        isTopObject = false;
    }
}
