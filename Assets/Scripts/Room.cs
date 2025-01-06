using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Room
{
    // id is local to every user
    public int ID { get; private set; }
    // A list of the furniture prefabs that are in the room
    public List<GameObject> Furniture { get; private set; } = new List<GameObject>();
    public List<GameObject> FurnitureInstances { get; private set; } = new List<GameObject>();
    public List<GameObject> Representations { get; private set; } = new List<GameObject>();
    public List<GameObject> RepresentationInstances { get; private set; } = new List<GameObject>();

    //TODO save the transforms of the instances here
    public List<SerializedTransform> FurnitureTransforms { get; private set; } = new List<SerializedTransform>();
    public List<SerializedTransform> RepresentationTransforms { get; private set; } = new List<SerializedTransform>();

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
        FurnitureTransforms = saveRoom.furnitureTransforms;
        RepresentationTransforms = saveRoom.representationTransforms;
        // Transform newTransform = new GameObject().transform;
        // // get the transforms from the save room and apply them to the furniture and representations
        // if (Furniture != null && Furniture.Count != 0 && saveRoom.furnitureTransforms != null && saveRoom.furnitureTransforms.Count != 0)
        // {
        //     for (int i = 0; i < Furniture.Count; i++)
        //     {
        //         Debug.Log("LENA: Pre-error_:" + saveRoom.furniture.Count + " =? " + saveRoom.furnitureTransforms.Count);
        //         newTransform.transform.position = new Vector3(saveRoom.furnitureTransforms[i]._position[0], saveRoom.furnitureTransforms[i]._position[1], saveRoom.furnitureTransforms[i]._position[2]);
        //         newTransform.transform.rotation = new Quaternion(saveRoom.furnitureTransforms[i]._rotation[1], saveRoom.furnitureTransforms[i]._rotation[2], saveRoom.furnitureTransforms[i]._rotation[3], saveRoom.furnitureTransforms[i]._rotation[0]);
        //         newTransform.transform.localScale = new Vector3(saveRoom.furnitureTransforms[i]._scale[0], saveRoom.furnitureTransforms[i]._scale[1], saveRoom.furnitureTransforms[i]._scale[2]);
        //         FurnitureTransforms.Add(newTransform);
        //         Debug.Log("LENA: The Transform of the furniture in the room has been set" + FurnitureTransforms[i].position);
        //     }
        //     Debug.Log("LENA: The Transform of the furniture 1 are" + FurnitureTransforms[0].position);
        //     Debug.Log("LENA: The Transform of the furniture 2 are" + FurnitureTransforms[1].position);
        //
        // }
        // if (Representations != null && Representations.Count != 0 && saveRoom.representationTransforms != null && saveRoom.representationTransforms.Count != 0)
        // {
        //     for (int i = 0; i < Representations.Count; i++)
        //     {
        //         newTransform = Representations[i].transform;
        //         newTransform.transform.position = new Vector3(saveRoom.representationTransforms[i]._position[0], saveRoom.representationTransforms[i]._position[1], saveRoom.representationTransforms[i]._position[2]);
        //         newTransform.transform.rotation = new Quaternion(saveRoom.representationTransforms[i]._rotation[1], saveRoom.representationTransforms[i]._rotation[2], saveRoom.representationTransforms[i]._rotation[3], saveRoom.representationTransforms[i]._rotation[0]);
        //         newTransform.transform.localScale = new Vector3(saveRoom.representationTransforms[i]._scale[0], saveRoom.representationTransforms[i]._scale[1], saveRoom.representationTransforms[i]._scale[2]);
        //     }
        // }

        Debug.Log("Room created from SaveRoom with ID: " + ID + " Furniture count: " + Furniture.Count);
    }

    public void LoadTransforms()
    {
        if (FurnitureInstances.Count != 0)
        {
            if (Furniture != null && Furniture.Count != 0 && FurnitureTransforms != null && FurnitureTransforms.Count != 0)
            {
                for (int i = 0; i < Furniture.Count; i++)
                {
                    Debug.Log("LENA: Pre-error_:" + Furniture.Count + " =? " + FurnitureTransforms.Count);
                    FurnitureInstances[i].transform.position = new Vector3(FurnitureTransforms[i]._position[0], FurnitureTransforms[i]._position[1], FurnitureTransforms[i]._position[2]);
                    FurnitureInstances[i].transform.rotation = new Quaternion(FurnitureTransforms[i]._rotation[1], FurnitureTransforms[i]._rotation[2], FurnitureTransforms[i]._rotation[3], FurnitureTransforms[i]._rotation[0]);
                    FurnitureInstances[i].transform.localScale = new Vector3(FurnitureTransforms[i]._scale[0], FurnitureTransforms[i]._scale[1], FurnitureTransforms[i]._scale[2]);
                    //FurnitureTransforms.Add(newTransform);
                }
            }
        }
        if (RepresentationInstances.Count != 0)
        {
            if (Representations != null && Representations.Count != 0 && RepresentationTransforms != null && RepresentationTransforms.Count != 0)
            {
                for (int i = 0; i < Representations.Count; i++)
                {
                    RepresentationInstances[i].transform.position = new Vector3(RepresentationTransforms[i]._position[0], RepresentationTransforms[i]._position[1], RepresentationTransforms[i]._position[2]);
                    RepresentationInstances[i].transform.rotation = new Quaternion(RepresentationTransforms[i]._rotation[1], RepresentationTransforms[i]._rotation[2], RepresentationTransforms[i]._rotation[3], RepresentationTransforms[i]._rotation[0]);
                    RepresentationInstances[i].transform.localScale = new Vector3(RepresentationTransforms[i]._scale[0], RepresentationTransforms[i]._scale[1], RepresentationTransforms[i]._scale[2]);
                }
            }
        }
    }
    public void AddFurniture(GameObject furniture, GameObject instance)
    {
        this.Furniture.Add(furniture);
        this.FurnitureInstances.Add(instance);
        //this.FurnitureTransforms.Add(instance.transform);
        //GameObject.Instantiate(furniture,new Vector3(1,1,0),Quaternion.identity);
    }

    public void AddFurnitureInstance(GameObject instance)
    {
        this.FurnitureInstances.Add(instance);
    }

    // public void AddFurnitureTransform(Transform transform)
    // {
    //     //this.FurnitureTransforms.Add(transform);
    // }

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
        saveRoom.furnitureTransforms = FurnitureTransforms;
        saveRoom.representationTransforms = RepresentationTransforms;

        Debug.Log("Saving the Transform of the furniture in the room");

        SaveData = saveRoom;
    }

    //TODO call when transforms change
    public void UpdateTransforms()
    {
        if (FurnitureInstances.Count != 0)
        {
            FurnitureTransforms.Clear();
            for (int i = 0; i < FurnitureInstances.Count; i++)
            {
                SerializedTransform newTransform = new SerializedTransform(FurnitureInstances[i].transform);
                FurnitureTransforms.Add(newTransform);
            }
        }
    }
}

