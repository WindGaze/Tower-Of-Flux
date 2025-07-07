using UnityEngine;
using System.Collections;

// Attach this script to your SlowTile prefab/object
public class SlowTile : MonoBehaviour
{
    public float speedReductionPercent = 0.4f; // 40% speed reduction
    public float slowDuration = 3f;
    private Collider2D tileCollider;

    [Header("Audio Settings")]
    public AudioSource slowTileAudio; // Assign in Inspector

    private void Awake()
    {
        tileCollider = GetComponent<Collider2D>();
        if (tileCollider == null)
        {
            Debug.LogError("No Collider2D found on the SlowTile object!");
        }
        else
        {
            tileCollider.isTrigger = true; // Using trigger collisions
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Early exit for ignored tags
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet") || other.CompareTag("Bullet"))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            // Play audio if assigned
            if (slowTileAudio != null)
            {
                slowTileAudio.Play();
            }

            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Check if player is already affected by a slow effect
                SlowEffect existingSlowEffect = playerMovement.GetComponent<SlowEffect>();

                if (existingSlowEffect != null)
                {
                    // Reset the timer on the existing slow effect
                    existingSlowEffect.ResetTimer();
                    Debug.Log("Slow effect timer reset to 3 seconds");
                }
                else
                {
                    // Add a new slow effect component to the player
                    SlowEffect newEffect = playerMovement.gameObject.AddComponent<SlowEffect>();
                    newEffect.Initialize(playerMovement, speedReductionPercent, slowDuration);
                }
            }
        }
    }
}

// Helper class to manage the slow effect and its timer
public class SlowEffect : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private float speedReductionPercent;
    private float slowDuration;
    private float timer;
    private float originalSpeed;

    public void Initialize(PlayerMovement player, float reduction, float duration)
    {
        playerMovement = player;
        speedReductionPercent = reduction;
        slowDuration = duration;
        timer = duration;

        // Store original speed and apply reduction
        originalSpeed = playerMovement.moveSpeed;
        playerMovement.moveSpeed = originalSpeed * (1f - speedReductionPercent);

        Debug.Log($"Player speed reduced from {originalSpeed} to {playerMovement.moveSpeed}");
    }

    public void ResetTimer()
    {
        timer = slowDuration;
    }

    private void Update()
    {
        // Count down the timer
        timer -= Time.deltaTime;

        // When timer expires, restore speed and remove this component
        if (timer <= 0)
        {
            playerMovement.moveSpeed = originalSpeed;
            Debug.Log($"Slow effect expired. Player speed restored to {originalSpeed}");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        // Ensure speed is restored if component is destroyed for any reason
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = originalSpeed;
        }
    }
}