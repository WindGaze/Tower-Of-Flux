using UnityEngine;
using UnityEngine.UI;

public class PlayerGoldDisplay : MonoBehaviour
{
    public Text goldText; // Assign this in the Inspector
    private PlayerMovement playerMovement;

    void Start()
    {
        // Automatically finds the PlayerMovement in the scene
        playerMovement = FindObjectOfType<PlayerMovement>();
        UpdateGoldDisplay();
    }

    void Update()
    {
        UpdateGoldDisplay();
    }

    void UpdateGoldDisplay()
    {
        if (goldText != null && playerMovement != null)
        {
            goldText.text = playerMovement.gold.ToString();
        }
    }
}