using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidHeart : MonoBehaviour
{
    [Header("Combat Settings")]
    public int health = 7000;
    public int maxHealth = 7000; // <--- Set this for % checks
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float idealDistance = 5f;
    public float distanceThreshold = 0.5f;
    public float randomMoveProbability = 0.02f;
    public float randomMoveSpeed = 2f;
    public float randomMoveDuration = 2f;
    public float movementThreshold = 0.05f;

    [Header("SMG Bullet Settings")]
    public EnemyBullet bulletPrefab;
    public Transform bulletSpawnPoint;
    public float shootInterval = 3f;
    public float burstInterval = 0.1f;
    public int bulletsPerBurst = 5;
    public float bulletSpeed = 10f;
    public float aimVariance = 5f;

    [Header("Spawn Settings")]
    public List<GameObject> spawnablePrefabs;
    public Transform spawnPosition;
    public Transform spawnPosition2; // <--- NEW for phase 2 double spawn
    public float spawnInterval = 8f;

    // Private variables
    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Animator animator;
    private Vector2 randomMoveDirection;
    private bool isMovingRandomly = false;
    private float randomMoveTimer = 0f;
    private Vector2 previousPosition;
    private bool wasMovingLastFrame = false;
    private float shootTimer = 0f;
    private bool isBursting = false;
    private float spawnTimer = 0f;
    private bool enteredPhaseTwo = false; // Only trigger phase two once

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        animator = GetComponent<Animator>();
        previousPosition = transform.position;
        shootTimer = shootInterval;
        spawnTimer = spawnInterval;

        SetRandomDirection();

        if (bulletSpawnPoint == null)
        {
            bulletSpawnPoint = transform;
            Debug.LogWarning("Bullet spawn point not set. Using VoidHeart's position.");
        }

        if (spawnPosition == null)
        {
            spawnPosition = transform;
            Debug.LogWarning("Spawn position not set. Using VoidHeart's position.");
        }
    }

    private void Update()
    {
        if (player == null) return;

        CheckPhaseTwo();

        HandleMovement();
        HandleShooting();
        HandleSpawning();
    }

    // Switches to phase two when under 45% health, only applies once
    private void CheckPhaseTwo()
    {
        if (!enteredPhaseTwo && health < 0.45f * maxHealth)
        {
            enteredPhaseTwo = true;
            shootInterval = 1f;
            bulletsPerBurst = 8;
            spawnInterval = 1.5f;
            Debug.Log("VoidHeart entered PHASE TWO: faster shooting, bigger bursts, 2x summoning.");
        }
    }

    private void HandleMovement()
    {
        Vector2 startPosition = transform.position;

        if (!isMovingRandomly && Random.value < randomMoveProbability)
        {
            StartRandomMovement();
        }

        if (isMovingRandomly)
        {
            UpdateRandomMovement();
        }
        else
        {
            MaintainDistanceFromPlayer();
        }

        UpdateSpriteAndAnimation(startPosition);
    }

    private void StartRandomMovement()
    {
        isMovingRandomly = true;
        randomMoveTimer = randomMoveDuration;
        SetRandomDirection();
    }

    private void UpdateRandomMovement()
    {
        randomMoveTimer -= Time.deltaTime;

        if (randomMoveTimer <= 0)
        {
            isMovingRandomly = false;
            return;
        }

        float currentDistance = Vector2.Distance(transform.position, player.position);
        Vector2 moveDirection = randomMoveDirection;

        if (currentDistance < idealDistance - distanceThreshold)
        {
            Vector2 awayFromPlayer = (transform.position - player.position).normalized;
            moveDirection = (moveDirection + awayFromPlayer).normalized;
        }
        else if (currentDistance > idealDistance + distanceThreshold)
        {
            Vector2 towardPlayer = (player.position - transform.position).normalized;
            moveDirection = (moveDirection + towardPlayer).normalized;
        }

        transform.position += (Vector3)moveDirection * randomMoveSpeed * Time.deltaTime;
    }

    private void SetRandomDirection()
    {
        float randomAngle = Random.Range(0f, Mathf.PI * 2);
        randomMoveDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
    }

    private void MaintainDistanceFromPlayer()
    {
        float currentDistance = Vector2.Distance(transform.position, player.position);

        if (Mathf.Abs(currentDistance - idealDistance) > distanceThreshold)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;

            if (currentDistance < idealDistance)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    transform.position - (Vector3)directionToPlayer,
                    moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    player.position,
                    moveSpeed * Time.deltaTime);
            }
        }
    }

    private void UpdateSpriteAndAnimation(Vector2 startPosition)
    {
        Vector2 currentPosition = transform.position;
        Vector2 frameMovement = currentPosition - startPosition;
        float movementMagnitude = frameMovement.magnitude;

        if (movementMagnitude > movementThreshold && spriteRenderer != null)
        {
            if (frameMovement.x > 0.01f) spriteRenderer.flipX = false;
            else if (frameMovement.x < -0.01f) spriteRenderer.flipX = true;
        }

        bool shouldMove = isMovingRandomly || movementMagnitude > movementThreshold;
        if (animator != null)
        {
            animator.SetInteger("AnimState", shouldMove ? 1 : 0);
        }

        previousPosition = currentPosition;
    }

    private void HandleShooting()
    {
        if (bulletPrefab == null || isBursting) return;

        shootTimer -= Time.deltaTime;
        if (shootTimer <= 0f)
        {
            StartCoroutine(BurstFire());
            shootTimer = shootInterval;
        }
    }

    private IEnumerator BurstFire()
    {
        isBursting = true;

        for (int i = 0; i < bulletsPerBurst; i++)
        {
            ShootSingleBullet();
            yield return new WaitForSeconds(burstInterval);
        }

        isBursting = false;
    }

    private void ShootSingleBullet()
    {
        Vector2 directionToPlayer = (player.position - bulletSpawnPoint.position).normalized;

        // Add slight random variance to aim
        float randomAngle = Random.Range(-aimVariance, aimVariance);
        Vector2 shootDirection = Quaternion.Euler(0, 0, randomAngle) * directionToPlayer;

        EnemyBullet bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
        bullet.speed = this.bulletSpeed;
        bullet.Initialize(shootDirection);

        // Optional: Visual feedback for shooting
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashMuzzle());
        }
    }

    private void HandleSpawning()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && spawnablePrefabs.Count > 0)
        {
            if (enteredPhaseTwo && spawnPosition2 != null)
            {
                SpawnRandomPrefabAt(spawnPosition);
                SpawnRandomPrefabAt(spawnPosition2);
            }
            else
            {
                SpawnRandomPrefabAt(spawnPosition);
            }
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnRandomPrefabAt(Transform positionT)
    {
        if (positionT == null) return;
        int randomIndex = Random.Range(0, spawnablePrefabs.Count);
        GameObject prefabToSpawn = spawnablePrefabs[randomIndex];

        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, positionT.position, Quaternion.identity);
        }
    }

    private IEnumerator FlashMuzzle()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        spriteRenderer.color = originalColor;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            HandleBulletCollision(collision.gameObject);
        }
    }

    private void HandleBulletCollision(GameObject bullet)
    {
        MonoBehaviour[] components = bullet.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            var type = component.GetType();
            var finalDamageProp = type.GetProperty("FinalDamage");
            if (finalDamageProp != null)
            {
                TakeDamage((int)finalDamageProp.GetValue(component));
                break;
            }

            var damageField = type.GetField("damage");
            if (damageField != null)
            {
                TakeDamage((int)damageField.GetValue(component));
                break;
            }
        }
        Destroy(bullet);
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
        if (spriteRenderer == null) yield break;

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

    private void Die()
    {
        // Freeze logic: prevent further actions
        enabled = false; // disables Update() and all custom behavior
        GetComponent<Collider2D>().enabled = false;
        if (animator != null) animator.SetInteger("AnimState", 0); // idle/neutral animation if applicable

        // Optionally set velocity to zero if using Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        // Start fade and destroy after
        StartCoroutine(FadeOutAndDestroy(2f));
    }
    
    private IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

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