using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AncestralSpectre : MonoBehaviour
{
    private bool hasTriggeredGhostPhase = false;

    public int health = 5000;
    public int attack = 25;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    private int maxSpikes = 6;           // Maximum number of spikes to spawn
    private Animator animator;

    public GameObject spikePrefab;
    public GameObject laserPrefab;
    public GameObject bulletPrefab;
    public GameObject[] enemyPrefabs;    // Array for different enemy types
    public float spreadAngle = 15f;

    public Transform teleportPoint1;
    public Transform teleportPoint2;
    public Transform teleportPoint3;

    public float laserSpawnInterval = 5f;
    public float shotgunShotInterval = 3f;
    public float multiDirectionalShotInterval = 7f;
    public float teleportInterval = 7f;

    public Transform spawnAnchor;    // Where the enemies will be spawned
    private int maxEnemies = 5;      // Max enemies to spawn
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // List of spawned enemies
    public int minEnemies = 4;       // Min enemies to spawn
    public float minSpawnDistance = 2f;  // Min distance from the player
    public float minWallDistance = 1.5f; // Min distance from walls

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D[] colliders;
    private Collider2D spawnAreaCollider;
    private bool isInvisible = false;

    [Header("Boss Audio")]
    public AudioClip laserAudioClip;
    public AudioClip bulletAudioClip;
    public AudioClip teleportAudioClip;
    public float minPitch = 1.2f;
    public float maxPitch = 1.8f;

    private AudioSource audioSource;

    private void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();
        spawnAreaCollider = spawnAnchor.GetComponent<Collider2D>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(BossAttackSequence());
    }

    private IEnumerator BossAttackSequence()
    {
        while (health > 0)
        {
            if (health <= 1500 && !hasTriggeredGhostPhase)
            {
                yield return StartCoroutine(HandleGhostPhase());
                hasTriggeredGhostPhase = true;
                continue;
            }

            yield return new WaitForSeconds(5f);
            TeleportTo(teleportPoint1.position);

            yield return new WaitForSeconds(3f);
            animator.SetInteger("AnimState", 1);

            // LASER ATTACK: play SFX with each spread
            yield return StartCoroutine(SpawnLaserSpread());

            yield return new WaitForSeconds(7f);
            TeleportTo(teleportPoint2.position);

            yield return new WaitForSeconds(5f);
            animator.SetInteger("AnimState", 2);

            // BULLET SHOTGUN: play SFX each bullet volley
            yield return StartCoroutine(FireShotgun());

            yield return new WaitForSeconds(7f);
            TeleportTo(teleportPoint3.position);

            yield return new WaitForSeconds(4f);
            animator.SetInteger("AnimState", 3);
        }
    }

    private IEnumerator HandleGhostPhase()
    {
        Debug.Log("Starting one-time ghost phase");

        RemoveColliders();
        yield return StartCoroutine(FadeOut());

        SpawnRandomEnemies();
        Debug.Log($"Spawned {spawnedEnemies.Count} ghosts");

        yield return StartCoroutine(CheckEnemiesDefeated());
        Debug.Log("All ghosts defeated");

        RestoreColliders();
        yield return StartCoroutine(FadeIn());

        TeleportTo(teleportPoint1.position);
        Debug.Log("Ghost phase complete, returning to normal attack pattern");
    }

    private void RemoveColliders()
    {
        foreach (Collider2D collider in colliders)
            collider.enabled = false;
    }

    private void RestoreColliders()
    {
        foreach (Collider2D collider in colliders)
            collider.enabled = true;
    }

    private void TeleportTo(Vector3 targetPosition)
    {
        transform.position = targetPosition;
        PlaySFX(teleportAudioClip);
        Debug.Log("Teleported to: " + targetPosition);
    }

    private IEnumerator FireShotgun()
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            PlaySFX(bulletAudioClip);
            SpawnBulletSpread(directionToPlayer);

            yield return new WaitForSeconds(0.7f);
        }
    }

    private void SpawnBulletSpread(Vector2 direction)
    {
        SpawnBullet(direction);
        Vector2 leftDirection = RotateVector(direction, spreadAngle);
        Vector2 rightDirection = RotateVector(direction, -spreadAngle);

        SpawnBullet(leftDirection);
        SpawnBullet(rightDirection);
    }

    private void SpawnBullet(Vector2 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
        if (bulletScript != null)
            bulletScript.Initialize(direction);
    }

    private IEnumerator SpawnLaserSpread()
    {
        if (laserPrefab != null && player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            // SFX for laser attack
            PlaySFX(laserAudioClip);
            SpawnLaser(directionToPlayer);

            Vector2 leftDirection = RotateVector(directionToPlayer, spreadAngle);
            SpawnLaser(leftDirection);

            Vector2 rightDirection = RotateVector(directionToPlayer, -spreadAngle);
            SpawnLaser(rightDirection);
        }

        yield return null;
    }

    private void SpawnLaser(Vector2 direction)
    {
        GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        laser.transform.right = direction;
        Destroy(laser, 2f);
    }

    private void SpawnRandomEnemies()
    {
        if (spawnAnchor == null || spawnAreaCollider == null)
        {
            Debug.LogError("SpawnAnchor or its collider is missing!");
            return;
        }

        spawnedEnemies.Clear();
        int enemyCount = Random.Range(minEnemies, maxEnemies + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPosition = GetRandomPointInBounds();

            if (!IsTooCloseToWall(spawnPosition))
            {
                int randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject enemyToSpawn = enemyPrefabs[randomIndex];

                GameObject spawnedEnemy = Instantiate(enemyToSpawn, spawnPosition, Quaternion.identity);
                spawnedEnemy.transform.SetParent(spawnAnchor);
                spawnedEnemies.Add(spawnedEnemy);
            }
            else
            {
                i--;
            }
        }
    }

    private Vector2 GetRandomPointInBounds()
    {
        Bounds bounds = spawnAreaCollider.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        Vector2 randomPoint = new Vector2(x, y);
        return randomPoint;
    }

    private bool IsTooCloseToWall(Vector2 spawnPosition)
    {
        Collider2D wallCheck = Physics2D.OverlapCircle(spawnPosition, minWallDistance, LayerMask.GetMask("Wall"));
        return wallCheck != null;
    }

    private IEnumerator CheckEnemiesDefeated()
    {
        while (spawnedEnemies.Count > 0)
        {
            spawnedEnemies.RemoveAll(enemy => enemy == null);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * vector.x - sin * vector.y, sin * vector.x + cos * vector.y);
    }

    private IEnumerator SpawnSpikeOnPlayer()
    {
        int spikeCount = 0;

        while (spikeCount < maxSpikes && player != null)
        {
            if (spikePrefab != null)
            {
                GameObject spike = Instantiate(spikePrefab, player.position, Quaternion.identity);
                Destroy(spike, 2f);
                spikeCount++;
            }
            yield return new WaitForSeconds(0.7f);
        }
    }

    private IEnumerator FadeOut()
    {
        isInvisible = true;
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < fadeDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = endColor;
    }

    private IEnumerator FadeIn()
    {
        isInvisible = false;
        float fadeDuration = 1f;
        float elapsedTime = 0f;
        Color startColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        Color endColor = originalColor;

        while (elapsedTime < fadeDuration)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = endColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            StraightBullet bullet = collision.gameObject.GetComponent<StraightBullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);
            }
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
        {
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
                spriteRenderer.color = Color.Lerp(darkenedColor, originalColor, elapsedTime / colorRecoveryTime);
                yield return null;
            }

            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        // Disable collisions and script, Fade out, then destroy
        RemoveColliders();
        this.enabled = false;
        StartCoroutine(FadeOutAndDestroy(0.4f));
    }

    private IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
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

    public void ResetAnimState()
    {
        animator.SetInteger("AnimState", 0);
    }

    // ---- Audio Helper ----
    private void PlaySFX(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip);
        audioSource.pitch = 1f;
    }
}