using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // Forces this GameObject to have a Button component
public class CloseButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Canvas/Panel to close when clicked")]
    [SerializeField] private GameObject targetCanvas; // Can be a Canvas or Panel GameObject

    private Button closeButton;

    private void Awake()
    {
        // Get the Button component
        closeButton = GetComponent<Button>();

        // Assign the close function to the button's onClick event
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCanvas);
        }
        else
        {
            Debug.LogError("CloseButton script requires a Button component!");
        }
    }

    private void CloseCanvas()
    {
        if (targetCanvas != null)
        {
            targetCanvas.SetActive(false); // Disable the Canvas/Panel
        }
        else
        {
            Debug.LogWarning("No target Canvas/Panel assigned to CloseButton!");
        }
    }

    private void OnDestroy()
    {
        // Clean up the listener when the object is destroyed
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseCanvas);
        }
    }
}