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

  void Awake()
  {
    if (instance == null || instance != this)
    {
      instance = this;
    }
    
    renderer = this.gameObject.GetComponent<MeshRenderer>();
  }

  void Start()
  {
    GlobalRainbowMode = false;
    ChangeColour(num);
  }

  public static bool GlobalRainbowMode = false;
  private float rainbowTimer = 0f;
  private float rainbowInterval = 0.3f;

  void Update()
  {
      if (GlobalRainbowMode)
      {
          rainbowTimer += Time.deltaTime;
          if (rainbowTimer >= rainbowInterval)
          {
              rainbowTimer = 0f;
              
               if (palettesDatabase != null && palettesDatabase.palettes != null && palettesDatabase.palettes.Count > 0)
               {
                   num++;
                   if (num >= palettesDatabase.palettes.Count) num = 0;
                   ChangeColour(num);
               }
          }
      }
  }

  public static System.Action<ColorPalette> OnPaletteChanged;

  public void ChangeColour(int n)
  {
    num = n;

    if (palettesDatabase != null && palettesDatabase.palettes != null && num >= 0 && num < palettesDatabase.palettes.Count)
    {
        ColorPalette palette = palettesDatabase.palettes[num];
        Darkest = palette.Darkest;
        Dark = palette.Dark;
        Light = palette.Light;
        Lightest = palette.Lightest;

        if (renderer != null && renderer.material != null)
        {
            renderer.material.SetColor("_Darkest", palette.Darkest);
            renderer.material.SetColor("_Dark", palette.Dark);
            renderer.material.SetColor("_Light", palette.Light);
            renderer.material.SetColor("_Lightest", palette.Lightest);
        }

        OnPaletteChanged?.Invoke(palette);
    }
    else
    {
        Debug.LogWarning($"[ColourChanger] Palette index {num} out of bounds or database missing!");
    }
  }
}