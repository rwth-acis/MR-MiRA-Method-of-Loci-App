using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Room
{
    private static int ID { get; set; }
    private List<GameObject> furniture = new List<GameObject>();
    private List<GameObject> representations = new List<GameObject>();
    public Room(int id)
    {
        ID = id;
    }

    public void AddFurniture(GameObject furniture)
    {
        this.furniture.Add(furniture);
        //GameObject.Instantiate(furniture,new Vector3(1,1,0),Quaternion.identity);
    }

    public void AddRepresentation(GameObject representation)
    {
        this.representations.Add(representation);
    }
}

