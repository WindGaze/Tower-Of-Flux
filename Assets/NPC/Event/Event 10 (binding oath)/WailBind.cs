using UnityEngine;
using UnityEngine.UI;

public class WailBind : MonoBehaviour
{
    [Header("UI Settings")]
    public Canvas canvasToClose;      // The Canvas to close when purchased
    public Button bindButton;         // Button that triggers the bind
    public int goldCost = 10;         // Gold cost for Wail Bind
    public Text costText;             // (Optional) UI Text to display price

    [Header("Event Trigger")]
    public Event7 event7;             // Reference to Event7 (assign in Inspector)

    private PlayerMovement player;    // Reference to PlayerMovement script

    private void Start()
    {
        // Find the player object with PlayerMovement in the scene
        player = FindObjectOfType<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError("WailBind: No PlayerMovement script found in the scene!");
        }

        // Register click event for the bind button
        if (bindButton != null)
        {
            bindButton.onClick.AddListener(OnBindButtonClick);
        }
        else
        {
            Debug.LogError("WailBind: Bind Button reference not set!");
        }

        // Set the cost text if assigned
        if (costText != null)
        {
            costText.text = $"Cost: {goldCost} Gold";
        }
    }

    private void OnBindButtonClick()
    {
        if (player == null)
        {
            Debug.LogError("WailBind: Player reference not found!");
            return;
        }

        // Check if player has enough gold to purchase Wail Bind
        if (player.gold >= goldCost)
        {
            // Deduct gold
            player.gold -= goldCost;
            Debug.Log($"WailBind: Spent {goldCost} gold. Remaining gold: {player.gold}");

            // Activate the Wail Bind effect
            player.wailBind = true;
            Debug.Log("WailBind: Effect activated! Player's bullet damage reduced by 50%.");

            // Set Event7's isInteracted to true if assigned
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
            Debug.Log("WailBind: Not enough gold to purchase Wail Bind!");
        }
    }
}