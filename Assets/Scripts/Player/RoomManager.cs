using System.Collections.Generic;
using UnityEngine;

public enum RoomType { A, B, C }
public enum RoomEntrance { Top, Bottom, Left, Right }

[System.Serializable]
public class RoomData
{
    public RoomType roomType;
    public List<RoomEntrance> entrances;
    public GameObject roomPrefab;
    public int maxSpawnCount;
}

public class RoomManager : MonoBehaviour
{
    public List<RoomData> roomTypes;
    private Dictionary<RoomType, int> roomCount = new Dictionary<RoomType, int>();
    public Dictionary<RoomType, int> roomTypeLimits = new Dictionary<RoomType, int>();

    private void Awake()
    {
        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        foreach (RoomData room in roomTypes)
        {
            if (!roomCount.ContainsKey(room.roomType))
            {
                roomCount[room.roomType] = 0;
                roomTypeLimits[room.roomType] = room.maxSpawnCount;
            }
        }
    }

    public RoomData GetRandomRoomWithMatchingEntrance(RoomEntrance requiredEntrance)
    {
        if (roomCount.Count == 0 || roomTypeLimits.Count == 0)
        {
            Debug.LogWarning("Dictionaries not initialized. Initializing now.");
            InitializeDictionaries();
        }

        List<RoomData> availableRooms = new List<RoomData>();

        foreach (RoomData room in roomTypes)
        {
            if (room.entrances.Contains(requiredEntrance) && 
                roomCount.ContainsKey(room.roomType) && 
                roomTypeLimits.ContainsKey(room.roomType) && 
                roomCount[room.roomType] < roomTypeLimits[room.roomType])
            {
                availableRooms.Add(room);
            }
        }

        if (availableRooms.Count > 0)
        {
            RoomData selectedRoom = availableRooms[Random.Range(0, availableRooms.Count)];
            roomCount[selectedRoom.roomType]++;
            return selectedRoom;
        }

        return null;
    }

    public void ResetDungeon()
    {
        foreach (RoomType type in roomCount.Keys)
        {
            roomCount[type] = 0;
        }
    }
}