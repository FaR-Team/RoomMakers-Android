using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class PlayGamesManager : MonoBehaviour
{
    public static PlayGamesManager Instance { get; private set; }
    public bool IsSignedIn => Social.localUser.authenticated;

    void Awake()
    {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitPlayGames();
        } else Destroy(gameObject);
    }

    void InitPlayGames()
    {
        PlayGamesPlatform.Activate();
        SignIn();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(SignInCallback);
    }

    private void SignInCallback(SignInStatus status)
    {
        if (status == SignInStatus.Success)
            Debug.Log("Play Games sign-in successful!");
        else
            Debug.LogWarning($"Play Games sign-in failed: {status}");
    }

    public void ShowLeaderboard(string leaderboardId = "CgkI-unR07wTEAIQAQ")
    {
        if (IsSignedIn)
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardId);
        }
        else
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Error);
            SignIn();
        }
    }

    public void SubmitScore(long score, string leaderboardId = "CgkI-unR07wTEAIQAQ")
    {
        if (IsSignedIn)
            Social.ReportScore(score, leaderboardId, success => {
                Debug.Log(success ? "Score submitted" : "Failed to submit score");
            });
        else
            Debug.LogWarning("Can't submit score, user not signed in!");
    }
}
