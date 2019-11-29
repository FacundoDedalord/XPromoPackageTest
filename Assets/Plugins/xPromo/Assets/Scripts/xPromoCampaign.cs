
// Unity Framework
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class xPromoCampaign : MonoBehaviorSingleton<xPromoCampaign>
{
    // Reference to the still image game object
    public RawImage gameScreenshot;
    
    // Reference to the raw image game object
    public RawImage video;

    // Image that is over the button
    public RawImage gameLogo;

    // Reference to the textmesh pro text of the button
    public TextMeshProUGUI buttonText;

    // Reference to the videoclip, only if it's embedded
    // in the game
    public string videoClipName;
    
    /// <summary>
    /// Button clicked
    /// </summary>
    public void ButtonClick() {
        xPromoManager.Instance.TapOnCampaign();
    }

    public void Show() {
        GetComponent<Animator>().SetTrigger("show");
    }

    public void Hide() {
        GetComponent<Animator>().SetTrigger("hide");
    }
}
