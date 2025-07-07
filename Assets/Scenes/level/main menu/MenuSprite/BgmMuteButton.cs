using UnityEngine;
using UnityEngine.UI;

public class BgmMuteButton : MonoBehaviour
{
    public SoundManager soundManager; 
    public Image buttonImage;         
    public float fadedAlpha = 0.5f;
    public float fullAlpha = 1f;

    void Start()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        UpdateButtonVisual();
    }

    // Call this from the Button’s OnClick() event
    public void ToggleBgmMute()
    {
        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();
        if (soundManager == null) return;

        soundManager.bgmMute = !soundManager.bgmMute;
        UpdateButtonVisual();
    }

    void UpdateButtonVisual()
    {
        if (buttonImage != null && soundManager != null)
        {
            var c = buttonImage.color;
            c.a = soundManager.bgmMute ? fadedAlpha : fullAlpha;
            buttonImage.color = c;
        }
    }

    // Auto-find SoundManager and keep the visual in sync
    void Update()
    {
        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();
        UpdateButtonVisual();
    }
}