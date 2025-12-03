using UnityEngine;
using UnityEngine.UI;

public class MainMenuLocalizationManager : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private bool forceSpanish = false;
    
    [Header("Screen Sprites")]
    [SerializeField] private SpriteRenderer controlsScreenRenderer;
    [SerializeField] private Sprite controlsEN;
    [SerializeField] private Sprite controlsES;
    
    [SerializeField] private SpriteRenderer creditsScreenRenderer;
    [SerializeField] private Sprite creditsEN;
    [SerializeField] private Sprite creditsES;
    
    [SerializeField] private SpriteRenderer buttonsMenuBackground;
    [SerializeField] private Sprite buttonsMenuEN;
    [SerializeField] private Sprite buttonsMenuES;
    
    [SerializeField] private SpriteRenderer gameModeMenuBackground;
    [SerializeField] private Sprite gameModeMenuEN;
    [SerializeField] private Sprite gameModeMenuES;
    
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button leaderboardsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button casualButton;
    [SerializeField] private Button classicButton;
    [SerializeField] private Button rogueButton;
    
    [Header("Button Sprites")]
    [SerializeField] private Sprite playEN;
    [SerializeField] private Sprite playES;
    [SerializeField] private Sprite controlsBtnEN;
    [SerializeField] private Sprite controlsBtnES;
    [SerializeField] private Sprite leaderboardsEN;
    [SerializeField] private Sprite leaderboardsES;
    [SerializeField] private Sprite creditsBtnEN;
    [SerializeField] private Sprite creditsBtnES;
    [SerializeField] private Sprite casualEN;
    [SerializeField] private Sprite casualES;
    [SerializeField] private Sprite classicEN;
    [SerializeField] private Sprite classicES;
    [SerializeField] private Sprite rogueEN;
    [SerializeField] private Sprite rogueES;

    private bool isSpanish;

    private void Start()
    {
        if (debugMode)
        {
            isSpanish = forceSpanish;
        }
        else if (LocalizationManager.Instance != null)
        {
            isSpanish = LocalizationManager.Instance.IsSpanish;
        }
        else
        {
            DetectLanguage();
        }

        UpdateAllSprites();
    }

    private void DetectLanguage()
    {
        SystemLanguage deviceLanguage = Application.systemLanguage;
        
        isSpanish = deviceLanguage == SystemLanguage.Spanish || 
                   deviceLanguage == SystemLanguage.Catalan;
        
        Debug.Log($"Main Menu - Device language detected: {deviceLanguage}. Spanish mode: {isSpanish}");
    }

    private void UpdateAllSprites()
    {
        UpdateControlsSprite();
        UpdateCreditsSprite();
        UpdateBackgroundSprites();
        UpdateButtonSprites();
    }

    private void UpdateControlsSprite()
    {
        if (controlsScreenRenderer == null)
        {
            Debug.LogError("Controls Screen Renderer not assigned in MainMenuLocalizationManager!");
            return;
        }

        if (controlsEN == null || controlsES == null)
        {
            Debug.LogError("Control sprites not assigned in MainMenuLocalizationManager!");
            return;
        }

        controlsScreenRenderer.sprite = isSpanish ? controlsES : controlsEN;
        Debug.Log($"Updated controls screen to {(isSpanish ? "Spanish" : "English")} version");
    }

    private void UpdateCreditsSprite()
    {
        if (creditsScreenRenderer == null)
        {
            Debug.LogError("Credits Screen Renderer not assigned in MainMenuLocalizationManager!");
            return;
        }

        if (creditsEN == null || creditsES == null)
        {
            Debug.LogError("Credits sprites not assigned in MainMenuLocalizationManager!");
            return;
        }

        creditsScreenRenderer.sprite = isSpanish ? creditsES : creditsEN;
    }

    private void UpdateBackgroundSprites()
    {
        UpdateSpriteRenderer(buttonsMenuBackground, buttonsMenuEN, buttonsMenuES, "ButtonsMenuBackground");
        UpdateSpriteRenderer(gameModeMenuBackground, gameModeMenuEN, gameModeMenuES, "GameModeMenuBackground");
    }

    private void UpdateSpriteRenderer(SpriteRenderer renderer, Sprite englishSprite, Sprite spanishSprite, string name)
    {
        if (renderer == null) return;

        if (englishSprite == null || spanishSprite == null)
        {
            Debug.LogError($"{name} sprites not assigned in MainMenuLocalizationManager!");
            return;
        }

        renderer.sprite = isSpanish ? spanishSprite : englishSprite;
    }

    private void UpdateButtonSprites()
    {
        UpdateButtonSprite(playButton, playEN, playES);
        UpdateButtonSprite(controlsButton, controlsBtnEN, controlsBtnES);
        UpdateButtonSprite(leaderboardsButton, leaderboardsEN, leaderboardsES);
        UpdateButtonSprite(creditsButton, creditsBtnEN, creditsBtnES);
        UpdateButtonSprite(casualButton, casualEN, casualES);
        UpdateButtonSprite(classicButton, classicEN, classicES);
        UpdateButtonSprite(rogueButton, rogueEN, rogueES);
    }

    private void UpdateButtonSprite(Button button, Sprite englishSprite, Sprite spanishSprite)
    {
        if (button == null) return;

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage == null) return;

        if (englishSprite == null || spanishSprite == null)
        {
            Debug.LogError($"Button sprites not assigned for {button.name} in MainMenuLocalizationManager!");
            return;
        }

        buttonImage.sprite = isSpanish ? spanishSprite : englishSprite;
    }

    public void SetLanguage(bool spanish)
    {
        isSpanish = spanish;
        UpdateAllSprites();
    }
}
