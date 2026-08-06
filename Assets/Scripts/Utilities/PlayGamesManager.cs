using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class PlayGamesManager : MonoBehaviour
{
    public static PlayGamesManager Instance { get; private set; }
    
    public bool IsSignedIn =>
#if UNITY_ANDROID
        Social.localUser.authenticated;
#else
        false;
#endif

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
#if UNITY_ANDROID
        PlayGamesPlatform.Activate();
        SignIn();
#endif
    }

    public void SignIn()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(SignInCallback);
#endif
    }

    public void ManualSignIn()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.ManuallyAuthenticate(SignInCallback);
#endif
    }

#if UNITY_ANDROID
    private void SignInCallback(SignInStatus status)
    {
        if (status == SignInStatus.Success)
            Debug.Log("Play Games sign-in successful!");
        else
            Debug.LogWarning($"Play Games sign-in failed: {status}");
    }
#endif
    
    public void TrySubmitHighScore(int score, string leaderboardId)
    {
#if UNITY_ANDROID
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
#endif
    }

    public void ShowLeaderboard(string leaderboardId = null)
    {
#if UNITY_ANDROID
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
#endif
    }

    public void SubmitScore(long score, string leaderboardId)
    {
#if UNITY_ANDROID
        if (IsSignedIn)
            Social.ReportScore(score, leaderboardId, success => {
                Debug.Log(success ? "Score submitted" : "Failed to submit score");
            });
        else
            Debug.LogWarning("Can't submit score, user not signed in!");
#endif
    }
}
