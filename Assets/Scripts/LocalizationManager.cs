using UnityEngine;
using System.Globalization;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [SerializeField] private GameObject tutorialEN;
    [SerializeField] private GameObject tutorialES;

    public bool IsSpanish { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DetectLanguage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ActivateCorrectTutorial();
    }

    private void DetectLanguage()
    {
        // Get the system language
        SystemLanguage deviceLanguage = Application.systemLanguage;
        
        // Check if the language is Spanish or a Spanish variant
        IsSpanish = deviceLanguage == SystemLanguage.Spanish || 
                   deviceLanguage == SystemLanguage.Catalan || 
                   deviceLanguage == SystemLanguage.Spanish;
        
        Debug.Log($"Device language detected: {deviceLanguage}. Spanish mode: {IsSpanish}");
    }
    private void ActivateCorrectTutorial()
    {
        if (tutorialEN != null && tutorialES != null)
        {
            tutorialEN.SetActive(!IsSpanish);
            tutorialES.SetActive(IsSpanish);
            Debug.Log($"Activated tutorial: {(IsSpanish ? "Spanish" : "English")}");
        }
        else
        {
            Debug.LogWarning("Tutorial GameObjects not assigned in LocalizationManager!");
        }
    }

    // Public method to force language change (for testing or manual override)
    public void SetLanguage(bool spanish)
    {
        IsSpanish = spanish;
        ActivateCorrectTutorial();
    }
}
