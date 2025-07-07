using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SingleGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public string weaponName; // Add this to identify which weapon this is
    public float rotationOffset = 0f;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public int level = 1;

    [Header("Firing Settings")]
    public int bulletsPerShot = 5; // Number of bullets fired in one click
    public float bulletSpeed = 10f;
    public float fireRate = 0.1f; // Delay between each bullet in succession
    public float cooldownTime = 1f; // Cooldown after firing all bullets
    public bool useMouseAim = true;
    public Transform aimTarget;

    [Header("Audio")]
    public AudioClip fireClip;
    public float minPitch = 1.2f;
    public float maxPitch = 1.9f;
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
    public float EffectiveBulletSpeed => bulletSpeed + bulletBonus;
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool canFire = true;  // Flag to check if we can fire
    private bool isShooting = false;  // To check if currently firing a burst
    [SerializeField]
    private int _levelTemp = 0;  // Private backing field
    public int levelTemp
    {
        get { return _levelTemp; }
        set
        {
            _levelTemp = value;
            UpdateLevel();  // Update level when levelTemp changes
        }
    }
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
        if (canFire && Input.GetMouseButtonDown(0) && !isShooting)  // Fire on click if not currently firing
        {
            StartCoroutine(FireBulletsInSuccession());  // Start firing bullets in succession
        }
    }

    IEnumerator FireBulletsInSuccession()
    {
        canFire = false;
        isShooting = true;

        for (int i = 0; i < bulletsPerShot; i++)  // Fire bullets one by one
        {
            FireSingleBullet();
            yield return new WaitForSeconds(fireRate);  // Delay between each shot
        }

        isShooting = false;

        yield return new WaitForSeconds(cooldownTime);  // Wait for cooldown after firing all bullets

        canFire = true;  // Allow firing again after cooldown
    }

    void FireSingleBullet()
    {
        if (bulletPrefab != null)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 bulletDirection = (mousePosition - bulletSpawnPoint.position).normalized;

            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

            // Apply the same scale direction as the gun to prevent unwanted flipping
            bullet.transform.localScale = new Vector3(
                transform.localScale.x > 0 ? Mathf.Abs(bullet.transform.localScale.x) : -Mathf.Abs(bullet.transform.localScale.x),
                bullet.transform.localScale.y,
                bullet.transform.localScale.z
            );

            StraightBullet straightBullet = bullet.GetComponent<StraightBullet>();
            if (straightBullet != null)
            {
                straightBullet.Initialize(bulletDirection, EffectiveBulletSpeed, level);
            }

            // Play fire sound PER bullet, with random pitch
            PlayFireSoundPerBullet();
        }
    }

    void PlayFireSoundPerBullet()
    {
        if (fireClip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(fireClip, 1f);
        }
    }

    void UpdateLevel()
    {
        // Update level based on levelTemp
        level = level + _levelTemp;

        // Reset levelTemp to 0 after applying it
        if (_levelTemp != 0)
        {
            Debug.Log("Level updated to: " + level);
            _levelTemp = 0;
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
    public void ResetSpeed()
    {
        // Restore all main stats and statuses to your default
        bulletBonus = 0;
    }
}