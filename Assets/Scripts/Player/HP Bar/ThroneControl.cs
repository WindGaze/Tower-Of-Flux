using UnityEngine;

public class ThroneControl : MonoBehaviour
{
    private Collider2D myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
            Debug.LogWarning($"No Collider2D found on {gameObject.name}");

        // Set initial state: disable if Boss exists
        if (myCollider != null && GameObject.FindGameObjectWithTag("Boss") != null)
            myCollider.enabled = false;
    }

    void Update()
    {
        if (myCollider == null) return;

        // Enable collider if boss is gone, disable if boss exists
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        myCollider.enabled = (boss == null);
    }
}