using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
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
    private bool isAnimated = false;
    private float animationSpeed = 0.5f;
    private RoomTag furnitureTag = RoomTag.None;
    private int tagMatchBonusPoints = 50;

    // Template references
    private GameObject top_template_1x1;
    private GameObject top_template_2x1;
    private GameObject top_template_2x2;
    private GameObject top_template_3x1;
    private GameObject bottom_template_1x1;
    private GameObject bottom_template_2x1;
    private GameObject bottom_template_2x2;
    private GameObject bottom_template_3x1;

    // Template paths
    private const string TOP_TEMPLATE_PATH_1x1 = "Assets/Prefabs/Furniture/Templates/Top_Template_1x1.prefab";
    private const string TOP_TEMPLATE_PATH_2x1 = "Assets/Prefabs/Furniture/Templates/Top_Template_2x1.prefab";
    private const string TOP_TEMPLATE_PATH_2x2 = "Assets/Prefabs/Furniture/Templates/Top_Template_2x2.prefab";
    private const string TOP_TEMPLATE_PATH_3x1 = "Assets/Prefabs/Furniture/Templates/Top_Template_3x1.prefab";
    private const string BOTTOM_TEMPLATE_PATH_1x1 = "Assets/Prefabs/Furniture/Templates/Bottom_Template_1x1.prefab";
    private const string BOTTOM_TEMPLATE_PATH_2x1 = "Assets/Prefabs/Furniture/Templates/Bottom_Template_2x1.prefab";
    private const string BOTTOM_TEMPLATE_PATH_2x2 = "Assets/Prefabs/Furniture/Templates/Bottom_Template_2x2.prefab";
    private const string BOTTOM_TEMPLATE_PATH_3x1 = "Assets/Prefabs/Furniture/Templates/Bottom_Template_3x1.prefab";

    [MenuItem("DevTools/Furniture Maker")]
    public static void ShowWindow()
    {
        GetWindow<FurnitureMaker>("Furniture Maker");
    }

    private void OnEnable()
    {
        top_template_1x1 = AssetDatabase.LoadAssetAtPath<GameObject>(TOP_TEMPLATE_PATH_1x1);
        top_template_2x1 = AssetDatabase.LoadAssetAtPath<GameObject>(TOP_TEMPLATE_PATH_2x1);
        top_template_2x2 = AssetDatabase.LoadAssetAtPath<GameObject>(TOP_TEMPLATE_PATH_2x2);
        top_template_3x1 = AssetDatabase.LoadAssetAtPath<GameObject>(TOP_TEMPLATE_PATH_3x1);
        bottom_template_1x1 = AssetDatabase.LoadAssetAtPath<GameObject>(BOTTOM_TEMPLATE_PATH_1x1);
        bottom_template_2x1 = AssetDatabase.LoadAssetAtPath<GameObject>(BOTTOM_TEMPLATE_PATH_2x1);
        bottom_template_2x2 = AssetDatabase.LoadAssetAtPath<GameObject>(BOTTOM_TEMPLATE_PATH_2x2);
        bottom_template_3x1 = AssetDatabase.LoadAssetAtPath<GameObject>(BOTTOM_TEMPLATE_PATH_3x1);
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
        GUILayout.Label("Furniture Creator", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(20);
        DrawSeparator();

        // Basic Information Section
        DrawBasicInfoSection();

        EditorGUILayout.Space(15);
        DrawSeparator();

        // Size and Type Section
        DrawSizeSection();

        EditorGUILayout.Space(15);
        DrawSeparator();

        // Properties Section
        DrawPropertiesSection();

        EditorGUILayout.Space(15);
        DrawSeparator();

        // Animation Section
        DrawAnimationSection();

        EditorGUILayout.Space(15);
        DrawSeparator();

        // Tagging Section
        DrawTaggingSection();

        EditorGUILayout.Space(15);
        DrawSeparator();

        // Sprites Section
        DrawSpritesSection();

        EditorGUILayout.Space(15);
        DrawSeparator();

        // Combo Section
        DrawComboSection();

        EditorGUILayout.Space(15);
        DrawSeparator();

        // Compatibles Section
        DrawCompatiblesSection();

        EditorGUILayout.Space(15);
        DrawSeparator();

        // Save Path Section
        DrawSavePathSection();

        EditorGUILayout.Space(20);
        DrawSeparator();

        // Action Buttons
        DrawActionButtons();

        EditorGUILayout.Space(10);
        EditorGUILayout.EndScrollView();
    }

    private void DrawBasicInfoSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);

        GUILayout.Label("Basic Information", sectionStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);

        GUILayout.Label("Name (English):", EditorStyles.miniLabel);
        furnitureName = EditorGUILayout.TextField(furnitureName);

        EditorGUILayout.Space(3);

        GUILayout.Label("Name (Spanish):", EditorStyles.miniLabel);
        spanishName = EditorGUILayout.TextField(spanishName);

        EditorGUILayout.Space(3);

        GUILayout.Label("Price:", EditorStyles.miniLabel);
        price = EditorGUILayout.IntField(price);

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawSizeSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.6f, 0.2f);

        GUILayout.Label("Size", sectionStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);

        GUILayout.Label("Select Size:", EditorStyles.miniLabel);
        EditorGUILayout.Space(3);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = (size.x == 1 && size.y == 1) ? new Color(0.7f, 0.9f, 0.7f) : Color.white;
        if (GUILayout.Button("1x1", EditorStyles.miniButtonLeft))
        {
            size = new Vector2Int(1, 1);
            typeOfSize = TypeOfSize.one_one;
        }

        GUI.backgroundColor = (size.x == 2 && size.y == 1) ? new Color(0.7f, 0.9f, 0.7f) : Color.white;
        if (GUILayout.Button("2x1", EditorStyles.miniButtonMid))
        {
            size = new Vector2Int(2, 1);
            typeOfSize = TypeOfSize.two_one;
        }

        GUI.backgroundColor = (size.x == 2 && size.y == 2) ? new Color(0.7f, 0.9f, 0.7f) : Color.white;
        if (GUILayout.Button("2x2", EditorStyles.miniButtonMid))
        {
            size = new Vector2Int(2, 2);
            typeOfSize = TypeOfSize.two_two;
        }

        GUI.backgroundColor = (size.x == 3 && size.y == 1) ? new Color(0.7f, 0.9f, 0.7f) : Color.white;
        if (GUILayout.Button("3x1", EditorStyles.miniButtonRight))
        {
            size = new Vector2Int(3, 1);
            typeOfSize = TypeOfSize.three_one;
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        GUIStyle currentSizeStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
        currentSizeStyle.normal.textColor = new Color(0.4f, 0.4f, 0.4f);
        GUILayout.Label($"Current: {size.x}x{size.y} ({typeOfSize})", currentSizeStyle);

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawPropertiesSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.6f, 0.2f, 0.9f);

        GUILayout.Label("Properties", sectionStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Is Top Object:", GUILayout.Width(120));
        isTopObject = EditorGUILayout.Toggle(isTopObject);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawAnimationSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.2f, 0.6f);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Is Animated:", GUILayout.Width(120));
        isAnimated = EditorGUILayout.Toggle(isAnimated);
        EditorGUILayout.EndHorizontal();

        if (isAnimated)
        {
            EditorGUILayout.Space(5);
            EditorGUI.indentLevel++;

            GUILayout.Label("Animation Speed:", EditorStyles.miniLabel);
            animationSpeed = EditorGUILayout.Slider(animationSpeed, 0.1f, 2.0f);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawTaggingSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.9f, 0.6f);

        GUILayout.Label("Tagging System", sectionStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);

        GUILayout.Label("Room Tag:", EditorStyles.miniLabel);
        furnitureTag = (RoomTag)EditorGUILayout.EnumPopup(furnitureTag);

        EditorGUILayout.Space(3);

        GUILayout.Label("Tag Match Bonus Points:", EditorStyles.miniLabel);
        tagMatchBonusPoints = EditorGUILayout.IntField(tagMatchBonusPoints);

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawSpritesSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.6f, 0.2f);

        EditorGUILayout.BeginHorizontal();
        showSprites = EditorGUILayout.Foldout(showSprites, "Sprites", true);
        if (sprites.Count > 0)
        {
            GUIStyle countStyle = new GUIStyle(EditorStyles.miniLabel);
            countStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"({sprites.Count} sprites)", countStyle);
        }
        EditorGUILayout.EndHorizontal();

        if (showSprites)
        {
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Sprite Count:", EditorStyles.miniLabel, GUILayout.Width(80));

            int spriteCount = sprites.Count;
            int newSpriteCount = Mathf.Max(0, EditorGUILayout.IntField(spriteCount));

            if (GUILayout.Button("Clear All", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                sprites.Clear();
            }
            EditorGUILayout.EndHorizontal();

            if (newSpriteCount != spriteCount)
            {
                while (sprites.Count > newSpriteCount)
                    sprites.RemoveAt(sprites.Count - 1);

                while (sprites.Count < newSpriteCount)
                    sprites.Add(null);
            }

            EditorGUILayout.Space(5);

            for (int i = 0; i < sprites.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"Sprite {i + 1}:", GUILayout.Width(60));
                sprites[i] = (Sprite)EditorGUILayout.ObjectField(sprites[i], typeof(Sprite), false);

                GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    sprites.RemoveAt(i);
                    break;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }

            if (isAnimated && sprites.Count < 2)
            {
                EditorGUILayout.HelpBox("⚠️ Animated furniture requires at least 2 sprites!", MessageType.Warning);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawComboSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.6f, 0.9f, 0.2f);

        GUILayout.Label("Combo System", sectionStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Has Combo Sprite:", GUILayout.Width(120));
        hasComboSprite = EditorGUILayout.Toggle(hasComboSprite);
        EditorGUILayout.EndHorizontal();

        if (hasComboSprite)
        {
            EditorGUILayout.Space(5);
            EditorGUI.indentLevel++;

            GUILayout.Label("Combo Trigger Furniture:", EditorStyles.miniLabel);
            comboTriggerFurniture = (FurnitureOriginalData)EditorGUILayout.ObjectField(
                comboTriggerFurniture, typeof(FurnitureOriginalData), false);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawCompatiblesSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);

        EditorGUILayout.BeginHorizontal();
        showCompatibles = EditorGUILayout.Foldout(showCompatibles, "Compatible Furniture", true);
        if (compatibles.Count > 0)
        {
            GUIStyle countStyle = new GUIStyle(EditorStyles.miniLabel);
            countStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            GUILayout.FlexibleSpace();
            GUILayout.Label($"({compatibles.Count} items)", countStyle);
        }
        EditorGUILayout.EndHorizontal();

        if (showCompatibles)
        {
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Compatible Count:", EditorStyles.miniLabel, GUILayout.Width(100));

            int compatibleCount = compatibles.Count;
            int newCompatibleCount = Mathf.Max(0, EditorGUILayout.IntField(compatibleCount));

            if (GUILayout.Button("Clear All", EditorStyles.miniButton, GUILayout.Width(60)))
            {
                compatibles.Clear();
            }
            EditorGUILayout.EndHorizontal();

            if (newCompatibleCount != compatibleCount)
            {
                while (compatibles.Count > newCompatibleCount)
                    compatibles.RemoveAt(compatibles.Count - 1);

                while (compatibles.Count < newCompatibleCount)
                    compatibles.Add(null);
            }

            EditorGUILayout.Space(5);

            for (int i = 0; i < compatibles.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"Item {i + 1}:", GUILayout.Width(60));
                compatibles[i] = (FurnitureOriginalData)EditorGUILayout.ObjectField(
                    compatibles[i], typeof(FurnitureOriginalData), false);

                GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                if (GUILayout.Button("×", EditorStyles.miniButton, GUILayout.Width(20)))
                {
                    compatibles.RemoveAt(i);
                    break;
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(2);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawSavePathSection()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.2f, 0.6f);

        GUILayout.Label("Save Location", sectionStyle);
        EditorGUILayout.Space(5);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);

        GUILayout.Label("Save Path:", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        savePath = EditorGUILayout.TextField(savePath);
        if (GUILayout.Button("📁", EditorStyles.miniButton, GUILayout.Width(30)))
        {
            string path = EditorUtility.SaveFolderPanel("Choose Save Location", savePath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    path = "Assets" + path.Substring(Application.dataPath.Length);
                }
                savePath = path;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }

    private void DrawActionButtons()
    {
        EditorGUILayout.Space(10);

        bool canCreate = !string.IsNullOrEmpty(furnitureName) && sprites.Count > 0;
        if (isAnimated && sprites.Count < 2)
            canCreate = false;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f);
        if (GUILayout.Button("🗑️ Reset Form", GUILayout.Width(120), GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Reset Form",
                "Are you sure you want to reset all fields?", "Yes", "Cancel"))
            {
                ResetForm();
            }
        }

        GUILayout.Space(10);

        GUI.enabled = canCreate;
        GUI.backgroundColor = canCreate ? new Color(0.7f, 0.9f, 0.7f) : new Color(0.8f, 0.8f, 0.8f);
        if (GUILayout.Button("Create Furniture", GUILayout.Width(150), GUILayout.Height(30)))
        {
            CreateFurniture();
        }
        GUI.enabled = true;
        GUI.backgroundColor = Color.white;

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (!canCreate)
        {
            EditorGUILayout.Space(10);
            string errorMessage = "";
            if (string.IsNullOrEmpty(furnitureName))
                errorMessage += "• Furniture name is required\n";
            if (sprites.Count == 0)
                errorMessage += "• At least one sprite is required\n";
            if (isAnimated && sprites.Count < 2)
                errorMessage += "• Animated furniture needs at least 2 sprites\n";

            EditorGUILayout.HelpBox($"⚠️ Cannot create furniture:\n{errorMessage.TrimEnd()}", MessageType.Warning);
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "💡 Tips:\n" +
            "• Use descriptive names for both English and Spanish\n" +
            "• Top objects appear above other furniture\n" +
            "• Compatible furniture can be placed on this item\n" +
            "• Animation speed affects how fast sprites cycle",
            MessageType.Info);
    }

    private void DrawSeparator()
    {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(5);
    }

    private GameObject GetTemplateForSize()
    {
        if (size.x == 1 && size.y == 1)
        {
            return isTopObject ? top_template_1x1 : bottom_template_1x1;
        }
        else if (size.x == 2 && size.y == 1)
        {
            return isTopObject ? top_template_2x1 : bottom_template_2x1;
        }
        else if (size.x == 2 && size.y == 2)
        {
            return isTopObject ? top_template_2x2 : bottom_template_2x2;
        }
        else if (size.x == 3 && size.y == 1)
        {
            return isTopObject ? top_template_3x1 : bottom_template_3x1;
        }

        return bottom_template_1x1;
    }

    private void CreateFurniture()
    {
        if (string.IsNullOrEmpty(furnitureName))
        {
            EditorUtility.DisplayDialog("❌ Error", "Furniture name cannot be empty.", "OK");
            return;
        }

        if (sprites.Count == 0)
        {
            EditorUtility.DisplayDialog("❌ Error", "At least one sprite is required.", "OK");
            return;
        }

        if (isAnimated && sprites.Count < 2)
        {
            EditorUtility.DisplayDialog("❌ Error", "Animated furniture requires at least 2 sprites.", "OK");
            return;
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        FurnitureOriginalData furniture = ScriptableObject.CreateInstance<FurnitureOriginalData>();
        furniture.Name = furnitureName;
        furniture.es_Name = spanishName;
        furniture.price = price;
        furniture.size = size;
        furniture.typeOfSize = typeOfSize;
        furniture.hasComboSprite = hasComboSprite;
        furniture.comboTriggerFurniture = comboTriggerFurniture;
        furniture.sprites = sprites.ToArray();
        furniture.compatibles = compatibles.ToArray();
        furniture.furnitureTag = furnitureTag;
        furniture.tagMatchBonusPoints = tagMatchBonusPoints;

        GameObject templatePrefab = GetTemplateForSize();

        if (templatePrefab != null)
        {
            string prefabPath = $"{savePath}/{furnitureName}_Prefab.prefab";
            GameObject prefabInstance = Instantiate(templatePrefab);
            prefabInstance.name = $"{furnitureName}_Prefab";

            SpriteRenderer spriteRenderer = prefabInstance.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprites[0];

                if (isAnimated && sprites.Count >= 2)
                {
                    string animationFolder = $"{savePath}/Animations";
                    if (!Directory.Exists(animationFolder))
                    {
                        Directory.CreateDirectory(animationFolder);
                    }

                    Animator animator = prefabInstance.GetComponentInChildren<Animator>();
                    if (animator == null)
                    {
                        animator = spriteRenderer.gameObject.AddComponent<Animator>();
                    }

                    string controllerPath = $"{animationFolder}/{furnitureName}_Controller.controller";
                    AnimatorController animController = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

                    string clipPath = $"{animationFolder}/{furnitureName}_Animation.anim";
                    AnimationClip animClip = new AnimationClip();
                    animClip.name = $"{furnitureName}_Animation";

                    animClip.frameRate = 12;
                    animClip.wrapMode = WrapMode.Loop;

                    EditorCurveBinding spriteBinding = new EditorCurveBinding();
                    spriteBinding.type = typeof(SpriteRenderer);
                    spriteBinding.path = spriteRenderer.gameObject.name == prefabInstance.name ? "" : spriteRenderer.gameObject.name;
                    spriteBinding.propertyName = "m_Sprite";

                    ObjectReferenceKeyframe[] spriteKeyFrames = new ObjectReferenceKeyframe[sprites.Count];
                    float timePerFrame = 1.0f / (sprites.Count * animationSpeed);

                    for (int i = 0; i < sprites.Count; i++)
                    {
                        spriteKeyFrames[i] = new ObjectReferenceKeyframe();
                        spriteKeyFrames[i].time = i * timePerFrame;
                        spriteKeyFrames[i].value = sprites[i];
                    }

                    AnimationUtility.SetObjectReferenceCurve(animClip, spriteBinding, spriteKeyFrames);

                    AssetDatabase.CreateAsset(animClip, clipPath);

                    AnimatorState state = animController.AddMotion(animClip);
                    state.name = "Default";

                    animator.runtimeAnimatorController = animController;
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No SpriteRenderer found in the template. Sprites will not be applied.");
            }

            GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
            DestroyImmediate(prefabInstance);

            furniture.prefab = createdPrefab;
        }
        else
        {
            EditorUtility.DisplayDialog("⚠️ Warning", "No template found for the selected size. Creating furniture without a prefab.", "OK");
        }

        string assetPath = $"{savePath}/{furnitureName}.asset";
        AssetDatabase.CreateAsset(furniture, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = furniture;

        EditorUtility.DisplayDialog("✅ Success",
            $"Furniture '{furnitureName}' created successfully!\n\n" +
            $"📁 Asset: {assetPath}\n" +
            $"🎮 Prefab: {(furniture.prefab != null ? AssetDatabase.GetAssetPath(furniture.prefab) : "None")}", "OK");

        if (EditorUtility.DisplayDialog("🔄 Create Another?",
            "Would you like to reset the form to create another furniture item?", "Yes", "No"))
        {
            ResetForm();
        }
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
        isAnimated = false;
        animationSpeed = 0.5f;
        furnitureTag = RoomTag.None;
        tagMatchBonusPoints = 50;
        showCompatibles = false;
        showSprites = false;
    }
}