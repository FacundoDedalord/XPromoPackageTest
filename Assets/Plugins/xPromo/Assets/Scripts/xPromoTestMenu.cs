using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// Unity Framework
using UnityEngine;
using UnityEngine.UI;

public class xPromoTestMenu : MonoBehaviorSingleton<xPromoTestMenu>
{
    public GameObject container;
    
    public TMP_Dropdown dropdown;
    
    public Image downloadProgressImage;

    public TextMeshProUGUI managerLogText;
    public TextMeshProUGUI downloadManagerlogText;
    public TextMeshProUGUI localStorageText;
    public TextMeshProUGUI campaignsText;
    public TextMeshProUGUI campaignsFilteredText;
    public TextMeshProUGUI trackingDataText;
    public TextMeshProUGUI downloadManagerStatusText;
    public TextMeshProUGUI currentCampaignText;
    
    public GameObject managerPanel;
    public GameObject downloadManagerPanel;
    public GameObject localAssetsPanel;
    public GameObject campaignsPanel;

    public GameObject debugOpenPanelButton;
    
    private xPromoCampaignListData campaignListData;
    
    /// <summary>
    /// Unity Start Method
    /// </summary>
    void Start()
    {
        PopupateDropdownList();

        RefreshValues();
        xPromoLogger.Instance.SetNotifyOnChange(RefreshValues);
        
#if !SUBMISSION
        debugOpenPanelButton.SetActive(true);
#endif
    }

    new void OnDestroy()
    {
        if (xPromoLogger.IsAvailable()) {
            xPromoLogger.Instance.ClearNotifyOnChange();
        }
        base.OnDestroy();
    }

    /// <summary>
    /// Unity Update Method
    /// </summary>
    void Update()
    {
        if (campaignListData == null) PopupateDropdownList();

        downloadProgressImage.fillAmount = xPromoDownloadManager.Instance.downloadProgress;
    }

    void PopupateDropdownList()
    {
        campaignListData = xPromoManager.Instance.GetCampaignListData();
        
        if (campaignListData != null && campaignListData.games != null)
        {
            dropdown.options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < campaignListData.games.Length; i++)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(campaignListData.games[i].campaignId));
            }

            dropdown.value = -1;
            dropdown.value = 0;
        }
    }

    public void ShowButtonClicked()
    {
        var campaignId = dropdown.options[dropdown.value].text;
        xPromoManager.Instance.ShowCampaign(campaignId);
        
        currentCampaignText.text = $"Current: {campaignId}";
    }

    public void ShowManager()
    {
        managerPanel.SetActive(true);
        downloadManagerPanel.SetActive(false);
        localAssetsPanel.SetActive(false);
        campaignsPanel.SetActive(false);
    }
    
    public void ShowDownloadManager()
    {
        managerPanel.SetActive(false);
        downloadManagerPanel.SetActive(true);
        localAssetsPanel.SetActive(false);
        campaignsPanel.SetActive(false);
    }

    public void ShowLocalAssets()
    {
        managerPanel.SetActive(false);
        downloadManagerPanel.SetActive(false);
        localAssetsPanel.SetActive(true);
        campaignsPanel.SetActive(false);
    }

    public void ShowCampaigns()
    {
        managerPanel.SetActive(false);
        downloadManagerPanel.SetActive(false);
        localAssetsPanel.SetActive(false);
        campaignsPanel.SetActive(true);
    }

    public void GetNextCampaign()
    {
        var campaignId = xPromoManager.Instance.GetNextCampaign(true);
        xPromoManager.Instance.ShowCampaign(campaignId);

        currentCampaignText.text = $"Current: {campaignId}";
    }

    public void ResetViewValues()
    {
        xPromoManager.Instance.ResetViewsOnAllCampaigns();
    }

    public void RemoveLocalFiles()
    {
        xPromoDownloadManager.Instance.RemoveAllLocalFiles();
    }

    public void Show()
    {
        container.SetActive(true);
    }

    public void Hide()
    {
        container.SetActive(false);
    }

    public bool IsVisible()
    {
        return container.activeSelf;
    }

    public void RefreshValues()
    {
        var logger = xPromoLogger.Instance;

        managerLogText.text = logger.managerLog;
        
        downloadManagerlogText.text = logger.downloadManagerLog;
        downloadManagerStatusText.text = logger.downloadManagerStatus;

        localStorageText.text = logger.localStorageLog;
        campaignsText.text = logger.campaignsLog;
        campaignsFilteredText.text = logger.campaignsFilteredLog;
        trackingDataText.text = logger.trackingDataLog;

        currentCampaignText.text = xPromoManager.Instance.GetCurrentCampaignId();
    }

    public void CloseButtonClick()
    {
        Hide();
    }

    public void ClearManagerLog()
    {
        xPromoLogger.Instance.ClearManagerLog();
        managerLogText.text = xPromoLogger.Instance.managerLog;
    }

    public void ClearDownloadManagerLog()
    {
        xPromoLogger.Instance.ClearDownloadManagerLog();
        downloadManagerlogText.text = xPromoLogger.Instance.downloadManagerLog;
    }
}
