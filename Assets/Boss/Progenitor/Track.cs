using UnityEngine;

public class Track : MonoBehaviour
{
    [Tooltip("The target object to track (player)")]
    public Transform target;

    [Tooltip("Rotation speed in degrees per second")]
    public float rotationSpeed = 180f;

    [Tooltip("Should the object look directly at the target?")]
    public bool instantRotation = false;

    private void Start()
    {
        // Automatically find player if target not set
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void Update()
    {
        if (target == null) return;

        // Calculate direction to target
        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotate to face target
        if (instantRotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}