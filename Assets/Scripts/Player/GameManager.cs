using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public delegate void OnPlayerLevelChangedDelegate(int newLevel);
    public event OnPlayerLevelChangedDelegate OnPlayerLevelChanged;

    public delegate void OnSpeedLevelChangedDelegate(int newLevel);
    public event OnSpeedLevelChangedDelegate OnSpeedLevelChanged;

    [Header("Player Bind Status")]
    public bool wailBind = false;
    public bool soulBind = false;
    public bool heartBind = false;

    [Header("Objects to Enable on Cheat")]
    [SerializeField] private List<GameObject> cheatObjects = new List<GameObject>();

    [SerializeField]
    private int _playerLevel = 1;
    public int playerLevel
    {
        get { return _playerLevel; }
        set
        {
            _playerLevel = value;
            OnPlayerLevelChanged?.Invoke(_playerLevel);
            UpdatePlayerMovement();
        }
    }

    [SerializeField]
    private int _speedLevel = 1;
    public int speedLevel
    {
        get { return _speedLevel; }
        set
        {
            _speedLevel = value;
            OnSpeedLevelChanged?.Invoke(_speedLevel);
            UpdatePlayerMovement();
        }
    }

    [SerializeField]
    private int _invincibilityLevel = 1;
    public int invincibilityLevel
    {
        get { return _invincibilityLevel; }
        set
        {
            _invincibilityLevel = value;
            UpdatePlayerMovement();
        }
    }

    public int playerGems = 0;
    public delegate void OnMarkerCollisionDelegate(int newCount);
    public event OnMarkerCollisionDelegate OnMarkerCollision;

    [SerializeField]
    private int _markerCollisions = 0;
    public int markerCollisions
    {
        get { return _markerCollisions; }
        set
        {
            _markerCollisions = value;
            OnMarkerCollision?.Invoke(_markerCollisions);
        }
    }

    private PlayerMovement playerMovement;
    private int gPressCount = 0;
    private float lastPressTime = 0f;
    private const float PRESS_WINDOW = 1f;

    [System.Serializable]
    public struct Weapon
    {
        public string name;
        public GameObject prefab;
        public int level;
        public bool isUnlocked;

        public Weapon(string name, GameObject prefab)
        {
            this.name = name;
            this.prefab = prefab;
            this.level = 0;
            this.isUnlocked = false;
        }
    }

    [System.Serializable]
    public struct Vexon
    {
        public string name;
        public GameObject prefab;
        public int level;
        public bool isUnlocked;

        public Vexon(string name, GameObject prefab)
        {
            this.name = name;
            this.prefab = prefab;
            this.level = 0;
            this.isUnlocked = false;
        }
    }

    public List<Weapon> weapons = new List<Weapon>();
    public List<Vexon> vexons = new List<Vexon>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeWeapons();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SaveSystem.LoadGame();
        FindPlayerMovement();
    }

    private void Update()
    {
        // Manual save/load
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveSystem.SaveGame();
            Debug.Log("Game Saved");
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveSystem.LoadGame();
            Debug.Log("Game Loaded");
        }

        // Cheat code detection
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
                ActivateCheat();
                gPressCount = 0;
            }
        }
        SaveSystem.SaveGame();
    }

    private void ActivateCheat()
    {
        Debug.Log("CHEAT ACTIVATED!");

        // Enable all cheat objects
        EnableCheatObjects();

        // Set all binds
        wailBind = true;
        soulBind = true;
        heartBind = true;

        // Set levels
        playerLevel = 5;
        speedLevel = 5;
        invincibilityLevel = 5;

        // Set gems
        playerGems = 99999;

        // Unlock and upgrade weapons
        for (int i = 0; i < weapons.Count; i++)
        {
            UnlockWeapon(weapons[i].name);
            for (int lvl = weapons[i].level; lvl < 5; lvl++)
            {
                UpgradeWeapon(weapons[i].name);
            }
        }

        // Unlock and upgrade vexon
        for (int i = 0; i < vexons.Count; i++)
        {
            UnlockVexon(vexons[i].name);
            for (int lvl = vexons[i].level; lvl < 3; lvl++)
            {
                UpgradeVexon(vexons[i].name);
            }
        }

        SaveSystem.SaveGame();
    }

    private void EnableCheatObjects()
    {
        foreach (GameObject obj in cheatObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                Debug.Log($"Enabled cheat object: {obj.name}");
            }
        }
    }

    // Optional: Method to disable all cheat objects (if you need to reset them)
    public void DisableCheatObjects()
    {
        foreach (GameObject obj in cheatObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"Disabled cheat object: {obj.name}");
            }
        }
    }

    public void IncrementMarkerCollisions()
    {
        markerCollisions++;
        Debug.Log($"Marker collision! Total: {markerCollisions}");
        SaveSystem.SaveGame();
    }

    public void ResetMarkerCollisions()
    {
        markerCollisions = 0;
        Debug.Log("Marker collisions reset to 0");
    }

    private void FindPlayerMovement()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement component not found!");
        }
    }

    private void UpdatePlayerMovement()
    {
        if (playerMovement == null)
        {
            FindPlayerMovement();
        }

        if (playerMovement != null)
        {
            playerMovement.SyncWithGameManager();
        }
    }

    private void InitializeWeapons()
    {
        weapons.Add(new Weapon("Sword", Resources.Load<GameObject>("Prefabs/Sword")));
        weapons.Add(new Weapon("Axe", Resources.Load<GameObject>("Prefabs/Axe")));
        weapons.Add(new Weapon("Bow", Resources.Load<GameObject>("Prefabs/Bow")));
        weapons.Add(new Weapon("Dagger", Resources.Load<GameObject>("Prefabs/Dagger")));
    }

    public void UpgradeWeapon(string weaponName)
    {
        int index = weapons.FindIndex(w => w.name == weaponName);
        if (index != -1 && weapons[index].isUnlocked)
        {
            Weapon updatedWeapon = weapons[index];
            updatedWeapon.level++;
            weapons[index] = updatedWeapon;
            SaveSystem.SaveGame();
            Debug.Log($"{weaponName} upgraded to level {updatedWeapon.level}");
        }
        else
        {
            Debug.LogError($"Weapon {weaponName} not found or locked!");
        }
    }

    public int GetWeaponLevel(string weaponName)
    {
        int index = weapons.FindIndex(w => w.name == weaponName);
        return index != -1 ? weapons[index].level : -1;
    }

    public void UnlockWeapon(string weaponName)
    {
        int index = weapons.FindIndex(w => w.name == weaponName);
        if (index != -1)
        {
            Weapon updatedWeapon = weapons[index];
            updatedWeapon.isUnlocked = true;
            weapons[index] = updatedWeapon;
            SaveSystem.SaveGame();
            Debug.Log($"{weaponName} unlocked");
        }
        else
        {
            Debug.LogError($"Weapon {weaponName} not found!");
        }
    }

    public int GetVexonLevel(string vexonName)
    {
        int index = vexons.FindIndex(v => v.name == vexonName);
        return index != -1 ? vexons[index].level : -1;
    }

    public void UnlockVexon(string vexonName)
    {
        int index = vexons.FindIndex(v => v.name == vexonName);
        if (index != -1)
        {
            Vexon updatedVexon = vexons[index];
            updatedVexon.isUnlocked = true;
            vexons[index] = updatedVexon;
            SaveSystem.SaveGame();
            Debug.Log($"{vexonName} unlocked");
        }
        else
        {
            Debug.LogError($"Vexon {vexonName} not found!");
        }
    }

    public void UpgradeVexon(string vexonName)
    {
        int index = vexons.FindIndex(v => v.name == vexonName);
        if (index != -1 && vexons[index].isUnlocked)
        {
            Vexon updatedVexon = vexons[index];
            updatedVexon.level++;
            vexons[index] = updatedVexon;
            SaveSystem.SaveGame();
            Debug.Log($"{vexonName} upgraded to level {updatedVexon.level}");
        }
        else
        {
            Debug.LogError($"Vexon {vexonName} not found or locked!");
        }
    }
}