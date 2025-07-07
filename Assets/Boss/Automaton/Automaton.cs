using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Automaton : MonoBehaviour
{
    [Header("Health Settings")]
    public int health = 5000;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;

    [Header("Enemy Spawning")]
    public GameObject[] enemyPrefabs;
    public Transform spawnAnchor;
    private int maxEnemies = 2;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    public int minEnemies = 4;
    public float minSpawnDistance = 2f;
    public float minWallDistance = 1.5f;

    [Header("Missile Attack")]
    public GameObject bulletPrefab;
    public Transform missileSpawnPoint;
    public float bulletSpeed = 10f;
    public float idleTime = 10f;
    public float firingDuration = 7f;
    public float firingInterval = 2f;

    [Header("Shotgun Attack")]
    public GameObject shotgunBulletPrefab;
    public Transform[] shotgunSpawnPoints;
    public float spreadAngle = 15f;

    [Header("Laser Attack")]
    public GameObject laserPrefab;
    public Transform[] laserSpawnPoints;

    [Header("Speed Phase")]
    public float speedPhaseDuration = 30f;
    public float speedPhaseBulletInterval = 1f;
    public float speedPhaseLaserInterval = 4f;
    public Transform[] speedPhaseBulletSpawnPoints;
    public Transform[] speedPhaseLaserSpawnPoints;
    public float postSpeedPhaseCooldown = 5f;
    public GameObject speedPhaseBullet;
    public GameObject speedPhaseLaser;
    public float bulletSpeedphase = 10f;

    [Header("Audio Clips")]
    public AudioClip shotgunAudioClip;
    public AudioClip laserAudioClip;
    public AudioClip speedBulletAudioClip;
    public AudioClip speedLaserAudioClip;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Collider2D spawnAreaCollider;
    private Transform player;
    private bool isInSpeedPhase = false;
    private Coroutine currentAttackCycle;
    private Animator animator;
    private Collider2D myCollider;
    private AudioSource audioSource;
    private bool isDead = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnAreaCollider = spawnAnchor.GetComponent<Collider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
            myCollider = gameObject.AddComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        StartAttackCycle();
    }

    private void StartAttackCycle()
    {
        if (currentAttackCycle != null)
            StopCoroutine(currentAttackCycle);
        currentAttackCycle = StartCoroutine(AttackCycle());
    }

    private IEnumerator AttackCycle()
    {
        while (true)
        {
            if (isInSpeedPhase) yield return null;

            yield return new WaitForSeconds(idleTime);

            float firingEndTime = Time.time + firingDuration;
            while (Time.time < firingEndTime && !isInSpeedPhase)
            {
                FireMissile();
                yield return new WaitForSeconds(firingInterval);
            }

            if (isInSpeedPhase) continue;

            yield return new WaitForSeconds(2f);
            if (!isInSpeedPhase) SpawnRandomEnemies();

            yield return new WaitForSeconds(1f);
            if (!isInSpeedPhase) 
            {
                yield return StartCoroutine(FireShotgunBurst());
                if (!isInSpeedPhase)
                {
                    animator.SetInteger("AnimState", 2);
                    yield return new WaitForSeconds(8f/12f);
                    animator.SetInteger("AnimState", 0);
                }
            }
            if (!isInSpeedPhase) yield return new WaitForSeconds(6f);

            if (!isInSpeedPhase)
            {
                yield return StartCoroutine(ExecuteSpeedPhase());
            }

            if (!isInSpeedPhase) yield return new WaitForSeconds(postSpeedPhaseCooldown);
        }
    }

    private IEnumerator ExecuteSpeedPhase()
    {
        isInSpeedPhase = true;

        animator.SetInteger("AnimState", 1);

        float speedPhaseEndTime = Time.time + speedPhaseDuration;
        float nextBulletTime = Time.time;
        float nextLaserTime = Time.time;

        while (Time.time < speedPhaseEndTime)
        {
            if (Time.time >= nextBulletTime)
            {
                FireSpeedPhaseBullet();
                nextBulletTime = Time.time + speedPhaseBulletInterval;
            }

            if (Time.time >= nextLaserTime)
            {
                FireSpeedPhaseLaser();
                nextLaserTime = Time.time + speedPhaseLaserInterval;
            }
            yield return null;
        }

        animator.SetInteger("AnimState", 0);

        isInSpeedPhase = false;
    }

    private void FireMissile()
    {
        if (bulletPrefab != null && player != null && missileSpawnPoint != null)
        {
            Vector2 targetPosition = player.position;
            Vector2 spawnPos = missileSpawnPoint.position;
            Vector2 direction = (targetPosition - spawnPos).normalized;

            GameObject missile = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            MissileAutomaton missileScript = missile.GetComponent<MissileAutomaton>();
            if (missileScript != null)
            {
                missileScript.Initialize(direction, bulletSpeed);
                missileScript.destination = true;
                missileScript.SetTargetPosition(targetPosition);
            }
        }
    }

    private IEnumerator FireShotgunBurst()
    {
        int bursts = 4;
        float burstInterval = 0.7f;

        for (int i = 0; i < bursts && !isInSpeedPhase; i++)
        {
            if (player != null && shotgunSpawnPoints.Length > 0)
            {
                foreach (Transform spawnPoint in shotgunSpawnPoints)
                {
                    Vector2 directionToPlayer = (player.position - spawnPoint.position).normalized;
                    FireShotgunSpread(directionToPlayer, spawnPoint.position);
                }
                // -- Shotgun SFX --
                PlayAudio(shotgunAudioClip);
            }
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private void FireShotgunSpread(Vector2 baseDirection, Vector2 spawnPosition)
    {
        if (isInSpeedPhase) return;

        SpawnShotgunBullet(baseDirection, spawnPosition);

        Vector2 leftDirection = RotateVector(baseDirection, spreadAngle);
        SpawnShotgunBullet(leftDirection, spawnPosition);

        Vector2 rightDirection = RotateVector(baseDirection, -spreadAngle);
        SpawnShotgunBullet(rightDirection, spawnPosition);
    }

    private void SpawnShotgunBullet(Vector2 direction, Vector2 spawnPosition)
    {
        if (isInSpeedPhase) return;

        GameObject bullet = Instantiate(shotgunBulletPrefab, spawnPosition, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }
    }

    public void FireLasers()
    {
        if (isInSpeedPhase || laserPrefab == null || player == null || laserSpawnPoints.Length == 0)
            return;

        foreach (Transform spawnPoint in laserSpawnPoints)
        {
            GameObject laser = Instantiate(laserPrefab, spawnPoint.position, spawnPoint.rotation);
            Destroy(laser, 2f);
        }
        // -- Laser SFX --
        PlayAudio(laserAudioClip);
    }

    private void FireSpeedPhaseBullet()
    {
        if (speedPhaseBullet == null || speedPhaseBulletSpawnPoints.Length == 0 || player == null)
            return;

        foreach (Transform spawnPoint in speedPhaseBulletSpawnPoints)
        {
            Vector2 direction = (player.position - spawnPoint.position).normalized;
            GameObject bullet = Instantiate(speedPhaseBullet, spawnPoint.position, Quaternion.identity);
            
            // Set rotation to face the player
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction * bulletSpeedphase;
            }
        }
        // -- Speed-phase bullet SFX --
        PlayAudio(speedBulletAudioClip);
    }

    private void FireSpeedPhaseLaser()
    {
        if (speedPhaseLaser == null || speedPhaseLaserSpawnPoints.Length == 0 || player == null)
            return;

        foreach (Transform spawnPoint in speedPhaseLaserSpawnPoints)
        {
            Vector2 direction = (player.position - spawnPoint.position).normalized;
            GameObject laser = Instantiate(speedPhaseLaser, spawnPoint.position, Quaternion.identity);
            laser.transform.right = direction;
            Destroy(laser, 2f);
        }
        // -- Speed-phase laser SFX --
        PlayAudio(speedLaserAudioClip);
    }

    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * vector.x - sin * vector.y, sin * vector.x + cos * vector.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Bullet"))
        {
            StraightBullet bullet = collision.gameObject.GetComponent<StraightBullet>();
            if (bullet != null)
                TakeDamage(bullet.damage);

            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        health -= damageAmount;
        StartCoroutine(TemporaryDarkenEffect());

        if (health <= 0)
            Die();
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

    public void SpawnRandomEnemies()
    {
        if (isInSpeedPhase || spawnAnchor == null || spawnAreaCollider == null)
            return;

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
        return new Vector2(x, y);
    }

    private bool IsTooCloseToWall(Vector2 spawnPosition)
    {
        Collider2D wallCheck = Physics2D.OverlapCircle(spawnPosition, minWallDistance, LayerMask.GetMask("Wall"));
        return wallCheck != null;
    }

    private void Die()
    {
        isDead = true;
        if (myCollider != null) myCollider.enabled = false;
        this.enabled = false;
        StartCoroutine(FadeOutAndDestroy(0.4f));
    }

    private IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
        float elapsed = 0f;
        if (spriteRenderer == null) { Destroy(gameObject); yield break; }
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

    private void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}