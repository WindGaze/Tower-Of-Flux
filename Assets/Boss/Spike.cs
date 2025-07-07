using System.Collections;
using UnityEngine;

public class Spike : MonoBehaviour
{
    private Animator animator;   // Animator component reference
    private Collider2D spikeCollider; // Collider to activate during the animation
    public float idleDuration = 1.5f; // Adjustable delay for Idle state
    public float activeColliderDelay = 0.5f; // Time delay before activating collider during strike
    public int damage = 10;

    [Header("Audio")]
    public AudioClip spikeSFX;
    public float minPitch = 0.85f;
    public float maxPitch = 1.2f;

    private AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        spikeCollider = GetComponent<Collider2D>();

        if (animator == null)
        {
            Debug.LogError("Animator component not found on Spike!");
            return;
        }

        if (spikeCollider == null)
        {
            Debug.LogError("Collider component not found on Spike!");
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // You may want the spike collider disabled at start, if not already
        spikeCollider.enabled = false;

        StartCoroutine(IdleThenStrike());
    }

    IEnumerator IdleThenStrike()
    {
        animator.SetInteger("Attack", 0);

        yield return new WaitForSeconds(idleDuration);

        animator.SetInteger("Attack", 1);

        yield return new WaitForSeconds(activeColliderDelay);
        ActivateCollider();

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    void ActivateCollider()
    {
        if (spikeCollider != null)
        {
            spikeCollider.enabled = true;
            Debug.Log("Spike Collider activated");
        }
    }

    // Call this from animation event at the strike frame
    public void PlaySpikeSFX()
    {
        if (spikeSFX != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(spikeSFX);
            audioSource.pitch = 1f;
        }
    }
}