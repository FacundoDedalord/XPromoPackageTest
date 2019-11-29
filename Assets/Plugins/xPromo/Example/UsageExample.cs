using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class UsageExample : MonoBehaviour
{
    public TextAsset RemoteJsonFile;

    public xPromoCampaign[] EmbeddedCampaigns;
    public RectTransform GameContentTransform;
    private string _remoteJson = null;
    
    void Start() {
        StartCoroutine(InitializeXPromo());
    }

    public IEnumerator InitializeXPromo() {        
        
        // Analytic tracking goes here
        xPromoManager.Instance.OnShow += () => {
            Debug.Log("A promo was shown!");
        };

        xPromoManager.Instance.OnTap += () => {
            Debug.Log("The promo was tapped!");
        };

        // Adjust the UI when a promo is shown or hidden
        xPromoManager.Instance.OnShow += MoveUIToDisplayPromo;
        xPromoManager.Instance.OnHide += RestoreUIPosition;

        // Log handling goes here
        xPromoLogger.Instance.OnLog += (string log) => {
            Debug.Log($"[xPromo] {log}");
        };

        // Fetching of latest version of campaign data goes here
        // xPromoManager will automatically store and use the latest valid one
        yield return FakeFetchCampaingJson();


        // Initialize the XPromoManager
        xPromoManager.Instance.Init(_remoteJson, EmbeddedCampaigns);
    } 

    // Simulate fetching json from server
    private IEnumerator FakeFetchCampaingJson() {
        yield return CoroutineWait.ForSeconds(this, 2);
        _remoteJson = RemoteJsonFile.text;
    }

    public void MoveUIToDisplayPromo() {        
        if (GameContentTransform != null) {
            // Move the play button out of the way of the promo
            GameContentTransform.anchorMin = new Vector2(0f, 0.19f);
            GameContentTransform.anchoredPosition = Vector2.zero;
        }
    }

    public void RestoreUIPosition() {
        if (GameContentTransform != null) {
            // Move the play button to it's original position
            GameContentTransform.anchorMin = new Vector2(0f, 0f);
            GameContentTransform.anchoredPosition = Vector2.zero;
        }
    }
    /*
    public void DefaultSetup()
    {        
        OnValidXPromo += (string json) => {
            FirebaseRemoteConfigManager.Instance.SaveOnLocal(RemoteConfigKey.xPromo, json);
        };

        OnShowXPromo += () => {
            if (!AnalyticsManager.Instance.IsTestUser()) {
                AnalyticsManager.Instance.SendEvent(AnalyticEvnType.EvnxPromoShow);
            }
        };

        OnTapOnXPromo += () => {
            if (!AnalyticsManager.Instance.IsTestUser()) {
                AnalyticsManager.Instance.SendEvent(AnalyticEvnType.EvnxPromoTap);
            }
        };

        #if SUBMISSION
        RemoteConfigKey remoteConfigKey = RemoteConfigKey.xPromo;
        #else
        RemoteConfigKey remoteConfigKey = RemoteConfigKey.xPromoDebug;
        #endif

        string remoteJson = FirebaseRemoteConfigManager.Instance.GetConfigValue(remoteConfigKey); 
        string defaultJson = FirebaseRemoteConfigManager.Instance.GetConfigValueOnLocal(RemoteConfigKey.xPromo);
        
        FirebaseRemoteConfigManager.Instance.OnLoad.AddListener(() => {
            xPromoDownloadManager.Instance.baseUrl = FirebaseRemoteConfigManager.Instance.GetConfigValue(RemoteConfigKey.xPromoAssetsURL);
            Init(defaultJson, remoteJson);
        });
    }
    */
}
