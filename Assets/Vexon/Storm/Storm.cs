using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // For OrderBy

public class Storm : MonoBehaviour
{
    public int level = 1;
    public GameObject stormEffectPrefab;
    public float searchRadius = 50f; // Adjust based on your game's scale
    private Animator animator;
    public AudioClip spawnSFX;
    private AudioSource audioSource;
    
    private void Start()
    {
        if (GameManager.Instance != null)
        {
            level = GameManager.Instance.GetVexonLevel("Storm");
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }

        animator = GetComponent<Animator>();

        // --- Properly initialize audioSource first ---
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // --- Now, safely play the sound! ---
        if (spawnSFX != null)
            audioSource.PlayOneShot(spawnSFX);

        if (level == 2)
        {
            animator.SetInteger("AnimState", 2); 
        }

        if (level == 1)
        {
            animator.SetInteger("AnimState", 1); 
        }
        else if (level == 3)
        {
            animator.SetInteger("AnimState", 3); 
        }
    }

    private void Level1Behavior()
    {
        // Find all enemies and bosses within radius, ordered by proximity
        List<GameObject> validTargets = FindValidTargets();

        if (validTargets.Count == 0)
        {
            Debug.LogWarning("No valid targets found within radius");
            return;
        }

        // Spawn on ALL found targets (up to 4, but accepts fewer)
        int targetsToSpawn = Mathf.Min(validTargets.Count, 4);
        StartCoroutine(SpawnToTargetsRepeatedly(validTargets, targetsToSpawn, 3, 0.2f));
    }

    private List<GameObject> FindValidTargets()
    {
        // Combine both tags
        string[] tagsToSearch = { "Boss", "Enemy" };
        List<GameObject> allTargets = new List<GameObject>();

        foreach (string tag in tagsToSearch)
        {
            allTargets.AddRange(GameObject.FindGameObjectsWithTag(tag));
        }

        // Filter by proximity and remove duplicates
        return allTargets
            .Where(target => target != null)
            .Where(target => Vector3.Distance(transform.position, target.transform.position) <= searchRadius)
            .OrderBy(target => Vector3.Distance(transform.position, target.transform.position))
            .ToList();
    }

    private IEnumerator SpawnToTargetsRepeatedly(List<GameObject> targets, int numTargetsToUse, int times, float delayPerSpawn)
    {
        for (int repeat = 0; repeat < times; repeat++)
        {
            for (int i = 0; i < numTargetsToUse; i++)
            {
                if (stormEffectPrefab != null && targets[i] != null)
                {
                    Instantiate(stormEffectPrefab, targets[i].transform.position, Quaternion.identity);
                }
                yield return new WaitForSeconds(delayPerSpawn);
            }
        }
    }

    private void Level2Behavior()
    {
         List<GameObject> validTargets = FindValidTargets();

        if (validTargets.Count == 0)
        {
            Debug.LogWarning("No valid targets found within radius");
            return;
        }

        // Spawn on ALL found targets (up to 4, but accepts fewer)
        int targetsToSpawn = Mathf.Min(validTargets.Count, 4);
        StartCoroutine(SpawnToTargetsRepeatedly(validTargets, targetsToSpawn, 3, 0.2f));

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                StartCoroutine(TemporarySpeedBoost(playerMovement, 3, 10f));
            }
            else
            {
                Debug.LogWarning("PlayerMovement component not found on Player!");
            }
        }
        else
        {
            Debug.LogWarning("Player object not found!");
        }
    }
    private IEnumerator TemporarySpeedBoost(PlayerMovement player, int boostAmount, float duration)
    {
        player.speedBoost += boostAmount;
        player.UpdatePlayerMovement(); // ensure moveSpeed updates
        Debug.Log($"Storm Level 2: Speed boosted by {boostAmount}!");

        yield return new WaitForSeconds(duration);

        player.speedBoost -= boostAmount;
        player.UpdatePlayerMovement(); // revert moveSpeed back
        Debug.Log("Storm Level 2: Speed boost ended.");
    }

    private void Level3Behavior()
    {
        List<GameObject> validTargets = FindValidTargets();

        if (validTargets.Count == 0)
        {
            Debug.LogWarning("No valid targets found within radius");
            return;
        }

        // Spawn on ALL found targets (up to 4, but accepts fewer)
        int targetsToSpawn = Mathf.Min(validTargets.Count, 5);
        StartCoroutine(SpawnToTargetsRepeatedly(validTargets, targetsToSpawn, 4, 0.2f));
        
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                StartCoroutine(TemporarySpeedBoost(playerMovement, 4, 10f));
            }
            else
            {
                Debug.LogWarning("PlayerMovement component not found on Player!");
            }
        }
        else
        {
            Debug.LogWarning("Player object not found!");
        }
    }

     private void Disappear()
    {
        Destroy(gameObject);
    }
}