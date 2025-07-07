using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AOEGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public string weaponName; // Add this to identify which weapon this is
    public float rotationOffset = 0f;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;  // The AOE bullet prefab
    
    [Header("Firing Settings")]
    public float bulletSpeed = 10f;
    public float fireRate = 0.5f;
    public bool useMouseAim = true;
    public Transform aimTarget;

    [Header("Level Settings")]
    public int level = 1;  // Current level

    [SerializeField] // <-- ADDED
    private float _bulletBonus = 0f; // <-- ADDED
    public float bulletBonus      // <-- ADDED
    {
        get => _bulletBonus;
        set
        {
            _bulletBonus = value;
            Debug.Log($"Bullet speed bonus updated to: {_bulletBonus}");
        }
    }
    public float EffectiveBulletSpeed => bulletSpeed + bulletBonus; // <-- ADDED

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private bool canFire = true;  // Flag to check if we can fire

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        if (bulletSpawnPoint == null)
        {
            Debug.LogWarning("Bullet spawn point not set. Using gun's position as spawn point.");
            bulletSpawnPoint = transform;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        level = 1; // Reset level when scene loads
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
        if (canFire && Input.GetMouseButtonDown(0))  // Fire once per click
        {
            FireBullet();
        }
    }

    void FireBullet()
    {
        if (bulletPrefab != null && canFire)
        {
            Vector3 mousePosition = GetMouseWorldPosition();
            Vector3 bulletDirection = (mousePosition - bulletSpawnPoint.position).normalized;
            
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            
            // Set the bullet's scale to match the gun's orientation
            bullet.transform.localScale = new Vector3(
                transform.localScale.x > 0 ? Mathf.Abs(bullet.transform.localScale.x) : -Mathf.Abs(bullet.transform.localScale.x),
                bullet.transform.localScale.y,
                bullet.transform.localScale.z
            );
            
            AOEExplosionBullet aoeBullet = bullet.GetComponent<AOEExplosionBullet>();
            if (aoeBullet != null)
            {
                aoeBullet.Initialize(bulletDirection, EffectiveBulletSpeed); // <-- CHANGED
            }

            StartCoroutine(Cooldown());  // Start cooldown before firing again
        }
    }
    
    IEnumerator Cooldown()
    {
        canFire = false;
        yield return new WaitForSeconds(fireRate);
        canFire = true;  // Allow firing again
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

    // Example method to increase level
    public void IncreaseLevel(int amount)
    {
        level += amount;
    }
    public void ResetSpeed()
    {
        // Restore all main stats and statuses to your default
        bulletBonus = 0;
    }
}