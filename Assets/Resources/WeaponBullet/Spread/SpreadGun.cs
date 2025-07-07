using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SpreadGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public string weaponName;
    public float rotationOffset = 0f;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public int level = 1;

    [Header("Firing Settings")]
    public int bulletsPerShot = 3;
    public float bulletSpeed = 10f;
    public float maxSpreadAngle = 30f;
    public float fireRate = 0.5f;
    public bool useMouseAim = true;
    public Transform aimTarget;

    [Header("Audio")]
    public AudioClip fireClip; // Assign in Inspector
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;
    private AudioSource audioSource;

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

    // Final effective bullet speed (base + bonus)
    public float EffectiveBulletSpeed => bulletSpeed + bulletBonus;

    private Vector3 originalScale;
    private Vector3 originalPosition;
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
            audioSource = gameObject.AddComponent<AudioSource>();
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
        if (canFire && Input.GetMouseButtonDown(0))
        {
            FireSpreadBullets();
        }
    }

    void FireSpreadBullets()
    {
        if (bulletPrefab != null && canFire)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 bulletDirection = (mousePosition - bulletSpawnPoint.position).normalized;

            float angleStep = maxSpreadAngle / (bulletsPerShot - 1);
            float startAngle = -(maxSpreadAngle / 2);

            for (int i = 0; i < bulletsPerShot; i++)
            {
                float currentAngle = startAngle + (i * angleStep);
                Vector2 finalDirection = Quaternion.Euler(0, 0, currentAngle) * bulletDirection;

                GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

                bullet.transform.localScale = new Vector3(
                    transform.localScale.x > 0 ? Mathf.Abs(bullet.transform.localScale.x) : -Mathf.Abs(bullet.transform.localScale.x),
                    bullet.transform.localScale.y,
                    bullet.transform.localScale.z
                );

                StraightBullet straightBullet = bullet.GetComponent<StraightBullet>();
                if (straightBullet != null)
                {
                    straightBullet.Initialize(finalDirection, EffectiveBulletSpeed, level);
                }
            }

            PlayFireSoundOncePerShot(); // Play the shot sound ONCE per shot

            StartCoroutine(Cooldown());
        }
    }

    void PlayFireSoundOncePerShot()
    {
        if (fireClip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(fireClip, 1f);
        }
    }

    IEnumerator Cooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(fireRate);
        canFire = true;
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

    public void ResetSpeed()
    {
        // Restore all main stats and statuses to your default
        bulletBonus = 0;
    }
    
}