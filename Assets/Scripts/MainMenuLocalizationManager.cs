using UnityEngine;

public class MainMenuLocalizationManager : MonoBehaviour
{
    [SerializeField] private SpriteRenderer controlsScreenRenderer;
    [SerializeField] private Sprite englishControlsSprite;
    [SerializeField] private Sprite spanishControlsSprite;

    private bool isSpanish;

    private void Start()
    {
        // Check if the main LocalizationManager exists and get language preference
        if (LocalizationManager.Instance != null)
        {
            isSpanish = LocalizationManager.Instance.IsSpanish;
        }
        else
        {
            // Fallback to system language detection if the main manager isn't available
            DetectLanguage();
        }

        // Apply the appropriate sprite
        UpdateControlsSprite();
    }

    private void DetectLanguage()
    {
        // Get the system language
        SystemLanguage deviceLanguage = Application.systemLanguage;
        
        // Check if the language is Spanish or a Spanish variant
        isSpanish = deviceLanguage == SystemLanguage.Spanish || 
                   deviceLanguage == SystemLanguage.Catalan;
        
        Debug.Log($"Main Menu - Device language detected: {deviceLanguage}. Spanish mode: {isSpanish}");
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

        // Set the appropriate sprite based on language
        controlsScreenRenderer.sprite = isSpanish ? spanishControlsSprite : englishControlsSprite;
        Debug.Log($"Updated controls screen to {(isSpanish ? "Spanish" : "English")} version");
    }

    // Public method to manually update the controls sprite
    public void SetLanguage(bool spanish)
    {
        isSpanish = spanish;
        UpdateControlsSprite();
    }
}
