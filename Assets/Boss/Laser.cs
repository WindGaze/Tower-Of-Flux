using UnityEngine;

public class Laser : MonoBehaviour
{
    public float extendSpeed = 5f;    // Speed at which the laser extends
    public float maxLength = 10f;     // Maximum length of the laser
    private float currentLength = 0f; // Current length of the laser
    public int damage = 10;
    
    private SpriteRenderer spriteRenderer;
    private float initialWidth;       // Original width of the sprite
    private Vector2 initialSize;      // Original size in pixels

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Get the initial size in world units
        initialWidth = spriteRenderer.bounds.size.x;
        initialSize = spriteRenderer.sprite.bounds.size;
    }

    void Update()
    {
        if (currentLength < maxLength)
        {
            // Calculate extension
            float extendAmount = extendSpeed * Time.deltaTime;
            currentLength = Mathf.Min(currentLength + extendAmount, maxLength);
            
            // Calculate new scale (preserving original height)
            float scaleX = (initialWidth + currentLength) / initialSize.x;
            float scaleY = transform.localScale.y; // Maintain original Y scale
            
            // Apply scaling (works for any sprite shape)
            transform.localScale = new Vector3(scaleX, scaleY, 1);
            
            // Adjust position to extend rightward
            transform.position += transform.right * (extendAmount / 2f);
        }
    }
}