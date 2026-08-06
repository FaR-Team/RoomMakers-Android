using UnityEngine;
using UnityEngine.UI;

public class PaletteGlowTint : MonoBehaviour
{
    [Header("Glow UI Images")]
    public Image landscapeGlowImage;

    public Image portraitGlowImage;

    [Header("Settings")]
    public bool preserveImageAlpha = true;

    [Header("Shader Settings")]
    public bool useGlowShader = true;

    private Material glowMaterial;

    private void Awake()
    {
        if (useGlowShader)
        {
            if (landscapeGlowImage != null) EnsureGlowMaterial(landscapeGlowImage);
            if (portraitGlowImage != null) EnsureGlowMaterial(portraitGlowImage);
        }
        ApplyCurrentPaletteColor();
    }

    private void OnEnable()
    {
        ColourChanger.OnPaletteChanged += HandlePaletteChanged;
        ApplyCurrentPaletteColor();
    }

    private void OnDisable()
    {
        ColourChanger.OnPaletteChanged -= HandlePaletteChanged;
    }

    private void Start()
    {
        ApplyCurrentPaletteColor();
    }

    private void HandlePaletteChanged(ColorPalette newPalette)
    {
        if (newPalette != null)
        {
            ApplyColor(newPalette.Lightest);
        }
    }

    public void ApplyCurrentPaletteColor()
    {
        if (ColourChanger.instance != null && ColourChanger.instance.palettesDatabase != null)
        {
            var db = ColourChanger.instance.palettesDatabase;
            int num = ColourChanger.instance.num;
            if (db.palettes != null && num >= 0 && num < db.palettes.Count)
            {
                ApplyColor(db.palettes[num].Lightest);
            }
            else if (ColourChanger.instance.Lightest != default)
            {
                ApplyColor(ColourChanger.instance.Lightest);
            }
        }
    }

    private void ApplyColor(Color lightestColor)
    {
        TintImage(landscapeGlowImage, lightestColor);
        TintImage(portraitGlowImage, lightestColor);
    }

    private void TintImage(Image img, Color paletteColor)
    {
        if (img == null) return;

        if (useGlowShader)
        {
            EnsureGlowMaterial(img);
        }

        float alpha = preserveImageAlpha ? img.color.a : paletteColor.a;
        img.color = new Color(paletteColor.r, paletteColor.g, paletteColor.b, alpha);
    }

    private void EnsureGlowMaterial(Image img)
    {
        if (glowMaterial == null)
        {
            Shader shader = Shader.Find("UI/GlowTint");
            if (shader != null)
            {
                glowMaterial = new Material(shader);
            }
            else
            {
                Debug.LogWarning("[PaletteGlowTint] Shader 'UI/GlowTint' not found!");
                return;
            }
        }

        if (img.material != glowMaterial)
        {
            img.material = glowMaterial;
        }
    }
}
