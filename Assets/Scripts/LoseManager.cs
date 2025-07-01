using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseManager : MonoBehaviour
{
    public static LoseManager Instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public GameObject GameOverScreen;
    public AudioClip loseMusic;
    private Controls controls;
    private bool hasLost;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            controls = new Controls();
        }
        else
        {
            Destroy(this);
        }
    }

    void OnEnable()
    {
        controls.Enable();
        controls.Movement.Interact.performed += OnAPressed;
        controls.Movement.Rotate.performed += OnBPressed;
    }

    void OnDisable()
    {
        controls.Movement.Interact.performed -= OnAPressed;
        controls.Movement.Rotate.performed -= OnBPressed;
        controls.Disable();
    }

    private void OnBPressed(InputAction.CallbackContext context)
    {
        if (!hasLost) return;
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
        PlayGamesManager.Instance.ShowLeaderboard();
    }

    private void OnAPressed(InputAction.CallbackContext context)
    {
        if (!hasLost) return;
        AudioManager.instance.PlaySfx(GlobalSfx.Click);
        SceneManager.LoadScene(0);
    }

    public void Lose()
    {
        if (hasLost) return;
        TimerManager.StopTimer();
        StateManager.SwitchGameOverMode();
        GameOverScreen.SetActive(true);
        hasLost = true;
        scoreText.text = House.instance.Score.ToString();
        highScoreText.text = GetHighScore();
        PlayGamesManager.Instance.SubmitScore(House.instance.Score);

        AudioManager.instance.ResetMusicPitch();
        AudioManager.instance.ChangeMusic(loseMusic);
    }

    public string GetHighScore()
    {
        if (PlayerPrefs.HasKey("HighScore") && PlayerPrefs.GetInt("HighScore") > House.instance.Score)
        {
            return PlayerPrefs.GetInt("HighScore").ToString();
        }
        else
        {
            PlayerPrefs.SetInt("HighScore", House.instance.Score);
            return House.instance.Score.ToString();
        }
    }
}