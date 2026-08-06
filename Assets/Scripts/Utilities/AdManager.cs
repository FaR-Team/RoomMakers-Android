using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_ANDROID || UNITY_IOS
using GoogleMobileAds.Api;
#endif
using UnityEngine.SceneManagement;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

#if UNITY_ANDROID || UNITY_IOS
#if UNITY_EDITOR
    private string _adUnitId = "ca-app-pub-3940256099942544/6300978111"; // TEST ID
#elif UNITY_ANDROID
    private string _adUnitId = "ca-app-pub-8228169304379784/8589092954"; // Production ID
#elif UNITY_IPHONE
    private string _adUnitId = "ca-app-pub-3940256099942544/2934735716"; // TEST AD UNIT ID
#else
    private string _adUnitId = "unused";
#endif

    private BannerView _bannerView;
#endif
    private bool _isInitialized = false;
    private int _retryCount = 0;
    private const int MaxRetryCount = 3;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

#if UNITY_ANDROID || UNITY_IOS
            MobileAds.RaiseAdEventsOnUnityMainThread = true;

            Debug.Log("AdManager: Initializing MobileAds SDK...");
            MobileAds.Initialize((InitializationStatus initStatus) =>
            {
                _isInitialized = true;
                Debug.Log("AdManager: MobileAds SDK Initialized.");

                var requestConfiguration = new RequestConfiguration
                {
                    MaxAdContentRating = MaxAdContentRating.G,
                    TagForChildDirectedTreatment = TagForChildDirectedTreatment.True
                };
                MobileAds.SetRequestConfiguration(requestConfiguration);
                
                LoadAd();
            });
#endif
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

    }

    public void LoadAd()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!_isInitialized)
        {
            Debug.LogWarning("AdManager: Attempted to load ad before initialization. Deferring.");
            return;
        }

        if(_bannerView == null)
        {
            CreateBannerView();
        }

        var adRequest = new AdRequest();

        Debug.Log("AdManager: Loading banner ad...");
        _bannerView.LoadAd(adRequest);
#endif
    }

    public void CreateBannerView()
    {
#if UNITY_ANDROID || UNITY_IOS
        Debug.Log("AdManager: Creating banner view");

        if (_bannerView != null)
        {
            DestroyAd();
        }

        _bannerView = new BannerView(_adUnitId, AdSize.GetPortraitAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth), AdPosition.Bottom);
        
        ListenToAdEvents();
#endif
    }

    public void DestroyAd()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (_bannerView != null)
        {
            Debug.Log("AdManager: Destroying banner view.");
            _bannerView.Destroy();
            _bannerView = null;
        }
#endif
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DestroyAd();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("AdManager: Scene '" + scene.name + "' loaded. Checking banner status.");
        
        _retryCount = 0;

        DestroyAd();
        LoadAd();
    }

#if UNITY_ANDROID || UNITY_IOS
    private void ListenToAdEvents()
    {
        _bannerView.OnBannerAdLoaded += () =>
        {
            _retryCount = 0;
            Debug.Log("AdManager: Banner view loaded successfully. Response: " + _bannerView.GetResponseInfo());
        };

        _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError(string.Format("AdManager: Banner view failed to load with error: {0}\nCode: {1}\nDomain: {2}", 
                error.GetMessage(), error.GetCode(), error.GetDomain()));

            if (_retryCount < MaxRetryCount)
            {
                _retryCount++;
                float delay = 5f * Mathf.Pow(2, _retryCount - 1);
                Debug.Log(string.Format("AdManager: Retrying in {0} seconds... (Attempt {1}/{2})", delay, _retryCount, MaxRetryCount));
                StartCoroutine(RetryLoadAd(delay));
            }
            else
            {
                Debug.LogError("AdManager: Max retry attempts reached. Stopping ad load requests.");
            }
        };

        _bannerView.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("AdManager: Banner view paid {0} {1}.", adValue.Value, adValue.CurrencyCode));
        };

        _bannerView.OnAdImpressionRecorded += () =>
        {
            Debug.Log("AdManager: Banner view recorded an impression.");
        };

        _bannerView.OnAdClicked += () =>
        {
            Debug.Log("AdManager: Banner view was clicked.");
        };

        _bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("AdManager: Banner view full screen content opened.");
        };

        _bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("AdManager: Banner view full screen content closed.");
        };
    }

    private IEnumerator RetryLoadAd(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }
#endif
}
