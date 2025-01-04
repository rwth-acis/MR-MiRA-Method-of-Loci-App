using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using OVRSimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    private List<User> _users = new List<User>();
    private User _currentUser;
    private Room _currentRoom;
    private int _freeSceneID = 2;

    //for testing
    [SerializeField] public GameObject furniture;
    [SerializeField] public GameObject furniture2;
    [SerializeField] public string json;

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
        LoadUser("Lena");


        // User newUser = new User("Lena");
        // _users.Add(newUser);
        // _currentUser = _users[0];

        // Create a new room and add it to the list of rooms and the user

        if(_currentUser.GetFirstRoom() == null)
        {
            Debug.Log("First room is null");
            Room room = new Room(0);
            _currentUser.AddRoom(room);
            _currentRoom = _currentUser.GetFirstRoom();
            Debug.Log("Current Room ID in RoomManager start: " + _currentRoom.ID);
        }
        LoadRoom(_currentRoom);
        //testing
        // _rooms[_currentUser.GetCurrentRoomID()].AddFurniture(furniture);
        // Debug.Log("Furniture added to room " + _currentUser.GetCurrentRoomID());
        // SaveRoom saveRoom = new SaveRoom();
        // saveRoom.roomID = _currentUser.GetCurrentRoomID();
        // saveRoom.saveObject = furniture;
        // json = JsonUtility.ToJson(saveRoom);
        // Debug.Log("Furniture saved to JSON: " + json);

        // AddFurniture();
    }

    // Called when enter is pressed in the input field
    public void GetInputName(string name)
    {
        SwitchUser(name);
    }

    public void LoadUser(string username)
    {
        string path = Path.Combine(Application.persistentDataPath, username + ".json");
        User newUser;
        // Check if the user's save file exists
        if (!File.Exists(path))
        {
            Debug.Log("File not found");
            newUser = new User(username);
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
    }

    /// <summary>
    /// Creates a new room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "New Room"</param>
    public void CreateRoom(bool value)
    {
        Debug.Log("Creating new room for user " + _currentUser);
        int newRoomID = _currentUser.GetFreeRoomID();
        Room room = new Room(newRoomID);
        _currentUser.AddRoom(room);
        // Go to the newly created room
        _currentRoom = _currentUser.GetCurrentRoom();
        LoadRoom(room);
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
        // // get the scene that is currently active
        // int currentSceneID = SceneManager.GetActiveScene().buildIndex + 1;
        // // load the next room
        // SceneManager.LoadScene("Room " + _freeSceneID);
        // // set the free scene ID to the current scene ID
        // _freeSceneID = currentSceneID;
        // _currentRoomID = nextRoomID;
        LoadRoom(nextRoom);
    }

    public void PreviousScene(bool value)
    {
        Room previousRoom = _currentUser.PreviousRoom();
        if (previousRoom == null)
        {
            Debug.Log("There is no previous room available in RoomManager");
            return; // TODO: Error Message or Grey out Button
        }
        // int currentSceneID = SceneManager.GetActiveScene().buildIndex + 1;
        // SceneManager.LoadScene("Room " + _freeSceneID);
        // _freeSceneID = currentSceneID;
        // _currentRoomID = previousRoomID;
        LoadRoom(previousRoom);
    }

    public void LoadRoom(Room room)
    {
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
        Debug.Log("We are loading room " + room.ID);
        if (room != null && room.HasFurniture())
        {
            List<GameObject> roomFurniture = room.Furniture;
            foreach (GameObject furniture in roomFurniture)
            {   furniture.tag = "Furniture";

            }
            WaitForSeconds wait = new WaitForSeconds(2);

            foreach (GameObject furniture in roomFurniture)
            {
                GameObject.Instantiate(furniture, new Vector3(1, 1, 0), Quaternion.identity);
            }
        }
    }

    public void AddFurniture(bool value)
    {
        // Add furniture to the current room's list of furniture
        _currentRoom.AddFurniture(furniture);
        _currentRoom.AddFurniture(furniture2);
        Debug.Log("Furniture added to room " + _currentUser.GetCurrentRoom().ID);

        // Instantiate the furniture in the scene
        GameObject newObject = GameObject.Instantiate(furniture, new Vector3(1, 1, 0), Quaternion.identity);
        GameObject newObject2 = GameObject.Instantiate(furniture2, new Vector3(2, 1, 0), Quaternion.identity);
        newObject.tag = "Furniture";
        // Save the furniture to JSON
        SaveRoom saveRoom = new SaveRoom();
        saveRoom.roomID = _currentUser.CurrentRoomID;
        saveRoom.furniture = _currentRoom.Furniture;
        json = JsonUtility.ToJson(saveRoom); // TODO Save and load to/from User
        Debug.Log("Furniture saved to JSON: " + json);
    }

    /// <summary>
    /// Switch the user and save all the rooms of the current user
    /// </summary>
    public void SwitchUser(string username)
    {
        // Save the current user
        _currentUser.SaveUser();
        // Switch to the next user
        _users.Add(new User(username));
        // LoadUser(username);
    }
}
