using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int health = 100;          // Health of the player
    public int gold = 40;     
    public int gem = 40;       
    public int maxHealth = 100;       // Maximum health based on level
    public float invincibilityDuration = 1f;
    public Color invincibilityColor = new Color(0.5f, 0.5f, 0.5f, 1f);
    public float reversedControlDuration = 5f;
    public float reverseChance = 0.5f;
    public bool luckBuff = false;
    public bool luckBuff2 = false;
    private Animator animator;
    public bool player = true;
    public bool isShield = false;
    public bool isRevive = false;
    public float reviveInvincibilityDuration = 3f;
    public bool bulletFlask = false;
    public int bonusDamage = 0;
    public bool isFrozen = false;

    [Header("Death Transition")]
    public GameObject deathCanvas; // Assign this in the Inspector
    private CanvasGroup deathCanvasGroup;
    public float deathTransitionFadeDuration = 1.5f;
    public float delayBeforeFade = 1.5f;

    private bool isDead = false;

    public AudioClip hurtClip; // Assign this in the Inspector
    public float minPitch = 0.85f;
    public float maxPitch = 1.15f;
    private AudioSource audioSource;

    private Vector2 currentVelocity;
    public float smoothTime = 0.1f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public GameObject dashGhostPrefab;
    [Header("Speed Settings")]
    public float speedIncreasePerLevel = 0.4f; // Now properly used in calculations
    public float baseMoveSpeed = 5f; // Explicit base speed
    public AudioClip dashClip;

    public bool wailBind = false;
    public bool soulBind= false;

    private bool isDashing = false;
    private float dashTimeLeft;
    public float lastDashTime;

    [SerializeField]
    private int _healthBoost = 0;
    public int healthBoost
    {
        get => _healthBoost;
        set
        {
            _healthBoost = value;
            maxHealth = 100 + (_playerLevel - 1) * healthIncreasePerLevel + _healthBoost;
        }
    }

    [SerializeField]
    private int _playerLevel = 1;
    public int playerLevel
    {
        get { return _playerLevel; }
        set
        {
            _playerLevel = value;
            UpdateHealthBasedOnLevel(); 
        }
    }

    [SerializeField]
    private int _speedLevel = 1; // Default player speed
    public int speedLevel
    {
        get { return _speedLevel; }
        set
        {
            _speedLevel = value;
            UpdatePlayerMovement(); // Sync speed with PlayerMovement
        }
    }

    [SerializeField]
    private int _invincibilityLevel = 0; // Default invincibility level
    public int invincibilityLevel
    {
        get { return _invincibilityLevel; }
        set
        {
            _invincibilityLevel = value;
            UpdatePlayerInvincible(); // Sync invincibility level with PlayerMovement
        }
    }

    private int _speedBoost = 0;
    public int speedBoost
    {
        get { return _speedBoost; }
        set
        {
            _speedBoost = value;
            UpdatePlayerMovement(); // Ensure moveSpeed is updated
        }
    }

    [SerializeField]
    private bool _heartBind = false;
    private bool _heartBindTriggered = false; // Track if heart bind has been triggered

    public bool heartBind
    {
        get => _heartBind;
        set
        {
            if (_heartBind != value)
            {
                _heartBind = value;
                
                if (_heartBind && !_heartBindTriggered)
                {
                    // Apply heart bind effect (reduce max health by 50%)
                    int newMaxHealth = Mathf.Max(1, maxHealth / 2); // Ensure at least 1 HP
                    health = Mathf.Min(health, newMaxHealth); // Adjust current health if needed
                    maxHealth = newMaxHealth;
                    _heartBindTriggered = true; // Mark as triggered
                    Debug.Log("Heart Bind applied! Max health reduced to " + maxHealth);
                }
                else if (!_heartBind)
                {
                    Debug.Log("Heart Bind removed!");
                    // Note: Max health won't be restored automatically
                }
            }
        }
    }

    private void OnValidate()
    {
        if (_heartBind && !_heartBindTriggered)
        {
            ApplyHeartBindEffect();
            _heartBindTriggered = true;
        }
    }
    private void PlayDashSound()
    {
        if (dashClip != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(dashClip, 1f);
        }
    }
    private void ApplyHeartBindEffect()
    {
        int newMaxHealth = Mathf.Max(1, maxHealth / 2);
        health = Mathf.Min(health, newMaxHealth);
        maxHealth = newMaxHealth;
        _heartBindTriggered = true;
        Debug.Log("Heart Bind applied via Inspector! Max health reduced to " + maxHealth);
    }

    public int maxPlayerLevel = 10;   // Max player level
    public int healthIncreasePerLevel = 50; // Health increase per level

    // Public variables to adjust invincibility duration level
    public float invincibilityIncreasePerLevel = 0.5f; // Invincibility duration increase per level

    private Vector2 movement;
    private Rigidbody2D rb;
    private bool isInvincible = false;
    private bool controlsReversed = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private static PlayerMovement instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Register scene change event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")  // Replace with the name of your specific scene
        {
            Destroy(gameObject);  // Destroy this object if it's the unwanted scene
        }
        else
        {
            speedBoost = 0;
            isRevive = false;
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // Initialize the Animator
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the player object!");
        }
        else
        {
            originalColor = spriteRenderer.color;
        }
        if (deathCanvas != null)
        {
            deathCanvasGroup = deathCanvas.GetComponent<CanvasGroup>();
            if (deathCanvasGroup == null)
                deathCanvasGroup = deathCanvas.AddComponent<CanvasGroup>();
            deathCanvas.SetActive(false);
            deathCanvasGroup.alpha = 0f;
        }
        // Sync initial level with GameManager if it exists
        if (GameManager.Instance != null)
        {
            SyncWithGameManager();
        }

        UpdateSpeedAndInvincibility(); // Update speed and invincibility duration on start
    }
    public void PlayHurtSoundWithRandomPitch()
    {
        if (hurtClip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(hurtClip, 1f); // 1f = normal volume
        }
    }
    private void UpdateHealthBasedOnLevel()
    {
        maxHealth = 100 + (_playerLevel - 1) * healthIncreasePerLevel + healthBoost;
        health = Mathf.Min(health, maxHealth);
    }

    public void UpdatePlayerMovement()
    {
        // Base speed calculation (5f base + 0.4 per level)
        float baseSpeed = 5f + (speedLevel - 1) * speedIncreasePerLevel;
        
        // Apply soul bind reduction if active
        moveSpeed = soulBind ? baseSpeed * 0.6f : baseSpeed;
        
        // Apply speed boost (if any)
        moveSpeed += speedBoost;

        Debug.Log($"Speed updated - Level: {speedLevel} | Base: {baseSpeed} | Final: {moveSpeed}");
    }


    private void UpdatePlayerInvincible()
    {
        invincibilityDuration = 1f + (invincibilityLevel - 1) * 0.5f; 
        Debug.Log($"Invincibility level: {invincibilityLevel}. New invincibility duration: {invincibilityDuration}");
    }

    // New method to sync with GameManager
    public void SyncWithGameManager()
    {
        if (GameManager.Instance != null)
        {
            // Update level and recalculate health
            playerLevel = GameManager.Instance.playerLevel;
            maxHealth = 100 + (playerLevel - 1) * healthIncreasePerLevel;
            health = maxHealth;

            // Sync speed level
            speedLevel = GameManager.Instance.speedLevel;
            invincibilityLevel = GameManager.Instance.invincibilityLevel;
            UpdateSpeedAndInvincibility(); // Update speed based on the new speed level
        }
    }

    public void SyncSpeedFromGameManager()
    {
        moveSpeed = GameManager.Instance.speedLevel;
    }

    private void UpdateSpeedAndInvincibility()
    {
        // Dynamically increase speed and invincibility duration based on their own levels
        moveSpeed = 5f + (speedLevel - 1) * speedIncreasePerLevel;
        invincibilityDuration = 1f + (invincibilityLevel - 1) * invincibilityIncreasePerLevel;

        Debug.Log($"Speed level: {speedLevel}. New move speed: {moveSpeed}. Invincibility level: {invincibilityLevel}. New invincibility duration: {invincibilityDuration}");
    }

    void Update()
    {
        if (isFrozen) 
        {
            rb.velocity = Vector2.zero;
            return;
        }
        movement.x = Input.GetAxis("Horizontal") * (controlsReversed ? -1 : 1);
        movement.y = Input.GetAxis("Vertical") * (controlsReversed ? -1 : 1);
        isShield = GameObject.FindWithTag("Shield") != null;
        // Update Animator based on movement
        if (movement.x != 0 || movement.y != 0)
        {
            animator.SetInteger("AnimState", 1); // Player is moving
        }
        else
        {
            animator.SetInteger("AnimState", 0); // Player is idle
        }
        // Flip the sprite based on movement direction
        if (movement.x > 0) // Moving right
        {
            spriteRenderer.flipX = false; // Ensure the sprite is not flipped
        }
        else if (movement.x < 0) // Moving left
        {
            spriteRenderer.flipX = true; // Flip the sprite horizontally
        }

        if (bulletFlask)
        {
            SyncBonusDamageToBullets();
        }

        // Revive logic was here previously, now moved safely to TakeDamage()
        
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown && movement != Vector2.zero)
        {
            PlayDashSound();
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        while (dashTimeLeft > 0)
        {
            rb.velocity = movement.normalized * dashSpeed;
            SpawnDashGhost();
            dashTimeLeft -= Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        isDashing = false;
    }
    void SpawnDashGhost()
    {
        if (dashGhostPrefab != null)
        {
            GameObject ghost = Instantiate(dashGhostPrefab, transform.position, Quaternion.identity);
            SpriteRenderer ghostSr = ghost.GetComponent<SpriteRenderer>();
            SpriteRenderer playerSr = GetComponent<SpriteRenderer>();

            ghostSr.sprite = playerSr.sprite;
            ghostSr.flipX = playerSr.flipX;
            ghostSr.color = new Color(0f, 0f, 0f, 0.5f); // dark translucent

            Destroy(ghost, 0.3f); // destroy after fade out
        }
    }

    private void SyncBonusDamageToBullets()
    {
        StraightBullet[] bullets = FindObjectsOfType<StraightBullet>();

        foreach (StraightBullet bullet in bullets)
        {
            if (bullet.gunBullet) // Only apply to player bullets
            {
                bullet.bonusDamage = bonusDamage;
            }
        }
    }
    void FixedUpdate()
    {
        if (!isDashing)
        {
            Vector2 targetVelocity = movement * moveSpeed;
            Vector2 smoothedVelocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref currentVelocity, smoothTime);
            rb.velocity = smoothedVelocity;
        }
    }

   private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isShield) return;
        // Check for Enemy, EnemyBullet, ConfuseBullet, or Trap
        if (collision.gameObject.CompareTag("EnemyBullet") || 
            collision.gameObject.CompareTag("Enemy") ||                
            collision.gameObject.CompareTag("Boss") || 
            collision.gameObject.CompareTag("ConfuseBullet") ||
            collision.gameObject.CompareTag("Trap") ||
            collision.gameObject.CompareTag("DetonatorExplosion"))
        {
            if (!isInvincible)
            {
                // Check if LuckBuff is active and if we should nullify the damage
                bool nullifyDamage = false;
                
                if (luckBuff2)
                {
                    nullifyDamage = Random.value <= 0.7f; // 70% chance to nullify damage if luckBuff2 is active
                }
                else if (luckBuff)
                {
                    nullifyDamage = Random.value <= 0.5f; // 50% chance to nullify damage if luckBuff is active
                }
                
                if (!nullifyDamage)
                {
                    Debug.Log("Player got hit!");
                    
                    // Handle different types of collisions
                   if (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("ConfuseBullet"))
                    {
                        var bullet = collision.gameObject.GetComponent<EnemyBullet>();
                        if (bullet != null)
                        {
                            TakeDamage(bullet.damage);
                        } 
                        else
                        {
                            var progBullet = collision.gameObject.GetComponent<ProgenitorBullet>();
                            if (progBullet != null)
                            {
                                TakeDamage(progBullet.damage);
                            }
                        }
                        Destroy(collision.gameObject);
                    }
                    else if (collision.gameObject.CompareTag("DetonatorExplosion"))
                    {
                        var explosion = collision.gameObject.GetComponent<DetonatorExplosion>();
                        if (explosion != null)
                        {
                            TakeDamage(explosion.damage);
                        }
                        // Don't destroy the explosion - let it continue
                    }
                    else if (collision.gameObject.CompareTag("Enemy"))
                    {
                        HandleEnemyCollision(collision);
                    }
                    else if (collision.gameObject.CompareTag("Boss"))
                    {
                        HandleEnemyCollision(collision);
                    }
                    else if (collision.gameObject.CompareTag("Trap"))
                    {
                        var trap = collision.gameObject.GetComponent<Trap>();
                        if (trap != null)
                        {
                            TakeDamage(trap.damage);
                        }
                        // Trap doesn't get destroyed
                    }
                    
                    // If hit by a ConfuseBullet, apply reverse controls chance
                    if (collision.gameObject.CompareTag("ConfuseBullet") && Random.value <= reverseChance)
                    {
                        StartCoroutine(ReverseControls());
                    }
                    
                    // Activate invincibility after taking damage
                    StartCoroutine(ActivateInvincibility(invincibilityDuration));                
                }
                else
                {
                    Debug.Log("Lucky! Damage nullified!");
                    
                    // Destroy bullet if it was a bullet type (but not traps or explosions)
                    if (collision.gameObject.CompareTag("EnemyBullet") || collision.gameObject.CompareTag("ConfuseBullet"))
                    {
                        Destroy(collision.gameObject);
                    }
                }
            }
        }
    }

    private void HandleEnemyCollision(Collision2D collision)
    {
        // Get the MonoBehaviour component (could be any enemy or trap with a damage property)
        var damageable = collision.gameObject.GetComponent<MonoBehaviour>();
        if (damageable != null)
        {
            // Check if the component has a 'damage' field or property and apply damage if so
            var damageField = damageable.GetType().GetField("damage");
            if (damageField != null)
            {
                int damageAmount = (int)damageField.GetValue(damageable);
                TakeDamage(damageAmount); // Apply damage to the player
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        if (isShield) return;

        health -= damageAmount;
        Debug.Log("Player took damage! Remaining Health: " + health);

        if (health <= 0)
        {
            // Revive logic: handle immediately when health <= 0 and isRevive active
            if (isRevive)
            {
                Debug.Log("Revive activated! Health restored to 50");
                isRevive = false;
                health = 50;
                StartCoroutine(ActivateInvincibility(reviveInvincibilityDuration));
                return;
            }
            OnPlayerDeath();
        }
    }
    private void OnPlayerDeath()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died");

        // 1. Play death animation
        if (animator != null)
            animator.SetInteger("AnimState", 3);

        // 2. Disable PlayerMovement *script* to freeze further input
        this.enabled = false;
        // Disable ALL gun scripts (not objects) in the whole scene
        foreach (var gun in FindObjectsOfType<StraightGun>(true)) gun.enabled = false;
        foreach (var gun in FindObjectsOfType<SingleGun>(true)) gun.enabled = false;
        foreach (var gun in FindObjectsOfType<SpreadGun>(true)) gun.enabled = false;
        foreach (var gun in FindObjectsOfType<AOEGun>(true)) gun.enabled = false;
        Debug.Log("All gun scripts disabled on player death.");
        // 4. Show and fade-in death canvas after delay
        if (deathCanvas != null && deathCanvasGroup != null)
        {
            deathCanvas.SetActive(true);
            StartCoroutine(FadeInDeathCanvas());
        }

        // 5. Destroy all enemies and bosses
        DestroyAllEnemiesAndBosses();
    }
    
    private IEnumerator FadeInDeathCanvas()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        deathCanvasGroup.alpha = 0f;
        float t = 0f;
        while (t < deathTransitionFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            deathCanvasGroup.alpha = Mathf.Clamp01(t / deathTransitionFadeDuration);
            yield return null;
        }
        deathCanvasGroup.alpha = 1f;
    }
    private void DestroyAllEnemiesAndBosses()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        foreach (GameObject obj in enemies)
            Destroy(obj);
        foreach (GameObject obj in bosses)
            Destroy(obj);
        Debug.Log($"Destroyed {enemies.Length} enemies and {bosses.Length} bosses on player death.");
    }
   private IEnumerator ActivateInvincibility(float duration)
    {
        if (isShield) yield break;
        isInvincible = true;
        Debug.Log("Player is now invincible!");

        // Change player color to indicate invincibility
        if (spriteRenderer != null)
        {
            spriteRenderer.color = invincibilityColor;
        }

        // Disable collisions with Enemy and EnemyBullet layers
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("EnemyBullet"), true);

        yield return new WaitForSeconds(duration);

        // Re-enable collisions
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("EnemyBullet"), false);

        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvincible = false;
        Debug.Log("Player can be hit again!");
    }

    private IEnumerator ReverseControls()
    {
        if (isShield) yield break;
        controlsReversed = true;
        Debug.Log("Controls have been reversed!");

        yield return new WaitForSeconds(reversedControlDuration);

        controlsReversed = false;
        Debug.Log("Controls are back to normal!");
    }

    // Modified LevelUp method to sync with GameManager
    public void LevelUp()
    {
        if (playerLevel < maxPlayerLevel)
        {
            playerLevel++;
            maxHealth += healthIncreasePerLevel;
            health = maxHealth; // Instantly refill health to max
            Debug.Log("Player leveled up to " + playerLevel + "! Max Health: " + maxHealth);
        }
        else
        {
            Debug.Log("Player has reached the maximum level!");
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"Added {amount} gold. New total: {gold}");
    }

    public void ResetPlayerState()
    {
        // Restore all main stats and statuses to your default
        health = maxHealth;
        gold = 40;
        gem = 50;
        isDead = false;
        isShield = false;
        isRevive = false;
        bulletFlask = false;
        bonusDamage = 0;
        wailBind = false;
        soulBind = false;
        speedBoost = 0;
        luckBuff = false;
        luckBuff2 = false;
        controlsReversed = false;
        heartBind = false;
        _heartBindTriggered = false;
        Debug.Log("Player stats reset by retry.");
    }
}