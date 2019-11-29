// .NET Framework
using System;
using System.Collections.Generic;

// Unity Framework
using UnityEngine;

public class xPromoCampaignTracking
{
    private const string xPromoTrackingKey = "xPromoTracking";
    private const string xPromoTrackingDayKey = "xPromoTrackingDay";

    private static DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    
    /// <summary>
    /// Read from local storage
    /// </summary>
    public static xPromoCampaignTrackingListData ReadFromLocal() {
        string json = PlayerPrefs.GetString(xPromoTrackingKey, string.Empty);

        if (!string.IsNullOrEmpty(json)) {
            xPromoCampaignTrackingListData trackingData = JsonUtility.FromJson<xPromoCampaignTrackingListData>(json);

            if (trackingData != null && trackingData.campaigns.Length > 0) {
                var lastDayMeasuredStr = PlayerPrefs.GetString(xPromoTrackingDayKey, string.Empty);
                
                if (!string.IsNullOrEmpty(lastDayMeasuredStr)) {
                    long lastDayMeasuredInSeconds = long.Parse(lastDayMeasuredStr);
                    DateTime lastDayMeasured = baseDate.ToLocalTime();
                    lastDayMeasured = lastDayMeasured.AddSeconds(lastDayMeasuredInSeconds);
                    var now = DateTime.Now;
                    bool isAnotherDay = (lastDayMeasured.Year != now.Year || lastDayMeasured.Month != now.Month || lastDayMeasured.Day != now.Day);

                    if (isAnotherDay) {
                        for (int i = 0; i < trackingData.campaigns.Length; i++) {
                            trackingData.campaigns[i].todayViews = 0;
                        }

                        lastDayMeasuredInSeconds = ((DateTimeOffset)lastDayMeasured).ToUnixTimeSeconds();
                        PlayerPrefs.SetString(xPromoTrackingDayKey, lastDayMeasuredInSeconds.ToString());
                        PlayerPrefs.Save();
                    }
                }
            }

            return trackingData;

        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Save the tracking of the campaigns to local storage
    /// </summary>
    public static void SaveToLocal(xPromoCampaignTrackingListData trackingData) {
        string json = JsonUtility.ToJson(trackingData);
        PlayerPrefs.SetString(xPromoTrackingKey, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Purge the campaign tracking data
    /// </summary>
    public static xPromoCampaignTrackingListData PurgeCampaigns(xPromoCampaignListData campaignListList, xPromoCampaignTrackingListData trackingData) {
        List<xPromoCampaignTrackingData> trackingDataList = new List<xPromoCampaignTrackingData>();

        for (int i = 0; i < campaignListList.games.Length; i++) {
            xPromoCampaignTrackingData trackingDataItem = GetTrackingDataItem(trackingData, campaignListList.games[i].campaignId);
            if (trackingDataItem == null) {
                trackingDataItem = new xPromoCampaignTrackingData {campaignId = campaignListList.games[i].campaignId, views = 0, todayViews = 0};
            }
            trackingDataList.Add(trackingDataItem);
        }

        xPromoCampaignTrackingListData newTrackingData = new xPromoCampaignTrackingListData();
        newTrackingData.campaigns = new xPromoCampaignTrackingData[trackingDataList.Count];
        for (int i = 0; i < trackingDataList.Count; i++) {
            newTrackingData.campaigns[i] = trackingDataList[i];
        }

        // Save to local the new tracking data
        SaveToLocal(newTrackingData);

        return newTrackingData;
    }

    private static xPromoCampaignTrackingData GetTrackingDataItem(xPromoCampaignTrackingListData trackingData, string campaignId) {
        if (trackingData == null) return null;

        for (int i = 0; i < trackingData.campaigns.Length; i++) {
            if (string.Compare(trackingData.campaigns[i].campaignId, campaignId, StringComparison.InvariantCulture) == 0) {
                return trackingData.campaigns[i];
            }
        }

        return null;
    }
}
