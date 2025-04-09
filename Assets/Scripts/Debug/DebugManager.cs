using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;

    // Reference to the debug panel prefab (can be minimal - just an empty panel)
    [SerializeField] private GameObject debugPanelPrefab;
    
    // Main components
    private GameObject debugPanel;
    private GameObject debugButton;

    // UI Scaling settings
    [Header("UI Scaling")]
    [SerializeField] [Range(0.5f, 2.0f)] private float uiScaleFactor = 1.0f;
    [SerializeField] private bool autoScaleWithScreen = true;
    
    // UI scaling factors
    private float baseFontSize = 18f;
    private float buttonHeight = 80f;
    private float sliderHeight = 60f;
    private float toggleHeight = 60f;
    private float sectionSpacing = 25f;
    private float elementSpacing = 15f;
    private float panelPadding = 20f;
    
    // UI References
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
    
    private Button addRoomButton;
    private Button clearRoomButton;
    
    // Values for sliders
    private int scoreToAdd = 100;
    private int moneyToAdd = 100;
    private int maxDoorPrice = 1000;
    
    private bool isDebugEnabled = false;

    private void Awake()
    {
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
        
        // Adjust UI scaling based on screen resolution
        AdjustUIScaling();
    }
    
    private void Start()
    {
        // Create the debug panel but keep it hidden
        CreateDebugPanel();
        debugPanel.SetActive(false);
    }
    
    private void AdjustUIScaling()
    {
        float scaleFactor = uiScaleFactor; // Start with editor-defined scale
        
        if (autoScaleWithScreen)
        {
            // Get screen dimensions
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            // Calculate scaling factor based on screen resolution
            // Use the smaller dimension to ensure everything fits
            float screenScaleFactor = Mathf.Min(screenWidth, screenHeight) / 1080f; // Base on 1080p reference
            
            // Combine editor scale with screen-based scale
            scaleFactor *= screenScaleFactor;
        }
        
        // Apply scaling to UI elements (with minimum values to prevent too small UI)
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
        // Create canvas for the debug panel if needed
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DebugCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add canvas scaler for proper scaling on different devices
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // Reference resolution for mobile
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f; // Balance between width and height
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create debug panel
        debugPanel = new GameObject("DebugPanel");
        debugPanel.transform.SetParent(canvas.transform, false);
        
        // Add image component for background
        Image panelImage = debugPanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        
        // Set panel size and position - make it fill most of the screen on mobile
        RectTransform panelRect = debugPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.05f);
        panelRect.anchorMax = new Vector2(0.9f, 0.95f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = Vector2.zero; // Size determined by anchors
        
        // Add a scroll view for content
        GameObject scrollViewObj = CreateScrollView(debugPanel.transform);
        Transform contentTransform = scrollViewObj.transform.Find("Viewport/Content");
        
        // Add a vertical layout group to the content
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
        
        // Add content size fitter
        ContentSizeFitter sizeFitter = contentTransform.gameObject.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Create title
        CreateTitle(contentTransform, "DEBUG MENU");
        
        // Create info section
        CreateInfoSection(contentTransform);
        
        // Create growth factor controls
        CreateGrowthFactorControls(contentTransform);
        
        // Create score controls
        CreateScoreControls(contentTransform);
        
        // Create money controls
        CreateMoneyControls(contentTransform);
        
        // Create door price controls
        CreateDoorPriceControls(contentTransform);
        
        // Create room controls
        CreateRoomControls(contentTransform);
        
        // Create close button
        CreateCloseButton(debugPanel.transform);
    }
    
    private GameObject CreateScrollView(Transform parent)
    {
        // Create scroll view
        GameObject scrollView = new GameObject("ScrollView", typeof(RectTransform));
        scrollView.transform.SetParent(parent, false);
        
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        
        // Create viewport
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
        viewport.transform.SetParent(scrollView.transform, false);
        
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0.1f);
        
        // Add mask
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        
        // Create content
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        
        // Setup scroll rect
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.content = content.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f; // Slower deceleration for better control
        
        // Setup RectTransforms
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
        contentRectTransform.sizeDelta = new Vector2(0, 1500); // Initial height, will be adjusted by content size fitter
        
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
        
        // Add rounded corners with a 9-slice sprite if available
        // If you have a rounded rect sprite, you can assign it here
        // image.sprite = yourRoundedRectSprite;
        // image.type = Image.Type.Sliced;
        
                RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0, 0); // Will be sized by content
        
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
        
        // Create horizontal layout
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
        
        // Create toggle
        GameObject toggleControl = new GameObject("ToggleControl", typeof(RectTransform));
        toggleControl.transform.SetParent(toggleObj.transform, false);
        
        Toggle toggle = toggleControl.AddComponent<Toggle>();
        toggle.isOn = isOn;
        toggle.onValueChanged.AddListener(onValueChanged);
        
        // Create background
        GameObject background = new GameObject("Background", typeof(RectTransform));
        background.transform.SetParent(toggleControl.transform, false);
        
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = Color.white;
        
        // Create checkmark
        GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform));
        checkmark.transform.SetParent(background.transform, false);
        
        Image checkmarkImage = checkmark.AddComponent<Image>();
        checkmarkImage.color = Color.green;
        
        // Setup toggle
        toggle.targetGraphic = backgroundImage;
        toggle.graphic = checkmarkImage;
        
        // Create label
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(toggleObj.transform, false);
        
        TextMeshProUGUI text = labelObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = baseFontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        
        // Setup RectTransforms
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
        
        // Create vertical layout
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
        
        // Create label
        GameObject labelObj = new GameObject("Label", typeof(RectTransform));
        labelObj.transform.SetParent(sliderObj.transform, false);
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = baseFontSize;
        labelText.color = Color.white;
        labelText.alignment = TextAlignmentOptions.Left;
        
        // Create slider control
        GameObject sliderControl = new GameObject("SliderControl", typeof(RectTransform));
        sliderControl.transform.SetParent(sliderObj.transform, false);
        
        Slider slider = sliderControl.AddComponent<Slider>();
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.value = value;
        slider.onValueChanged.AddListener(onValueChanged);
        
        // Create background
        GameObject background = new GameObject("Background", typeof(RectTransform));
        background.transform.SetParent(sliderControl.transform, false);
        
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        // Create fill area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderControl.transform, false);
        
        // Create fill
        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.7f, 0.2f, 1f);
        
        // Create handle area
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderControl.transform, false);
        
        // Create handle
        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(1f, 1f, 1f, 1f);
        
        // Create value text
        GameObject valueTextObj = new GameObject("Value", typeof(RectTransform));
        valueTextObj.transform.SetParent(sliderObj.transform, false);
        
        valueText = valueTextObj.AddComponent<TextMeshProUGUI>();
        valueText.fontSize = baseFontSize * 0.9f;
        valueText.color = new Color(0.8f, 0.8f, 0.8f);
        valueText.alignment = TextAlignmentOptions.Right;
        
        // Setup slider
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.handleRect = handle.GetComponent<RectTransform>();
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        
        // Setup RectTransforms
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
        
        // Create text
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.fontSize = baseFontSize * 1.1f;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        // Setup RectTransforms
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0, buttonHeight);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        // Add color transition
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
        
        // Create text
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(closeButton.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
                buttonText.text = "X";
        buttonText.fontSize = baseFontSize * 1.5f;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAlignmentOptions.Center;
        
        // Setup RectTransforms
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
        
        // Add vertical layout
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
        
        // Create info labels
        scoreText = CreateLabel(infoPanel.transform, "Score: 0", buttonHeight * 0.5f);
        doorPriceText = CreateLabel(infoPanel.transform, "Door Price: 0", buttonHeight * 0.5f);
        roomsBuiltText = CreateLabel(infoPanel.transform, "Rooms Built: 0", buttonHeight * 0.5f);
        availableTilesText = CreateLabel(infoPanel.transform, "Available Tiles: 0", buttonHeight * 0.5f);
        
        // Set panel height based on content
        RectTransform panelRect = infoPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, buttonHeight * 2.5f);
    }
    
    private void CreateGrowthFactorControls(Transform parent)
    {
        CreateSectionHeader(parent, "PRICE GROWTH SETTINGS");
        
        GameObject growthPanel = CreatePanel(parent, "GrowthPanel");
        
        // Add vertical layout
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
        
        // Create exponential growth toggle
        exponentialGrowthToggle = CreateToggle(growthPanel.transform, "Use Exponential Growth", 
                                              GetExponentialGrowthSetting(), ToggleExponentialGrowth);
        
        // Create growth factor slider
        growthFactorSlider = CreateSlider(growthPanel.transform, "Growth Factor", 1.0f, 2.0f, 
                                         GetGrowthFactor(), UpdateGrowthFactor, out growthFactorText);
        growthFactorText.text = $"Growth Factor: {GetGrowthFactor():F2}";
        
        // Set panel height based on content
        RectTransform panelRect = growthPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, toggleHeight + sliderHeight * 2.2f + elementSpacing * 2 + panelPadding * 2);
    }
    
    private void CreateScoreControls(Transform parent)
    {
        CreateSectionHeader(parent, "SCORE CONTROLS");
        
        GameObject scorePanel = CreatePanel(parent, "ScorePanel");
        
        // Add vertical layout
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
        
        // Create score slider
        scoreSlider = CreateSlider(scorePanel.transform, "Score to Add", 10, 1000, 
                                  scoreToAdd, UpdateScoreSlider, out scoreValueText);
        scoreValueText.text = $"Add Score: {scoreToAdd}";
        
        // Create add score button
        addScoreButton = CreateButton(scorePanel.transform, "Add Score", AddScore);
        
        // Set panel height based on content
        RectTransform panelRect = scorePanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, sliderHeight * 2.2f + buttonHeight + elementSpacing * 2 + panelPadding * 2);
    }
    
    private void CreateMoneyControls(Transform parent)
    {
        CreateSectionHeader(parent, "MONEY CONTROLS");
        
        GameObject moneyPanel = CreatePanel(parent, "MoneyPanel");
        
        // Add vertical layout
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
        
        // Create money slider
        moneySlider = CreateSlider(moneyPanel.transform, "Money to Add", 10, 10000, 
                                  moneyToAdd, UpdateMoneySlider, out moneyValueText);
        moneyValueText.text = $"Add Money: {moneyToAdd}";
        
        // Create add money button
        addMoneyButton = CreateButton(moneyPanel.transform, "Add Money", AddMoney);
        
        // Set panel height based on content
        RectTransform panelRect = moneyPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, sliderHeight * 2.2f + buttonHeight + elementSpacing * 2 + panelPadding * 2);
    }
    
    private void CreateDoorPriceControls(Transform parent)
    {
        CreateSectionHeader(parent, "DOOR PRICE CONTROLS");
        
        GameObject doorPricePanel = CreatePanel(parent, "DoorPricePanel");
        
        // Add vertical layout
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
        
        // Create max door price slider
        maxDoorPriceSlider = CreateSlider(doorPricePanel.transform, "Max Door Price", 0, 10000, 
                                         maxDoorPrice, UpdateMaxDoorPriceSlider, out maxDoorPriceValueText);
        maxDoorPriceValueText.text = $"Max Door Price: {maxDoorPrice} (0 = unlimited)";
        
        // Create set max door price button
        setMaxDoorPriceButton = CreateButton(doorPricePanel.transform, "Set Max Door Price", SetMaxDoorPrice);
        
        // Set panel height based on content
        RectTransform panelRect = doorPricePanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, sliderHeight * 2.2f + buttonHeight + elementSpacing * 2 + panelPadding * 2);
    }
    
    private void CreateRoomControls(Transform parent)
    {
        CreateSectionHeader(parent, "ROOM CONTROLS");
        
        GameObject roomPanel = CreatePanel(parent, "RoomPanel");
        
        // Add vertical layout
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
        
        // Create add room button
        addRoomButton = CreateButton(roomPanel.transform, "Add Free Room", AddFreeRoom);
        
        // Create clear room button
        clearRoomButton = CreateButton(roomPanel.transform, "Clear Current Room", ClearCurrentRoom);
        
        // Set panel height based on content
        RectTransform panelRect = roomPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(0, buttonHeight * 2 + elementSpacing + panelPadding * 2);
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
        
        // Update growth factor text
        if (growthFactorText != null)
        {
            growthFactorText.text = $"Growth Factor: {GetGrowthFactor():F2}";
        }
        
        // Update available tiles text if MainRoom exists
        if (availableTilesText != null && MainRoom.instance != null)
        {
            availableTilesText.text = $"Available Tiles: {MainRoom.instance.availableTiles}";
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
        
        // Get the current room
        Room currentRoom = House.instance.currentRoom;
        if (currentRoom == null) return;
        
        // Find an available door in the current room
        DoorData[] doors = currentRoom.GetComponentsInChildren<DoorData>();
        foreach (DoorData door in doors)
        {
            if (!door.isUnlocked)
            {
                // Simulate opening the door without cost
                door.BuyNextRoom(true); // Pass true to indicate it's a free door
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
        
        // Get all furniture objects in the room
        List<FurnitureObjectBase> furnitureToRemove = new List<FurnitureObjectBase>();
        
        // Collect all furniture objects
        foreach (var placementData in roomFurnitures.PlacementDatasInPosition.Values)
        {
            if (placementData.instantiatedFurniture != null && !furnitureToRemove.Contains(placementData.instantiatedFurniture))
            {
                furnitureToRemove.Add(placementData.instantiatedFurniture);
            }
            
                        if (placementData.instantiatedFurnitureOnTop != null && !furnitureToRemove.Contains(placementData.instantiatedFurnitureOnTop))
            {
                furnitureToRemove.Add(placementData.instantiatedFurnitureOnTop);
            }
        }
        
        // Destroy all furniture objects
        foreach (var furniture in furnitureToRemove)
        {
            Destroy(furniture.gameObject);
        }
        
        // Clear the placement data dictionary
        roomFurnitures.PlacementDatasInPosition.Clear();
        
        // Reset available tiles if it's the main room
        if (currentRoom is MainRoom)
        {
            MainRoom mainRoom = currentRoom as MainRoom;
            mainRoom.availableTiles = 54; // Reset to default value
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
    
    // Methods to interact with House settings via reflection (since they're private)
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
}



