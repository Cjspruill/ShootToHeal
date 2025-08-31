using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using Unity.Services.LevelPlay;

public class LevelPlayAds : MonoBehaviour
{
    public static LevelPlayAds Instance { get; private set; }

    [Header("LevelPlay config")]
    [Tooltip("Your LevelPlay App Key")]
    public string appKey = "2369c7645";

    [Header("Ad Unit IDs")]
    public string cashAdUnitId = "smmmnqrxt18at15u";
    public string healthAdUnitId = "8uf2fme1jfgrdo5e";

    private LevelPlayRewardedAd cashAd;
    private LevelPlayRewardedAd healthAd;

    PlayerController playerController;
    [SerializeField] GameObject upgradesPanel;
    [SerializeField]public bool adIsPlaying;
    public event Action OnAnyAdClosed;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }



    void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();

        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;

        LevelPlay.Init(appKey);
    }

    void OnInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay initialized successfully.");
        CreateCashAd();
        CreateHealthAd();

        LoadCashAd();
        LoadHealthAd();
    }

    void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay init failed: {error}");
    }

    #region Create Ads

    private void CreateCashAd()
    {
        cashAd = new LevelPlayRewardedAd(cashAdUnitId);
        RegisterEvents(cashAd, "Cash");
    }

    private void CreateHealthAd()
    {
        healthAd = new LevelPlayRewardedAd(healthAdUnitId);
        RegisterEvents(healthAd, "Health");
    }

    public bool IsAdPlaying()
    {
        return adIsPlaying;
    }

    private void RegisterEvents(LevelPlayRewardedAd ad, string type)
    {
        ad.OnAdLoaded += (info) => Debug.Log($"{type} ad loaded.");
        ad.OnAdLoadFailed += (error) => Debug.LogError($"{type} ad failed to load: {error}");

        ad.OnAdDisplayed += (info) =>
        {
            adIsPlaying = true;
            if (upgradesPanel != null)
                upgradesPanel.SetActive(false);  // ✅ hide when ad shows
            Debug.Log($"{type} ad displayed.");
        };

        ad.OnAdDisplayFailed += (error) =>
        {
            adIsPlaying = false;
            if (upgradesPanel != null)
                upgradesPanel.SetActive(true);   // ✅ re-show in case of failure
            Debug.LogError($"{type} ad display failed: {error}");
        };

        ad.OnAdClicked += (info) => Debug.Log($"{type} ad clicked.");

        ad.OnAdClosed += (info) =>
        {
            adIsPlaying = false;
            if (upgradesPanel != null)
                upgradesPanel.SetActive(true);   // ✅ re-show when ad closes
            Debug.Log($"{type} ad closed. Preloading next ad...");
            LoadAd(ad, type);
        };

        ad.OnAdRewarded += (adInfo, reward) => HandleReward(adInfo, reward);
        ad.OnAdInfoChanged += (info) => Debug.Log($"{type} ad info changed.");

        ad.OnAdClosed += (info) =>
        {
            adIsPlaying = false;
            Debug.Log($"{type} ad closed.");
            LoadAd(ad, type);

            OnAnyAdClosed?.Invoke(); // ✅ notify listeners
        };
    }

    #endregion

    #region Load & Show

    public void LoadCashAd() => LoadAd(cashAd, "Cash");
    public void LoadHealthAd() => LoadAd(healthAd, "Health");

    private void LoadAd(LevelPlayRewardedAd ad, string type)
    {
        if (ad == null) return;
        Debug.Log($"Loading {type} ad...");
        ad.LoadAd();
    }

    public bool IsCashAdReady() => cashAd != null && cashAd.IsAdReady();
    public bool IsHealthAdReady() => healthAd != null && healthAd.IsAdReady();

    public void ShowCashAd()
    {
        if (IsCashAdReady()) cashAd.ShowAd();
        else
        {
            Debug.LogWarning("Cash ad not ready. Loading...");
            LoadCashAd();
        }
    }

    public void ShowHealthAd()
    {
        if (IsHealthAdReady()) healthAd.ShowAd();
        else
        {
            Debug.LogWarning("Health ad not ready. Loading...");
            LoadHealthAd();
        }
    }

    #endregion

    #region Reward Handling

    private void HandleReward(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        if (adInfo == null || reward == null)
        {
            Debug.LogWarning("Reward or AdInfo is null.");
            return;
        }

        // Log info for debugging
        Debug.Log($"AdUnitName: {adInfo.AdUnitName}, Reward Name: {reward.Name}, Reward Amount: {reward.Amount}");

        int rewardAmount = reward.Amount;
        string adUnit = adInfo.AdUnitName;

        // Handle Editor mock specifically
#if UNITY_EDITOR
        if (adUnit == "editor_mock_name" && reward.Name == "editor_reward" && reward.Amount == 20)
        {
            if (LevelPlayAds.Instance.IsCashAdReady())
            {
                adUnit = "Cash";
                rewardAmount = 200;
            }
            else
            {
                adUnit = "Health";
                rewardAmount = 25;
            }
        }
#endif

        // Apply rewards automatically based on ad unit
        if (adUnit.Contains("Cash"))
        {
            playerController.GetCash += rewardAmount;
            Debug.Log($"Cash reward applied: {rewardAmount}");
        }
        else if (adUnit.Contains("Health"))
        {
            playerController.UpdateHealth(rewardAmount);
            Debug.Log($"Health reward applied: {rewardAmount}");
        }
        else
        {
            Debug.LogWarning($"Unknown ad unit: {adUnit}. No reward applied.");
        }
    }

    #endregion

    void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;

        UnregisterAdEvents(cashAd);
        UnregisterAdEvents(healthAd);
    }



    private void UnregisterAdEvents(LevelPlayRewardedAd ad)
    {
        if (ad == null) return;

        // You need to unsubscribe each event using the same method you subscribed
        ad.OnAdLoaded -= (info) => Debug.Log("Ad loaded."); // example placeholder
        ad.OnAdLoadFailed -= (error) => Debug.LogError("Ad load failed.");
        ad.OnAdDisplayed -= (info) => Debug.Log("Ad displayed.");
        ad.OnAdDisplayFailed -= (error) => Debug.LogError("Ad display failed.");
        ad.OnAdClicked -= (info) => Debug.Log("Ad clicked.");
        ad.OnAdClosed -= (info) => Debug.Log("Ad closed.");
        //ad.OnAdRewarded -= HandleReward;
        ad.OnAdInfoChanged -= (info) => Debug.Log("Ad info changed.");
    }
}
