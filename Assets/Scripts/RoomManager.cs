using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    private List<Room> _rooms = new List<Room>();
    private List<User> _users = new List<User>();
    private User _currentUser;

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
    }

    public void PreviousScene(bool value)
    {
        int nextRoomID = _currentUser.NextRoom();
        if (nextRoomID == -1)
        {
            return; // TODO: Error Message or Grey out Button
        }
        SceneManager.LoadScene("Room 1");
    }

    public void LoadRoom(int roomID, int sceneID)
    {
        // TODO: Load Room Interiors
    }
}
