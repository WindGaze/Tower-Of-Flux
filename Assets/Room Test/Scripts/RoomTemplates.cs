using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplates : MonoBehaviour {
public Dictionary<Vector2, bool> roomMap = new Dictionary<Vector2, bool>();

	public GameObject[] bottomRooms;
	public GameObject[] topRooms;
	public GameObject[] leftRooms;
	public GameObject[] rightRooms;

	public GameObject closedRoom;

	public List<GameObject> rooms;

	public float waitTime;
	private bool spawnedBoss;
	public GameObject boss;
    public int maxRooms = 15;

	void Update(){
    if (waitTime <= 0 && spawnedBoss == false) {
        for (int i = 0; i < rooms.Count; i++) {
            if (i == rooms.Count - 1) { // Last room
                GameObject lastRoom = rooms[i];

                // Check if there is an "Event"-tagged object in the room
                GameObject eventObject = GameObject.FindGameObjectWithTag("Event");
                if (eventObject != null && eventObject.transform.position == lastRoom.transform.position) {
                    Destroy(eventObject); // Remove event object
                    Debug.Log("Event object removed, relocating boss.");
                }

                // Spawn boss at last room position
                Instantiate(boss, lastRoom.transform.position, Quaternion.identity);
                spawnedBoss = true;
            }
        }
    } else {
        waitTime -= Time.deltaTime;
    }
}

}
