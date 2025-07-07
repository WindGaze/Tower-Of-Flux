using UnityEngine;

public class BloodMetalFlask : MonoBehaviour
{
    public int bonusDamageAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.bulletFlask = true;
                playerMovement.bonusDamage += bonusDamageAmount;

                Debug.Log("BloodMetalFlask activated: bulletFlask set to TRUE and bonusDamage increased by " + bonusDamageAmount);
            }
            else
            {
                Debug.LogWarning("PlayerMovement component not found on Player.");
            }

            Destroy(gameObject);
        }
    }
}
