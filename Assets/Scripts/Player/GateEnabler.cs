using UnityEngine;

public class GateEnabler : MonoBehaviour
{
    [Header("Gate Objects (set in Inspector)")]
    public GameObject[] gatesToEnable;
    public bool[] isGateEnabled;

    public static GateEnabler Instance;

    private int gPressCount = 0;
    private float lastPressTime = 0f;
    private const float PRESS_WINDOW = 1f;

    private void Awake()
    {
        Instance = this;
        InitializeGateBools();
    }

    private void Start()
    {
        // This applies whatever is currently in isGateEnabled.
        ApplyGateStates();
    }

    private void Update()
    {
        // Cheat: Press G 5 times within a short interval to enable all gates and save.
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (Time.time - lastPressTime > PRESS_WINDOW)
            {
                gPressCount = 0;
            }

            gPressCount++;
            lastPressTime = Time.time;

            if (gPressCount >= 5)
            {
                Debug.Log("GateEnabler CHEAT: All gates enabled & saved!");
                EnableAllGates();
                UpdateGateBoolsFromObjects();
                SaveSystem.SaveGame(null, this);
                gPressCount = 0;
            }
        }
    }

    // Initializes the state array based on inspector objects' current states
    private void InitializeGateBools()
    {
        if (gatesToEnable != null)
        {
            isGateEnabled = new bool[gatesToEnable.Length];
            for (int i = 0; i < gatesToEnable.Length; i++)
            {
                isGateEnabled[i] = gatesToEnable[i] != null && gatesToEnable[i].activeSelf;
            }
        }
    }

    // Sync the bool array from the current state of scene GameObjects
    public void UpdateGateBoolsFromObjects()
    {
        if (gatesToEnable != null && isGateEnabled != null)
        {
            for (int i = 0; i < gatesToEnable.Length; i++)
            {
                if (gatesToEnable[i] != null)
                    isGateEnabled[i] = gatesToEnable[i].activeSelf;
            }
        }
    }

    // Activates/Deactivates gates in the scene based on the bool array
    public void ApplyGateStates()
    {
        if (gatesToEnable != null && isGateEnabled != null)
        {
            for (int i = 0; i < gatesToEnable.Length && i < isGateEnabled.Length; i++)
            {
                if (gatesToEnable[i] != null)
                    gatesToEnable[i].SetActive(isGateEnabled[i]);
            }
        }
    }

    // Call after loading the game to ensure the correct gate state is active in the scene
    public void LoadGatesFromSave()
    {
        ApplyGateStates();
    }

    // Enables all gates and updates state array
    public void EnableAllGates()
    {
        if (gatesToEnable != null)
        {
            for (int i = 0; i < gatesToEnable.Length; i++)
            {
                if (gatesToEnable[i] != null)
                {
                    gatesToEnable[i].SetActive(true);
                    isGateEnabled[i] = true;
                }
            }
        }
    }

    // Disables all gates and updates state array
    public void DisableAllGates()
    {
        if (gatesToEnable != null)
        {
            for (int i = 0; i < gatesToEnable.Length; i++)
            {
                if (gatesToEnable[i] != null)
                {
                    gatesToEnable[i].SetActive(false);
                    isGateEnabled[i] = false;
                }
            }
        }
    }
}