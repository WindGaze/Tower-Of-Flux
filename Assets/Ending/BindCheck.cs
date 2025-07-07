using UnityEngine;

public class BindCheck : MonoBehaviour
{
    [Header("Bind Objects")]
    [Tooltip("Object to activate when heartBind is true")]
    public GameObject heartBindObject;
    [Tooltip("Object to activate when soulBind is true")]
    public GameObject soulBindObject;
    [Tooltip("Object to activate when wailBind is true")]
    public GameObject wailBindObject;

    private void Start()
    {
        // Initial check when the script starts
        CheckBinds();
    }

    private void Update()
    {
        // Continuous check (optional - remove if you only need one-time check)
        CheckBinds();
    }

    private void CheckBinds()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager instance not found!");
            return;
        }

        // Heart Bind
        if (heartBindObject != null)
        {
            heartBindObject.SetActive(GameManager.Instance.heartBind);
        }

        // Soul Bind
        if (soulBindObject != null)
        {
            soulBindObject.SetActive(GameManager.Instance.soulBind);
        }

        // Wail Bind
        if (wailBindObject != null)
        {
            wailBindObject.SetActive(GameManager.Instance.wailBind);
        }
    }

    // Call this method if you want to manually trigger a bind check
    public void RefreshBindStatus()
    {
        CheckBinds();
    }
}