using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    private Collider2D explosionCollider;
    public int damage = 10;

    [Header("Explosion Audio")]
    public AudioClip explosionClip;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private void Start()
    {
        // Get the Collider2D component (assuming it's attached to the same object)
        explosionCollider = GetComponent<Collider2D>();
        PlayExplosionSoundDetached();
    }

    // Plays the explosion sound on a temporary GameObject so it is not cut off.
    private void PlayExplosionSoundDetached()
    {
            if (explosionClip == null) return;

        // Create a new temp GameObject for audio only
        GameObject audioObj = new GameObject("ExplosionSound");
        AudioSource tempSource = audioObj.AddComponent<AudioSource>();
        tempSource.clip = explosionClip;
        tempSource.pitch = Random.Range(minPitch, maxPitch);
        tempSource.playOnAwake = false;
        tempSource.volume = 0.5f; // Set volume to half

        tempSource.Play();
        Destroy(audioObj, explosionClip.length / tempSource.pitch);
    }

    
    public void SetDamage(int gunLevel)
    {
        damage = CalculateDamage(gunLevel);
        Debug.Log("Explosion damage set to: " + damage);
    }

    private int CalculateDamage(int gunLevel)
    {
        return damage + ((gunLevel - 1) * 2);
    }

    public void EnableCollider()
    {
        if (explosionCollider != null)
        {
            explosionCollider.enabled = true;
        }
    }

    public void DisableCollider()
    {
        if (explosionCollider != null)
        {
            explosionCollider.enabled = false;
        }
    }

    public void DestroyExplosion()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Deliver damage to objects with tag "Enemy" OR "Boss"
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            Debug.Log("Explosion hit: " + other.name);
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}