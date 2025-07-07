using System.Collections;
using UnityEngine;

public class ShadowTitan : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public int bulletsPerShot = 3;
    public float maxSpreadAngle = 30f;
    public Transform bulletSpawnPoint;
    public GameObject hitEffectPrefab;
    public float timeBetweenShots = 0.3f; // Time between each volley
    public int volleyCount = 3; // Number of volleys to fire
    [Header("Animation Event SFX")]
    public AudioClip animEventClip;
    private AudioSource audioSource;
    
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("AnimState", 1); // Always Level 1 animation for ShadowTitan
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    public void PlayAnimEventSFX()
    {
        if (animEventClip != null && audioSource != null)
            audioSource.PlayOneShot(animEventClip);
    }
    public void StartTripleVolley()
    {
        StartCoroutine(FireMultipleVolleys());
    }

    private IEnumerator FireMultipleVolleys()
    {
        for (int v = 0; v < volleyCount; v++)
        {
            FireThreeWayAtPlayer();
            yield return new WaitForSeconds(timeBetweenShots);
        }
    }

    private void FireThreeWayAtPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || bulletPrefab == null || bulletSpawnPoint == null)
        {
            Debug.LogWarning("Player, bulletPrefab, or bulletSpawnPoint not assigned/found!");
            return;
        }

        // Get the direction from the spawn point to the player
        Vector3 direction = (player.transform.position - bulletSpawnPoint.position).normalized;
        float angleToPlayer = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float angleStep = maxSpreadAngle / (bulletsPerShot - 1);
        float startAngle = angleToPlayer - (maxSpreadAngle / 2);

        for (int i = 0; i < bulletsPerShot; i++)
        {
            float currentAngle = startAngle + (i * angleStep);
            Vector2 finalDirection = new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad)).normalized;

            Quaternion bulletRotation = Quaternion.Euler(0, 0, currentAngle);

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletRotation);

            StraightBullet straightBullet = bullet.GetComponent<StraightBullet>();
            if (straightBullet != null)
            {
                straightBullet.Initialize(finalDirection, bulletSpeed, 1); // Always level 1 for shadow
                straightBullet.hitEffectPrefab = hitEffectPrefab;
                straightBullet.gunBullet = false;
            }
            else
            {
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = finalDirection * bulletSpeed;
                }
            }
        }
    }

    public void Disappear()
    {
        Destroy(gameObject);
    }
}