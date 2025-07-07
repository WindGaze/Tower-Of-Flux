using UnityEngine;

public class DestroyIfBossDead : MonoBehaviour
{
    private void Update()
    {
        // Check if any GameObject with the "Boss" tag exists
        if (GameObject.FindGameObjectWithTag("Boss") == null)
        {
            // If no boss exists, destroy this object
            Destroy(gameObject);
        }
    }
}