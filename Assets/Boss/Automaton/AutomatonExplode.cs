using UnityEngine;
using System.Collections;

public class AutomatonExplode : MonoBehaviour
{
    public int baseDamage = 10;
    public float explosionDuration = 0.1f;
    public float effectDuration = 1f;

    private Collider2D explosionCollider;
    public int damage;

    [Header("Explosion Audio")]
    public AudioClip explosionClip;
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private void Start()
    {
        explosionCollider = GetComponent<Collider2D>();
        PlayExplosionSoundDetached();

        // Activate damage phase
        StartCoroutine(ExplosionSequence());
    }

    // Detached audio playback, prevents sound cutoff.
    private void PlayExplosionSoundDetached()
    {
        if (explosionClip == null) return;

        GameObject audioObj = new GameObject("ExplosionSound");
        AudioSource tempSource = audioObj.AddComponent<AudioSource>();
        tempSource.clip = explosionClip;
        tempSource.pitch = Random.Range(minPitch, maxPitch);
        tempSource.playOnAwake = false;
        tempSource.volume = 0.5f;
        tempSource.Play();
        Destroy(audioObj, explosionClip.length / tempSource.pitch);
    }

    private IEnumerator ExplosionSequence()
    {
        if (explosionCollider != null)
        {
            explosionCollider.enabled = true;
        }

        yield return new WaitForSeconds(explosionDuration);

        if (explosionCollider != null)
        {
            explosionCollider.enabled = false;
        }

        yield return new WaitForSeconds(effectDuration - explosionDuration);

        Destroy(gameObject);
    }

    public void SetDamage(int gunLevel)
    {
        damage = CalculateDamage(gunLevel);
        Debug.Log("Explosion damage set to: " + damage);
    }

    private int CalculateDamage(int gunLevel)
    {
        return baseDamage + ((gunLevel - 1) * 2);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Drifter hit by explosion: " + collision.gameObject.name);
            collision.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}