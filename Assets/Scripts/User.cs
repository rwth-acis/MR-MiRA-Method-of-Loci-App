using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class User
{
    private List<Room> _rooms = new List<Room>();
    public string _name { get; private set; }
    private SaveData _saveData;
    public string SaveDataJSON { get; private set; }

    // place in the array of rooms
    public int CurrentRoomID { get; private set; } = 0;

    public User(string name)
    {
        _name = name;
    }

    public User(string name, SaveData saveData)
    {
        _name = name;
        _saveData = saveData;
        foreach (SaveRoom saveRoom in _saveData.SaveRooms)
        {
            _rooms.Add(new Room(saveRoom));
        }
    }

    public void AddRoom(Room room)
    {
        _rooms.Add(room);
        // set the current room to the last room added
        CurrentRoomID = _rooms.Count - 1;
        Debug.Log("Current Room ID in User addroom: " + CurrentRoomID);
        Debug.Log("Current Room in User addroom: " + _rooms[CurrentRoomID]);
        Debug.Log("Rooms addroom: " + _rooms);
    }

    public Room NextRoom()
    {
        if (CurrentRoomID >= _rooms.Count)
        {
            Debug.Log("No more rooms available");
            return null;
        }

        CurrentRoomID++;
        Debug.Log("Current Room ID in User next: " + CurrentRoomID);
        Debug.Log("Current Room in User next: " + _rooms[CurrentRoomID]);
        Debug.Log("Rooms in user next: " + _rooms);
        return _rooms[CurrentRoomID];
    }

    public Room PreviousRoom()
    {
        if (CurrentRoomID > 0)
        {
            CurrentRoomID--;
            Debug.Log("Current Room ID in User prev: " + CurrentRoomID);
            Debug.Log("Current Room in User prev: " + _rooms[CurrentRoomID]);
            Debug.Log("Rooms in user prev: " + _rooms);
            return _rooms[CurrentRoomID];
        }

        return null;
    }

    // return the id of the current room
    public Room GetCurrentRoom()
    {
        Debug.Log("Current Room ID in User getid: " + CurrentRoomID);
        Debug.Log("Current Room in User getid: " + _rooms[CurrentRoomID]);
        Debug.Log("Rooms in user getid: " + _rooms);
        return _rooms[CurrentRoomID];
    }

    public Room GetFirstRoom()
    {
        if (_rooms.Count > 0)
        {
            return _rooms[0];
        }

        return null;
    }

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
        File.WriteAllText(path, SaveDataJSON);
    }
}