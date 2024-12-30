using System;
using System.Collections;
using System.Collections.Generic;
using OVRSimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    private List<Room> _rooms = new List<Room>();
    private List<User> _users = new List<User>();
    private User _currentUser;

    //for testing
    [SerializeField] public GameObject furniture;
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
        User lena = new User("Lena");
        _users.Add(lena);
        _currentUser = _users[0];
        Room room = new Room(_rooms.Count);
        _rooms.Add(room);
        _currentUser.AddRoom(_rooms.Count);
        //testing
        _rooms[_currentUser.GetCurrentRoomID()].AddFurniture(furniture);
        Debug.Log("Furniture added to room " + _currentUser.GetCurrentRoomID());
        SaveRoom saveRoom = new SaveRoom();
        saveRoom.roomID = _currentUser.GetCurrentRoomID();
        saveRoom.saveObject = furniture;
        json = JsonUtility.ToJson(saveRoom);
        Debug.Log("Furniture saved to JSON: " + json);

        AddFurniture();
    }

    /// <summary>
    /// Creates a new room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "New Room"</param>
    public void CreateRoom(bool value)
    {
        // TODO: neue Szene hinzufügen
        Room room = new Room(_rooms.Count);
        _rooms.Add(room);
        _currentUser.AddRoom(_rooms.Count);
        // Go to the newly created room
        NextScene(true);
    }

    /// <summary>
    /// Changes the scene to the next room when the corresponding button is pressed
    /// </summary>
    /// <param name="value">The value of the button "Next Room"</param>
    public void NextScene(bool value)
    {
        int nextRoomID = _currentUser.NextRoom();
        if (nextRoomID == -1)
        {
            return; // TODO: Error Message or Grey out Button
        }
        SceneManager.LoadScene("Room 2");
        LoadRoom(nextRoomID);
    }

    public void PreviousScene(bool value)
    {
        int previousRoomID = _currentUser.PreviousRoom();
        if (previousRoomID == -1)
        {
            return; // TODO: Error Message or Grey out Button
        }
        SceneManager.LoadScene("Room 1");
        LoadRoom(previousRoomID);
    }

    public void LoadRoom(int roomID)
    {
        // TODO: Load Room Interiors
        GameObject myfurniture = JsonUtility.FromJson<SaveRoom>(json).saveObject;
        WaitForSeconds wait = new WaitForSeconds(2);
        GameObject.Instantiate(myfurniture, new Vector3(1, 1, 0), Quaternion.identity);
        Debug.Log("Furniture loaded from JSON: " + json);
        //_rooms[_currentUser.GetCurrentRoomID()].AddFurniture(furniture);
    }

    public void AddFurniture()
    {
        // _rooms[_currentUser.GetCurrentRoomID()].AddFurniture(furniture);
        // Debug.Log("Furniture added to room " + _currentUser.GetCurrentRoomID());
        // SaveRoom saveRoom = new SaveRoom();
        // saveRoom.roomID = _currentUser.GetCurrentRoomID();
        // saveRoom.saveObject = furniture;
        // json = JsonUtility.ToJson(saveRoom);
        // Debug.Log("Furniture saved to JSON: " + json);
        LoadRoom(_currentUser.GetCurrentRoomID());
    }
}
