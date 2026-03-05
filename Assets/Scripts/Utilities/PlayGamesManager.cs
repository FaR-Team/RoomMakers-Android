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

    public void ManualSignIn()
    {
        PlayGamesPlatform.Instance.ManuallyAuthenticate(SignInCallback);
    }

    private void SignInCallback(SignInStatus status)
    {
        if (status == SignInStatus.Success)
            Debug.Log("Play Games sign-in successful!");
        else
            Debug.LogWarning($"Play Games sign-in failed: {status}");
    }
    
    public void TrySubmitHighScore(int score, string leaderboardId)
    {
        if (IsSignedIn)
        {
            string prefKey = leaderboardId == GPGSIds.leaderboard_highscore ? "HighScore" : "HighScore_" + leaderboardId;
            long currentHigh = PlayerPrefs.GetInt(prefKey, 0);
            if (score > currentHigh)
            {
                Social.ReportScore(score, leaderboardId, success => {
                    Debug.Log(success ? "Highscore submitted" : "Failed to submit highscore");
                });
                PlayerPrefs.SetInt(prefKey, score);
            }
        }
    }

    public void ShowLeaderboard(string leaderboardId = null)
    {
        if (IsSignedIn)
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            if (string.IsNullOrEmpty(leaderboardId))
                PlayGamesPlatform.Instance.ShowLeaderboardUI();
            else
                PlayGamesPlatform.Instance.ShowLeaderboardUI(leaderboardId);
        }
        else
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Error);
            ManualSignIn();
        }
    }

    public void SubmitScore(long score, string leaderboardId)
    {
        if (IsSignedIn)
            Social.ReportScore(score, leaderboardId, success => {
                Debug.Log(success ? "Score submitted" : "Failed to submit score");
            });
        else
            Debug.LogWarning("Can't submit score, user not signed in!");
    }
}
