using System;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static GameState currentGameState = GameState.Moving;

    public static event Action<GameState> OnStateChanged;

    public static bool IsPaused()
    {
        return currentGameState == GameState.Pause;
    }
    public static bool IsMoving()
    {
        return currentGameState == GameState.Moving;
    }
    public static bool IsEditing()
    {
        return currentGameState == GameState.Editing;
    }

    public static bool IsGameOver()
    {
        return currentGameState == GameState.Lose;
    }
    public static void StartGame()
    {
        currentGameState = GameState.Moving;
        OnStateChanged?.Invoke(currentGameState);
    }
    public static void SwitchEditMode()
    {
        currentGameState = currentGameState == GameState.Moving ? GameState.Editing : GameState.Moving;
        OnStateChanged?.Invoke(currentGameState);
    }

    public static void SwitchGameOverMode()
    {
        currentGameState = GameState.Lose;
        OnStateChanged?.Invoke(currentGameState);
    }
}
