using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool isInvalid = false;  // Tracks if the door is invalid (can't be destroyed)
    private Collider2D doorCollider;

    private void Start()
    {
        // Get the collider component
        doorCollider = GetComponent<Collider2D>();
        if (doorCollider == null)
        {
            Debug.LogError("No Collider2D found on the door object!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the object is the player, turn off the trigger
        if (other.CompareTag("Player") && doorCollider != null)
        {
            doorCollider.isTrigger = false;
            Debug.Log("Player touched the door. Trigger turned off!");
        }
        
        // If the object is a wall, mark the door as invalid permanently
        if (other.CompareTag("Wall"))
        {
            isInvalid = true;  // Set the door to an invalid state if a wall is nearby (permanent)
            Debug.Log("Wall detected! Door is now permanently invalid.");
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If the player is no longer in contact, turn the trigger back on
        if (collision.gameObject.CompareTag("Player") && doorCollider != null)
        {
            doorCollider.isTrigger = true;
            Debug.Log("Player left the door. Trigger turned back on!");
        }
    }

    // Removed the resetting of invalid state, so it stays permanently invalid
    private void OnTriggerExit2D(Collider2D other)
    {
        // No changes to isInvalid here, so it won't reset if the wall exits
        if (other.CompareTag("Wall"))
        {
            Debug.Log("Wall exited, but door remains permanently invalid.");
        }
    }
}
