using System.Collections;
using UnityEngine;

public class Grendel : MonoBehaviour
{
    public int level = 1;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public int bulletsPerShot = 3;
    public float maxSpreadAngle = 30f;
    public Transform bulletSpawnPoint;
    public GameObject hitEffectPrefab;
    public float timeBetweenShots = 0.3f;
    public int totalShots = 3;
    private Animator animator;
    public GameObject weakenEffectPrefab;
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    public AudioClip spawnSFX;
    private AudioSource audioSource;
    private bool hasFired = false;
    private float destroyDelay = 3f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (GameManager.Instance != null)
        {
            level = GameManager.Instance.GetVexonLevel("Titan");
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        if (spawnSFX != null && audioSource != null)
            audioSource.PlayOneShot(spawnSFX);
        if (level == 1)
        {
            animator.SetInteger("AnimState", 1);
        }
        else if (level == 2)
        {
            animator.SetInteger("AnimState", 2);
        }
        else if (level == 3)
        {
            animator.SetInteger("AnimState", 3);
        }
    }
    public void PlayAnimEventSFX()
    {
        if (animEventClip != null && audioSource != null)
            audioSource.PlayOneShot(animEventClip);
    }

    private void Level1Behavior()
    {
        if (!hasFired)
        {
            HealPlayer(0.3f);
            StartCoroutine(FireMultipleVolleys());
            hasFired = true;
        }
    }

    private void Level2Behavior()
    {
        if (!hasFired)
        {
            HealPlayer(0.3f);
            StartCoroutine(FireMultipleVolleys());
            WeakenAllEnemiesInstantly();
            hasFired = true;
        }
    }

    private void Level3Behavior()
    {
        if (!hasFired)
        {
            HealPlayer(0.5f);
            StartCoroutine(FireMultipleVolleys());
            WeakenAllEnemiesInstantly();
            hasFired = true;
        }
    }

    private void HealPlayer(float healPercentage)
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            int healAmount = Mathf.RoundToInt(player.health * healPercentage);
            player.health = Mathf.Min(player.health + healAmount, player.maxHealth);
            Debug.Log($"Titan healed player for {healAmount} health");
        }
        else
        {
            Debug.LogWarning("PlayerMovement not found for healing!");
        }
    }

    private IEnumerator FireMultipleVolleys()
    {
        for (int i = 0; i < totalShots; i++)
        {
            FireAtNearestEnemy();
            yield return new WaitForSeconds(timeBetweenShots);
        }

        // Optional: Destroy after firing
        Invoke(nameof(Disappear), destroyDelay);
    }

    private void FireAtNearestEnemy()
    {
        GameObject nearestEnemy = FindNearestEnemy();
        if (nearestEnemy == null)
        {
            Debug.LogWarning("No enemy/boss found to target!");
            return;
        }

        Vector3 direction = (nearestEnemy.transform.position - bulletSpawnPoint.position).normalized;
        float angleStep = maxSpreadAngle / (bulletsPerShot - 1);
        float startAngle = -(maxSpreadAngle / 2);

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            Vector2 finalDirection = Quaternion.Euler(0, 0, currentAngle) * direction;

            float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, rotation);
            StraightBullet straightBullet = bullet.GetComponent<StraightBullet>();
            if (straightBullet != null)
            {
                straightBullet.Initialize(finalDirection, bulletSpeed, level);
                straightBullet.hitEffectPrefab = hitEffectPrefab;
                straightBullet.gunBullet = false;
            }
        }
    }

    private GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        GameObject nearestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTarget = enemy;
            }
        }

        foreach (GameObject boss in bosses)
        {
            float distance = Vector3.Distance(transform.position, boss.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestTarget = boss;
            }
        }

        return nearestTarget;
    }

    private void WeakenAllEnemiesInstantly()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");

        var allTargets = new System.Collections.Generic.List<GameObject>();
        allTargets.AddRange(enemies);
        allTargets.AddRange(bosses);

        foreach (GameObject target in allTargets)
        {
            if (target != null)
            {
                var healthComponent = target.GetComponent<MonoBehaviour>();
                if (healthComponent != null)
                {
                    System.Type type = healthComponent.GetType();
                    var healthField = type.GetField("health");

                    if (healthField != null)
                    {
                        int currentHealth = (int)healthField.GetValue(healthComponent);
                        int reducedHealth;

                        if (target.CompareTag("Boss"))
                            reducedHealth = currentHealth - 50; // Subtract 50 HP (raw)
                        else
                            reducedHealth = Mathf.RoundToInt(currentHealth * 0.7f); // 70% for enemies

                        reducedHealth = Mathf.Max(1, reducedHealth); // Prevents negative or zero HP

                        healthField.SetValue(healthComponent, reducedHealth);

                        if (weakenEffectPrefab != null)
                        {
                            Instantiate(weakenEffectPrefab, target.transform.position, Quaternion.identity, target.transform);
                        }

                        Debug.Log($"Weakened {target.name}'s health: {currentHealth} -> {reducedHealth}");
                    }
                }
            }
        }

        Debug.Log($"Weakened {allTargets.Count} enemies/bosses instantly");
    }

    private void Disappear()
    {
        Destroy(gameObject);
    }
}
