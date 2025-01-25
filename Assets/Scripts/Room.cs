using System.Collections.Generic;
using UnityEngine;

public class Room
{
    [Tooltip("Id of the room local to every user")]
    public int ID { get; private set; }
    [Tooltip("A list of the furniture prefabs that are in the room")]
    public List<GameObject> Furniture { get; private set; } = new List<GameObject>();
    [Tooltip("A list of the furniture instances that are in the room after instantiation")]
    public List<GameObject> FurnitureInstances { get; private set; } = new List<GameObject>();
    [Tooltip("A list of the representation prefabs that are in the room")]
    public List<GameObject> Representations { get; private set; } = new List<GameObject>();
    [Tooltip("A list of the representation instances that are in the room after instantiation")]
    public List<GameObject> RepresentationInstances { get; private set; } = new List<GameObject>();
    [Tooltip("A list of the serialized transforms of the furniture instances in the room")]
    public List<SerializedTransform> FurnitureTransforms { get; private set; } = new List<SerializedTransform>();
    [Tooltip("A list of the serialized transforms of the representation instances in the room")]
    public List<SerializedTransform> RepresentationTransforms { get; private set; } = new List<SerializedTransform>();
    [Tooltip("A list of the loci in the room, represented by halo objects")]
    public List<GameObject> Loci { get; private set; } = new List<GameObject>();

    public Color WallColour { get; private set; } = Color.white;
    public SaveRoom SaveData { get; private set; }

    [Tooltip("The index of the locus to be replaced next")]
    public int ReplacingLocusIndex { get; private set; } = 0;

    /// <summary>
    /// Create a new room with the given id
    /// </summary>
    /// <param name="id">ID for the new room</param>
    public Room(int id)
    {
        ID = id;
    }

    /// <summary>
    /// Create a new room from a SaveRoom object from permanent storage
    /// </summary>
    /// <param name="saveRoom">The SaveRoom object that has been loaded from a save file</param>
    public Room(SaveRoom saveRoom)
    {
        SaveData = saveRoom;
        ID = saveRoom.roomID;
        Furniture = saveRoom.furniture;
        Representations = saveRoom.representations;
        FurnitureTransforms = saveRoom.furnitureTransforms;
        RepresentationTransforms = saveRoom.representationTransforms;
        WallColour = saveRoom.wallColour;
    }

    /// <summary>
    /// Loads the transforms of the furniture and representation instances in the room after those are instantiated.
    /// This cannot be called before the instances are created as Transform is a component of GameObject.
    /// </summary>
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

    /// <summary>
    /// Loads the transforms of the loci after images to mark them are instantiated.
    /// This cannot be called before the instances are created as Transform is a component of GameObject.
    /// </summary>
    public void LoadTransformsLoci()
    {
        if (Loci.Count != 0)
        {
            if (Representations != null && Representations.Count != 0 && RepresentationTransforms != null && RepresentationTransforms.Count != 0)
            {
                for (int i = 0; i < Loci.Count; i++)
                {
                    Loci[i].transform.position = new Vector3(RepresentationTransforms[i]._position[0], RepresentationTransforms[i]._position[1], RepresentationTransforms[i]._position[2]);
                    Loci[i].transform.rotation = new Quaternion(RepresentationTransforms[i]._rotation[1], RepresentationTransforms[i]._rotation[2], RepresentationTransforms[i]._rotation[3], RepresentationTransforms[i]._rotation[0]);
                    Loci[i].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }
        }
    }

    /// <summary>
    /// Add a furniture prefab to the list of furniture in the room.
    /// And add the instance of the furniture to the list of furniture instances.
    /// </summary>
    /// <param name="prefab">The prefab of the added furniture</param>
    /// <param name="instance">The instance of the added furniture</param>
    public void AddFurniture(GameObject prefab, GameObject instance)
    {
        this.Furniture.Add(prefab);
        this.FurnitureInstances.Add(instance);
    }

    /// <summary>
    /// Add a furniture instance to the list of furniture in the room.
    /// Only after the furniture has been instantiated.
    /// </summary>
    /// <param name="instance">The furniture instance</param>
    public void AddFurnitureInstance(GameObject instance)
    {
        this.FurnitureInstances.Add(instance);
    }

    /// <summary>
    /// Add a locus halo instance to the list of Loci
    /// </summary>
    /// <param name="locus"></param>
    public void AddLocusInstance(GameObject locus)
    {
        this.Loci.Add(locus);
    }

    /// <summary>
    /// Add a representation prefab to the list of representations in the room.
    /// </summary>
    /// <param name="representation">The prefab to add to the representations list</param>
    public void AddRepresentation(GameObject representation, GameObject instance)
    {
        this.Representations.Add(representation);
        this.RepresentationInstances.Add(instance);
    }

    /// <summary>
    /// Add a representation instance to the list of representations in the room.
    /// Only after the representation has been instantiated.
    /// </summary>
    /// <param name="representationInstance"></param>
    public void AddRepresentationInstance(GameObject representationInstance)
    {
        this.RepresentationInstances.Add(representationInstance);
    }

    /// <summary>
    /// Returs whether the room has furniture or not.
    /// </summary>
    /// <returns>True if the room has furniture. False if not.</returns>
    public bool HasFurniture()
    {
        return Furniture.Count > 0;
    }
    public bool HasRepresentations()
    {
        return Representations.Count > 0;
    }

    /// <summary>
    /// Creates a SaveRoom object from the room and stores it in the SaveData property.
    /// </summary>
    public void SaveRoom()
    {
        SaveRoom saveRoom = new SaveRoom();
        saveRoom.roomID = ID;
        saveRoom.furniture = Furniture;
        saveRoom.representations = Representations;
        UpdateTransforms();
        saveRoom.furnitureTransforms = FurnitureTransforms;
        saveRoom.representationTransforms = RepresentationTransforms;
        saveRoom.wallColour = WallColour;
        SaveData = saveRoom;
    }

    /// <summary>
    /// Updates the list of furniture and representation transforms with the current transforms of the instances.
    /// </summary>
    public void UpdateTransforms()
    {
        if(Loci.Count != 0 && RepresentationInstances.Count != 0)
        {
            // Do not change the transforms when reusing the room
        }
        else
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
            if (RepresentationInstances.Count != 0)
            {
                RepresentationTransforms.Clear();
                for (int i = 0; i < RepresentationInstances.Count; i++)
                {
                    SerializedTransform newRepTransform = new SerializedTransform(RepresentationInstances[i].transform);
                    RepresentationTransforms.Add(newRepTransform);
                }
            }
        }
    }

    /// <summary>
    /// Changes the colour of the walls in the room.
    /// </summary>
    /// <param name="newColour">The new colour of the walls</param>
    public void ChangeWallColour(Color newColour)
    {
        WallColour = newColour;
    }

    /// <summary>
    /// Replaces the representation at the given index with the new representation.
    /// </summary>
    /// <param name="index">The index in the Representation List</param>
    /// <param name="newRepresentation">The prefab of the new Representation</param>
    /// <param name="newRepresentationInstance">The instantiated GameObject of the new Representation</param>
    public void ReplaceRepresentation(int index, GameObject newRepresentation, GameObject newRepresentationInstance)
    {
        Representations[index] = newRepresentation;
        RepresentationInstances.Add(newRepresentationInstance);
        ReplacingLocusIndex = index + 1;
    }

}

