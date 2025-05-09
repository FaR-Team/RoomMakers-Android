using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ColorPaletteUtility
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

#if UNITY_EDITOR
    [MenuItem("Tools/Color Palettes/Export All Palettes")]
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

    [MenuItem("Tools/Color Palettes/Import Palette")]
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
    
    [MenuItem("Tools/Color Palettes/Create Palette Template")]
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
#endif
}