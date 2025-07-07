using UnityEngine;

public class GunBasic : MonoBehaviour
{
    public float rotationOffset = 0f;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        direction.z = 0;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Check if the mouse is to the left of the object
        if (mousePos.x < transform.position.x)
        {
            // Flip the angle for left side
            angle += 180;

            // Flip the scale on X-axis
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else
        {
            // Keep original scale for right side
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }

        // Normalize the angle to be between -180 and 180
        angle = NormalizeAngle(angle);

        // Apply rotation
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);

        // Keep the original position
        transform.localPosition = originalPosition;
    }

    // Helper function to normalize angle to -180 to 180 range
    float NormalizeAngle(float angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
}