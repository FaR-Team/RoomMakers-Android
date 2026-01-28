using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PalettesDatabase", menuName = "Palettes Database", order = 1)]
public class PalettesDatabase : ScriptableObject
{
    public List<ColorPalette> palettes = new List<ColorPalette>();

#if UNITY_EDITOR
    [ContextMenu("Refresh Palettes")]
    public void RefreshPalettes()
    {
        palettes.Clear();
        string[] guids = AssetDatabase.FindAssets("t:ColorPalette");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ColorPalette palette = AssetDatabase.LoadAssetAtPath<ColorPalette>(path);
            if (palette != null)
            {
                palettes.Add(palette);
            }
        }
        Debug.Log($"Found and added {palettes.Count} palettes to the database.");
        EditorUtility.SetDirty(this);
    }
#endif
}
