using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;

    [SerializeField] private GameObject debugPanelPrefab;
    
    private GameObject debugPanel;
    private GameObject debugButton;

    [Header("UI Scaling")]
    [SerializeField] [Range(0.5f, 2.0f)] private float uiScaleFactor = 1.0f;
    [SerializeField] private bool autoScaleWithScreen = true;
    
    [Header("Debug Activation")]
    [SerializeField] private int requiredTaps = 5;
    [SerializeField] private float tapTimeWindow = 3f;
    
    private float baseFontSize = 18f;
    private float buttonHeight = 80f;
    private float sliderHeight = 60f;
    private float toggleHeight = 60f;
    private float sectionSpacing = 25f;
    private float elementSpacing = 15f;
    private float panelPadding = 20f;
    
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI doorPriceText;
    private TextMeshProUGUI roomsBuiltText;
    private TextMeshProUGUI availableTilesText;
    
    private Toggle exponentialGrowthToggle;
    private Slider growthFactorSlider;
    private TextMeshProUGUI growthFactorText;
    
    private Slider scoreSlider;
    private TextMeshProUGUI scoreValueText;
    private Button addScoreButton;
    
    private Slider moneySlider;
    private TextMeshProUGUI moneyValueText;
    private Button addMoneyButton;
    
    private Slider maxDoorPriceSlider;
    private TextMeshProUGUI maxDoorPriceValueText;
    private Button setMaxDoorPriceButton;

    private TextMeshProUGUI difficultyInfoText;
    private Toggle difficultySystemToggle;
    private Slider difficultyTierSlider;
    private TextMeshProUGUI difficultyTierText;
    private Button resetDifficultyButton;
    
    private Button addRoomButton;
    private Button clearRoomButton;
    
    private int scoreToAdd = 100;
    private int moneyToAdd = 100;
    private int maxDoorPrice = 1000;

    private Transform consoleContent;
    private ScrollRect scrollRect;
    private List<GameObject> logEntries = new List<GameObject>();
    private Queue<LogEntry> logQueue = new Queue<LogEntry>();
    private bool showLogs = true;
    private bool showWarnings = true;
    private bool showErrors = true;
    private int maxLogEntries = 50;
    
    private bool isDebugEnabled = false;
    private bool isDebugUnlocked = false;
    
    private List<float> tapTimes = new List<float>();

    private void Awake()
    {
        if (!Debug.isDebugBuild && !Application.isEditor)
        {
            Destroy(gameObject);
            return;
        }
        
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        AdjustUIScaling();
    }
    
    private void Start()
    {
        if (!Debug.isDebugBuild && !Application.isEditor) return;
        
        CreateDebugPanel();
        debugPanel.SetActive(false);
        
        if (Application.isEditor)
        {
            UnlockDebugMenu();
        }
        else
        {
            FindAndSetupVersionButton();
        }
    }
    
    private void FindAndSetupVersionButton()
    {
        GameObject versionButton = GameObject.Find("VersionButton");
        if (versionButton == null)
        {
            versionButton = FindVersionButtonByText();
        }
        
        if (versionButton != null)
        {
            Button button = versionButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnVersionButtonPressed);
            }
        }
        else
        {
            Debug.LogWarning("DebugManager: Version button not found. Debug menu activation may not work.");
        }
    }
    
    private GameObject FindVersionButtonByText()
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text.text.Contains("v") || text.text.ToLower().Contains("version"))
            {
                Button button = text.GetComponentInParent<Button>();
                if (button != null)
                {
                    return button.gameObject;
                }
            }
        }
        
        Text[] allLegacyTexts = FindObjectsOfType<Text>();
        foreach (var text in allLegacyTexts)
        {
            if (text.text.Contains("v") || text.text.ToLower().Contains("version"))
            {
                Button button = text.GetComponentInParent<Button>();
                if (button != null)
                {
                    return button.gameObject;
                }
            }
        }
        
        return null;
    }
    
    private void OnVersionButtonPressed()
    {
        if (!Debug.isDebugBuild && !Application.isEditor) return;
        
        float currentTime = Time.unscaledTime;
        tapTimes.Add(currentTime);
        
        tapTimes.RemoveAll(time => currentTime - time > tapTimeWindow);
        
        if (tapTimes.Count >= requiredTaps)
        {
            UnlockDebugMenu();
            tapTimes.Clear();
        }
    }
    
    private void UnlockDebugMenu()
    {
        if (isDebugUnlocked) return;
        
        isDebugUnlocked = true;
                
        CreateDebugToggleButton();
    }
    
    private void CreateDebugToggleButton()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        debugButton = new GameObject("DebugToggleButton");
        debugButton.transform.SetParent(canvas.transform, false);
        
        Image buttonImage = debugButton.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        
        Button button = debugButton.AddComponent<Button>();
        button.onClick.AddListener(ToggleDebugPanel);
        button.targetGraphic = buttonImage;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(debugButton.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "DEBUG";
        buttonText.fontSize = baseFontSize * 0.8f;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonRect = debugButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.sizeDelta = new Vector2(100, 50);
        buttonRect.anchoredPosition = new Vector2(-10, -10);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 0.9f);
        colors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 0.9f);
        button.colors = colors;
    }
    
    public static bool IsDebugBuildAndUnlocked()
    {
        return (Debug.isDebugBuild || Application.isEditor) && instance != null && instance.isDebugUnlocked;
    }
    
    private void AdjustUIScaling()
    {
        float scaleFactor = uiScaleFactor;
        
        if (autoScaleWithScreen)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            float screenScaleFactor = Mathf.Min(screenWidth, screenHeight) / 1080f;
            
            scaleFactor *= screenScaleFactor;
        }
        
        baseFontSize = Mathf.Max(18f * scaleFactor, 16f);
        buttonHeight = Mathf.Max(80f * scaleFactor, 60f);
        sliderHeight = Mathf.Max(60f * scaleFactor, 40f);
        toggleHeight = Mathf.Max(60f * scaleFactor, 40f);
        sectionSpacing = Mathf.Max(25f * scaleFactor, 15f);
        elementSpacing = Mathf.Max(15f * scaleFactor, 10f);
        panelPadding = Mathf.Max(20f * scaleFactor, 15f);
    }
    
    private void CreateDebugPanel()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DebugCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); 
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        debugPanel = new GameObject("DebugPanel");
        debugPanel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = debugPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        RectTransform panelRect = debugPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.05f);
        panelRect.anchorMax = new Vector2(0.9f, 0.95f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero;
        
        GameObject scrollViewObj = CreateScrollView(debugPanel.transform);
        Transform contentTransform = scrollViewObj.transform.Find("Viewport/Content");

        scrollRect = scrollViewObj.GetComponent<ScrollRect>();
        
        VerticalLayoutGroup layoutGroup = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = sectionSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
        
        ContentSizeFitter sizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        CreateTitle(contentTransform, "DEBUG MENU");
        
        CreateInfoSection(contentTransform);
        
        CreateGrowthFactorControls(contentTransform);
        
        CreateScoreControls(contentTransform);
        
        CreateMoneyControls(contentTransform);
        
        CreateDoorPriceControls(contentTransform);
    
        CreateShopControls(contentTransform);
        
        CreateRoomControls(contentTransform);
        
        CreateDifficultyControls(contentTransform);

        CreateConsole(contentTransform);
        
        CreateCloseButton(debugPanel.transform);
    }
    
    private GameObject CreateScrollView(Transform parent)
    {
        GameObject scrollView = new GameObject("ScrollView", typeof(RectTransform));
        scrollView.transform.SetParent(parent, false);
        
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(scrollView.transform, false);
        
        Image viewportImage = viewport.AddComponent<Image>();
                viewportImage.color = new Color(0, 0, 0, 0.1f);
        
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = content.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;
        
        RectTransform scrollRectTransform = scrollView.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0, 0);
        scrollRectTransform.anchorMax = new Vector2(1, 1);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.offsetMin = new Vector2(10, 10);
        scrollRectTransform.offsetMax = new Vector2(-10, -10);
        
        RectTransform viewportRectTransform = viewport.GetComponent<RectTransform>();
        viewportRectTransform.anchorMin = new Vector2(0, 0);
        viewportRectTransform.anchorMax = new Vector2(1, 1);
        viewportRectTransform.pivot = new Vector2(0.5f, 0.5f);
        viewportRectTransform.offsetMin = Vector2.zero;
        viewportRectTransform.offsetMax = Vector2.zero;
        
        RectTransform contentRectTransform = content.GetComponent<RectTransform>();
        contentRectTransform.anchorMin = new Vector2(0, 1);
        contentRectTransform.anchorMax = new Vector2(1, 1);
        contentRectTransform.pivot = new Vector2(0.5f, 1);
        contentRectTransform.anchoredPosition = Vector2.zero;
        contentRectTransform.sizeDelta = new Vector2(0, 1500);
        
        return scrollView;
    }
    
    private void CreateTitle(Transform parent, string titleText)
    {
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = titleObj.AddComponent<TextMeshProUGUI>();
        text.text = titleText;
        text.fontSize = baseFontSize * 1.5f;
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform rectTransform = titleObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, buttonHeight);
    }
    
    private void CreateSectionHeader(Transform parent, string headerText)
    {
        GameObject headerObj = new GameObject("Header_" + headerText, typeof(RectTransform));
        headerObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI text = headerObj.AddComponent<TextMeshProUGUI>();
        text.text = headerText;
        text.fontSize = baseFontSize * 1.2f;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(0.8f, 0.8f, 0.2f);
        text.alignment = TextAlignmentOptions.Left;
        
        RectTransform rectTransform = headerObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, buttonHeight * 0.6f);
    }
    
    private GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, 0);
        
        return panel;
    }
    
    private TextMeshProUGUI CreateLabel(Transform parent, string text, float height = 30)
    {
        GameObject labelObj = new GameObject("Label_" + text, typeof(RectTransform));
        labelObj.transform.SetParent(parent, false);
        
        TextMeshProUGUI textComponent = labelObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = baseFontSize;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Left;
        
        RectTransform rectTransform = labelObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, height);
        
        return textComponent;
    }
    
    private Toggle CreateToggle(Transform parent, string label, bool isOn, UnityEngine.Events.UnityAction<bool> onValueChanged)
    {
        GameObject toggleObj = new GameObject("Toggle_" + label, typeof(RectTransform));
        toggleObj.transform.SetParent(parent, false);
        
        HorizontalLayoutGroup layoutGroup = toggleObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.spacing = elementSpacing;
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding/2), 
            Mathf.RoundToInt(panelPadding/2), 
            Mathf.RoundToInt(panelPadding/2), 
            Mathf.RoundToInt(panelPadding/2)
        );
        
        GameObject toggleControl = new GameObject("ToggleControl", typeof(RectTransform));
        toggleControl.transform.SetParent(toggleObj.transform, false);
        
        Toggle toggle = toggleControl.AddComponent<Toggle>();
        toggle.isOn = isOn;
        toggle.onValueChanged.AddListener(onValueChanged);
        
        GameObject background = new GameObject("Background", typeof(RectTransform));
        background.transform.SetParent(toggleControl.transform, false);
        
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = Color.white;
        
        GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform));
        checkmark.transform.SetParent(background.transform, false);
        
        Image checkmarkImage = checkmark.AddComponent<Image>();
        checkmarkImage.color = Color.green;
        
        toggle.targetGraphic = backgroundImage;
        toggle.graphic = checkmarkImage;
        
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(toggleObj.transform, false);
        
        TextMeshProUGUI text = labelObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = baseFontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        RectTransform toggleObjRect = toggleObj.GetComponent<RectTransform>();
        toggleObjRect.sizeDelta = new Vector2(0, toggleHeight);
        
        RectTransform toggleControlRect = toggleControl.GetComponent<RectTransform>();
        toggleControlRect.sizeDelta = new Vector2(toggleHeight, toggleHeight);
        
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(toggleHeight * 0.6f, toggleHeight * 0.6f);
        
        RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkmarkRect.pivot = new Vector2(0.5f, 0.5f);
        checkmarkRect.sizeDelta = new Vector2(toggleHeight * 0.4f, toggleHeight * 0.4f);
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(400, toggleHeight);
        
        return toggle;
    }
    
    private Slider CreateSlider(Transform parent, string label, float minValue, float maxValue, float value, 
                               UnityEngine.Events.UnityAction<float> onValueChanged, out TextMeshProUGUI valueText)
    {
        GameObject sliderObj = new GameObject("Slider_" + label, typeof(RectTransform));
        sliderObj.transform.SetParent(parent, false);
        
        VerticalLayoutGroup layoutGroup = sliderObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.spacing = elementSpacing / 2;
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding/2), 
            Mathf.RoundToInt(panelPadding/2), 
            Mathf.RoundToInt(panelPadding/2), 
            Mathf.RoundToInt(panelPadding/2)
        );
        
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(sliderObj.transform, false);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = baseFontSize;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Left;
        
        GameObject sliderControl = new GameObject("SliderControl", typeof(RectTransform));
        sliderControl.transform.SetParent(sliderObj.transform, false);
        
        Slider slider = sliderControl.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = value;
        slider.onValueChanged.AddListener(onValueChanged);
        
        GameObject background = new GameObject("Background", typeof(RectTransform));
        background.transform.SetParent(sliderControl.transform, false);
        
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderControl.transform, false);
        
        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.7f, 0.2f, 1f);
        
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderControl.transform, false);
        
        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1f, 1f, 1f, 1f);
        
        GameObject valueTextObj = new GameObject("Value", typeof(RectTransform));
        valueTextObj.transform.SetParent(sliderObj.transform, false);
        
        valueText = valueTextObj.AddComponent<TextMeshProUGUI>();
        valueText.fontSize = baseFontSize * 0.9f;
        valueText.color = new Color(0.8f, 0.8f, 0.8f);
        valueText.alignment = TextAlignmentOptions.Right;
        
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        
        RectTransform sliderObjRect = sliderObj.GetComponent<RectTransform>();
        sliderObjRect.sizeDelta = new Vector2(0, sliderHeight * 2.2f);
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(0, sliderHeight * 0.6f);
        
        RectTransform sliderControlRect = sliderControl.GetComponent<RectTransform>();
        sliderControlRect.sizeDelta = new Vector2(0, sliderHeight * 0.8f);
        
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0, 0.5f);
        backgroundRect.anchorMax = new Vector2(1, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(0, sliderHeight * 0.2f);
        
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.5f);
        fillAreaRect.anchorMax = new Vector2(1, 0.5f);
        fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
        fillAreaRect.sizeDelta = new Vector2(-20, sliderHeight * 0.2f);
        fillAreaRect.anchoredPosition = new Vector2(-5, 0);
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.sizeDelta = new Vector2(10, 0);
        
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0, 0.5f);
        handleAreaRect.anchorMax = new Vector2(1, 0.5f);
        handleAreaRect.pivot = new Vector2(0.5f, 0.5f);
        handleAreaRect.sizeDelta = new Vector2(-20, sliderHeight * 0.8f);
        handleAreaRect.anchoredPosition = Vector2.zero;
        
        RectTransform handleRect = handle.GetComponent<RectTransform>();
                handleRect.anchorMin = new Vector2(0, 0.5f);
        handleRect.anchorMax = new Vector2(0, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(sliderHeight * 0.6f, sliderHeight * 0.6f);
        
        RectTransform valueTextRect = valueTextObj.GetComponent<RectTransform>();
        valueTextRect.sizeDelta = new Vector2(0, sliderHeight * 0.6f);
        
        return slider;
    }
    
    private Button CreateButton(Transform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObj = new GameObject("Button_" + text, typeof(RectTransform));
        buttonObj.transform.SetParent(parent, false);
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        button.targetGraphic = buttonImage;
        
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = baseFontSize * 1.1f;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0, buttonHeight);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.selectedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        button.colors = colors;
        
        return button;
    }
    
    private void CreateCloseButton(Transform parent)
    {
        GameObject closeButton = new GameObject("CloseButton", typeof(RectTransform));
        closeButton.transform.SetParent(parent, false);
        
        Image buttonImage = closeButton.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        
        Button button = closeButton.AddComponent<Button>();
        button.onClick.AddListener(ToggleDebugPanel);
        button.targetGraphic = buttonImage;
        
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(closeButton.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "X";
        buttonText.fontSize = baseFontSize * 1.5f;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        RectTransform buttonRect = closeButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1, 1);
        buttonRect.anchorMax = new Vector2(1, 1);
        buttonRect.pivot = new Vector2(1, 1);
        buttonRect.sizeDelta = new Vector2(buttonHeight * 0.8f, buttonHeight * 0.8f);
        buttonRect.anchoredPosition = new Vector2(0, 0);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }
    
    private void CreateInfoSection(Transform parent)
    {
        CreateSectionHeader(parent, "GAME INFO");
        
        GameObject infoPanel = CreatePanel(parent, "InfoPanel");
        
        VerticalLayoutGroup layoutGroup = infoPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        
        scoreText = CreateLabel(infoPanel.transform, "Score: 0", buttonHeight * 0.5f);
        doorPriceText = CreateLabel(infoPanel.transform, "Door Price: 0", buttonHeight * 0.5f);
        roomsBuiltText = CreateLabel(infoPanel.transform, "Rooms Built: 0", buttonHeight * 0.5f);
        availableTilesText = CreateLabel(infoPanel.transform, "Available Tiles: 0", buttonHeight * 0.5f);
        
        RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, buttonHeight * 2.5f);
    }
    
    private void CreateGrowthFactorControls(Transform parent)
    {
        CreateSectionHeader(parent, "PRICE GROWTH SETTINGS");
        
        GameObject growthPanel = CreatePanel(parent, "GrowthPanel");
        
        VerticalLayoutGroup layoutGroup = growthPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        
        exponentialGrowthToggle = CreateToggle(growthPanel.transform, "Use Exponential Growth", 
                                              GetExponentialGrowthSetting(), ToggleExponentialGrowth);
        
        growthFactorSlider = CreateSlider(growthPanel.transform, "Growth Factor", 1.0f, 2.0f, 
                                         GetGrowthFactor(), UpdateGrowthFactor, out growthFactorText);
        growthFactorText.text = $"Growth Factor: {GetGrowthFactor():F2}";
        
        RectTransform panelRect = growthPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, toggleHeight + sliderHeight * 2.2f + elementSpacing * 2 + panelPadding * 2);
    }
    
    private void CreateScoreControls(Transform parent)
    {
        CreateSectionHeader(parent, "SCORE CONTROLS");
        
        GameObject scorePanel = CreatePanel(parent, "ScorePanel");
        
        VerticalLayoutGroup layoutGroup = scorePanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        
        scoreSlider = CreateSlider(scorePanel.transform, "Score to Add", 10, 1000, 
                                  scoreToAdd, UpdateScoreSlider, out scoreValueText);
        scoreValueText.text = $"Add Score: {scoreToAdd}";
        
        addScoreButton = CreateButton(scorePanel.transform, "Add Score", AddScore);
        
        RectTransform panelRect = scorePanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, sliderHeight * 2.2f + buttonHeight + elementSpacing * 2 + panelPadding * 2);
    }
    
    private void CreateMoneyControls(Transform parent)
    {
        CreateSectionHeader(parent, "MONEY CONTROLS");
        
        GameObject moneyPanel = CreatePanel(parent, "MoneyPanel");
        
        VerticalLayoutGroup layoutGroup = moneyPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        
        moneySlider = CreateSlider(moneyPanel.transform, "Money to Add", 10, 10000, 
                                  moneyToAdd, UpdateMoneySlider, out moneyValueText);
        moneyValueText.text = $"Add Money: {moneyToAdd}";
        
        addMoneyButton = CreateButton(moneyPanel.transform, "Add Money", AddMoney);
        
        RectTransform panelRect = moneyPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, sliderHeight * 2.2f + buttonHeight + elementSpacing * 2 + panelPadding * 2);
    }

    private void CreateShopControls(Transform parent)
    {
        CreateSectionHeader(parent, "SHOP CONTROLS");
        
        GameObject shopPanel = CreatePanel(parent, "ShopPanel");
        
        VerticalLayoutGroup layoutGroup = shopPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        
        Slider shopProbabilitySlider = CreateSlider(shopPanel.transform, "Shop Spawn Probability", 0f, 1f, 
                                                GetShopSpawnProbability(), UpdateShopProbabilitySlider, out TextMeshProUGUI shopProbabilityText);
        shopProbabilityText.text = $"Probability: {GetShopSpawnProbability():P0}";
        
        Toggle allowShopsInCornersToggle = CreateToggle(shopPanel.transform, "Allow Shops in Corners", 
                                                    GetAllowShopsInCorners(), ToggleAllowShopsInCorners);
        
        RectTransform panelRect = shopPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, sliderHeight * 2.2f + toggleHeight + elementSpacing * 2 + panelPadding * 2);
    }

    private float GetShopSpawnProbability()
    {
        if (House.instance == null) return 0.15f;
        
        RoomGenerator generator = House.instance.GetRoomGenerator();
        if (generator != null)
        {
            var field = typeof(RoomGenerator).GetField("baseShopProbability", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (field != null)
                return (float)field.GetValue(generator);
        }
        
        return 0.15f;
    }

    private void UpdateShopProbabilitySlider(float value)
    {
        if (House.instance == null) return;
        
        House.instance.SetShopSpawnProbability(value);
        
        var textComponents = GameObject.FindObjectsOfType<TextMeshProUGUI>();
        foreach (var text in textComponents)
        {
            if (text.gameObject.name == "Value" && text.transform.parent.name.Contains("Shop"))
            {
                text.text = $"Probability: {value:P0}";
                break;
            }
        }
    }

    private bool GetAllowShopsInCorners()
    {
        if (House.instance == null) return false;
        
        RoomGenerator generator = House.instance.GetRoomGenerator();
        if (generator != null)
        {
            var field = typeof(RoomGenerator).GetField("allowShopsInCorners", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (field != null)
                return (bool)field.GetValue(generator);
        }
        
        return false;
    }

    private void ToggleAllowShopsInCorners(bool isOn)
    {
        if (House.instance == null) return;
        
        House.instance.SetAllowShopsInCorners(isOn);
    }
    
    private void CreateDoorPriceControls(Transform parent)
    {
        CreateSectionHeader(parent, "DOOR PRICE CONTROLS");

        GameObject doorPricePanel = CreatePanel(parent, "DoorPricePanel");

        VerticalLayoutGroup layoutGroup = doorPricePanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding),
            Mathf.RoundToInt(panelPadding),
            Mathf.RoundToInt(panelPadding),
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;

        maxDoorPriceSlider = CreateSlider(doorPricePanel.transform, "Max Door Price", 0, 10000,
                                         maxDoorPrice, UpdateMaxDoorPriceSlider, out maxDoorPriceValueText);
        maxDoorPriceValueText.text = $"Max Door Price: {maxDoorPrice} (0 = unlimited)";

        setMaxDoorPriceButton = CreateButton(doorPricePanel.transform, "Set Max Door Price", SetMaxDoorPrice);

        RectTransform panelRect = doorPricePanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, sliderHeight * 2.2f + buttonHeight + elementSpacing * 2 + panelPadding * 2);
    }
    
    private void CreateRoomControls(Transform parent)
    {
        CreateSectionHeader(parent, "ROOM CONTROLS");
        
        GameObject roomPanel = CreatePanel(parent, "RoomPanel");
        
        VerticalLayoutGroup layoutGroup = roomPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        
        addRoomButton = CreateButton(roomPanel.transform, "Add Free Room", AddFreeRoom);
        
        clearRoomButton = CreateButton(roomPanel.transform, "Clear Current Room", ClearCurrentRoom);
        
        RectTransform panelRect = roomPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, buttonHeight * 2 + elementSpacing + panelPadding * 2);
    }
    
    private void CreateDifficultyControls(Transform parent)
    {
        CreateSectionHeader(parent, "DIFFICULTY CONTROLS");
        
        GameObject difficultyPanel = CreatePanel(parent, "DifficultyPanel");
        
        VerticalLayoutGroup layoutGroup = difficultyPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding), 
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        
        difficultyInfoText = CreateLabel(difficultyPanel.transform, "Difficulty: Disabled", buttonHeight * 0.5f);
        
        difficultySystemToggle = CreateToggle(difficultyPanel.transform, "Enable Difficulty System", 
                                            GetDifficultySystemEnabled(), ToggleDifficultySystem);
        
        difficultyTierSlider = CreateSlider(difficultyPanel.transform, "Manual Difficulty Tier", 0, 10, 
                                        GetCurrentDifficultyTier(), UpdateDifficultyTier, out difficultyTierText);
        difficultyTierText.text = $"Tier: {GetCurrentDifficultyTier()}";
        
        resetDifficultyButton = CreateButton(difficultyPanel.transform, "Reset Difficulty", ResetDifficulty);
        
        RectTransform panelRect = difficultyPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, buttonHeight * 1.5f + toggleHeight + sliderHeight * 2.2f + buttonHeight + elementSpacing * 3 + panelPadding * 2);
    }

    private bool GetDifficultySystemEnabled()
    {
        if (House.instance == null) return false;
        
        var field = typeof(House).GetField("useDifficultySystem", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
            return (bool)field.GetValue(House.instance);
            
        return false;
    }

    private int GetCurrentDifficultyTier()
    {
        if (House.instance == null) return 0;
        return House.instance.CurrentDifficultyTier;
    }

    private void ToggleDifficultySystem(bool isOn)
    {
        if (House.instance == null) return;
        House.instance.SetDifficultySystem(isOn);
    }

    private void UpdateDifficultyTier(float value)
    {
        int tier = Mathf.RoundToInt(value);
        if (House.instance != null)
        {
            House.instance.SetDifficultyTier(tier);
        }
        
        if (difficultyTierText != null)
        {
            difficultyTierText.text = $"Tier: {tier}";
        }
    }

    private void ResetDifficulty()
    {
        if (House.instance == null) return;
        House.instance.ResetDifficulty();
        
        if (difficultyTierSlider != null)
        {
            difficultyTierSlider.value = 0;
        }
    }

    private void CreateConsole(Transform parent)
    {
        CreateSectionHeader(parent, "CONSOLE");

        GameObject consolePanel = CreatePanel(parent, "ConsolePanel");

        VerticalLayoutGroup layoutGroup = consolePanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding),
            Mathf.RoundToInt(panelPadding),
            Mathf.RoundToInt(panelPadding),
            Mathf.RoundToInt(panelPadding)
        );
        layoutGroup.spacing = elementSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;

        GameObject controlsObj = new GameObject("ConsoleControls", typeof(RectTransform));
        controlsObj.transform.SetParent(consolePanel.transform, false);

        HorizontalLayoutGroup controlsLayout = controlsObj.AddComponent<HorizontalLayoutGroup>();
        controlsLayout.spacing = elementSpacing;
        controlsLayout.childAlignment = TextAnchor.MiddleLeft;
        controlsLayout.childControlWidth = false;
        controlsLayout.childForceExpandWidth = false;

        RectTransform controlsRect = controlsObj.GetComponent<RectTransform>();
        controlsRect.sizeDelta = new Vector2(0, buttonHeight * 0.8f);

        Toggle logsToggle = CreateConsoleToggle(controlsObj.transform, "Logs", true, Color.white, ToggleLogMessages);
        Toggle warningsToggle = CreateConsoleToggle(controlsObj.transform, "Warnings", true, new Color(1f, 0.8f, 0.2f), ToggleWarningMessages);
        Toggle errorsToggle = CreateConsoleToggle(controlsObj.transform, "Errors", true, new Color(1f, 0.3f, 0.3f), ToggleErrorMessages);

        GameObject spacerObj = new GameObject("Spacer", typeof(RectTransform));
        spacerObj.transform.SetParent(controlsObj.transform, false);
        LayoutElement spacer = spacerObj.AddComponent<LayoutElement>();
        spacer.flexibleWidth = 1;

        GameObject clearButtonObj = new GameObject("ClearButton", typeof(RectTransform));
        clearButtonObj.transform.SetParent(controlsObj.transform, false);

        Image clearButtonImage = clearButtonObj.AddComponent<Image>();
        clearButtonImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        Button clearButton = clearButtonObj.AddComponent<Button>();
        clearButton.onClick.AddListener(ClearConsole);
        clearButton.targetGraphic = clearButtonImage;

        ColorBlock clearColors = clearButton.colors;
        clearColors.normalColor = new Color(0.4f, 0.2f, 0.2f, 1f);
        clearColors.highlightedColor = new Color(0.5f, 0.3f, 0.3f, 1f);
        clearColors.pressedColor = new Color(0.3f, 0.1f, 0.1f, 1f);
        clearButton.colors = clearColors;

        GameObject clearTextObj = new GameObject("Text", typeof(RectTransform));
        clearTextObj.transform.SetParent(clearButtonObj.transform, false);

        TextMeshProUGUI clearButtonText = clearTextObj.AddComponent<TextMeshProUGUI>();
        clearButtonText.text = "Clear";
        clearButtonText.fontSize = baseFontSize * 0.9f;
        clearButtonText.color = Color.white;
        clearButtonText.alignment = TextAlignmentOptions.Center;

        RectTransform clearButtonRect = clearButtonObj.GetComponent<RectTransform>();
        clearButtonRect.sizeDelta = new Vector2(buttonHeight * 1.5f, buttonHeight * 0.8f);

        RectTransform clearTextRect = clearTextObj.GetComponent<RectTransform>();
        clearTextRect.anchorMin = Vector2.zero;
        clearTextRect.anchorMax = Vector2.one;
        clearTextRect.sizeDelta = Vector2.zero;

        GameObject consoleOutputObj = new GameObject("ConsoleOutput", typeof(RectTransform));
        consoleOutputObj.transform.SetParent(consolePanel.transform, false);

        Image outputBgImage = consoleOutputObj.AddComponent<Image>();
        outputBgImage.color = new Color(0.08f, 0.08f, 0.08f, 1f);

        GameObject scrollViewObj = CreateScrollView(consoleOutputObj.transform);
        Transform contentTransform = scrollViewObj.transform.Find("Viewport/Content");

        VerticalLayoutGroup contentLayout = contentTransform.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(
            Mathf.RoundToInt(panelPadding / 2),
            Mathf.RoundToInt(panelPadding / 2),
            Mathf.RoundToInt(panelPadding / 2),
            Mathf.RoundToInt(panelPadding / 2)
        );
        contentLayout.spacing = elementSpacing / 2;
        contentLayout.childAlignment = TextAnchor.UpperLeft;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandHeight = false;

        ContentSizeFitter contentSizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        consoleContent = contentTransform;
        scrollRect = scrollViewObj.GetComponent<ScrollRect>();

        RectTransform outputRect = consoleOutputObj.GetComponent<RectTransform>();
        outputRect.sizeDelta = new Vector2(0, buttonHeight * 5);

        RectTransform panelRect = consolePanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, buttonHeight * 6 + elementSpacing * 2 + panelPadding * 2);

        Application.logMessageReceived += HandleLog;
    }
    
    private Toggle CreateConsoleToggle(Transform parent, string label, bool isOn, Color textColor, UnityEngine.Events.UnityAction<bool> onValueChanged)
    {
        GameObject toggleObj = new GameObject("Toggle_" + label, typeof(RectTransform));
        toggleObj.transform.SetParent(parent, false);
        
        HorizontalLayoutGroup layoutGroup = toggleObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.spacing = elementSpacing/2;
        
        GameObject toggleControl = new GameObject("ToggleControl", typeof(RectTransform));
        toggleControl.transform.SetParent(toggleObj.transform, false);
        
        Toggle toggle = toggleControl.AddComponent<Toggle>();
        toggle.isOn = isOn;
        toggle.onValueChanged.AddListener(onValueChanged);
        
        GameObject background = new GameObject("Background", typeof(RectTransform));
        background.transform.SetParent(toggleControl.transform, false);
        
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        
        GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform));
        checkmark.transform.SetParent(background.transform, false);
        
        Image checkmarkImage = checkmark.AddComponent<Image>();
        checkmarkImage.color = textColor;
        
        toggle.targetGraphic = backgroundImage;
        toggle.graphic = checkmarkImage;
        
        ColorBlock colors = toggle.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        toggle.colors = colors;
        
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(toggleObj.transform, false);
        
        TextMeshProUGUI text = labelObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = baseFontSize * 0.9f;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Left;
        text.fontStyle = FontStyles.Bold;
        
        RectTransform toggleObjRect = toggleObj.GetComponent<RectTransform>();
        toggleObjRect.sizeDelta = new Vector2(buttonHeight * 2f, buttonHeight * 0.8f);
        
        RectTransform toggleControlRect = toggleControl.GetComponent<RectTransform>();
        toggleControlRect.sizeDelta = new Vector2(buttonHeight * 0.5f, buttonHeight * 0.5f);
        
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.sizeDelta = new Vector2(buttonHeight * 0.4f, buttonHeight * 0.4f);
        
        RectTransform checkmarkRect = checkmark.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkmarkRect.pivot = new Vector2(0.5f, 0.5f);
        checkmarkRect.sizeDelta = new Vector2(buttonHeight * 0.25f, buttonHeight * 0.25f);
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(buttonHeight * 1.5f, buttonHeight * 0.8f);
        
        return toggle;
    }
    
    private void CreateLogEntry(string message, string stackTrace, LogType type)
    {
        if (consoleContent == null) return;
        
        if ((type == LogType.Log && !showLogs) ||
            (type == LogType.Warning && !showWarnings) ||
            (type == LogType.Error && !showErrors && type != LogType.Exception) ||
            (type == LogType.Exception && !showErrors))
        {
            return;
        }
        
        GameObject logEntryObj = new GameObject("LogEntry", typeof(RectTransform));
        logEntryObj.transform.SetParent(consoleContent, false);
        
        Image bgImage = logEntryObj.AddComponent<Image>();
        
        Color bgColor;
        Color textColor;
        
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                bgColor = new Color(0.25f, 0.08f, 0.08f, 0.9f);
                textColor = new Color(1f, 0.4f, 0.4f);
                break;
            case LogType.Warning:
                bgColor = new Color(0.25f, 0.22f, 0.08f, 0.9f);
                textColor = new Color(1f, 0.9f, 0.4f);
                break;
            default:
                bgColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
                textColor = new Color(0.9f, 0.9f, 0.9f);
                break;
        }
        
        bgImage.color = bgColor;
        
        VerticalLayoutGroup layoutGroup = logEntryObj.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(8, 8, 6, 6);
        layoutGroup.spacing = 4;
        layoutGroup.childAlignment = TextAnchor.UpperLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        
        GameObject messageObj = new GameObject("Message", typeof(RectTransform));
        messageObj.transform.SetParent(logEntryObj.transform, false);
        
        TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
        messageText.text = message;
        messageText.fontSize = baseFontSize * 0.85f;
        messageText.color = textColor;
        messageText.alignment = TextAlignmentOptions.Left;
        
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            messageText.fontStyle = FontStyles.Bold;
        }
        
        RectTransform messageRect = messageObj.GetComponent<RectTransform>();
        messageRect.sizeDelta = new Vector2(0, baseFontSize * 1.2f);
        
        if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert) && !string.IsNullOrEmpty(stackTrace))
        {
            GameObject stackTraceObj = new GameObject("StackTrace", typeof(RectTransform));
            stackTraceObj.transform.SetParent(logEntryObj.transform, false);
            
            TextMeshProUGUI stackTraceText = stackTraceObj.AddComponent<TextMeshProUGUI>();
            
            string truncatedStackTrace = stackTrace;
            if (stackTrace.Length > 300)
            {
                truncatedStackTrace = stackTrace.Substring(0, 300) + "...";
            }
            
            stackTraceText.text = truncatedStackTrace;
            stackTraceText.fontSize = baseFontSize * 0.7f;
            stackTraceText.color = new Color(0.7f, 0.7f, 0.7f);
            stackTraceText.alignment = TextAlignmentOptions.Left;
            
            RectTransform stackTraceRect = stackTraceObj.GetComponent<RectTransform>();
            stackTraceRect.sizeDelta = new Vector2(0, baseFontSize * 0.7f * 3);
        }
        
        float entryHeight = baseFontSize * 1.5f;
        if ((type == LogType.Error || type == LogType.Exception || type == LogType.Assert) && !string.IsNullOrEmpty(stackTrace))
        {
            entryHeight += baseFontSize * 2.5f;
        }
        
        RectTransform logEntryRect = logEntryObj.GetComponent<RectTransform>();
        logEntryRect.sizeDelta = new Vector2(0, entryHeight);
        
        logEntries.Add(logEntryObj);
        
        if (logEntries.Count > maxLogEntries)
        {
            GameObject oldestEntry = logEntries[0];
            logEntries.RemoveAt(0);
            Destroy(oldestEntry);
        }
        
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logQueue.Enqueue(new LogEntry { message = logString, stackTrace = stackTrace, type = type });
    }

    private void Update()
    {
        if (isDebugEnabled)
        {
            UpdateDebugInfo();
            
            while (logQueue.Count > 0)
            {
                LogEntry entry = logQueue.Dequeue();
                CreateLogEntry(entry.message, entry.stackTrace, entry.type);
            }
        }
    }

    private void ToggleLogMessages(bool show)
    {
        showLogs = show;
        RefreshConsole();
    }
    
    private void ToggleWarningMessages(bool show)
    {
        showWarnings = show;
        RefreshConsole();
    }
    
    private void ToggleErrorMessages(bool show)
    {
        showErrors = show;
        RefreshConsole();
    }
    
    private void ClearConsole()
    {
        foreach (GameObject entry in logEntries)
        {
            Destroy(entry);
        }
        logEntries.Clear();
    }

    private void RefreshConsole()
    {
        List<LogEntry> currentLogs = new List<LogEntry>();
        foreach (GameObject entry in logEntries)
        {
            string message = "";
            string stackTrace = "";
            LogType type = LogType.Log;
            
            Transform messageTransform = entry.transform.Find("Message");
            if (messageTransform != null)
            {
                TextMeshProUGUI messageText = messageTransform.GetComponent<TextMeshProUGUI>();
                if (messageText != null)
                {
                    message = messageText.text;
                }
            }
            
            Transform stackTraceTransform = entry.transform.Find("StackTrace");
            if (stackTraceTransform != null)
            {
                TextMeshProUGUI stackTraceText = stackTraceTransform.GetComponent<TextMeshProUGUI>();
                if (stackTraceText != null)
                {
                    stackTrace = stackTraceText.text;
                }
            }
            
            Image bgImage = entry.GetComponent<Image>();
            if (bgImage != null)
            {
                if (bgImage.color.r > bgImage.color.g)
                {
                    type = LogType.Error;
                }
                else if (Mathf.Approximately(bgImage.color.r, bgImage.color.g) && bgImage.color.r > bgImage.color.b)
                {
                    type = LogType.Warning;
                }
                else
                {
                    type = LogType.Log;
                }
            }
            
            currentLogs.Add(new LogEntry { message = message, stackTrace = stackTrace, type = type });
        }
        
        ClearConsole();
        
        foreach (LogEntry entry in currentLogs)
        {
            CreateLogEntry(entry.message, entry.stackTrace, entry.type);
        }
    }
    
    private void UpdateScoreSlider(float value)
    {
        scoreToAdd = Mathf.RoundToInt(value);
        if (scoreValueText != null)
        {
            scoreValueText.text = $"Add Score: {scoreToAdd}";
        }
    }
    
    private void UpdateMoneySlider(float value)
    {
        moneyToAdd = Mathf.RoundToInt(value);
        if (moneyValueText != null)
        {
            moneyValueText.text = $"Add Money: {moneyToAdd}";
        }
    }
    
    private void UpdateMaxDoorPriceSlider(float value)
    {
        maxDoorPrice = Mathf.RoundToInt(value);
        if (maxDoorPriceValueText != null)
        {
            maxDoorPriceValueText.text = $"Max Door Price: {maxDoorPrice}" + (maxDoorPrice == 0 ? " (unlimited)" : "");
        }
    }
    
    public void ToggleDebugPanel()
    {
        if (!isDebugUnlocked) return;
        
        isDebugEnabled = !isDebugEnabled;
        debugPanel.SetActive(isDebugEnabled);
        
        if (isDebugEnabled)
        {
            UpdateDebugInfo();
        }
    }

    public void UpdateDebugInfo()
    {
        if (!isDebugEnabled || House.instance == null) return;

        scoreText.text = $"Score: {House.instance.Score}";
        doorPriceText.text = $"Door Price: {House.instance.DoorPrice}";
        roomsBuiltText.text = $"Rooms Built: {GetRoomsBuilt()}";

        if (growthFactorText != null)
        {
            growthFactorText.text = $"Growth Factor: {GetGrowthFactor():F2}";
        }

        if (availableTilesText != null && MainRoom.instance != null)
        {
            availableTilesText.text = $"Available Tiles: {MainRoom.instance.availableTiles}";
        }
        
        if (difficultyInfoText != null)
        {
            difficultyInfoText.text = House.instance.GetDifficultyInfo();
        }
    }
    
    public void AddScore()
    {
        if (House.instance == null) return;
        
        House.instance.UpdateScore(scoreToAdd);
        UpdateDebugInfo();
    }
    
    public void AddMoney()
    {
        if (PlayerController.instance == null) return;
        
        PlayerController.instance.Inventory.UpdateMoney(moneyToAdd);
        UpdateDebugInfo();
    }
    
    public void SetMaxDoorPrice()
    {
        if (House.instance == null) return;
        
        SetMaxDoorPriceValue(maxDoorPrice);
        UpdateDebugInfo();
    }
    
    private void AddFreeRoom()
    {
        if (House.instance == null || PlayerController.instance == null) return;
        
        Room currentRoom = House.instance.currentRoom;
        if (currentRoom == null) return;
        
        DoorData[] doors = currentRoom.GetComponentsInChildren<DoorData>();
        foreach (DoorData door in doors)
        {
            if (!door.isUnlocked)
            {
                door.BuyNextRoom(true);
                break;
            }
        }
        
        UpdateDebugInfo();
    }
    
    private void ClearCurrentRoom()
    {
        if (House.instance == null || House.instance.currentRoom == null) return;
        
        Room currentRoom = House.instance.currentRoom;
        RoomFurnitures roomFurnitures = currentRoom.roomFurnitures;
        
        if (roomFurnitures == null) return;
        
        List<FurnitureObjectBase> furnitureToRemove = new List<FurnitureObjectBase>();
        
        foreach (var placementData in roomFurnitures.PlacementDatasInPosition.Values)
        {
            if (placementData.instantiatedFurniture != null && !furnitureToRemove.Contains(placementData.instantiatedFurniture))
            {
                furnitureToRemove.Add(placementData.instantiatedFurniture);
            }

            for (int i = 0; i < placementData.topPlacementDatas.Count; i++)
            {
                if (placementData.topPlacementDatas[i].instantiatedFurnitureOnTop != null && !furnitureToRemove.Contains(placementData.topPlacementDatas[i].instantiatedFurnitureOnTop))
                {
                    furnitureToRemove.Add(placementData.topPlacementDatas[i].instantiatedFurnitureOnTop);
                }
            }
            
        }
        
        foreach (var furniture in furnitureToRemove)
        {
            Destroy(furniture.gameObject);
        }
        
        roomFurnitures.PlacementDatasInPosition.Clear();
        
        if (currentRoom is MainRoom)
        {
            MainRoom mainRoom = currentRoom as MainRoom;
            mainRoom.availableTiles = 54;
        }
        
        UpdateDebugInfo();
    }
    
    private void ToggleExponentialGrowth(bool isOn)
    {
        SetExponentialGrowthSetting(isOn);
        UpdateDebugInfo();
    }
    
    private void UpdateGrowthFactor(float value)
    {
        SetGrowthFactor(value);
        if (growthFactorText != null)
        {
            growthFactorText.text = $"Growth Factor: {value:F2}";
        }
    }
    
    private bool GetExponentialGrowthSetting()
    {
        if (House.instance == null) return true;
        
        var field = typeof(House).GetField("useExponentialGrowth", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
            return (bool)field.GetValue(House.instance);
            
        return true;
    }
    
    private void SetExponentialGrowthSetting(bool value)
    {
        if (House.instance == null) return;
        
        var field = typeof(House).GetField("useExponentialGrowth", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
            field.SetValue(House.instance, value);
    }
    
    private float GetGrowthFactor()
    {
        if (House.instance == null) return 1.2f;
        
        var field = typeof(House).GetField("priceGrowthFactor", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
            return (float)field.GetValue(House.instance);
            
        return 1.2f;
    }
    
    private void SetGrowthFactor(float value)
    {
        if (House.instance == null) return;
        
        var field = typeof(House).GetField("priceGrowthFactor", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
            field.SetValue(House.instance, value);
    }
    
    private int GetRoomsBuilt()
    {
        if (House.instance == null) return 0;
        
        var field = typeof(House).GetField("roomsBuilt", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
            return (int)field.GetValue(House.instance);
            
        return 0;
    }
    
    private void SetMaxDoorPriceValue(int value)
    {
        if (House.instance == null) return;
        
        var field = typeof(House).GetField("maxDoorPrice", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (field != null)
            field.SetValue(House.instance, value);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private struct LogEntry
    {
        public string message;
        public string stackTrace;
        public LogType type;
    }
}