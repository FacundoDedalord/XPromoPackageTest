[System.Serializable]
public class xPromoCampaignData
{
    public string game;
    public string campaignId;
    public int maxViews;
    public int maxViewsDay;
    public int prob;
    public long startDate;
    public long endDate;
    public string video;
    public string gameScreenshot;
    public string gameLogo;
    public string buttonText;
    public string embeddedPrefab;
    public string linkiOS;
    public string linkSkillz;
    public string linkSamsung;

    // Calculated variables
    public System.DateTime startDateTime;
    public System.DateTime endDateTime;
}