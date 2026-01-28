using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Animator blackFade;
    [SerializeField] private SpriteRenderer controles;
    [SerializeField] private GameObject credits;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button leaderboardsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button CasualButton;
    [SerializeField] private Button ClassicButton;
    [SerializeField] private Button RogueButton;
    private Controls controls;
    private Button[] menuButtons;
    private Button[] gameModeButtons;
    private Button[] currentButtons;
    private int currentButtonIndex = 0;
    private int value = 0;
    bool IsAlreadyChangingScene;
    bool _fading;
    private bool controlsOpen = false;
    private bool creditsOpen = false;
    private bool gameModeMenuOpen = false;
    private HashSet<Button> shakingButtons = new HashSet<Button>();
    [SerializeField] private float creditsAutoReturnTime = 100f;
    private Coroutine creditsAutoReturnCoroutine;

    void Awake()
    {
        controls = new Controls();
        menuButtons = new Button[] { playButton, controlsButton, leaderboardsButton, creditsButton };
        gameModeButtons = new Button[] { CasualButton, ClassicButton, RogueButton };
        currentButtons = menuButtons;
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Movement.Movement.performed += OnNavigate;
        controls.Movement.Interact.performed += OnInteract;
        controls.Movement.Rotate.performed += OnBPressed;

        controls.Movement.Movement.performed += CheckKonamiDirection;
        controls.Movement.Interact.performed += CheckKonamiA;
        controls.Movement.Rotate.performed += CheckKonamiB;
    }

    void OnDisable()
    {
        controls.Movement.Movement.performed -= OnNavigate;
        controls.Movement.Interact.performed -= OnInteract;
        controls.Movement.Rotate.performed -= OnBPressed;

        controls.Movement.Movement.performed -= CheckKonamiDirection;
        controls.Movement.Interact.performed -= CheckKonamiA;
        controls.Movement.Rotate.performed -= CheckKonamiB;
        controls.Disable();
    }

    private List<string> inputSequence = new List<string>();
    private readonly string[] konamiCode = { "Up", "Up", "Down", "Down", "Left", "Right", "Left", "Right", "B", "A" };

    private void CheckKonamiDirection(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (input.y > 0.5f) AddInputToSequence("Up");
        else if (input.y < -0.5f) AddInputToSequence("Down");
        else if (input.x > 0.5f) AddInputToSequence("Right");
        else if (input.x < -0.5f) AddInputToSequence("Left");
    }

    private void CheckKonamiA(InputAction.CallbackContext context) => AddInputToSequence("A");
    private void CheckKonamiB(InputAction.CallbackContext context) => AddInputToSequence("B");

    private void AddInputToSequence(string input)
    {
        inputSequence.Add(input);
        
        if (inputSequence.Count > konamiCode.Length)
        {
            inputSequence.RemoveAt(0);
        }

        if (inputSequence.Count == konamiCode.Length)
        {
            bool match = true;
            for (int i = 0; i < konamiCode.Length; i++)
            {
                if (inputSequence[i] != konamiCode[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                ActivateRainbowMode();
                inputSequence.Clear();
            }
        }
    }

    private void ActivateRainbowMode()
    {
        Debug.Log("Easter Egg Activated: Rainbow Mode!");
        ColourChanger.GlobalRainbowMode = !ColourChanger.GlobalRainbowMode;
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
    }

    void Start()
    {
        menuPanel.SetActive(false);
        gameModePanel.SetActive(false);
        SetupButtons();
        SetupNavigation();
    }

    void Update()
    {
        if (Input.anyKey && !IsAlreadyChangingScene && !_fading && !menuPanel.activeInHierarchy && !gameModePanel.activeInHierarchy)
        {

            if (value < 2)
            {
                value++;
                switch (value)
                {
                    case 0:
                        ActivateControllerScreen(false);
                        break;

                    case 1:
                        ActivateControllerScreen(true);
                        AudioManager.instance.PlaySfx(GlobalSfx.Click);
                        break;
                    default:
                        AudioManager.instance.PlaySfx(GlobalSfx.Grab);
                        blackFade.SetTrigger("StartBlackFade");
                        Invoke("FadeToMenu", 0.5f);
                        break;
                }
            }
        }
    }

    private void OnBPressed(InputAction.CallbackContext context)
    {
        if (controlsOpen)
        {
            CloseControls();
            return;
        }

        if (creditsOpen)
        {
            CloseCredits();
            return;
        }

        if (gameModeMenuOpen)
        {
            BackToMainMenu();
            return;
        }
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if ((!menuPanel.activeInHierarchy && !gameModePanel.activeInHierarchy) || controlsOpen || creditsOpen || _fading) return;

        Vector2 input = context.ReadValue<Vector2>();

        if (input.y > 0.5f)
        {
            NavigateUp();
        }
        else if (input.y < -0.5f)
        {
            NavigateDown();
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if ((!menuPanel.activeInHierarchy && !gameModePanel.activeInHierarchy) || controlsOpen || creditsOpen || _fading) return;

        currentButtons[currentButtonIndex].onClick.Invoke();
    }

    private void NavigateUp()
    {
        currentButtonIndex = (currentButtonIndex - 1 + currentButtons.Length) % currentButtons.Length;
        UpdateButtonSelection();
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
    }

    private void NavigateDown()
    {
        currentButtonIndex = (currentButtonIndex + 1) % currentButtons.Length;
        UpdateButtonSelection();
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
    }

    private void UpdateButtonSelection()
    {
        currentButtonIndex = Mathf.Clamp(currentButtonIndex, 0, currentButtons.Length - 1);

        for (int i = 0; i < currentButtons.Length; i++)
        {
            Image buttonImage = currentButtons[i].GetComponent<Image>();

            if (i == currentButtonIndex)
            {
                if (buttonImage != null)
                {
                    Color imageColor = buttonImage.color;
                    imageColor.a = 1f;
                    buttonImage.color = imageColor;
                }
                currentButtons[i].Select();
            }
            else
            {
                if (buttonImage != null)
                {
                    Color imageColor = buttonImage.color;
                    imageColor.a = 0.5f;
                    buttonImage.color = imageColor;
                }
            }
        }
    }

    private void SetupButtons()
    {
        playButton.onClick.AddListener(ShowGameModeMenu);
        controlsButton.onClick.AddListener(ShowControls);
        leaderboardsButton.onClick.AddListener(ShowLeaderboards);
        creditsButton.onClick.AddListener(ShowCredits);

        CasualButton.onClick.AddListener(() => LoadScene(2));
        ClassicButton.onClick.AddListener(() => LoadScene(3));
        RogueButton.onClick.AddListener(() => ShakeButtonWithError(RogueButton));
    }

    private void ShakeButtonWithError(Button button)
    {
        if (shakingButtons.Contains(button)) return;

        AudioManager.instance.PlaySfx(GlobalSfx.Error);
        StartCoroutine(ShakeButton(button));
    }

    private IEnumerator ShakeButton(Button button)
    {
        shakingButtons.Add(button);
        Vector3 originalPosition = button.transform.localPosition;
        float shakeDuration = 0.3f;
        float shakeIntensity = 0.05f;
        float elapsed = 0f;

        try
        {
            while (elapsed < shakeDuration)
            {
                float x = originalPosition.x + Random.Range(-shakeIntensity, shakeIntensity);
                float y = originalPosition.y + Random.Range(-shakeIntensity, shakeIntensity);

                button.transform.localPosition = new Vector3(x, y, originalPosition.z);

                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        finally
        {
            if (button != null && button.transform != null)
            {
                button.transform.localPosition = originalPosition;
            }
            shakingButtons.Remove(button);
        }
    }

    private void SetupNavigation()
    {
        SetupButtonNavigation(menuButtons);
        SetupButtonNavigation(gameModeButtons);
    }

    private void SetupButtonNavigation(Button[] buttons)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            nav.selectOnUp = buttons[(i - 1 + buttons.Length) % buttons.Length];
            nav.selectOnDown = buttons[(i + 1) % buttons.Length];

            buttons[i].navigation = nav;
        }
    }

    private void FadeToMenu()
    {
        Invoke("ShowMainMenu", 0.5f);
        blackFade.SetTrigger("EndBlackFade");
        controles.enabled = false;
        mainMenuPanel.SetActive(false);
    }

    private void ShowMainMenu()
    {
        menuPanel.SetActive(true);
        gameModePanel.SetActive(false);
        currentButtons = menuButtons;
        currentButtonIndex = 0;
        gameModeMenuOpen = false;
        UpdateButtonSelection();
        playButton.Select();
    }

    private void ShowGameModeMenu()
    {
        StartCoroutine(GameModeMenuData());
    }

    private IEnumerator GameModeMenuData()
    {
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
        menuPanel.SetActive(false);
        gameModePanel.SetActive(true);
        currentButtons = gameModeButtons;
        currentButtonIndex = 0;
        gameModeMenuOpen = true;
        yield return null;
        UpdateButtonSelection();
        CasualButton.Select();
    }

    private void BackToMainMenu()
    {
        AudioManager.instance.PlaySfx(GlobalSfx.Grab);
        gameModePanel.SetActive(false);
        menuPanel.SetActive(true);
        currentButtons = menuButtons;
        currentButtonIndex = 0;
        gameModeMenuOpen = false;
        UpdateButtonSelection();
        playButton.Select();
    }

    private void LoadScene(int sceneIndex)
    {
        if (!IsAlreadyChangingScene)
        {
            IsAlreadyChangingScene = true;
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            blackFade.SetTrigger("StartBlackFade");
            StartCoroutine(LoadSceneAfterDelay(sceneIndex, 1.5f));
        }
    }

    private IEnumerator LoadSceneAfterDelay(int sceneIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneIndex);
    }

    #region Controls
    private void ShowControls()
    {
        StartCoroutine(ShowControlsCoroutine());
    }

    IEnumerator ShowControlsCoroutine()
    {
        yield return null;
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
        ActivateControllerScreen(true);
        menuPanel.SetActive(false);
        controlsOpen = true;
    }

    private void CloseControls()
    {
        AudioManager.instance.PlaySfx(GlobalSfx.Grab);
        ActivateControllerScreen(false);
        menuPanel.SetActive(true);
        controlsOpen = false;
        UpdateButtonSelection();
    }
    #endregion

    private void ShowLeaderboards()
    {
        PlayGamesManager.Instance.ShowLeaderboard();
    }

    #region Credits
    private void ShowCredits()
    {
        StartCoroutine(ShowCreditsCoroutine());
    }

    IEnumerator ShowCreditsCoroutine()
    {
        blackFade.SetTrigger("StartBlackFade");
        _fading = true;
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
        yield return new WaitForSeconds(1f);
        ActivateCreditsScreen(true);
        menuPanel.SetActive(false);
        blackFade.SetTrigger("EndBlackFade");
        yield return new WaitForSeconds(1f);
        _fading = false;
        creditsOpen = true;

        creditsAutoReturnCoroutine = StartCoroutine(AutoReturnFromCredits());
    }

    private void CloseCredits()
    {
        StartCoroutine(CloseCreditsCoroutine());
    }

    IEnumerator CloseCreditsCoroutine()
    {
        // Stop auto-return timer if it's running
        if (creditsAutoReturnCoroutine != null)
        {
            StopCoroutine(creditsAutoReturnCoroutine);
            creditsAutoReturnCoroutine = null;
        }

        blackFade.SetTrigger("StartBlackFade");
        _fading = true;
        AudioManager.instance.PlaySfx(GlobalSfx.Grab);
        yield return new WaitForSeconds(1f);
        ActivateCreditsScreen(false);
        menuPanel.SetActive(true);
        creditsOpen = false;
        blackFade.SetTrigger("EndBlackFade");
        yield return new WaitForSeconds(1f);
        _fading = false;
        UpdateButtonSelection();
    }

    private IEnumerator AutoReturnFromCredits()
    {
        yield return new WaitForSeconds(creditsAutoReturnTime);

        // Only auto-return if credits are still open
        if (creditsOpen)
        {
            CloseCredits();
        }
    }
    #endregion

    private void ActivateControllerScreen(bool setActive)
    {
        controles.enabled = setActive;
    }

    private void ActivateCreditsScreen(bool setActive)
    {
        credits.SetActive(setActive);
    }
}
