using UnityEngine;

public class ShadowLightning : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 15;

    [Header("Audio")]
    public AudioClip lightningClip; // Assign in Inspector
    private AudioSource audioSource;

    [Header("Components")]
    private Collider2D strikeCollider;
    private bool canDamage = false;

    private void Awake()
    {
        strikeCollider = GetComponent<Collider2D>();
        strikeCollider.isTrigger = true;
        strikeCollider.enabled = false; // Start disabled

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Called via animation event to play lightning sound
    public void PlayLightningSound()
    {
        if (lightningClip != null && audioSource != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(lightningClip, 1f);
        }
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

        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
}