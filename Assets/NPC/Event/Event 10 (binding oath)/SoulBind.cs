using UnityEngine;
using UnityEngine.UI;

public class SoulBind : MonoBehaviour
{
    [Header("UI Settings")]
    public Canvas canvasToClose;      // Canvas to close when purchased
    public Button bindButton;         // Button to trigger the binding
    public int goldCost = 10;         // Gold cost for Soul Bind
    public Text costText;             // (Optional) UI Text to display cost

    [Header("Event Trigger")]
    public Event7 event7;             // Reference to relevant Event7 script (assign in Inspector)

    private PlayerMovement player;    // Reference to the player

    private void Start()
    {
        // Find the player
        player = FindObjectOfType<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError("SoulBind: No PlayerMovement script found in the scene!");
        }

        // Assign the button listener
        if (bindButton != null)
        {
            bindButton.onClick.AddListener(OnBindButtonClick);
        }
        else
        {
            Debug.LogError("SoulBind: Bind Button reference not set!");
        }

        // Set price text
        if (costText != null)
        {
            costText.text = $"Cost: {goldCost} Gold";
        }
    }

    private void OnBindButtonClick()
    {
        if (player == null)
        {
            Debug.LogError("SoulBind: Player reference not found!");
            return;
        }

        // Check for enough gold
        if (player.gold >= goldCost)
        {
            player.gold -= goldCost;
            Debug.Log($"SoulBind: Spent {goldCost} gold. Remaining gold: {player.gold}");

            // Activate Soul Bind
            player.soulBind = true;
            Debug.Log("SoulBind: Effect activated! Move speed reduced by 40%.");

            // Set Event7's isInteracted to true (if assigned)
            if (event7 != null)
            {
                event7.isInteracted = true;
            }

            // Close the canvas if assigned
            if (canvasToClose != null)
            {
                canvasToClose.enabled = false;
            }
        }
        else
        {
            Debug.Log("SoulBind: Not enough gold to purchase Soul Bind!");
        }
    }
}