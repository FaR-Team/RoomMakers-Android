#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ColorPalettePreviewWindow : EditorWindow
{
    private ColorPalette currentPalette;
    private Vector2 scrollPosition;
    private string hexDarkest, hexDark, hexLight, hexLightest;
    private bool showHexInput = false;

    [MenuItem("Tools/Color Palettes/Preview Window")]
    public static void ShowWindow()
    {
        GetWindow<ColorPalettePreviewWindow>("Color Palette Preview");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Color Palette Preview", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        currentPalette = EditorGUILayout.ObjectField("Palette", currentPalette, typeof(ColorPalette), false) as ColorPalette;
        
        if (currentPalette != null)
        {
            EditorGUILayout.Space(10);
            
            showHexInput = EditorGUILayout.Toggle("Show Hex Input", showHexInput);
            
            EditorGUILayout.Space(10);
            
            if (showHexInput)
            {
                DrawHexInputs();
            }
            
            EditorGUILayout.Space(10);
            
            DrawColorPreview("Darkest", currentPalette.Darkest);
            DrawColorPreview("Dark", currentPalette.Dark);
            DrawColorPreview("Light", currentPalette.Light);
            DrawColorPreview("Lightest", currentPalette.Lightest);
            
            EditorGUILayout.Space(20);
            
            if (GUILayout.Button("Export This Palette"))
            {
                string exportFolder = EditorUtility.SaveFolderPanel("Export Color Palette", "", "");
                if (!string.IsNullOrEmpty(exportFolder))
                {
                    ColorPaletteUtility.ExportPalette(currentPalette, exportFolder);
                    EditorUtility.DisplayDialog("Export Complete", $"Exported {currentPalette.name} to {exportFolder}", "OK");
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select a ColorPalette asset to preview", MessageType.Info);
        }
    }
    
    private void DrawHexInputs()
    {
        EditorGUI.BeginChangeCheck();
        
        hexDarkest = EditorGUILayout.TextField("Darkest Hex", hexDarkest ?? ColorUtility.ToHtmlStringRGBA(currentPalette.Darkest));
        hexDark = EditorGUILayout.TextField("Dark Hex", hexDark ?? ColorUtility.ToHtmlStringRGBA(currentPalette.Dark));
        hexLight = EditorGUILayout.TextField("Light Hex", hexLight ?? ColorUtility.ToHtmlStringRGBA(currentPalette.Light));
        hexLightest = EditorGUILayout.TextField("Lightest Hex", hexLightest ?? ColorUtility.ToHtmlStringRGBA(currentPalette.Lightest));
        
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
    }
    
    private void DrawColorPreview(string label, Color color)
    {
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.LabelField(label, GUILayout.Width(60));
        
        EditorGUILayout.ColorField(color, GUILayout.Width(60));
        
        EditorGUILayout.LabelField($"#{ColorUtility.ToHtmlStringRGBA(color)}", GUILayout.Width(100));
        
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(20), GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, color);
        
        EditorGUILayout.EndHorizontal();
    }
}
#endif