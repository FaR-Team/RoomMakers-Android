using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Animator blackFade;
    [SerializeField] private SpriteRenderer controles;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button leaderboardsButton;
    [SerializeField] private Button creditsButton;

    private int value = 0;
    bool IsAlreadyChangingScene;
    
    void Start()
    {
        menuPanel.SetActive(false);
        SetupButtons();
    }

    void Update()
    {
        if (Input.anyKeyDown && !IsAlreadyChangingScene && value < 2)
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

    private void SetupButtons()
    {
        playButton.onClick.AddListener(() => LoadScene(2));
        controlsButton.onClick.AddListener(ShowControls);
        leaderboardsButton.onClick.AddListener(ShowLeaderboards);
        creditsButton.onClick.AddListener(ShowCredits);
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

    private void ShowControls()
    {
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
        ActivateControllerScreen(true);
    }

    private void ShowLeaderboards()
    {
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
    }

    private void ShowCredits()
    {
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
    }

    private void ActivateControllerScreen(bool setActive)
    {
        controles.enabled = setActive;
    }
}