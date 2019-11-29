// .NET Framework
using System;
using System.Collections.Generic;

// Unity Framework
using UnityEngine;

public class xPromoLogger : MonoBehaviorSingleton<xPromoLogger>
{
    public Action<string> OnLog;
    private string _managerLog = "";
    public string managerLog {
        get { return _managerLog; }
    }
    
    private string _downloadManagerLog = "";
    public string downloadManagerLog {
        get { return _downloadManagerLog; }
    }
    
    private string _downloadManagerStatus = "";
    public string downloadManagerStatus {
        get { return _downloadManagerStatus; }
    }
    
    private string _campaignsLog = "";
    public string campaignsLog {
        get { return _campaignsLog; }
    }
    
    private string _campaignsFilteredLog = "";
    public string campaignsFilteredLog {
        get { return _campaignsFilteredLog; }
    }
    
    private string _localStorageLog = "";
    public string localStorageLog {
        get { return _localStorageLog; }
    }
    
    private string _trackingDataLog = "";
    public string trackingDataLog {
        get { return _trackingDataLog; }
    }
    
    private Action _notifyOnChange;
    
    public void SetNotifyOnChange(Action cb)
    {
        _notifyOnChange = cb;
    }

    public void ClearNotifyOnChange()
    {
        _notifyOnChange = null;
    }
    
    public void LogLocalStorageItems(List<string> localFiles)
    {
        _localStorageLog = "";
        
        if (localFiles != null)
        {
            for (int i = 0; i < localFiles.Count; i++)
            {
                _localStorageLog += $"{localFiles[i]}\n";
            }
        }
        else
        {
            _localStorageLog = "Empty";
        }
        OnLog?.Invoke(_localStorageLog);
        _notifyOnChange?.Invoke();
    }

    public void LogManagerAppend(string log)
    {        
        _managerLog += log + "\n";
        OnLog?.Invoke(log);
        _notifyOnChange?.Invoke();
    }
    
    public void LogDownloadManagerAppend(string log)
    {        
        _downloadManagerLog += log + "\n";
        OnLog?.Invoke(log);
        _notifyOnChange?.Invoke();
    }
    
    public void LogDownloadManagerStatus(xPromoDownloadManager.State state)
    {
        _downloadManagerStatus = $"Status: {state}";
        //OnLog?.Invoke(_downloadManagerStatus);
        _notifyOnChange?.Invoke();
    }

    public void LogCampaigns(xPromoCampaignListData campaignListData)
    {
        _campaignsLog = "";
        if (campaignListData != null && campaignListData.games.Length > 0)
        {
            for (int i = 0; i < campaignListData.games.Length; i++)
            {
                _campaignsLog += $"{campaignListData.games[i].campaignId}\t{campaignListData.games[i].game}\t{campaignListData.games[i].maxViews}\t{campaignListData.games[i].maxViewsDay}\n";
            }
        }
        else
        {
            _campaignsLog = "Empty";
        }
        OnLog?.Invoke(_campaignsLog);
        _notifyOnChange?.Invoke();
    }
    
    public void LogFilteredCampaigns(List<xPromoCampaignIdProb> filteredCampaignDataList)
    {
        _campaignsFilteredLog = "";
        if (filteredCampaignDataList != null && filteredCampaignDataList.Count > 0)
        {
            for (int i = 0; i < filteredCampaignDataList.Count; i++)
            {
                _campaignsFilteredLog += $"{filteredCampaignDataList[i].campaignId}\t{filteredCampaignDataList[i].accumProb}\n";
            }
        }
        else
        {
            _campaignsFilteredLog = "Empty";
        }
        OnLog?.Invoke(_campaignsFilteredLog);
        _notifyOnChange?.Invoke();
    }

    public void LogTrackingData(xPromoCampaignTrackingListData trackingData)
    {
        _trackingDataLog = "";
        if (trackingData != null && trackingData.campaigns.Length > 0)
        {
            for (int i = 0; i < trackingData.campaigns.Length; i++)
            {
                _trackingDataLog += $"{trackingData.campaigns[i].campaignId}\t{trackingData.campaigns[i].views}\t{trackingData.campaigns[i].todayViews}\n";
            }
        }
        else
        {
            _trackingDataLog = "Empty";
        }
        OnLog?.Invoke(_trackingDataLog);
        _notifyOnChange?.Invoke();
    }

    public void ClearManagerLog()
    {
        _managerLog = "";
    }

    public void ClearDownloadManagerLog()
    {
        _downloadManagerLog = "";
    }
}
