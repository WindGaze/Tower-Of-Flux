using UnityEngine;

public class StraightBullet : MonoBehaviour
{
    public float lifetime = 5f;
    public int damage = 10;

    private Vector2 direction;
    private float speed;
    private Rigidbody2D rb;
    public GameObject hitEffectPrefab;
    public bool gunBullet = true;
    public bool bulletFlash = false;

    private PlayerMovement player;

    [SerializeField]
    private int _bonusDamage = 0;
    private int _baseDamage; // Store the base damage separately
    
    [SerializeField]
    private float _damageMultiplier = 1.0f; // New field for damage multiplier

    public int bonusDamage
    {
        get => _bonusDamage;
        set
        {
            _bonusDamage = value;
            damage = Mathf.RoundToInt((_baseDamage + _bonusDamage) * _damageMultiplier); // Apply multiplier
        }
    }
    
    public float damageMultiplier
    {
        get => _damageMultiplier;
        set
        {
            _damageMultiplier = value;
            damage = Mathf.RoundToInt((_baseDamage + _bonusDamage) * _damageMultiplier); // Update damage when multiplier changes
        }
    }

    public int FinalDamage => damage;

    private void Awake()
    {
        _baseDamage = damage; // Store the original damage value
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on the bullet object!");
        }
        else
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Find player reference
        player = GameObject.FindWithTag("Player")?.GetComponent<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError("PlayerMovement not found!");
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }

        if (bulletFlash && player != null)
        {
            bonusDamage = player.bonusDamage;
        }
    }

    public void Initialize(Vector2 dir, float spd, int level = 1, float multiplier = 1.0f)
    {
        direction = dir.normalized;
        speed = spd;
        
        // Check for wailBind and override multiplier if needed
        if (player != null && player.wailBind)
        {
            multiplier = 0.5f;
        }
        
        _damageMultiplier = multiplier;
        int levelBonus = (level > 1) ? (level - 1) * 1 : 0;
        damage = Mathf.RoundToInt((_baseDamage + levelBonus + _bonusDamage) * _damageMultiplier);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Bullet collided with: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Bullet") || collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("EnemyBullet"))
        {
            return;
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}