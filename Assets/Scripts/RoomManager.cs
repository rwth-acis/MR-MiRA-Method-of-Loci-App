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
        LoadUser("Lena");
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
        Debug.Log("Creating new room for user " + _currentUser);
        int newRoomID = _currentUser.GetFreeRoomID();
        Room room = new Room(newRoomID);
        _currentUser.AddRoom(room);
        // Go to the newly created room
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
        Debug.Log("LENA: We are loading room " + room.ID);
        if (room != null && room.HasFurniture())
        {
            // List of the prefabs of the furniture in the room
            List<GameObject> roomFurniture = room.Furniture;
            for (int i = 0; i < roomFurniture.Count; i++)
            {
                //Debug.Log("LENA: Instantiating furniture on position " + room.FurnitureTransforms[i].position);
                Debug.Log("LENA: Instantiating furniture" + i + roomFurniture[i].transform.position);
                GameObject myObject = GameObject.Instantiate(roomFurniture[i]);
                // myObject.transform.position = room.FurnitureTransforms[i].position;
                // myObject.transform.rotation = room.FurnitureTransforms[i].rotation;
                // myObject.transform.localScale = room.FurnitureTransforms[i].localScale;
                Debug.Log("LENA:  " + room.FurnitureInstances.Count + "<=" + roomFurniture.Count);
                if (room.FurnitureInstances.Count < roomFurniture.Count)
                {
                    room.AddFurnitureInstance(myObject);
                }
            }
            //TODO maybe load the transforms step by step
            room.LoadTransforms();
            foreach (GameObject furniture in roomFurniture)
            {
                furniture.tag = "Furniture";
            }
        }
    }

    public void AddFurniture(bool value)
    {
        Debug.Log("LENA: Furniture added to room " + _currentUser.GetCurrentRoom().ID);

        // Instantiate the furniture in the scene
        GameObject newObject = GameObject.Instantiate(furniture, Vector3.zero, Quaternion.identity);
        GameObject newObject2 = GameObject.Instantiate(furniture2, Vector3.zero, Quaternion.identity);
        newObject.tag = "Furniture";
        newObject2.tag = "Furniture";
        furniture.tag = "Furniture";
        furniture2.tag = "Furniture";
        // Add furniture to the current room's list of furniture
        _currentRoom.AddFurniture(furniture, newObject);
        _currentRoom.AddFurniture(furniture2, newObject2);
        // _currentRoom.AddFurnitureTransform(newObject.transform);
        // _currentRoom.AddFurnitureTransform(newObject2.transform);
        _currentRoom.UpdateTransforms();
        _currentRoom.SaveRoom();

        //for testing //TODO remove
        // Debug.Log("Saving User " + _currentUser._name);
        // _currentUser.SaveUser();
        // LoadUser("Lena");
    }

    /// <summary>
    /// Switch the user and save all the rooms of the current user
    /// </summary>
    public void SwitchUser(string username)
    {
        // Save the current user
        _currentUser.SaveUser();
        // Switch to the next user
        LoadUser(username);
    }
}
