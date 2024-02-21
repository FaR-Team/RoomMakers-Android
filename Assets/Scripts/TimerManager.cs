using System;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    private static float maxTime = 300f;
    private static float time;

    [SerializeField] private SpriteRenderer clock;
    [SerializeField] Sprite[] clockSprites;

    public static Action timerAction;

    private void Start()
    {
        time = maxTime;
        StateManager.StartGame();
    }

    private void Update()
    {
        timerAction?.Invoke();

        if (time > 240)
        {
            clock.sprite = clockSprites[0];
        }
        else if (time > 180)
        {
            clock.sprite = clockSprites[1];
        }
        else if (time > 120)
        {
            clock.sprite = clockSprites[2];
        }
        else if (time > 60)
        {
            clock.sprite = clockSprites[3];
        }
        else clock.sprite = clockSprites[4];
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
