using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public bool bgmMute;
    public bool sfxMute;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        ApplyBgmMute();
        ApplySfxMute();
    }

    void ApplyBgmMute()
    {
        GameObject[] bgms = GameObject.FindGameObjectsWithTag("BGM");
        foreach (GameObject bgm in bgms)
        {
            AudioSource[] sources = bgm.GetComponents<AudioSource>();
            foreach (AudioSource src in sources)
            {
                src.enabled = !bgmMute;
            }
        }
    }

    void ApplySfxMute()
    {
        AudioSource[] allSources = FindObjectsOfType<AudioSource>(true);
        foreach (AudioSource src in allSources)
        {
            // Only affect sources NOT on BGM-tagged objects
            if (!src.gameObject.CompareTag("BGM"))
            {
                src.enabled = !sfxMute;
            }
        }
    }
}