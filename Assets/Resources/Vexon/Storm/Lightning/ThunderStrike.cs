using UnityEngine;

public class ThunderStrike : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 15;
    
    [Header("Components")]
    private Collider2D strikeCollider;
    private bool canDamage = false;

    private void Awake()
    {
        strikeCollider = GetComponent<Collider2D>();
        strikeCollider.isTrigger = true;
        strikeCollider.enabled = false; // Start disabled
    }

    // Called via animation event when strike should start dealing damage
    public void ActivateStrike()
    {
        strikeCollider.enabled = true;
        canDamage = true;
    }

    // Called via animation event when strike should stop dealing damage
    public void DeactivateStrike()
    {
        strikeCollider.enabled = false;
        canDamage = false;
    }

    // Called via animation event when the animation completes
    public void DestroyStrike()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            MonoBehaviour[] scripts = other.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                // Try method-based damage
                var takeDamage = script.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
                if (takeDamage != null)
                {
                    takeDamage.Invoke(script, new object[] { damage });
                    break;
                }

                // Fallback: Try direct health field (not recommended, but kept from your original code)
                var healthField = script.GetType().GetField("health");
                if (healthField != null)
                {
                    int currentHealth = (int)healthField.GetValue(script);
                    healthField.SetValue(script, currentHealth - damage);
                    break;
                }
            }
        }
    }
}