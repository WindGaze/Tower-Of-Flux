using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Reward
{
    public GameObject uiPrefab; // Must be a UI prefab
    [Range(0.01f, 1f), Tooltip("Higher values = better chance")] 
    public float chance = 0.5f;
}

public class RewardCard : MonoBehaviour
{
    [Header("REWARD SETTINGS")]
    public List<Reward> rewardPool = new List<Reward>();
    public int rewardsToSelect = 3;
    public bool spawnOnStart = true;

    [Header("SPAWN ANCHORS")]
    [Tooltip("Drag 3 empty UI GameObjects here")]
    public RectTransform[] spawnAnchors = new RectTransform[3];

    private List<GameObject> currentRewards = new List<GameObject>();
    private PlayerMovement playerMovement;
    private List<GameObject> gunHolderObjects = new List<GameObject>();
    private bool hasAppliedFreeze = false;

    void Start()
    {
        // Find player movement
        playerMovement = FindObjectOfType<PlayerMovement>();
        
        // Find all objects with GunHolder component
        GunHolder[] gunHolders = FindObjectsOfType<GunHolder>();
        foreach (GunHolder gunHolder in gunHolders)
        {
            gunHolderObjects.Add(gunHolder.gameObject);
        }

        if (spawnOnStart)
        {
            SpawnRewards();
        }
    }

    void OnEnable()
    {
        FreezePlayer();
        DisableGunHolders();
        hasAppliedFreeze = true;
        Debug.Log("RewardCard enabled - Player frozen and guns disabled");
    }

    void OnDisable()
    {
        if (hasAppliedFreeze)
        {
            UnfreezePlayer();
            EnableGunHolders();
            hasAppliedFreeze = false;
            Debug.Log("RewardCard disabled - Player unfrozen and guns enabled");
        }
    }

    private void FreezePlayer()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();
            
        if (playerMovement != null)
        {
            playerMovement.isFrozen = true;
            Debug.Log("Player frozen");
        }
        else
        {
            Debug.LogWarning("PlayerMovement not found!");
        }
    }

    private void UnfreezePlayer()
    {
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();
            
        if (playerMovement != null)
        {
            playerMovement.isFrozen = false;
            Debug.Log("Player unfrozen");
        }
        else
        {
            Debug.LogWarning("PlayerMovement not found!");
        }
    }

    private void DisableGunHolders()
    {
        // Refresh the list in case new gun holders were created
        if (gunHolderObjects.Count == 0)
        {
            GunHolder[] gunHolders = FindObjectsOfType<GunHolder>();
            foreach (GunHolder gunHolder in gunHolders)
            {
                gunHolderObjects.Add(gunHolder.gameObject);
            }
        }

        foreach (GameObject gunHolder in gunHolderObjects)
        {
            if (gunHolder != null)
            {
                gunHolder.SetActive(false);
                Debug.Log($"Disabled gun holder: {gunHolder.name}");
            }
        }
    }

    private void EnableGunHolders()
    {
        foreach (GameObject gunHolder in gunHolderObjects)
        {
            if (gunHolder != null)
            {
                gunHolder.SetActive(true);
                Debug.Log($"Enabled gun holder: {gunHolder.name}");
            }
        }
    }

    public void SpawnRewards()
    {
        if (!ValidateSetup()) return;

        ClearExistingRewards();

        List<Reward> selected = SelectWeightedRewards(rewardsToSelect);
        
        for (int i = 0; i < selected.Count; i++)
        {
            if (selected[i].uiPrefab == null || spawnAnchors[i] == null) continue;

            // Instantiate as child and reset transform
            GameObject reward = Instantiate(
                selected[i].uiPrefab,
                spawnAnchors[i].transform,
                false
            );

            // Properly center in anchor
            RectTransform rt = reward.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;

            currentRewards.Add(reward);
        }
    }

    List<Reward> SelectWeightedRewards(int count)
    {
        List<Reward> available = new List<Reward>(rewardPool);
        List<Reward> selected = new List<Reward>();

        for (int i = 0; i < count; i++)
        {
            float totalWeight = available.Sum(r => r.chance);
            float randomPoint = Random.Range(0f, totalWeight);

            float cumulativeWeight = 0f;
            foreach (Reward reward in available)
            {
                cumulativeWeight += reward.chance;
                if (randomPoint <= cumulativeWeight)
                {
                    selected.Add(reward);
                    available.Remove(reward);
                    break;
                }
            }
        }

        return selected;
    }

    void ClearExistingRewards()
    {
        foreach (GameObject reward in currentRewards)
        {
            if (reward != null) Destroy(reward);
        }
        currentRewards.Clear();
    }

    bool ValidateSetup()
    {
        if (rewardPool.Count < rewardsToSelect)
        {
            Debug.LogError($"Need at least {rewardsToSelect} rewards in pool!");
            return false;
        }

        if (spawnAnchors.Length < rewardsToSelect || spawnAnchors.Any(x => x == null))
        {
            Debug.LogError($"Need at least {rewardsToSelect} valid spawn anchors!");
            return false;
        }

        return true;
    }

    // Method to manually close the canvas (call this when player selects a reward)
    public void CloseCanvas()
    {
        gameObject.SetActive(false);
    }

    [ContextMenu("Test Spawn Now")]
    public void TestSpawn()
    {
        SpawnRewards();
    }

    // Debug method to manually test unfreezing
    [ContextMenu("Test Unfreeze")]
    public void TestUnfreeze()
    {
        UnfreezePlayer();
        EnableGunHolders();
    }
}