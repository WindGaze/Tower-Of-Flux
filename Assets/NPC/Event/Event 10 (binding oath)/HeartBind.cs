using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HeartBind : MonoBehaviour
{
    [Header("UI Settings")]
    public Canvas canvasToClose;       // Canvas to disable when clicked
    public Button bindButton;          // The button that triggers the bind
    public int goldCost = 10;          // Price to activate HeartBind
    public Text costText;              // (Optional) UI text for price
    public Event7 event7;  // Assign in Inspector or Find at runtime
    
    private PlayerMovement player;

    private void Start()
    {
        // Find the player
        player = FindObjectOfType<PlayerMovement>();
        
        // Setup button click listener
        if (bindButton != null)
        {
            bindButton.onClick.AddListener(OnBindButtonClick);
        }
        else
        {
            Debug.LogError("Bind Button reference not set in HeartBind script!");
        }

        // Display cost if text element exists
        if (costText != null)
        {
            costText.text = $"Cost: {goldCost} Gold";
        }
    }

   private void OnBindButtonClick()
{
    if (player != null)
    {
        if (player.gold >= goldCost)
        {
            player.gold -= goldCost;
            player.heartBind = true;
            Debug.Log($"Heart Bind activated! Player's max health halved. Spent {goldCost} gold. Remaining: {player.gold}");
            if (canvasToClose != null)
            {
                canvasToClose.enabled = false;
            }

            // Set isInteracted to true on the Event7 instance
            if (event7 != null)
            {
                event7.isInteracted = true;
            }
        }
        else
        {
            Debug.Log("Not enough gold for Heart Bind!");
        }
    }
    else
    {
        Debug.LogError("PlayerMovement not found!");
    }
}
}