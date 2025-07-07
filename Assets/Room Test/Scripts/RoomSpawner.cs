using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour {

	public int openingDirection;
	// 1 --> need bottom door
	// 2 --> need top door
	// 3 --> need left door
	// 4 --> need right door


	private RoomTemplates templates;
	private int rand;
	public bool spawned = false;

	public float waitTime = 4f;

	void Start(){
		Destroy(gameObject, waitTime);
		templates = GameObject.FindGameObjectWithTag("Rooms").GetComponent<RoomTemplates>();
		Invoke("Spawn", 0.5f);
	}


	void Spawn(){
    if (spawned == false){
        // Limit room count
        if (templates.rooms.Count >= templates.maxRooms) {
            // Out of allowed rooms: spawn a closed room instead and mark as spawned
            Instantiate(templates.closedRoom, transform.position, Quaternion.identity);
            spawned = true;
            return;
        }

        GameObject room = null;

        if(openingDirection == 1){
            rand = Random.Range(0, templates.bottomRooms.Length);
            room = Instantiate(templates.bottomRooms[rand], transform.position, templates.bottomRooms[rand].transform.rotation);
        } else if(openingDirection == 2){
            rand = Random.Range(0, templates.topRooms.Length);
            room = Instantiate(templates.topRooms[rand], transform.position, templates.topRooms[rand].transform.rotation);
        } else if(openingDirection == 3){
            rand = Random.Range(0, templates.leftRooms.Length);
            room = Instantiate(templates.leftRooms[rand], transform.position, templates.leftRooms[rand].transform.rotation);
        } else if(openingDirection == 4){
            rand = Random.Range(0, templates.rightRooms.Length);
            room = Instantiate(templates.rightRooms[rand], transform.position, templates.rightRooms[rand].transform.rotation);
        }

        if (room != null) {
            Vector2 roomPosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            if (!templates.roomMap.ContainsKey(roomPosition)) {
                templates.roomMap[roomPosition] = false;
            }
        }

        spawned = true;
    }
}


void OnTriggerEnter2D(Collider2D other){
		if(other.CompareTag("SpawnPoint")){
			if(other.GetComponent<RoomSpawner>().spawned == false && spawned == false){
				Instantiate(templates.closedRoom, transform.position, Quaternion.identity);
				Destroy(gameObject);
			} 
			spawned = true;
		}
	}
}
