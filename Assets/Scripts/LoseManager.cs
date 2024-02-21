﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseManager : MonoBehaviour
{
    public static LoseManager Instance;
    public TextMeshProUGUI scoreText;
    public GameObject GameOverScreen;
    public AudioClip loseMusic;
    private bool hasLost;

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void Lose()
    {
        if (hasLost) return;
        TimerManager.StopTimer();
        StateManager.SwitchGameOverMode();
        GameOverScreen.SetActive(true);
        hasLost = true;
        scoreText.text = House.instance.Score.ToString();
        AudioManager.instance.ChangeMusic(loseMusic);
        //Debug.Log("Lose");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && hasLost)
        {
            SceneManager.LoadScene(0);
        }
    }
}