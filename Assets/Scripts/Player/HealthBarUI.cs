using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerMovement player;
    public Image healthBarFill;
    private float currentFill = 1f; // starts as full
    public float fillSpeed = 5f;    // tweak for smoothness
    void Awake()
    {
        // Will automatically find the first object with a PlayerMovement component
        player = FindObjectOfType<PlayerMovement>();
        if (player == null)
            Debug.LogError("No PlayerMovement script found in scene!");
    }

    void Update()
    {
        if (player != null && healthBarFill != null)
        {
            float targetFill = (float)player.health / (float)player.maxHealth;
            // Smoothly animate towards the new value
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillSpeed);
            healthBarFill.fillAmount = Mathf.Clamp01(currentFill);
        }
    }
}