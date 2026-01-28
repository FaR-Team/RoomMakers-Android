#pragma warning disable CS0108
#pragma warning disable CS0108
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ColourChanger : MonoBehaviour
{
  public static ColourChanger instance;
  public Material mat;
  public Color Darkest, Dark, Light, Lightest;
  public PalettesDatabase palettesDatabase;
  private MeshRenderer renderer;
  public int num;

  void Start()
  {
    if (instance == null || instance != this)
    {
      instance = this;
    }
    
    renderer = this.gameObject.GetComponent<MeshRenderer>();
  }

  public void ChangeColour(int n)
  {
    num = n;

    if (palettesDatabase != null && palettesDatabase.palettes != null && num >= 0 && num < palettesDatabase.palettes.Count)
    {
        ColorPalette palette = palettesDatabase.palettes[num];
        renderer.material.SetColor("_Darkest", palette.Darkest);
        renderer.material.SetColor("_Dark", palette.Dark);
        renderer.material.SetColor("_Light", palette.Light);
        renderer.material.SetColor("_Lightest", palette.Lightest);
    }
    else
    {
        Debug.LogWarning($"[ColourChanger] Palette index {num} out of bounds or database missing!");
    }
  }
}