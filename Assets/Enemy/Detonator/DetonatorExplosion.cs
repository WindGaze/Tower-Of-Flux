using UnityEngine;

public class DetonatorExplosion : MonoBehaviour
{
    public int damage = 10;                 // Damage of the explosion
    private Collider2D explosionCollider;    // Reference to the explosion's collider

    [Header("Explosion Audio")]
    public AudioClip explosionClip;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private void Start()
    {
        // Get the Collider2D component
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

    // Call this from animation frame event to enable the collider
    public void EnableCollider()
    {
        if (explosionCollider != null)
        {
            explosionCollider.enabled = true;
        }
    }

    // Call this from animation frame event to disable the collider
    public void DisableCollider()
    {
        if (explosionCollider != null)
        {
            explosionCollider.enabled = false;
        }
    }

    // Call this from animation frame event to destroy the object
    public void DestroyExplosion()
    {
        Destroy(gameObject);
    }

    // (Optionally) You can add the trigger logic for enemies here
}