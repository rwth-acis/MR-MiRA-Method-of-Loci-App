using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class User
{
    [Tooltip("The name of the user")]
    public string _name { get; private set; }
    [Tooltip("The JSON data of the user")]
    public string SaveDataJSON { get; private set; }
    [Tooltip("Place in the list of rooms")]
    public int CurrentRoomID { get; private set; } = 0;

    private List<Room> _rooms = new List<Room>();
    private SaveData _saveData;

    /// <summary>
    /// Create a new user with the given name
    /// </summary>
    /// <param name="name">The name of the new user</param>
    public User(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Create a new user with the given name and save data for the rooms
    /// </summary>
    /// <param name="name">The name of the new user. Case-sensitive.</param>
    /// <param name="saveData">The save data of the list of rooms</param>
    public User(string name, SaveData saveData)
    {
        _name = name;
        _saveData = saveData;
        foreach (SaveRoom saveRoom in _saveData.SaveRooms)
        {
            _rooms.Add(new Room(saveRoom));
        }
    }

    /// <summary>
    /// Adds a room to the user's list of rooms. Sets the current room to the last room added.
    /// </summary>
    /// <param name="room">The room to be added</param>
    public void AddRoom(Room room)
    {
        _rooms.Add(room);
        CurrentRoomID = _rooms.Count - 1;
    }

    /// <summary>
    /// Changes the current room to the next room in the list of rooms. If there is no next room, returns null.
    /// </summary>
    /// <returns>The next room or null</returns>
    public Room NextRoom()
    {
        if (CurrentRoomID >= _rooms.Count - 1)
        {
            return null;
        }
        CurrentRoomID++;
        return _rooms[CurrentRoomID];
    }

    /// <summary>
    /// Changes the current room to the previous room in the list of rooms. If there is no previous room, returns null.
    /// </summary>
    /// <returns>The previous room or null</returns>
    public Room PreviousRoom()
    {
        if (CurrentRoomID > 0)
        {
            CurrentRoomID--;
            return _rooms[CurrentRoomID];
        }
        return null;
    }

    /// <summary>
    /// Returns the current room of the user
    /// </summary>
    /// <returns>The current room</returns>
    public Room GetCurrentRoom()
    {
        return _rooms[CurrentRoomID];
    }

    /// <summary>
    /// Returns the first room of the user
    /// </summary>
    /// <returns>The first room in the list of rooms or null</returns>
    public Room GetFirstRoom()
    {
        if (_rooms.Count > 0)
        {
            return _rooms[0];
        }
        return null;
    }

    /// <summary>
    /// Returns the next free room id. This is the number of rooms in the list of rooms as we start at 0.
    /// </summary>
    /// <returns>The next free room id</returns>
    public int GetFreeRoomID()
    {
        return _rooms.Count;
    }

    /// <summary>
    /// Generate SaveData for the user and write it to a file
    /// </summary>
    public void SaveUser()
    {
        _saveData = new SaveData();
        // Save all rooms of the user
        foreach (Room room in _rooms)
        {
            _saveData.SaveRooms.Add(room.SaveData);
        }
        // Save JSON data to file
        SaveDataJSON = JsonUtility.ToJson(_saveData);
        string path = Path.Combine(Application.persistentDataPath, _name + ".json");
        Debug.Log("Save to:" + path);
        File.WriteAllText(path, SaveDataJSON);
    }
}