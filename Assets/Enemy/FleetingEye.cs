using System.Collections;
using UnityEngine;

public class FleetingEye : MonoBehaviour
{
    public float speed = 2f;                 // Normal movement speed of the enemy
    public int damage = 25;
    public int health = 50;                  // Health of the enemy
    public float darkenAmount = 0.5f;        // Amount to darken the enemy when hit
    public float colorRecoveryTime = 0.5f;   // Time to recover original color after being hit
    public float moveToPlayerInterval = 4f;  // Time interval before moving toward the player
    public float followPlayerDuration = 5f;  // Time spent following the player

    private Transform player;                // Reference to the player object
    private Collider2D areaBoundary;         // Collider of the SpawnAnchor (the parent object)
    private Vector2 direction;               // Current direction of movement
    private bool followingPlayer = false;    // Is the enemy currently following the player
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool collidedWithPlayer = false; // If the enemy has collided with the player

    // ***** ADDED: Collision-related variables *****
    private float collisionCheckDistance = 0.1f; // Distance to check for collisions
    private ContactFilter2D wallFilter;          // Filter to detect only walls

    void Start()
    {
        // Find the parent object's Collider2D (SpawnAnchor's Collider)
        areaBoundary = transform.parent.GetComponent<Collider2D>();
        if (areaBoundary == null)
        {
            Debug.LogError("FleetingEye's parent (SpawnAnchor) does not have a Collider2D component!");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on FleetingEye!");
            return;
        }
        originalColor = spriteRenderer.color;

        // Find the player by tag (make sure the player is tagged as "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Make sure the Player is tagged correctly.");
        }

        // ***** ADDED: Initialize collision detection *****
        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(LayerMask.GetMask("Wall")); // Only detect objects on the "Wall" layer
        wallFilter.useLayerMask = true;

        // Start moving in a random direction
        ChooseRandomDirection();

        // Start moving towards random positions, and move towards the player at intervals
        StartCoroutine(MoveToPlayerEveryInterval());
    }

    void Update()
    {
        // Always move in the current direction if not following the player
        if (!followingPlayer)
        {
            MoveInDirection();
        }
    }

    // Choose a random direction to move in, in all directions (omni-directional)
    void ChooseRandomDirection()
    {
        // Random direction on X and Y
        direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    // Move in the current direction
    void MoveInDirection()
    {
        Vector2 currentPosition = transform.position;
        Vector2 newPosition = currentPosition + direction * speed * Time.deltaTime;

        // ***** ADDED: Check for collisions in the movement direction *****
        if (!IsPathBlocked(direction))
        {
            // Ensure the movement stays within the bounds of the areaBoundary collider
            if (areaBoundary.OverlapPoint(newPosition))
            {
                transform.position = newPosition;
            }
            else
            {
                // If the new position is out of bounds, choose a new random direction
                ChooseRandomDirection();
            }
        }
        else
        {
            // If the path is blocked, choose a new random direction
            ChooseRandomDirection();
        }

        // Randomly change direction periodically for continuous wandering
        if (Random.Range(0f, 1f) < 0.01f)  // 1% chance to change direction each frame
        {
            ChooseRandomDirection();
        }
    }

    // Coroutine to move toward the player every few seconds
    IEnumerator MoveToPlayerEveryInterval()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveToPlayerInterval); // Wait for interval (6s)
            
            if (player != null && !collidedWithPlayer)
            {
                yield return StartCoroutine(MoveTowardPlayerForDuration()); // Wait for follow duration (4s)
            }
        }
    }

    // Move towards the player for a certain duration
    IEnumerator MoveTowardPlayerForDuration()
    {
        followingPlayer = true;
        float elapsedTime = 0f;

        while (elapsedTime < followPlayerDuration && !collidedWithPlayer)
        {
            elapsedTime += Time.deltaTime;
            if (player != null)
            {
                Vector2 playerDirection = (player.position - transform.position).normalized;

                // ***** ADDED: Check for collisions in the player direction *****
                if (!IsPathBlocked(playerDirection))
                {
                    Vector2 newPosition = (Vector2)transform.position + playerDirection * speed * Time.deltaTime;

                    // Ensure the movement towards the player stays within the bounds
                    if (areaBoundary.OverlapPoint(newPosition))
                    {
                        transform.position = newPosition;
                    }
                    else
                    {
                        // If the position is out of bounds, stop following and return to random wandering
                        followingPlayer = false;
                        ChooseRandomDirection();
                        yield break;
                    }
                }
            }

            yield return null;
        }

        followingPlayer = false;
        collidedWithPlayer = false;  // Reset collision state after following player
        ChooseRandomDirection();     // Resume random wandering
    }

    // ***** ADDED: Method to check if the path is blocked by a wall *****
    bool IsPathBlocked(Vector2 direction)
    {
        // Perform a raycast in the movement direction to check for walls
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = Physics2D.Raycast(transform.position, direction, wallFilter, hits, collisionCheckDistance);

        // Debug visualization (optional)
        Debug.DrawRay(transform.position, direction * collisionCheckDistance, Color.red);

        // If the raycast hits something, the path is blocked
        if (hitCount > 0)
        {
            return true;
        }

        return false; // Path is clear
    }

    // Take damage, darken the sprite, and eventually die
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log("FleetingEye took damage! Remaining Health: " + health);

        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
        {
            Die();
        }
    }

    // Coroutine to temporarily darken the sprite when hit
    IEnumerator TemporaryDarkenEffect()
    {
        if (spriteRenderer != null)
        {
            // Darken the sprite
            Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
            spriteRenderer.color = darkenedColor;

            // Gradually return to original color
            float elapsedTime = 0f;
            while (elapsedTime < colorRecoveryTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / colorRecoveryTime;
                spriteRenderer.color = Color.Lerp(darkenedColor, originalColor, t);
                yield return null;
            }

            // Ensure the sprite ends up with the exact original color
            spriteRenderer.color = originalColor;
        }
    }

    // Destroy the enemy object when it dies
    void Die()
    {
        Debug.Log("FleetingEye has died!");
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // Use TryGetComponent to check for any MonoBehaviour that has a 'damage' field
            MonoBehaviour[] components = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                System.Reflection.FieldInfo damageField = component.GetType().GetField("damage");
                if (damageField != null)
                {
                    int bulletDamage = (int)damageField.GetValue(component);
                    TakeDamage(bulletDamage);
                    break; // Use the first damage value found
                }
            }

            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            // Stop following the player if contact is made and resume wandering
            collidedWithPlayer = true;
            followingPlayer = false;
            ChooseRandomDirection();  // Resume random wandering after collision with player
        }
    }
}