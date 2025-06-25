#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class ColorPaletteManagerWindow : EditorWindow
{
    [System.Serializable]
    public class ColorPaletteData
    {
        public string name;
        public string darkestHex;
        public string darkHex;
        public string lightHex;
        public string lightestHex;
    }

    private ColorPalette currentPalette;
    private Vector2 scrollPosition;
    private string hexDarkest, hexDark, hexLight, hexLightest;
    private bool showHexInput = false;
    private int selectedTab = 0;
    private string[] tabNames = { "Preview", "Import/Export", "Create" };

    [MenuItem("DevTools/Color Palette Manager")]
    public static void ShowWindow()
    {
        GetWindow<ColorPaletteManagerWindow>("Color Palette Manager");
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
        GUILayout.Label("🎨 Color Palette Manager", titleStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(15);
        
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames, GUILayout.Height(25));
        
        EditorGUILayout.Space(15);
        DrawSeparator();
        
        switch (selectedTab)
        {
            case 0:
                DrawPreviewTab();
                break;
            case 1:
                DrawImportExportTab();
                break;
            case 2:
                DrawCreateTab();
                break;
        }
        
        EditorGUILayout.Space(10);
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawPreviewTab()
    {
        EditorGUILayout.Space(10);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        GUILayout.Label("Select Palette:", EditorStyles.miniLabel);
        currentPalette = EditorGUILayout.ObjectField(currentPalette, typeof(ColorPalette), false) as ColorPalette;
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        if (currentPalette != null)
        {
            EditorGUILayout.Space(15);
            
            GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
            sectionStyle.fontSize = 14;
            sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("⚙️ Settings", sectionStyle);
            GUILayout.FlexibleSpace();
            showHexInput = EditorGUILayout.Toggle("Hex Editor", showHexInput, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            
            if (showHexInput)
            {
                EditorGUILayout.Space(10);
                DrawHexInputs();
            }
            
            EditorGUILayout.Space(15);
            DrawSeparator();
            
            EditorGUILayout.Space(10);
            GUILayout.Label("🎨 Color Preview", sectionStyle);
            EditorGUILayout.Space(10);
            
            DrawColorPreview("🌑 Darkest", currentPalette.Darkest);
            EditorGUILayout.Space(5);
            DrawColorPreview("🌓 Dark", currentPalette.Dark);
            EditorGUILayout.Space(5);
            DrawColorPreview("🌕 Light", currentPalette.Light);
            EditorGUILayout.Space(5);
            DrawColorPreview("☀️ Lightest", currentPalette.Lightest);
            
            EditorGUILayout.Space(20);
            DrawSeparator();
            
            EditorGUILayout.Space(15);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUI.backgroundColor = new Color(0.7f, 0.9f, 0.7f);
            if (GUILayout.Button("📤 Export This Palette", GUILayout.Height(35), GUILayout.Width(200)))
            {
                string exportFolder = EditorUtility.SaveFolderPanel("Export Color Palette", "", "");
                if (!string.IsNullOrEmpty(exportFolder))
                {
                    ExportPalette(currentPalette, exportFolder);
                    EditorUtility.DisplayDialog("✅ Export Complete", 
                        $"Successfully exported '{currentPalette.name}' to:\n{exportFolder}", "OK");
                }
            }
            GUI.backgroundColor = Color.white;
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.Space(20);
            
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            
            GUIStyle messageStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            messageStyle.fontSize = 14;
            messageStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("🎨", GUILayout.Width(30), GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Select a ColorPalette asset to preview", messageStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }
    }
    
    private void DrawImportExportTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📤 Export All Palettes", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Export all ColorPalette assets in the project to JSON files.", MessageType.Info);
        
        EditorGUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.7f, 0.9f, 0.7f);
        if (GUILayout.Button("📤 Export All Palettes", GUILayout.Height(30), GUILayout.Width(180)))
        {
            ExportAllPalettes();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(20);
        DrawSeparator();
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📥 Import Palette", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Import a ColorPalette from a JSON file.", MessageType.Info);
        
        EditorGUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.9f, 0.7f, 0.7f);
        if (GUILayout.Button("📥 Import Palette", GUILayout.Height(30), GUILayout.Width(150)))
        {
            ImportPalette();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawCreateTab()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.2f, 0.6f, 0.9f);
        
        EditorGUILayout.Space(10);
        GUILayout.Label("📝 Create Palette Template", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.HelpBox("Create a JSON template file that you can edit and import later.", MessageType.Info);
        
        EditorGUILayout.Space(5);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        GUI.backgroundColor = new Color(0.9f, 0.9f, 0.7f);
        if (GUILayout.Button("📝 Create Template", GUILayout.Height(30), GUILayout.Width(150)))
        {
            CreatePaletteTemplate();
        }
        GUI.backgroundColor = Color.white;
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(20);
        
        EditorGUILayout.HelpBox(
            "💡 Template Usage:\n" +
            "1. Create a template JSON file\n" +
            "2. Edit the hex values in any text editor\n" +
            "3. Import the modified JSON to create a ColorPalette asset\n" +
            "4. Templates are saved with default grayscale colors",
            MessageType.Info);
    }
    
    private void DrawHexInputs()
    {
        GUIStyle sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        sectionStyle.fontSize = 14;
        sectionStyle.normal.textColor = new Color(0.9f, 0.6f, 0.2f);
        
        GUILayout.Label("✏️ Hex Editor", sectionStyle);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUI.BeginChangeCheck();
        
        DrawHexInputField("🌑 Darkest:", ref hexDarkest, currentPalette.Darkest);
        DrawHexInputField("🌓 Dark:", ref hexDark, currentPalette.Dark);
        DrawHexInputField("🌕 Light:", ref hexLight, currentPalette.Light);
        DrawHexInputField("☀️ Lightest:", ref hexLightest, currentPalette.Lightest);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(currentPalette, "Edit Color Palette");
            
            Color color;
            if (ColorUtility.TryParseHtmlString("#" + hexDarkest, out color))
                currentPalette.Darkest = color;
            if (ColorUtility.TryParseHtmlString("#" + hexDark, out color))
                currentPalette.Dark = color;
            if (ColorUtility.TryParseHtmlString("#" + hexLight, out color))
                currentPalette.Light = color;
            if (ColorUtility.TryParseHtmlString("#" + hexLightest, out color))
                currentPalette.Lightest = color;
                
            EditorUtility.SetDirty(currentPalette);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawHexInputField(string label, ref string hexValue, Color currentColor)
    {
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Label(label, GUILayout.Width(80));
        
        EditorGUILayout.ColorField(currentColor, GUILayout.Width(50));
        
        GUILayout.Label("#", GUILayout.Width(15));
        hexValue = EditorGUILayout.TextField(hexValue ?? ColorUtility.ToHtmlStringRGBA(currentColor));
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(3);
    }
    
    private void DrawColorPreview(string label, Color color)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.Space(5);
        
        EditorGUILayout.BeginHorizontal();
        
        GUILayout.Label(label, EditorStyles.boldLabel, GUILayout.Width(100));
        
        GUILayout.FlexibleSpace();
        
        EditorGUILayout.ColorField(color, GUILayout.Width(60), GUILayout.Height(25));
        
        GUIStyle hexStyle = new GUIStyle(EditorStyles.miniLabel);
        hexStyle.alignment = TextAnchor.MiddleRight;
        hexStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label($"#{ColorUtility.ToHtmlStringRGBA(color)}", hexStyle, GUILayout.Width(100));
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        Rect colorRect = EditorGUILayout.GetControlRect(GUILayout.Height(30));
        colorRect.x += 5;
        colorRect.width -= 10;
        
        EditorGUI.DrawRect(colorRect, color);
        
                Rect borderRect = new Rect(colorRect.x - 1, colorRect.y - 1, colorRect.width + 2, colorRect.height + 2);
        EditorGUI.DrawRect(borderRect, new Color(0.3f, 0.3f, 0.3f, 0.8f));
        EditorGUI.DrawRect(colorRect, color);
        
        EditorGUILayout.Space(5);
        EditorGUILayout.EndVertical();
    }
    
    private void DrawSeparator()
    {
        EditorGUILayout.Space(5);
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(5);
    }
    
    // Utility Methods
    private static string HexToRGBA(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        if (hex.Length == 6)
            hex += "FF";

        if (hex.Length != 8)
            return "Invalid hex color";

        Color color = new Color(
            int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
            int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
            int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
            int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255f
        );

        return $"R:{color.r:F2}, G:{color.g:F2}, B:{color.b:F2}, A:{color.a:F2}";
    }

    private static string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }

    private static Color HexToColor(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
            return color;
        return Color.white;
    }

    public static void ExportAllPalettes()
    {
        string exportFolder = EditorUtility.SaveFolderPanel("Export Color Palettes", "", "ColorPalettes");
        if (string.IsNullOrEmpty(exportFolder))
            return;

        string[] guids = AssetDatabase.FindAssets("t:ColorPalette");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ColorPalette palette = AssetDatabase.LoadAssetAtPath<ColorPalette>(path);
            
            ExportPalette(palette, exportFolder);
        }
        
        EditorUtility.DisplayDialog("Export Complete", $"Exported {guids.Length} color palettes to {exportFolder}", "OK");
    }

    public static void ExportPalette(ColorPalette palette, string exportFolder)
    {
        ColorPaletteData data = new ColorPaletteData
        {
            name = palette.name,
            darkestHex = ColorToHex(palette.Darkest),
            darkHex = ColorToHex(palette.Dark),
            lightHex = ColorToHex(palette.Light),
            lightestHex = ColorToHex(palette.Lightest)
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path.Combine(exportFolder, $"{palette.name}.json"), json);
    }

    public static void ImportPalette()
    {
        string jsonPath = EditorUtility.OpenFilePanel("Import Color Palette", "", "json");
        if (string.IsNullOrEmpty(jsonPath))
            return;

        string json = File.ReadAllText(jsonPath);
        ColorPaletteData data = JsonUtility.FromJson<ColorPaletteData>(json);

        ColorPalette palette = ScriptableObject.CreateInstance<ColorPalette>();
        palette.Darkest = HexToColor(data.darkestHex);
        palette.Dark = HexToColor(data.darkHex);
        palette.Light = HexToColor(data.lightHex);
        palette.Lightest = HexToColor(data.lightestHex);

        string assetPath = $"Assets/palettes/{data.name}.asset";
        
        Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
        
        AssetDatabase.CreateAsset(palette, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Import Complete", $"Imported color palette '{data.name}' to {assetPath}", "OK");
    }
    
    public static void CreatePaletteTemplate()
    {
        string exportPath = EditorUtility.SaveFilePanel("Save Palette Template", "", "NewPalette.json", "json");
        if (string.IsNullOrEmpty(exportPath))
            return;
            
        ColorPaletteData data = new ColorPaletteData
        {
            name = "NewPalette",
            darkestHex = "#000000",
            darkHex = "#333333",
            lightHex = "#BBBBBB",
            lightestHex = "#FFFFFF"
        };
        
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(exportPath, json);
        
        EditorUtility.DisplayDialog("Template Created", $"Created palette template at {exportPath}", "OK");
    }
}
#endif
