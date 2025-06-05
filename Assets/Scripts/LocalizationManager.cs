using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [SerializeField] private GameObject tutorialEN;
    [SerializeField] private GameObject tutorialES;
    [SerializeField] private UnityEngine.UI.Image labelerBackground;
    [SerializeField] private Sprite labelerEN;
    [SerializeField] private Sprite labelerES; // TODO: Capaz mas facil hacer nuestros scripts LocalizedSprite y LocalizedText pa agregar como componentes, que vean el bool de aca y se actualicen solos

    [SerializeField] private bool debug;
    [SerializeField] private bool english;

    public bool IsSpanish { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DetectLanguage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateLocalizedObjects();
    }

    private void DetectLanguage()
    {
        SystemLanguage deviceLanguage = Application.systemLanguage;

        if (!debug)
        {
            IsSpanish = deviceLanguage == SystemLanguage.Spanish ||
                   deviceLanguage == SystemLanguage.Catalan ||
                   deviceLanguage == SystemLanguage.Spanish;
        }
        else
        {
            IsSpanish = !english;
        }
        
        Debug.Log($"Device language detected: {deviceLanguage}. Spanish mode: {IsSpanish}");
    }

    private void UpdateLocalizedObjects()
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

        if(labelerBackground) labelerBackground.sprite = IsSpanish ? labelerES : labelerEN;
    }

    public void SetLanguage(bool spanish)
    {
        IsSpanish = spanish;
        UpdateLocalizedObjects();
    }
}
