using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StraightGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public string weaponName; // Add this to identify which weapon this is
    public float rotationOffset = 0f;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;

    public AudioClip fireClip; // Assign in Inspector
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
    private AudioSource audioSource;

    [Header("Firing Settings")]
    public int bulletsPerShot = 1;
    public float bulletSpeed = 10f;
    public float maxTrajectoryDeviation = 5f;
    public float fireRate = 0.5f;
    public float cooldownTime = 1f;
    public bool autoFire = false;

    [Header("Optional Settings")]
    public bool useMouseAim = true;
    public Transform aimTarget;

    [Header("Level Settings")]
    public int level = 1;  // Current level (now directly modifiable)

    [SerializeField]
    private float _bulletBonus = 0f;
    public float bulletBonus
    {
        get => _bulletBonus;
        set
        {
            _bulletBonus = value;
            Debug.Log($"Bullet speed bonus updated to: {_bulletBonus}");
        }
    }
    // Effectively combines bulletSpeed and bulletBonus for use
    public float EffectiveBulletSpeed => bulletSpeed + bulletBonus;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Coroutine firingCoroutine;
    private bool canFire = true;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        if (bulletSpawnPoint == null)
        {
            Debug.LogWarning("Bullet spawn point not set. Using gun's position as spawn point.");
            bulletSpawnPoint = transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    private void PlayFireSound()
    {
        if (fireClip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(fireClip, 1f);
        }
    }
    void Update()
    {
        UpdateRotation();
        HandleShooting();
        if (GameManager.Instance != null)
        {
            level = GameManager.Instance.GetWeaponLevel(weaponName);
        }
    }

    void UpdateRotation()
    {
        Vector3 targetPosition = useMouseAim ? GetMouseWorldPosition() : aimTarget.position;
        Vector3 direction = targetPosition - transform.position;
        direction.z = 0;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (targetPosition.x < transform.position.x)
        {
            angle += 180;
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        angle = NormalizeAngle(angle);
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        transform.localPosition = originalPosition;
    }

    void HandleShooting()
    {
        if (canFire && (autoFire || Input.GetMouseButton(0)))
        {
            if (firingCoroutine == null)
            {
                firingCoroutine = StartCoroutine(FireSequence());
            }
        }
        else if (!Input.GetMouseButton(0) && firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
            firingCoroutine = null;
            canFire = true;
        }
    }

    IEnumerator FireSequence()
    {
        canFire = false;

        while (true)
        {
            for (int i = 0; i < bulletsPerShot; i++)
            {
                FireSingleBullet();
                if (bulletsPerShot > 1 && i < bulletsPerShot - 1)
                {
                    yield return new WaitForSeconds(0.05f);
                }
            }

            yield return new WaitForSeconds(fireRate);

            if (!Input.GetMouseButton(0))
            {
                break;
            }
        }

        yield return new WaitForSeconds(cooldownTime);
        canFire = true;
        firingCoroutine = null;
    }

    void FireSingleBullet()
    {
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            
            bullet.transform.localScale = new Vector3(
                transform.localScale.x > 0 ? bullet.transform.localScale.x : -bullet.transform.localScale.x,
                bullet.transform.localScale.y,
                bullet.transform.localScale.z
            );
            
            StraightBullet straightBullet = bullet.GetComponent<StraightBullet>();
            if (straightBullet != null)
            {
                Vector3 mousePosition = GetMouseWorldPosition();
                Vector3 bulletDirection = (mousePosition - bulletSpawnPoint.position).normalized;

                float deviation = Random.Range(-maxTrajectoryDeviation, maxTrajectoryDeviation);
                Vector2 finalDirection = Quaternion.Euler(0, 0, deviation) * bulletDirection;

                straightBullet.Initialize(finalDirection, EffectiveBulletSpeed, level);  // THE ONLY LINE CHANGED
                Debug.Log($"Firing bullet at level {level}");
            }
            PlayFireSound();    
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    // Simple method to increase level directly
    public void IncreaseLevel(int amount)
    {
        level += amount;
        Debug.Log($"Gun level increased to: {level}");
    }
    public void ResetSpeed()
    {
        // Restore all main stats and statuses to your default
        bulletBonus = 0;
    }
}