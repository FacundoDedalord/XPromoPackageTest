// .NET Framework

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Unity Framework
using UnityEngine;
using UnityEngine.Video;
using Random = UnityEngine.Random;

public class xPromoManager : MonoBehaviorSingleton<xPromoManager>
{

    // Public Hooks

    /// Called when a Promo is about to be shown
    public Action OnShow;

    /// Called when a Promo is about to be hidden
    public Action OnHide;

    /// Called when a Promo is tapped by the user
    public Action OnTap;



    private const int ImageWidth = 512;
    private const int ImageHeight = 512;
    
    // Reference to the basic layout prefab
    public GameObject basicLayoutPrefab;

    // Embedded campaigns elements
    private xPromoCampaign[] _embeddedCampaignsElements;

    
    // Reference to the video player component
    private VideoPlayer videoPlayer;

    // Campaign list data
    private xPromoCampaignListData campaignListData;

    // Current campaign tracking data (stored locally to check max views)
    private xPromoCampaignTrackingListData trackingData;

    // Structure used to get the next campaign to show
    private List<xPromoCampaignIdProb> filteredCampaignIdProbList;

    // Active campaign
    private xPromoCampaign activeCampaign;

    // Current campaign item
    private xPromoCampaignData campaignItem;

    // Object that manages tracking of app lifetime.      
    private AppLifetimeTracker _appLifetime;

    // Reference to the logger
    private xPromoLogger logger;

    /// <summary>
    /// Unity Awake Method
    /// </summary>
    new void Awake() {

        base.Awake();
        
        logger = GetComponent<xPromoLogger>();
    }
    
    /// <summary>
    /// Unity Start Method
    /// </summary>
    void Start() {
        
        videoPlayer = GetComponentInChildren<VideoPlayer>(true);

        _appLifetime = new AppLifetimeTracker();
    }

    public bool IsInitialized()
    {
        return campaignListData != null;
    }

    private void SaveLocalCampaingData(string json)
    {
        string xPromo = "xPromoCampaingData";
        PlayerPrefs.SetString(xPromo, json);
    }
    private string LoadLocalCampaingData()
    {
        string xPromo = "xPromoCampaingData";
        return PlayerPrefs.GetString(xPromo, null);
    }

    private bool LoadCampaingDataFromJson(string json)
    {
        bool success = false;
        try {
            campaignListData = JsonUtility.FromJson<xPromoCampaignListData>(json);
            if (campaignListData != null || campaignListData.games != null) {              
                success = true;  
            }
            else
            {
                Log("Campaign data is empty.");
            }
        }
        catch (Exception e) {
            Log($"Error reading json. Error: {e.Message}");
        }
        return success;
    }
    
    /// <summary>
    /// Init the xPromo Manager
    /// </summary>
    public void Init(string remoteJson, xPromoCampaign[] embeddedCampaigns = null) {
        Log("Init");

        // Set Embedded campaigns 
        _embeddedCampaignsElements = embeddedCampaigns;

        // Try using remote json
        bool remoteJsonIsValid = LoadCampaingDataFromJson(remoteJson);
        if(remoteJsonIsValid)
        {                       
            SaveLocalCampaingData("The provided remote json is valid");
        }
        else
        {
            Log("Remote campaign data is not valid");

            // Get the latest json from local storage
            string localJson = LoadLocalCampaingData();
            bool localJsonIsValid = LoadCampaingDataFromJson(localJson);
            if(!localJsonIsValid)
            {
                Log($"Campaigns were read from local json");
            }
            else
            {                
                Log("There are no campaigns stored locally. Exit from xPromo.");
                return;
            }
        }
        
        trackingData = xPromoCampaignTracking.ReadFromLocal();
        trackingData = xPromoCampaignTracking.PurgeCampaigns(campaignListData, trackingData);

        // Convert the dates to the local format
        for (int i = 0; i < campaignListData.games.Length; i++) {
            if (campaignListData.games[i].startDate > 0) {
                DateTime date1 = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                campaignListData.games[i].startDateTime = date1.AddSeconds(campaignListData.games[i].startDate);
            } else {
                campaignListData.games[i].startDateTime = DateTime.MinValue;
            }

            if (campaignListData.games[i].endDate > 0) {
                DateTime date2 = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
                campaignListData.games[i].endDateTime = date2.AddSeconds(campaignListData.games[i].endDate);
            } else {
                campaignListData.games[i].endDateTime = DateTime.MaxValue;
            }
        }

        ReBuildFileteredList();
            
        // Logger
        logger.LogCampaigns(campaignListData);
        logger.LogTrackingData(trackingData);
        
        xPromoDownloadManager.Instance.StartDownload(campaignListData.assetsURL, buildAssetsToDownloadList(campaignListData));
    }

    private List<xPromoCampaignIdProb> ReBuildFileteredList()
    {
        filteredCampaignIdProbList = new List<xPromoCampaignIdProb>();

        var now = DateTime.Now;
        long sumProbs = 0;

        if(campaignListData.games != null) {
            // Convert the dates to the local format
            for (int i = 0; i < campaignListData.games.Length; i++) {
                if (campaignListData.games[i].startDateTime <= now && now <= campaignListData.games[i].endDateTime) {
                    sumProbs += campaignListData.games[i].prob;
                }
            }

            

            // Set the accumulated probabilities
            float accumProb = 0f;
            for (int i = 0; i < campaignListData.games.Length; i++) {
                
                var installSource = InstallerSourceUtil.GetInstallSource();
                if (installSource == InstallerSource.eAppleAppStore && string.IsNullOrEmpty(campaignListData.games[i].linkiOS)) continue;
                if (installSource == InstallerSource.eSamsungGalaxyStore && string.IsNullOrEmpty(campaignListData.games[i].linkSamsung)) continue;
                if (installSource == InstallerSource.eSkillzStore && string.IsNullOrEmpty(campaignListData.games[i].linkSkillz)) continue;

                if (campaignListData.games[i].startDateTime <= now && now <= campaignListData.games[i].endDateTime &&
                    !HasReachTodayMaxViews(campaignListData.games[i]) &&
                    !HasReachTotalMaxViews(campaignListData.games[i])) {
                    accumProb += (campaignListData.games[i].prob / (float) sumProbs);

                    filteredCampaignIdProbList.Add(new xPromoCampaignIdProb {campaignId = campaignListData.games[i].campaignId, accumProb = accumProb});
                }
            }
        }
        
        // DEBUG
        logger.LogFilteredCampaigns(filteredCampaignIdProbList);

        return filteredCampaignIdProbList;
    }

    /// <summary>
    /// Returns the next campaign id to show
    /// </summary>
    public string GetNextCampaign(bool rebuildFilteredList = false)
    {
        if (_appLifetime.DaysSinceFirstInit < campaignListData.surfaceAfterDays)
        {
            Log($"Days since first init ({_appLifetime.DaysSinceFirstInit}) is still minor than surface after days remote config value ({campaignListData.surfaceAfterDays})");

#if SUBMISSION            
            return string.Empty;
#endif
            
            Log("Because SUBMISSION is not set, we will show ads anyway");
        }
        
        // Refine the filtered lists
        if (rebuildFilteredList) ReBuildFileteredList();
        
        const int attempts = 5;
        for (int j=0; j<attempts; j++) {
            float p = UnityEngine.Random.Range(0f, 1f);
            for (int i = 0; i < filteredCampaignIdProbList.Count; i++) {
                if (p < filteredCampaignIdProbList[i].accumProb && AllRequiredAssetsAreDownloaded(filteredCampaignIdProbList[i].campaignId)) {
                    Log($"Next Campaign: {filteredCampaignIdProbList[i].campaignId}");
                    return filteredCampaignIdProbList[i].campaignId;
                }
            }
        }

        // Tried to get a random campaign, but seems that no one has all its assets downloaded, try one by one
        for (int i = 0; i < filteredCampaignIdProbList.Count; i++) {
            if (AllRequiredAssetsAreDownloaded(filteredCampaignIdProbList[i].campaignId)) {
                Log($"Next Campaign: {filteredCampaignIdProbList[i].campaignId}");
                return filteredCampaignIdProbList[i].campaignId;
            }
        }
        
        // None campaign with all its assets downloaded
        
        Log($"Next Campaign: None");
        
        return string.Empty;
    }


    /// <summary>
    /// Has reached the max views number per day?
    /// </summary>
    private bool HasReachTodayMaxViews(xPromoCampaignData campaignData)
    {
        for (int i = 0; i < trackingData.campaigns.Length; i++)
        {
            if (string.Compare(trackingData.campaigns[i].campaignId, campaignData.campaignId, StringComparison.InvariantCulture) == 0)
            {
                return (trackingData.campaigns[i].todayViews >= campaignData.maxViewsDay);
            }
        }

        return false;
    }

    /// <summary>
    /// Has reached the max views number?
    /// </summary>
    private bool HasReachTotalMaxViews(xPromoCampaignData campaignData)
    {
        for (int i = 0; i < trackingData.campaigns.Length; i++)
        {
            if (string.Compare(trackingData.campaigns[i].campaignId, campaignData.campaignId, StringComparison.InvariantCulture) == 0)
            {
                return (trackingData.campaigns[i].views >= campaignData.maxViews);
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the list that should be downloaded if necessary
    /// </summary>
    private List<string> buildAssetsToDownloadList(xPromoCampaignListData campaignListList)
    {
        List<string> assetsToDownload = new List<string>();

        for (int i = 0; i < campaignListList.games.Length; i++)
        {

            if (!string.IsNullOrEmpty(campaignListList.games[i].video) && !assetsToDownload.Contains(campaignListList.games[i].video))
            {
                assetsToDownload.Add(campaignListList.games[i].video);
            }

            if (!string.IsNullOrEmpty(campaignListList.games[i].gameLogo) && !assetsToDownload.Contains(campaignListList.games[i].gameLogo))
            {
                assetsToDownload.Add(campaignListList.games[i].gameLogo);
            }

            if (!string.IsNullOrEmpty(campaignListList.games[i].gameScreenshot) && !assetsToDownload.Contains(campaignListList.games[i].gameScreenshot))
            {
                assetsToDownload.Add(campaignListList.games[i].gameScreenshot);
            }
        }

        return assetsToDownload;
    }

    /// <summary>
    /// Returns the campaign list data
    /// </summary>
    public xPromoCampaignListData GetCampaignListData()
    {
        return campaignListData;
    }

    /// <summary>
    /// Show the specified campaign
    /// </summary>
    public void ShowCampaign(string campaignId)
    {
        for (int i = 0; i < campaignListData.games.Length; i++) {
            if (string.Compare(campaignListData.games[i].campaignId, campaignId, StringComparison.InvariantCulture) == 0) {
                ShowCampaign(campaignListData.games[i]);        
            }
        }
    }

    /// <summary>
    /// Show the specified campaign
    /// </summary>
    public void ShowCampaign(xPromoCampaignData campaignItem)
    {
        if (activeCampaign != null)
        {
            activeCampaign.Hide();
            DestroyCampaign(activeCampaign);
        }

        if (AllRequiredAssetsAreDownloaded(campaignItem, true))
        {            
            this.campaignItem = campaignItem;
            CreateCurrentCampaign(campaignItem);
        }
        else
        {
            // About to stop showing promo, hook here to have the UI can react acordingly.
            OnHide?.Invoke();
            
            Log($"Warning: Not all assets are yet downloaded for the new campaign: {campaignItem.campaignId}");
            activeCampaign = null;
        }
    }

    /// <summary>
    /// Destroy the current campaign
    /// </summary>
    private void DestroyCampaign(xPromoCampaign campaignToDestroy)
    {
        if (videoPlayer.isPlaying) {
            videoPlayer.enabled = false;
            videoPlayer.Stop();
        }
        
        if (campaignToDestroy != null) DestroyImmediate(campaignToDestroy.gameObject);
    }

    /// <summary>
    /// Returns true if all the required assets for an specific campaign are already downloaded
    /// NOTE: Here we are not checking if we have downloaded the latest version of the asset
    /// </summary>
    private bool AllRequiredAssetsAreDownloaded(string campaignId, bool logWarnings = false) {
        
        for (int i = 0; i < campaignListData.games.Length; i++) {
            if (string.Compare(campaignListData.games[i].campaignId, campaignId, StringComparison.InvariantCulture) == 0) {
                return AllRequiredAssetsAreDownloaded(campaignListData.games[i], logWarnings);
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if all the required assets for an specific campaign are already downloaded
    /// NOTE: Here we are not checking if we have downloaded the latest version of the asset
    /// </summary>
    private bool AllRequiredAssetsAreDownloaded(xPromoCampaignData campaignItem, bool logWarnings = false) {
        
        // If was using an embedded prefab, everything is downloaded
        if (!string.IsNullOrEmpty(campaignItem.embeddedPrefab)) {
            return true;
        }
        
        if (!string.IsNullOrEmpty(campaignItem.video)) {
            if (!xPromoDownloadManager.Instance.ExistsInStorage(campaignItem.video)) {
                if (logWarnings) Log($"Asset {campaignItem.video} was not still downloaded.");
                return false;
            }
        }
        
        if (!string.IsNullOrEmpty(campaignItem.gameLogo)) {
            if (!xPromoDownloadManager.Instance.ExistsInStorage(campaignItem.gameLogo)) {
                if (logWarnings) Log($"Asset {campaignItem.gameLogo} was not still downloaded.");
                return false;
            }
        }
        
        if (!string.IsNullOrEmpty(campaignItem.gameScreenshot)) {
            if (!xPromoDownloadManager.Instance.ExistsInStorage(campaignItem.gameScreenshot)) {
                if (logWarnings)  Log($"Asset {campaignItem.gameScreenshot} was not still downloaded.");
                return false;
            }
        }

        return true;
    }
    
    /// <summary>
    /// Create the campaign that is stored in campaignItem
    /// </summary>
    private void CreateCurrentCampaign(xPromoCampaignData campaignItem) {
        
        GameObject prefab = null;

        if (string.IsNullOrEmpty(campaignItem.embeddedPrefab)) {
            prefab = basicLayoutPrefab;
        } else {
            var idx = GetEmbeddedCampaignElementIdx(campaignItem.embeddedPrefab);
            if (idx != -1) prefab = _embeddedCampaignsElements[idx].gameObject;
            
            if (idx == -1 || prefab == null) {
                Log($"Cannot find the prefab {campaignItem.embeddedPrefab} in the embedded list.");
                return;
            }
        }
        
            
        // [Analytics]
        // Also here the UI can react to the promo being shown by moving out of the way.
        OnShow?.Invoke();
        
        // Increment the number of views
        IncrementViewsOnCampaign(campaignItem.campaignId);
        
        var activeCampaignGO = GameObject.Instantiate(prefab, xPromoRoot.Instance.transform);
        activeCampaign = activeCampaignGO.GetComponent<xPromoCampaign>();
        
        // Change the images and video only for the basic layout
        if (string.IsNullOrEmpty(campaignItem.embeddedPrefab)) {
            
            if (!string.IsNullOrEmpty(campaignItem.gameScreenshot)) {
                Texture2D tex = new Texture2D(ImageWidth, ImageHeight);
                tex.LoadImage(xPromoDownloadManager.Instance.GetData(campaignItem.gameScreenshot));
                activeCampaign.gameScreenshot.texture = tex;
                activeCampaign.gameScreenshot.gameObject.SetActive(true);
            } else {
                activeCampaign.gameScreenshot.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(campaignItem.gameLogo)) {
                Texture2D tex = new Texture2D(ImageWidth, ImageHeight);
                tex.LoadImage(xPromoDownloadManager.Instance.GetData(campaignItem.gameLogo));
                activeCampaign.gameLogo.texture = tex;
                activeCampaign.gameLogo.gameObject.SetActive(true);
            } else {
                activeCampaign.gameLogo.gameObject.SetActive(false);
            }

            if (!string.IsNullOrEmpty(campaignItem.video)) {
                videoPlayer.enabled = true;
                videoPlayer.url = $"file://{Application.persistentDataPath}/{campaignItem.video}";
                videoPlayer.Play();
                activeCampaign.video.gameObject.SetActive(true);
            } else {
                activeCampaign.video.gameObject.SetActive(false);
                videoPlayer.Stop();
                videoPlayer.enabled = false;
            }

            var isoCode = "";// [REMOVED_REF] LordUILocalization.Instance.GetCurrentLanguageIsoCode();
            var buttonText = GetLocalizationText(isoCode, campaignItem.buttonText);
            if (buttonText != null) activeCampaign.buttonText.text = buttonText;
        }
        else
        {
            // Embedded campaign
            if (!string.IsNullOrEmpty(activeCampaign.videoClipName)) {
                videoPlayer.enabled = true;
                string videofilename = activeCampaign.videoClipName.Contains(".") ? activeCampaign.videoClipName : $"{activeCampaign.videoClipName}.mp4";
                videoPlayer.url = Path.Combine(Application.streamingAssetsPath, videofilename);
                videoPlayer.Play();
                activeCampaign.video.gameObject.SetActive(true);
            } else {
                videoPlayer.Stop();
                videoPlayer.enabled = false;
            }
        }
        
        // Logger
        logger.LogTrackingData(trackingData);
        
        activeCampaign.Show();
    }

    /// <summary>
    /// Increment the number of views of the specified campaign
    /// </summary>
    private void IncrementViewsOnCampaign(string campaignId) {
        for (int i = 0; i < trackingData.campaigns.Length; i++) {
            if (string.Compare(trackingData.campaigns[i].campaignId, campaignId, StringComparison.InvariantCulture) == 0) {
                trackingData.campaigns[i].views++;
                trackingData.campaigns[i].todayViews++;
                xPromoCampaignTracking.SaveToLocal(trackingData);
                return;
            }
        }
    }

    private int GetEmbeddedCampaignElementIdx(string embeddedPrefabName)
    {
        if (_embeddedCampaignsElements != null) {
            for (int i = 0; i < _embeddedCampaignsElements.Length; i++) {
                if (_embeddedCampaignsElements[i] != null && string.Compare(_embeddedCampaignsElements[i].name, embeddedPrefabName, StringComparison.InvariantCulture) == 0) {
                    return i;
                }
            }
        }

        return -1;
    }
    
    /// <summary>
    /// Returns the localization text
    /// </summary>
    public string GetLocalizationText(string isoCode, string textId) {
        for (int i = 0; i < campaignListData.localization.Length; i++) {
            if (string.Compare(campaignListData.localization[i].id, textId, StringComparison.InvariantCulture) == 0) {
                switch (isoCode) {
                    case "es":
                        return campaignListData.localization[i].textES;
                    case "de":
                        return campaignListData.localization[i].textDE;
                    case "fr":
                        return campaignListData.localization[i].textFR;
                    case "it":
                        return campaignListData.localization[i].textIT;
                    case "pt":
                        return campaignListData.localization[i].textPT;
                    case "zh":
                        return campaignListData.localization[i].textZH;
                    case "ja":
                        return campaignListData.localization[i].textJA;
                    case "ru":
                        return campaignListData.localization[i].textRU;
                    default:
                        return campaignListData.localization[i].textEN;
                }
            }
        }
        return string.Empty;
    }

    /// <summary>
    /// For testing purposes, reset the views number for all campaigns
    /// </summary>
    public void ResetViewsOnAllCampaigns() {
        
        for (int i = 0; i < trackingData.campaigns.Length; i++)
        {
            trackingData.campaigns[i].views = 0;
            trackingData.campaigns[i].todayViews = 0;
        }
        xPromoCampaignTracking.SaveToLocal(trackingData);

        // Rebuild the filtered list
        ReBuildFileteredList();
        
        // Logger
        logger.LogTrackingData(trackingData);
    }

    /// <summary>
    /// Returns the current campaign id
    /// </summary>
    public string GetCurrentCampaignGameId() {
        return (campaignItem != null ? campaignItem.game : null);
    }

    /// <summary>
    /// Returns the current game id
    /// </summary>
    public string GetCurrentCampaignId() {
        return (campaignItem != null ? campaignItem.campaignId : null);
    }

    /// <summary>
    /// Campaign tapped
    /// </summary>
    public void TapOnCampaign() {
        
        // [Analytics]
        OnTap?.Invoke();

        // Set the correct store URL
        var installerStore = InstallerSourceUtil.GetInstallSource();
        
        switch (installerStore) {
            case InstallerSource.eAppleAppStore:
                Application.OpenURL(campaignItem.linkiOS);
                break;
            case InstallerSource.eSamsungGalaxyStore:
                Application.OpenURL(campaignItem.linkSamsung);
                break;
            case InstallerSource.eSkillzStore:
                Application.OpenURL(campaignItem.linkSkillz);
                break;
            default:
                Application.OpenURL(campaignItem.linkSkillz);
                break;
        }
    }

    private void Log(string logText)
    {        
        logger.LogManagerAppend(logText);
    }
}
