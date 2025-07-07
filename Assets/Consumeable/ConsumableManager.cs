using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ConsumableItem
{
    public GameObject prefab;
    public bool isActive;
}

public class ConsumableManager : MonoBehaviour
{
    [Header("Consumable Lists")]
    public List<ConsumableItem> consumables = new List<ConsumableItem>();

    // Toggle item activation
    public void SetConsumableActive(int index, bool active)
    {
        if (index >= 0 && index < consumables.Count)
        {
            consumables[index].isActive = active;
        }
    }

    // Get all active consumables
    public List<GameObject> GetActiveConsumables()
    {
        List<GameObject> activeItems = new List<GameObject>();
        foreach (ConsumableItem item in consumables)
        {
            if (item.isActive)
            {
                activeItems.Add(item.prefab);
            }
        }
        return activeItems;
    }

    // Add new consumable to the list
    public void AddConsumable(GameObject prefab, bool isActive = true)
    {
        consumables.Add(new ConsumableItem()
        {
            prefab = prefab,
            isActive = isActive
        });
    }

    // Remove consumable from list
    public void RemoveConsumable(int index)
    {
        if (index >= 0 && index < consumables.Count)
        {
            consumables.RemoveAt(index);
        }
    }
}