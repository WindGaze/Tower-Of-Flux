using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DualListUIToggler : MonoBehaviour
{
    [Header("Objects to ACTIVATE on click")]
    public List<GameObject> objectsToActivate = new List<GameObject>();

    [Header("Objects to DEACTIVATE on click")]
    public List<GameObject> objectsToDeactivate = new List<GameObject>();

    [Header("Optional Settings")]
    public bool toggleMode = false; // If true, reverses activation on next click
    public Text statusText; // Optional UI feedback

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("No Button component found!", gameObject);
        }
    }

    void OnButtonClick()
    {
        // Activate all objects in the activation list
        foreach (var obj in objectsToActivate)
        {
            if (obj != null) obj.SetActive(!toggleMode);
        }

        // Deactivate all objects in the deactivation list
        foreach (var obj in objectsToDeactivate)
        {
            if (obj != null) obj.SetActive(toggleMode);
        }

        // Update optional status text
        if (statusText != null)
        {
            statusText.text = toggleMode ? "Disabled" : "Enabled";
        }

        // Toggle mode if needed
        if (toggleMode) toggleMode = !toggleMode;
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}