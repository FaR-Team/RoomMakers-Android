﻿using System;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    private GameState _currentGameState = GameState.Moving;
    public static GameState CurrentGameState => Instance != null ? Instance._currentGameState : GameState.Moving;

    public static event Action<GameState> OnStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static bool IsPaused()
    {
        return CurrentGameState == GameState.Pause;
    }
    public static bool IsMoving()
    {
        return CurrentGameState == GameState.Moving;
    }
    public static bool IsEditing()
    {
        return CurrentGameState == GameState.Editing;
    }

    public static bool IsGameOver()
    {
        return CurrentGameState == GameState.Lose;
    }
    public static void StartGame()
    {
        if (Instance != null)
        {
            Instance._currentGameState = GameState.Moving;
            OnStateChanged?.Invoke(Instance._currentGameState);
        }
    }
    public static void SwitchEditMode()
    {
        if (Instance != null)
        {
            Instance._currentGameState = Instance._currentGameState == GameState.Moving ? GameState.Editing : GameState.Moving;
            OnStateChanged?.Invoke(Instance._currentGameState);
        }
    }
    public static void PauseGame()
    {
        if (Instance != null)
        {
            Instance._currentGameState = GameState.Pause;
            OnStateChanged?.Invoke(Instance._currentGameState);
        }
    }
    public static void SwitchGameOverMode()
    {
        if (Instance != null)
        {
            Instance._currentGameState = GameState.Lose;
            OnStateChanged?.Invoke(Instance._currentGameState);
        }
    }

    public void ChangeGameState(GameState newState)
    {
        if (Instance != null)
        {
            Instance._currentGameState = newState;
            OnStateChanged?.Invoke(Instance._currentGameState);
        }
    }
}
