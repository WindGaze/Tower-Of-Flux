using UnityEngine;
using System.Collections;

public class Progenitor : MonoBehaviour
{
    [Header("Core Settings")]
    public int health = 5000;
    public float darkenAmount = 0.5f;
    public float colorRecoveryTime = 0.5f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isEnraged = false;
    private int maxHealth;
    private Collider2D myCollider;
    private AudioSource audioSource;
    private Coroutine attackRoutine;
    private bool isDead = false;

    [Header("Chase Settings")]
    public float chaseDuration = 8f; // CUT to 8 seconds
    public float moveSpeed = 3f;
    private Transform player;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform[] bulletSpawnPoints;
    public float bulletFireInterval = 4f; // Fire every 4 seconds
    public float bulletSpeed = 8f;

    [Header("Enrage Bullet Settings")]
    public Transform[] enrageSpawnPoints;

    [Header("Speed Shot Settings")]
    public Transform[] speedShot;
    public float speedShotSpeed = 10f;
    public GameObject speedShotBulletPrefab;

    [Header("Enrage Speed Shot Settings")]
    public Transform[] speedEnrageShot;

    [Header("Laser Settings")]
    public GameObject laserPrefab;
    public Transform[] laserSpawnPoints;

    [Header("Enrage Laser Settings")]
    public Transform[] laserEnragePoints;

    [Header("Attack SFX")]
    public AudioClip bulletAudioClip;
    public AudioClip speedShotAudioClip;
    public AudioClip laserAudioClip;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
            myCollider = gameObject.AddComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        maxHealth = health;
        StartAttackPattern();
    }

    private void StartAttackPattern()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(BehaviorCycle());
    }

    private IEnumerator BehaviorCycle()
    {
        while (true)
        {
            float startTime = Time.time;
            float chaseEndTime = startTime + chaseDuration;
            float nextBulletTime = startTime + bulletFireInterval;

            // Phase 1: Chase player and fire bullets simultaneously every 4 seconds
            while (Time.time < chaseEndTime)
            {
                ChasePlayer();

                if (Time.time >= nextBulletTime)
                {
                    FireBulletsSimultaneously();
                    nextBulletTime += bulletFireInterval;
                }

                yield return null;
            }

            yield return new WaitForSeconds(2f);

            // Phase 2: Fire speed shot
            FireSpeedShot();

            yield return new WaitForSeconds(1f);

            // Phase 3: Fire laser
            FireLaser();

            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator EnrageBehaviorCycle()
    {
        while (true)
        {
            float startTime = Time.time;
            float chaseEndTime = startTime + chaseDuration;
            float nextBulletTime = startTime + bulletFireInterval;

            // Enraged chase & bullet fire
            while (Time.time < chaseEndTime)
            {
                ChasePlayer();

                if (Time.time >= nextBulletTime)
                {
                    EnrageFireBulletsSimultaneously();
                    nextBulletTime += bulletFireInterval;
                }
                yield return null;
            }

            yield return new WaitForSeconds(2f);

            // Enraged speed shot
            EnrageFireSpeedShot();

            yield return new WaitForSeconds(1f);

            // Enraged laser
            EnrageFireLaser();

            yield return new WaitForSeconds(2f);
        }
    }

    private void ChasePlayer()
    {
        if (isDead || player == null) return;
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
    }

    // Attach bullet to anchor, then unparent and fire after delay
    private IEnumerator AttachAndLaunch(GameObject bulletObj, Transform anchor, Vector2 launchDir, float speed, float delay)
    {
        if (bulletObj == null) yield break;
        bulletObj.transform.SetParent(anchor, true);

        Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(delay);

        if (bulletObj != null)
            bulletObj.transform.SetParent(null);

        if (rb != null && bulletObj != null)
            rb.velocity = launchDir.normalized * speed;
    }

    private void FireBulletsSimultaneously()
    {
        if (bulletPrefab == null || isDead) return;

        foreach (Transform spawnPoint in bulletSpawnPoints)
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            PlayAudio(bulletAudioClip);

            StartCoroutine(AttachAndLaunch(
                bullet,
                spawnPoint,
                spawnPoint.right,
                bulletSpeed,
                2f // seconds to follow anchor
            ));
        }
    }

    private void EnrageFireBulletsSimultaneously()
    {
        if (bulletPrefab == null || isDead) return;

        foreach (Transform spawnPoint in enrageSpawnPoints)
        {
            GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
            PlayAudio(bulletAudioClip);

            StartCoroutine(AttachAndLaunch(
                bullet,
                spawnPoint,
                spawnPoint.right,
                bulletSpeed,
                2f // seconds to follow anchor
            ));
        }
    }

    private void FireSpeedShot()
    {
        if (isDead || player == null) return;
        foreach (Transform shotPoint in speedShot)
        {
            if (speedShotBulletPrefab != null)
            {
                // Calculate direction and angle to player
                Vector2 dirToPlayer = (player.position - shotPoint.position).normalized;
                float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0, 0, angle);

                // Instantiate bullet with correct rotation
                GameObject bullet = Instantiate(speedShotBulletPrefab, shotPoint.position, rot);
                PlayAudio(speedShotAudioClip);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = dirToPlayer * speedShotSpeed;
            }
        }
    }

    private void EnrageFireSpeedShot()
    {
        if (isDead || player == null) return;
        foreach (Transform shotEPoint in speedEnrageShot)
        {
            if (speedShotBulletPrefab != null)
            {
                Vector2 dirToPlayer = (player.position - shotEPoint.position).normalized;
                float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0, 0, angle);

                GameObject bullet = Instantiate(speedShotBulletPrefab, shotEPoint.position, rot);
                PlayAudio(speedShotAudioClip);
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = dirToPlayer * speedShotSpeed;
            }
        }
    }

    private void FireLaser()
    {
        if (isDead) return;
        foreach (Transform shotPoint in laserSpawnPoints)
        {
            if (laserPrefab != null)
            {
                GameObject laser = Instantiate(laserPrefab, shotPoint.position, shotPoint.rotation);
                PlayAudio(laserAudioClip);
                Destroy(laser, 2f);
            }
        }
    }

    private void EnrageFireLaser()
    {
        if (isDead) return;
        foreach (Transform shotEPoint in laserEnragePoints)
        {
            if (laserPrefab != null)
            {
                GameObject laser = Instantiate(laserPrefab, shotEPoint.position, shotEPoint.rotation);
                PlayAudio(laserAudioClip);
                Destroy(laser, 2f);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (collision.gameObject.CompareTag("Bullet"))
        {
            StraightBullet bullet = collision.gameObject.GetComponent<StraightBullet>();
            if (bullet != null) TakeDamage(bullet.damage);
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health -= damage;
        StartCoroutine(DamageFlash());

        if (!isEnraged && health <= maxHealth / 2)
        {
            isEnraged = true;
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            attackRoutine = StartCoroutine(EnrageBehaviorCycle());
        }

        if (health <= 0) Die();
    }

    private IEnumerator DamageFlash()
    {
        Color darkened = Color.Lerp(originalColor, Color.black, darkenAmount);
        spriteRenderer.color = darkened;

        float elapsed = 0f;
        while (elapsed < colorRecoveryTime)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(darkened, originalColor, elapsed / colorRecoveryTime);
            yield return null;
        }
        spriteRenderer.color = originalColor;
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