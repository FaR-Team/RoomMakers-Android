using UnityEngine;
using UnityEngine.UI;

public class MainMenuLocalizationManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer controlsScreenRenderer;
    [SerializeField] private Sprite englishControlsSprite;
    [SerializeField] private Sprite spanishControlsSprite;
    
    [SerializeField] private SpriteRenderer creditsScreenRenderer;
    [SerializeField] private Sprite englishCreditsSprite;
    [SerializeField] private Sprite spanishCreditsSprite;
    
    [SerializeField] private Button playButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button leaderboardsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button casualButton;
    [SerializeField] private Button rogueButton;
    
    [SerializeField] private Sprite englishPlaySprite;
    [SerializeField] private Sprite spanishPlaySprite;
    [SerializeField] private Sprite englishControlsButtonSprite;
    [SerializeField] private Sprite spanishControlsButtonSprite;
    [SerializeField] private Sprite englishLeaderboardsSprite;
    [SerializeField] private Sprite spanishLeaderboardsSprite;
    [SerializeField] private Sprite englishCreditsButtonSprite;
    [SerializeField] private Sprite spanishCreditsButtonSprite;
    [SerializeField] private Sprite englishCasualSprite;
    [SerializeField] private Sprite spanishCasualSprite;
    [SerializeField] private Sprite englishRogueSprite;
    [SerializeField] private Sprite spanishRogueSprite;

    private bool isSpanish;

    private void Start()
    {
        if (LocalizationManager.Instance != null)
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
        UpdateButtonSprites();
    }

    private void UpdateControlsSprite()
    {
        if (controlsScreenRenderer == null)
        {
            Debug.LogError("Controls Screen Renderer not assigned in MainMenuLocalizationManager!");
            return;
        }

        if (englishControlsSprite == null || spanishControlsSprite == null)
        {
            Debug.LogError("Control sprites not assigned in MainMenuLocalizationManager!");
            return;
        }

        controlsScreenRenderer.sprite = isSpanish ? spanishControlsSprite : englishControlsSprite;
        Debug.Log($"Updated controls screen to {(isSpanish ? "Spanish" : "English")} version");
    }

    private void UpdateCreditsSprite()
    {
        if (creditsScreenRenderer == null)
        {
            Debug.LogError("Credits Screen Renderer not assigned in MainMenuLocalizationManager!");
            return;
        }

        if (englishCreditsSprite == null || spanishCreditsSprite == null)
        {
            Debug.LogError("Credits sprites not assigned in MainMenuLocalizationManager!");
            return;
        }

        creditsScreenRenderer.sprite = isSpanish ? spanishCreditsSprite : englishCreditsSprite;
    }

    private void UpdateButtonSprites()
    {
        UpdateButtonSprite(playButton, englishPlaySprite, spanishPlaySprite);
        UpdateButtonSprite(controlsButton, englishControlsButtonSprite, spanishControlsButtonSprite);
        UpdateButtonSprite(leaderboardsButton, englishLeaderboardsSprite, spanishLeaderboardsSprite);
        UpdateButtonSprite(creditsButton, englishCreditsButtonSprite, spanishCreditsButtonSprite);
        UpdateButtonSprite(casualButton, englishCasualSprite, spanishCasualSprite);
        UpdateButtonSprite(rogueButton, englishRogueSprite, spanishRogueSprite);
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
