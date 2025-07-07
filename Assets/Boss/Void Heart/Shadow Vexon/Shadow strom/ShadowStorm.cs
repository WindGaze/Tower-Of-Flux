using System.Collections;
using UnityEngine;

public class ShadowStorm : MonoBehaviour
{
    public GameObject stormEffectPrefab;
    public float spawnDelay = 0.2f;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetInteger("AnimState", 1);
        }
    }

    // Call this via animation event or script to spawn on player
    public void SpawnStormOnPlayer()
    {
        StartCoroutine(SpawnOnPlayerRepeatedly(6, spawnDelay));
    }

    private IEnumerator SpawnOnPlayerRepeatedly(int times, float delay)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("No Player found to spawn storm effect!");
            yield break;
        }

        for (int i = 0; i < times; i++)
        {
            if (stormEffectPrefab != null)
            {
                Instantiate(stormEffectPrefab, player.transform.position, Quaternion.identity);
            }
            yield return new WaitForSeconds(delay);
        }
    }

    public void Disappear()
    {
        Destroy(gameObject);
    }
}