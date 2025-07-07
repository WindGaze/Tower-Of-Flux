using UnityEngine;
using UnityEngine.UI;

public class GemDisplay : MonoBehaviour
{
    [Tooltip("Drag a UI Text element here to display gem count")]
    [SerializeField] private Text gemText;

    private void Update()
    {
        if (gemText != null)
        {
            // Display as: "Gem Obtained: 12" (if markerCollisions = 3)
            gemText.text = "Gem Obtained: " + (GameManager.Instance.markerCollisions * 4);
        }
    }
}