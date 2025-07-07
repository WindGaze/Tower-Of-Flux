using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "saveData.json");

    [System.Serializable]
    public class SaveData
    {
        public int playerLevel;
        public int speedLevel;
        public int invincibilityLevel;
        public int playerGems;
        public string currentPrefabName;
        public List<WeaponData> weapons;
        public List<VexonData> vexons;
        public bool wailBind;
        public bool soulBind;
        public bool heartBind;
        public int markerCollisions;

        // Now references gate states for the scene's GateEnabler
        public List<bool> gateStates;
        
        // Intro manager state
        public bool introSeen;
    }

    [System.Serializable]
    public class WeaponData
    {
        public string name;
        public int level;
        public bool isUnlocked;
    }

    [System.Serializable]
    public class VexonData
    {
        public string name;
        public int level;
        public bool isUnlocked;
    }

    // SaveGame: Pass both spawner (optional) and gateEnabler (required for gate save/load)
    public static void SaveGame(TextToObjectSpawner spawner = null, GateEnabler gateEnabler = null)
    {
        SaveData saveData = new SaveData
        {
            playerLevel = GameManager.Instance.playerLevel,
            speedLevel = GameManager.Instance.speedLevel,
            invincibilityLevel = GameManager.Instance.invincibilityLevel,
            playerGems = GameManager.Instance.playerGems,
            markerCollisions = GameManager.Instance.markerCollisions,
            weapons = new List<WeaponData>(),
            vexons = new List<VexonData>(),
            wailBind = GameManager.Instance.wailBind,
            soulBind = GameManager.Instance.soulBind,
            heartBind = GameManager.Instance.heartBind,
            gateStates = gateEnabler != null && gateEnabler.isGateEnabled != null
                ? new List<bool>(gateEnabler.isGateEnabled)
                : new List<bool>(),
            introSeen = LoadIntroSeenState() // Preserve current intro state
        };

        // Save current prefab if spawner is provided
        if (spawner != null && spawner.objectPrefab != null)
        {
            saveData.currentPrefabName = spawner.objectPrefab.name;
        }

        // Weapons
        foreach (var weapon in GameManager.Instance.weapons)
        {
            saveData.weapons.Add(new WeaponData
            {
                name = weapon.name,
                level = weapon.level,
                isUnlocked = weapon.isUnlocked
            });
        }

        // Vexons
        foreach (var vexon in GameManager.Instance.vexons)
        {
            saveData.vexons.Add(new VexonData
            {
                name = vexon.name,
                level = vexon.level,
                isUnlocked = vexon.isUnlocked
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved with prefab: " + (spawner?.objectPrefab?.name ?? "none"));
    }

    // LoadGame: Pass both spawner (optional) and gateEnabler (for gate restore)
    public static void LoadGame(TextToObjectSpawner spawner = null, GateEnabler gateEnabler = null)
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            GameManager.Instance.playerLevel = saveData.playerLevel;
            GameManager.Instance.speedLevel = saveData.speedLevel;
            GameManager.Instance.invincibilityLevel = saveData.invincibilityLevel;
            GameManager.Instance.playerGems = saveData.playerGems;
            GameManager.Instance.markerCollisions = saveData.markerCollisions;
            GameManager.Instance.wailBind = saveData.wailBind;
            GameManager.Instance.soulBind = saveData.soulBind;
            GameManager.Instance.heartBind = saveData.heartBind;

            // Weapons
            for (int i = 0; i < saveData.weapons.Count; i++)
            {
                WeaponData weaponData = saveData.weapons[i];
                int index = GameManager.Instance.weapons.FindIndex(w => w.name == weaponData.name);
                if (index != -1)
                {
                    GameManager.Instance.weapons[index] = new GameManager.Weapon(weaponData.name, GameManager.Instance.weapons[index].prefab)
                    {
                        level = weaponData.level,
                        isUnlocked = weaponData.isUnlocked
                    };
                }
            }

            // Vexons
            for (int i = 0; i < saveData.vexons.Count; i++)
            {
                VexonData vexonData = saveData.vexons[i];
                int index = GameManager.Instance.vexons.FindIndex(v => v.name == vexonData.name);
                if (index != -1)
                {
                    GameManager.Instance.vexons[index] = new GameManager.Vexon(vexonData.name, GameManager.Instance.vexons[index].prefab)
                    {
                        level = vexonData.level,
                        isUnlocked = vexonData.isUnlocked
                    };
                }
            }

            // Gates: restore via GateEnabler
            if (gateEnabler != null && saveData.gateStates != null)
            {
                int gateCount = Mathf.Min(saveData.gateStates.Count, gateEnabler.gatesToEnable.Length);
                for (int i = 0; i < gateEnabler.gatesToEnable.Length; i++)
                {
                    gateEnabler.isGateEnabled[i] = (i < saveData.gateStates.Count) ? saveData.gateStates[i] : false;
                }
                gateEnabler.ApplyGateStates();
            }

            // Prefab
            if (spawner != null && !string.IsNullOrEmpty(saveData.currentPrefabName))
            {
                GameObject loadedPrefab = Resources.Load<GameObject>("Vexon/" + saveData.currentPrefabName);
                if (loadedPrefab != null)
                {
                    spawner.objectPrefab = loadedPrefab;
                    Debug.Log("Loaded saved prefab: " + saveData.currentPrefabName);
                }
                else
                {
                    Debug.LogWarning("Prefab not found in Resources/Vexon: " + saveData.currentPrefabName);
                }
            }

            Debug.Log("Game loaded successfully!");
        }
        else
        {
            Debug.LogWarning("Save file not found, starting new game.");
        }
    }

    // Intro-specific save/load methods
    public static void SaveIntroSeenState(bool seen)
    {
        SaveData saveData;
        
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            // Create new save data if file doesn't exist
            saveData = new SaveData
            {
                playerLevel = GameManager.Instance != null ? GameManager.Instance.playerLevel : 1,
                speedLevel = GameManager.Instance != null ? GameManager.Instance.speedLevel : 1,
                invincibilityLevel = GameManager.Instance != null ? GameManager.Instance.invincibilityLevel : 1,
                playerGems = GameManager.Instance != null ? GameManager.Instance.playerGems : 0,
                markerCollisions = GameManager.Instance != null ? GameManager.Instance.markerCollisions : 0,
                weapons = new List<WeaponData>(),
                vexons = new List<VexonData>(),
                wailBind = GameManager.Instance != null ? GameManager.Instance.wailBind : false,
                soulBind = GameManager.Instance != null ? GameManager.Instance.soulBind : false,
                heartBind = GameManager.Instance != null ? GameManager.Instance.heartBind : false,
                gateStates = new List<bool>()
            };
        }
        
        saveData.introSeen = seen;
        
        string newJson = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, newJson);
        Debug.Log("Intro seen state saved: " + seen);
    }

    public static bool LoadIntroSeenState()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            return saveData.introSeen;
        }
        
        return false; // Default to not seen if no save file exists
    }

    // Method to reset intro state (useful for testing)
    public static void ResetIntroState()
    {
        SaveIntroSeenState(false);
        Debug.Log("Intro state reset - intro will play again");
    }
}