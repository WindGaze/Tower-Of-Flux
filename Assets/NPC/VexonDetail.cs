using UnityEngine;
using UnityEngine.UI;

public class VexonDetail : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button showButton;    // The button that triggers the panel
    [SerializeField] private GameObject detailPanel; // The panel to show (child of Canvas)

    private void Start()
    {
        // Make sure panel starts hidden
        if (detailPanel != null)
        {
            detailPanel.SetActive(false);
        }

        // Setup button click event
        if (showButton != null)
        {
            showButton.onClick.AddListener(ShowPanel);
        }
        else
        {
            Debug.LogError("Show Button is not assigned in the inspector!");
        }
    }

    private void ShowPanel()
    {
        if (detailPanel != null)
        {
            detailPanel.SetActive(true); // Show the panel
        }
    }

    private void OnDestroy()
    {
        // Clean up the listener when object is destroyed
        if (showButton != null)
        {
            showButton.onClick.RemoveListener(ShowPanel);
        }
    }
}