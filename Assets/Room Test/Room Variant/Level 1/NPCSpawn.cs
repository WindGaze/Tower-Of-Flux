using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class NPCSpawnOption
{
    public GameObject npcPrefab;
    [Range(0f, 1f)] public float spawnChance = 0.5f;
}

public class NPCSpawn : MonoBehaviour
{
    [Header("NPC OPTIONS")]
    public List<NPCSpawnOption> npcOptions = new List<NPCSpawnOption>();
    public bool spawnOnStart = true;

    [Header("SPAWN POINT")]
    public Transform spawnPoint; // Manually set where NPC should spawn

    void Start()
    {
        if (spawnOnStart) SpawnNPC();
    }

    public void SpawnNPC()
    {
        if (npcOptions.Count == 0 || spawnPoint == null) return;

        NPCSpawnOption selected = SelectNPC();
        if (selected.npcPrefab != null)
        {
            Instantiate(selected.npcPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    NPCSpawnOption SelectNPC()
    {
        float totalWeight = npcOptions.Sum(opt => opt.spawnChance);
        float randomPoint = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        foreach (NPCSpawnOption option in npcOptions)
        {
            cumulativeWeight += option.spawnChance;
            if (randomPoint <= cumulativeWeight)
            {
                return option;
            }
        }

        return null;
    }

    [ContextMenu("Test Spawn Now")]
    public void TestSpawn()
    {
        SpawnNPC();
    }
}