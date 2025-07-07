using UnityEngine;
using UnityEngine.UI;

public class SFXMuteButton : MonoBehaviour
{
    public SoundManager soundManager; // Assign in Inspector or auto-assign
    public Image buttonImage;         // Assign in Inspector or auto-assign
    public float fadedAlpha = 0.5f;
    public float fullAlpha = 1f;

    private void Start()
    {
        if (buttonImage == null)
            buttonImage = GetComponent<Image>();
        // Don't search for SoundManager here, do it in Update for continuous linking
        UpdateButtonVisual();
    }

    // Call this from the Button’s OnClick() event
    public void ToggleSfxMute()
    {
        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();
        if (soundManager == null) return;

        soundManager.sfxMute = !soundManager.sfxMute;
        UpdateButtonVisual();
    }

    void UpdateButtonVisual()
    {
        if (buttonImage != null && soundManager != null)
        {
            var c = buttonImage.color;
            c.a = soundManager.sfxMute ? fadedAlpha : fullAlpha;
            buttonImage.color = c;
        }
    }

    // Auto-find SoundManager and keep the visual in sync with the mute state
    void Update()
    {
        if (soundManager == null)
            soundManager = FindObjectOfType<SoundManager>();
        UpdateButtonVisual();
    }
}