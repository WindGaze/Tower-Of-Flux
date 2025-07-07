using UnityEngine;

public class WallIgnore : MonoBehaviour
{
    [Tooltip("Tags of objects this GameObject should ignore collisions with")]
    public string[] tagsToIgnore = { "Player", "Bullet", "Shield" };

    [Tooltip("Layers this GameObject should ignore collisions with")]
    public LayerMask layersToIgnore;

    void Start()
    {
        Collider2D ownCollider = GetComponent<Collider2D>();
        if (ownCollider == null)
        {
            Debug.LogWarning("WallIgnore: No Collider2D found on this object.");
            return;
        }

        // Ignore objects with specific tags
        foreach (string tag in tagsToIgnore)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in taggedObjects)
            {
                Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D col in colliders)
                {
                    Physics2D.IgnoreCollision(ownCollider, col);
                }
            }
        }

        // Ignore objects on specific layers
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & layersToIgnore) != 0)
            {
                Collider2D[] colliders = obj.GetComponentsInChildren<Collider2D>();
                foreach (Collider2D col in colliders)
                {
                    Physics2D.IgnoreCollision(ownCollider, col);
                }
            }
        }
    }
}
