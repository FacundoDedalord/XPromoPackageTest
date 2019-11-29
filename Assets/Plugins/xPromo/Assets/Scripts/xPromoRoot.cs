// Unity Framwork
using UnityEngine;

public class xPromoRoot : MonoBehaviorSingleton<xPromoRoot>
{
    public xPromoTestMenu debugUIPrefab;

    private bool showCampaignCalled = false;
    
    /// <summary>
    /// Unity Start Method
    /// </summary>
    void Start() {
        // Inject debug UI to hierarchy        
        #if DEBUG_XPROMO
        GameObject debugUI = GameObject.Instantiate<GameObject>(debugUIPrefab.gameObject, transform.parent);
        debugUI.name = debugUIPrefab.name;
        #endif
    }
    
    /// <summary>
    /// Unity Update Method
    /// </summary>
    void Update() {        
        if (!showCampaignCalled && xPromoManager.Instance.IsInitialized()) {
            showCampaignCalled = true;
            string campaignId = xPromoManager.Instance.GetNextCampaign(true);
            if (!string.IsNullOrEmpty(campaignId)) {
                xPromoManager.Instance.ShowCampaign(campaignId);
            }
        }
    }
}
