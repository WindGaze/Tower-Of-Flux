using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GolemBullet2 : MonoBehaviour
{
    public float lifetime = 5f;        // How long the bullet lasts
    public int damage = 10;            // Bullet damage
    public float freezeDuration = 5f;  // How long targets stay frozen
    private Vector2 direction;         // Direction the bullet travels
    private float speed;               // Speed of the bullet
    private Rigidbody2D rb;            // Rigidbody2D for movement
    private int bonusDamage = 0;       // Additional damage based on Golem level
    private float hitCooldown = 0.5f;  // Cooldown between hits for same target
    private HashSet<int> hitTargets = new HashSet<int>(); // Track hit targets by instanceID

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on the bullet object!");
        }
        else
        {
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            else
            {
                Debug.LogError("No Collider2D found on the bullet object!");
            }
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
    }

    // Initialize the bullet with direction, speed, and golem level
    public void Initialize(Vector2 dir, float spd, int level = 1)
    {
        direction = dir.normalized;
        speed = spd;
        bonusDamage = (level > 1) ? (level - 1) * 2 : 0;
        damage += bonusDamage;
    }

    // Bullet can hit both "Enemy" and "Boss" tagged objects
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Bullet triggered with: " + collision.gameObject.name);

        // Check target tag
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        {
            int targetID = collision.gameObject.GetInstanceID();

            if (hitTargets.Contains(targetID))
                return;

            Debug.Log("Hit target: " + collision.gameObject.name);

            hitTargets.Add(targetID);
            StartCoroutine(RemoveTargetFromHitList(targetID, hitCooldown));

            // Try to call TakeDamage(int) on any attached script
            MonoBehaviour[] components = collision.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                var takeDamageMethod = component.GetType().GetMethod("TakeDamage",
                    new System.Type[] { typeof(int) });

                if (takeDamageMethod != null)
                {
                    takeDamageMethod.Invoke(component, new object[] { damage });
                    break; // Only call the first found
                }
            }

            // Apply freeze effect
            GameObject freezeEffectObj = new GameObject("FreezeEffect_" + collision.gameObject.name);
            freezeEffectObj.transform.parent = collision.transform;
            freezeEffectObj.transform.localPosition = Vector3.zero;
            FreezeEffect freezeEffect = freezeEffectObj.AddComponent<FreezeEffect>();
            freezeEffect.Initialize(collision.gameObject, freezeDuration);
        }
    }

    private IEnumerator RemoveTargetFromHitList(int targetID, float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        hitTargets.Remove(targetID);
    }
}

// Handles freezing (disabling scripts, freezing physics, coloring)
public class FreezeEffect : MonoBehaviour
{
    private GameObject target;
    private float duration;
    private Rigidbody2D targetRb;
    private Vector2 originalVelocity;
    private RigidbodyType2D originalBodyType;
    private MonoBehaviour[] enemyScripts;
    private bool[] scriptStates;

    public void Initialize(GameObject targetObj, float freezeDuration)
    {
        target = targetObj;
        duration = freezeDuration;
        StartCoroutine(FreezeCoroutine());
    }

    private IEnumerator FreezeCoroutine()
    {
        if (target == null)
        {
            Destroy(gameObject);
            yield break;
        }

        enemyScripts = target.GetComponents<MonoBehaviour>();
        scriptStates = new bool[enemyScripts.Length];

        // Save and disable all scripts except this component and FreezeEffect
        for (int i = 0; i < enemyScripts.Length; i++)
        {
            if (enemyScripts[i] != this && !(enemyScripts[i] is FreezeEffect))
            {
                scriptStates[i] = enemyScripts[i].enabled;
                enemyScripts[i].enabled = false;
            }
        }

        // Save and freeze physics
        targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb != null)
        {
            originalVelocity = targetRb.velocity;
            originalBodyType = targetRb.bodyType;
            targetRb.velocity = Vector2.zero;
            targetRb.bodyType = RigidbodyType2D.Static;
        }

        // Color freeze visual effect
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (renderer != null)
        {
            originalColor = renderer.color;
            renderer.color = new Color(0.7f, 0.9f, 1f); // ice blue
        }

        Debug.Log($"Target frozen: {target.name} for {duration} seconds");

        yield return new WaitForSeconds(duration);

        // Restore physics
        if (target != null)
        {
            if (targetRb != null)
            {
                targetRb.bodyType = originalBodyType;
                targetRb.velocity = originalVelocity;
            }

            // Restore all scripts
            for (int i = 0; i < enemyScripts.Length; i++)
            {
                if (enemyScripts[i] != null && enemyScripts[i] != this && !(enemyScripts[i] is FreezeEffect))
                {
                    enemyScripts[i].enabled = scriptStates[i];
                }
            }

            // Restore color
            if (renderer != null)
            {
                renderer.color = originalColor;
            }

            Debug.Log($"Target unfrozen: {target.name}");
        }

        Destroy(gameObject);
    }
}