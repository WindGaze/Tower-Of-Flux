using UnityEngine;

namespace MyGame.RoomSystem
{
    public class RoomSpawner : MonoBehaviour
    {
        public RoomManager roomManager;  // Reference to the RoomManager
        public RoomEntrance doorDirection; // Direction of the door in this room (Top, Bottom, Left, Right)
        public Transform leftSpawn, rightSpawn, topSpawn, bottomSpawn; // Spawn points for new rooms

        private bool hasSpawned = false; // To track whether the door has already triggered

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && !hasSpawned)
            {
                SpawnNewRoom();  // Call room spawning method when the player enters the trigger
                hasSpawned = true; // Prevent further spawning
            }
        }

        private void SpawnNewRoom()
        {
            Transform spawnPoint = null;
            RoomEntrance requiredEntrance = RoomEntrance.Top;

            // Determine which spawn point to use based on the door's direction
            switch (doorDirection)
            {
                case RoomEntrance.Left:
                    spawnPoint = leftSpawn;
                    requiredEntrance = RoomEntrance.Right; // The room to spawn needs a right door
                    break;
                case RoomEntrance.Right:
                    spawnPoint = rightSpawn;
                    requiredEntrance = RoomEntrance.Left; // The room to spawn needs a left door
                    break;
                case RoomEntrance.Top:
                    spawnPoint = topSpawn;
                    requiredEntrance = RoomEntrance.Bottom; // The room to spawn needs a bottom door
                    break;
                case RoomEntrance.Bottom:
                    spawnPoint = bottomSpawn;
                    requiredEntrance = RoomEntrance.Top; // The room to spawn needs a top door
                    break;
            }

            if (spawnPoint != null)
            {
                RoomData randomRoom = roomManager.GetRandomRoomWithMatchingEntrance(requiredEntrance);

                if (randomRoom != null)
                {
                    // Spawn the random room at the selected spawn point
                    GameObject newRoom = Instantiate(randomRoom.roomPrefab, spawnPoint.position, Quaternion.identity);

                    // Assign RoomManager to the RoomSpawner components of the doors in the newly instantiated room
                    RoomSpawner[] spawners = newRoom.GetComponentsInChildren<RoomSpawner>();
                    foreach (RoomSpawner spawner in spawners)
                    {
                        spawner.roomManager = this.roomManager;  // Assign the RoomManager reference
                    }

                    // Destroy the opposite door in the new room after it is spawned
                    DestroyOppositeDoor(newRoom);
                }
                else
                {
                    Debug.LogWarning("No valid room to spawn for this entrance!");
                }
            }
        }

        // Method to destroy the opposite door based on the direction the player entered from
        private void DestroyOppositeDoor(GameObject newRoom)
        {
            // Find the opposite door based on the entry direction
            string oppositeTag = GetOppositeTag();

            // Find the door with the opposite tag in the new room and destroy it
            foreach (Transform child in newRoom.transform)
            {
                if (child.CompareTag(oppositeTag))
                {
                    Destroy(child.gameObject);  // Destroy the opposite door
                    break;
                }
            }
        }

        // Get the tag of the opposite door based on the player's entry direction
        private string GetOppositeTag()
        {
            switch (doorDirection)
            {
                case RoomEntrance.Left:
                    return "Right"; // Entered from Left, opposite is Right
                case RoomEntrance.Right:
                    return "Left"; // Entered from Right, opposite is Left
                case RoomEntrance.Top:
                    return "Bottom"; // Entered from Top, opposite is Bottom
                case RoomEntrance.Bottom:
                    return "Top"; // Entered from Bottom, opposite is Top
                default:
                    return "";
            }
        }
    }
}