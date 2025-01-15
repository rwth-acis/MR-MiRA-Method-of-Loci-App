using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Tooltip("The instance of the RoomManager")]
    public static RoomManager Instance { get; private set; }
    [Tooltip("The agent controller")]
    public AgentController agentController;
    private List<User> _users = new List<User>();
    private User _currentUser;
    private Room _currentRoom;

    //for testing
    [SerializeField] public GameObject furniture;
    [SerializeField] public GameObject furniture2;

    private void Awake()
    {
        // Ensure that there is only one instance of RoomManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // TODO get name as input from user
        // Find the agentcontroller in the scene
        agentController = FindObjectOfType<AgentController>();
        LoadUser("Lena");
    }

    /// <summary>
    /// Loads a user from a save file or creates a new user if the save file does not exist.
    /// The first room of the user is loaded.
    /// </summary>
    /// <param name="username">The user that should be loaded</param>
    public void LoadUser(string username)
    {
        string path = Path.Combine(Application.persistentDataPath, username + ".json");
        User newUser;
        if (!File.Exists(path))
        {
            Debug.Log("File not found");
            newUser = new User(username);
            Debug.Log("First room is null");
            Room room = new Room(0);
            newUser.AddRoom(room);
        }
        else
        {
            // Read the user's save file
            string userjson = File.ReadAllText(path);
            newUser = new User(username, JsonUtility.FromJson<SaveData>(userjson));
        }
        _users.Add(newUser);
        _currentUser = newUser;
        _currentRoom = _currentUser.GetFirstRoom();
        LoadRoom(_currentRoom);
    }

    /// <summary>
    /// Creates a new room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "New Room"</param>
    public void CreateRoom(bool value)
    {
        int newRoomID = _currentUser.GetFreeRoomID();
        Room room = new Room(newRoomID);
        _currentUser.AddRoom(room);
        _currentRoom = _currentUser.GetCurrentRoom();
        LoadRoom(room);
        _currentRoom.SaveRoom();
    }

    /// <summary>
    /// Changes the scene to the next room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Next Room"</param>
    public void NextScene(bool value)
    {
        Room nextRoom = _currentUser.NextRoom();
        if (nextRoom == null)
        {
            Debug.Log("There is no next room available in RoomManager");
            return; // TODO: Error Message or Grey out Button
        }
        LoadRoom(nextRoom);
        _currentRoom = _currentUser.GetCurrentRoom();
    }

    /// <summary>
    /// Changes the scene to the previous room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Previous Room"</param>
    public void PreviousScene(bool value)
    {
        Room previousRoom = _currentUser.PreviousRoom();
        if (previousRoom == null)
        {
            Debug.Log("There is no previous room available in RoomManager");
            return; // TODO: Error Message or Grey out Button
        }
        LoadRoom(previousRoom);
        _currentRoom = _currentUser.GetCurrentRoom();
    }

    /// <summary>
    /// Saves the current room to JSON and loads the given room
    /// </summary>
    /// <param name="room">The room to be loaded</param>
    public void LoadRoom(Room room)
    {
        Debug.Log("Furniture instances :" + room.FurnitureInstances.Count);
        room.FurnitureInstances.Clear();
        // Save the current room to JSON
        _currentRoom.SaveRoom();
        _currentUser.SaveUser();
        // First remove all furniture from the scene
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Furniture");
        foreach (GameObject obj in allObjects)
        {
            GameObject.Destroy(obj);
        }
        // Load the room
        Debug.Log("LENA: We are loading room " + room.ID);
        if (room != null && room.HasFurniture())
        {
            // List of the prefabs of the furniture in the room
            List<GameObject> roomFurniture = room.Furniture;
            Debug.Log("Furniture instances :" + room.FurnitureInstances.Count);
            for (int i = 0; i < roomFurniture.Count; i++)
            {
                GameObject myObject = GameObject.Instantiate(roomFurniture[i]);
                myObject.tag = "Furniture";
                room.AddFurnitureInstance(myObject);
            }
            room.LoadTransforms();
        }
    }

    /// <summary>
    /// Adds furniture to the current room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Add Furniture"</param>
    public void AddFurniture(bool value)
    {
        Debug.Log("LENA: Furniture added to room " + _currentUser.GetCurrentRoom().ID);
        // Instantiate the furniture in the scene
        GameObject newObject = GameObject.Instantiate(furniture, Vector3.zero, Quaternion.identity);
        GameObject newObject2 = GameObject.Instantiate(furniture2, Vector3.zero, Quaternion.identity);
        newObject.tag = "Furniture";
        newObject2.tag = "Furniture";
        // Add furniture to the current room's list of furniture
        _currentRoom.AddFurniture(furniture, newObject);
        _currentRoom.AddFurniture(furniture2, newObject2);
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();
    }

    /// <summary>
    /// Save all the rooms of the current user and switch to the next user
    /// </summary>
    /// <param name="username">The user to switch to</param>
    public void SwitchUser(string username)
    {
        _currentUser.SaveUser();
        LoadUser(username);
    }
}
