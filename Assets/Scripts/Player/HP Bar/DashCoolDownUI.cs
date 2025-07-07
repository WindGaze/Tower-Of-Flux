using UnityEngine;
using UnityEngine.UI;

public class DashCooldownUI : MonoBehaviour
{
    private PlayerMovement player;            // reference to dash logic
    public Image dashIcon;                    // assign in inspector
    public Color readyColor = Color.white;    // color when dash ready
    public Color cooldownColor = new Color(1f, 1f, 1f, 0.3f); // dim color

    void Awake()
    {
        player = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        if (player == null || dashIcon == null) return;

        // Check if dash is on cooldown
        bool isOnCooldown = Time.time < player.lastDashTime + player.dashCooldown;

        // Set color based on dash state
        dashIcon.color = isOnCooldown ? cooldownColor : readyColor;
    }
}