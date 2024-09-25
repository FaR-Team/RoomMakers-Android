﻿using System;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static float maxTime = 150f;
    private static float time;
    [SerializeField] private SpriteRenderer clock;
    [SerializeField] Sprite[] clockSprites;

    public static Action timerAction;

    [SerializeField] private float flickerInterval = 0.5f;
    private float flickerTimer = 0f;
    private bool isVisible = true;
    private void Start()
    {
        time = maxTime;
        StateManager.StartGame();
    }

    private void Update()
    {
        timerAction?.Invoke();

        if (time > 120)
        {
            clock.sprite = clockSprites[0];
        }
        else if (time > 90)
        {
            clock.sprite = clockSprites[1];
        }
        else if (time > 60)
        {
            clock.sprite = clockSprites[2];
        }
        else if (time > 30)
        {
            clock.sprite = clockSprites[3];
        }
        else
        {
            clock.sprite = clockSprites[4];
            FlickerClock();
        }
    }

    private void FlickerClock()
    {
        flickerTimer += Time.deltaTime;
        if (flickerTimer >= flickerInterval)
        {
            isVisible = !isVisible;
            Color clockColor = clock.color;
            clockColor.a = isVisible ? 1f : 0f;
            clock.color = clockColor;
            flickerTimer = 0f;
        }
    }

    public static void Timer()
    {
        time -= Time.deltaTime;
        if (time <= 0) LoseManager.Instance.Lose();
    }
    public static void StartTimer()
    {
        time = maxTime;
        if(timerAction == null) timerAction += Timer;
    }
    public static void StopTimer()
    {
        timerAction -= Timer;
    }
}