using System.Collections;
using UnityEngine;

public class GrandSlime : MonoBehaviour
{
    public int damage = 20;
    public float moveSpeed = 2f;
    public int health = 5000;
    public float bulletCooldown = 12f;
    public float stopDuration = 2f;
    public GameObject bulletPrefab;
    public GameObject eightDirBulletPrefab;
    public float bulletSpeed = 5f;
    public float eightDirBulletSpeed = 5f;
    public float fireDelayAfterShoot = 3f;
    public float fireChance = 0.3f;
    public float fireWhileMovingInterval = 2f;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    public float speedMultiplierBelow2500HP = 1.5f;

    [Header("Audio Clips")]
    public AudioClip shootAudioClip;       // SFX for main big bullet
    public AudioClip eightDirAudioClip;    // SFX for 8-directional burst

    private Transform player;
    private bool isMoving = true;
    private bool isFiringBulletA = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator;
    private Collider2D myCollider;
    private AudioSource audioSource;
    private bool isDead = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Make sure it's tagged as 'Player'.");
            return;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
            myCollider = gameObject.AddComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(BossBehavior());
        StartCoroutine(FireWhileMoving());
    }

    private void Update()
    {
        if (isDead) return;
        if (isMoving && player != null)
        {
            MoveTowardsPlayer();
            animator.SetInteger("AnimState", 1);
        }
        else
        {
            animator.SetInteger("AnimState", 0);
        }
    }

    private void MoveTowardsPlayer()
    {
        float adjustedMoveSpeed = moveSpeed;
        if (health <= 1000)
            adjustedMoveSpeed *= speedMultiplierBelow2500HP;

        Vector2 direction = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, player.position, adjustedMoveSpeed * Time.deltaTime);
    }

    private IEnumerator BossBehavior()
    {
        while (health > 0)
        {
            if (health <= 2500)
            {
                bulletCooldown = 7f;
                fireChance = 0.6f;
            }

            yield return new WaitForSeconds(bulletCooldown);

            isMoving = false;
            isFiringBulletA = true;

            yield return new WaitForSeconds(stopDuration);

            // Play attack anim; FireBulletA should be called via anim event,
            // but you can call it here if not using animation events:
            FireBulletA();

            animator.SetInteger("AnimState", 2);

            yield return new WaitForSeconds(1f);

            animator.SetInteger("AnimState", 1);

            yield return new WaitForSeconds(fireDelayAfterShoot);
            isFiringBulletA = false;
            isMoving = true;
        }
    }

    private IEnumerator FireWhileMoving()
    {
        while (health > 0)
        {
            yield return new WaitForSeconds(fireWhileMovingInterval);

            if (isMoving && !isFiringBulletA && Random.value <= fireChance && !isDead)
            {
                FireEightDirectionalBullets();
            }
        }
    }

    // Call this from Animation Event during the stop/idle (main bullet shot)
    public void FireBulletA()
    {
        if (isDead || bulletPrefab == null || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = direction * bulletSpeed;

        // SFX for big bullet
        if (shootAudioClip != null && audioSource != null)
            audioSource.PlayOneShot(shootAudioClip);
    }

    private void FireEightDirectionalBullets()
    {
        if (isDead || eightDirBulletPrefab == null) return;

        float angleStep = 360f / 8;
        for (int i = 0; i < 8; i++)
        {
            float angle = i * angleStep;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            GameObject bullet = Instantiate(eightDirBulletPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = direction * eightDirBulletSpeed;
        }

        // SFX for eightway burst
        if (eightDirAudioClip != null && audioSource != null)
            audioSource.PlayOneShot(eightDirAudioClip);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Bullet"))
        {
            MonoBehaviour[] components = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                var type = component.GetType();

                var finalDamageProp = type.GetProperty("FinalDamage");
                if (finalDamageProp != null)
                {
                    int bulletDamage = (int)finalDamageProp.GetValue(component);
                    TakeDamage(bulletDamage);
                    break;
                }

                var damageField = type.GetField("damage");
                if (damageField != null)
                {
                    int bulletDamage = (int)damageField.GetValue(component);
                    TakeDamage(bulletDamage);
                    break;
                }
            }
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;
        Debug.Log("Grand Slime took damage! Remaining Health: " + health);

        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
        {
            isDead = true;
            Die();
        }
    }

    IEnumerator TemporaryDarkenEffect()
    {
        if (spriteRenderer != null)
        {
            Color darkenedColor = Color.Lerp(originalColor, Color.black, darkenAmount);
            spriteRenderer.color = darkenedColor;

            float elapsedTime = 0f;
            while (elapsedTime < colorRecoveryTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / colorRecoveryTime;
                spriteRenderer.color = Color.Lerp(darkenedColor, originalColor, t);
                yield return null;
            }

            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        Debug.Log("Grand Slime has been defeated!");
        isMoving = false;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        if (myCollider != null) myCollider.enabled = false;
        this.enabled = false;
        StartCoroutine(FadeOutAndDestroy(0.4f));
    }

    private IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
        if (spriteRenderer == null) { Destroy(gameObject); yield break; }
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
        while (elapsed < fadeDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = endColor;
        Destroy(gameObject);
    }
}