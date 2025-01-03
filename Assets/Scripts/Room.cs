using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Room
{
    // id is local to every user
    public int ID { get; private set; }
    public List<GameObject> Furniture { get; private set; } = new List<GameObject>();
    public List<GameObject> Representations { get; private set; } = new List<GameObject>();

    // JSON representation of the room, can be collected by user to generate a save file
    public SaveRoom SaveData { get; private set; }
    public Room(int id)
    {
        ID = id;
    }

    public Room(SaveRoom saveRoom)
    {
        SaveData = saveRoom;
        ID = saveRoom.roomID;
        Furniture = saveRoom.furniture;
        Representations = saveRoom.representations;
    }

    public void AddFurniture(GameObject furniture)
    {
        this.Furniture.Add(furniture);
        //GameObject.Instantiate(furniture,new Vector3(1,1,0),Quaternion.identity);
    }

    public void AddRepresentation(GameObject representation)
    {
        this.Representations.Add(representation);
    }

    public bool HasFurniture()
    {
        return Furniture.Count > 0;
    }

    public void SaveRoom()
    {
        SaveRoom saveRoom = new SaveRoom();
        saveRoom.roomID = ID;
        saveRoom.furniture = Furniture;
        saveRoom.representations = Representations;
        SaveData = saveRoom;
    }

}

