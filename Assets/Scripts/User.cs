using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User
{
    private List<int> _rooms = new List<int>();
    private string _name;
    private int _currentRoomID = 0;

    public User(string name)
    {
        _name = name;
    }

    public void AddRoom(int roomID)
    {
        _rooms.Add(roomID);
    }

    public int NextRoom()
    {
        // if (_currentRoomID >= _rooms.Count)
        // {
        //     Debug.Log("Failed in User.cs");
        //     return -1;
        // }
        // _currentRoomID++;
        // return _currentRoomID;
        return 2; //TODO implement, instead of hardcode
    }

    public int PreviousRoom()
    {
        return -1;
    }

}
