using UnityEngine;
using UnityEngine.UI;

public class VoidGate : MonoBehaviour
{
    [SerializeField] private GameObject gateCanvas; // Assign your gate canvas in inspector
    [SerializeField] private Text gateText; // Optional: a text element to show a message

    private bool gateOpened = false;

   private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.wailBind &&
                GameManager.Instance.soulBind &&
                GameManager.Instance.heartBind)
            {
                OpenGate();
            }
            else
            {
                Debug.Log("VoidGate: Bind conditions not met (all wailBind, soulBind, heartBind must be true).");
            }
        }
    }

    private void OpenGate()
    {
        gateOpened = true;

        if (gateCanvas != null)
        {
            gateCanvas.SetActive(true);

            if (gateText != null)
            {
                gateText.text = "The Void Gate Opens...";
            }
        }
        Debug.Log("VoidGate: All binds present, gate is now open!");
    }
}