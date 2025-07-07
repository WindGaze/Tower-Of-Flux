using UnityEngine;

public class MarkerCollisionCounter : MonoBehaviour
{
    public int markerHitCount = 0;

   private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Marker"))
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncrementMarkerCollisions();
        }
    }
}
}
