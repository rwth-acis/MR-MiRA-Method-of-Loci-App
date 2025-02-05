using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using i5.Toolkit.Core.ExperienceAPI;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using Guid = System.Guid;

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
    [Tooltip("A list of the UUIDs of spatial anchors of the furniture instances in the room")]
    public List<Guid> FurnitureAnchors { get; private set; } = new List<Guid>();
    [Tooltip("A list of the UUIDs of spatial anchors of the representation instances in the room")]
    public List<Guid> RepresentationAnchors { get; private set; } = new List<Guid>();
    [Tooltip("A list of the loci in the room, represented by halo objects")]
    public List<GameObject> Loci { get; private set; } = new List<GameObject>();

    public Color WallColour { get; private set; } = Color.white;
    public SaveRoom SaveData { get; private set; }

    [Tooltip("The index of the locus to be replaced next")]
    public int ReplacingLocusIndex { get; private set; } = 0;

    private List<OVRSpatialAnchor.UnboundAnchor> _unboundFurnitureAnchors = new();
    private List<OVRSpatialAnchor.UnboundAnchor> _unboundRepresentationAnchors = new();

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
        Debug.Log("ANCHOR: After loading from save: Furniture count: " + Furniture.Count + " FurnitureTransforms count: " + FurnitureTransforms.Count + " FurnitureAnchors count: " + FurnitureAnchors.Count);
        RepresentationTransforms = saveRoom.representationTransforms;
        WallColour = saveRoom.wallColour;
        FurnitureAnchors = saveRoom.furnitureAnchors.ConvertAll(Guid.Parse);
        RepresentationAnchors = saveRoom.representationAnchors.ConvertAll(Guid.Parse);
    }

    /// <summary>
    /// Load and localise the unbound anchors of the furniture and representation instances in the room.
    /// After initialising their respective GameObjects, we can then load them to the correct position immediately.
    /// </summary>
    /// <param name="furnitureAnchors">List of UUIDs from SaveRoom, for furniture anchors.</param>
    /// <param name="representationAnchors">List of UUIDs from SaveRoom, for representation anchors.</param>
    public async Task LoadUnboundAnchors(List<Guid> furnitureAnchors, List<Guid> representationAnchors)
    {
        Debug.Log("2");
        foreach (var guid in furnitureAnchors)
        {
            Debug.Log("ANCHOR: loading unbound furn. anchor " + guid);
        }

        foreach (var guid in representationAnchors)
        {
            Debug.Log("ANCHOR: loading unbound repr. anchor " + guid);
        }

        Debug.Log("ANCHOR: loading unbound anchors, furniture count: " + Furniture.Count + " furniture anchors: " + furnitureAnchors.Count);

        if (furnitureAnchors.Count == 0 && representationAnchors.Count == 0)
        {
            return;
        }

        if (furnitureAnchors.Count != 0)
        {
            _unboundFurnitureAnchors.Clear();
            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(furnitureAnchors, _unboundFurnitureAnchors);
            if (result.Success)
            {
                Debug.Log("3");
                foreach (var unboundAnchor in result.Value)
                {
                    unboundAnchor.LocalizeAsync().ContinueWith((success, anchor) =>
                    {
                        if (!success)
                        {
                            Debug.LogError($"Localization failed for anchor {unboundAnchor.Uuid}");
                        }
                    }, unboundAnchor);

                }
            }
            else
            {
                Debug.LogError("Failed to load unbound furniture anchors");
                Debug.LogError($"Load failed with error {result.Status}.");
            }
        }
        if (representationAnchors.Count != 0)
        {
            _unboundRepresentationAnchors.Clear();
            var result2 = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(representationAnchors, _unboundRepresentationAnchors);
            if (result2.Success)
            {
                Debug.Log("4");
                foreach (var unboundAnchor in result2.Value)
                {
                    unboundAnchor.LocalizeAsync().ContinueWith((success, anchor) =>
                    {
                        if (!success)
                        {
                            Debug.LogError($"Localization failed for anchor {unboundAnchor.Uuid}");
                        }
                    }, unboundAnchor);
                }

                bool done = false;
                while (!done)
                {
                    done = true;
                    foreach (var anchor in result2.Value)
                    {
                        if (!anchor.Localized)
                        {
                            await anchor.LocalizeAsync();
                        }

                        if (!anchor.Localized)
                        {
                            done = false;
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Failed to load unbound representation anchors");
                Debug.LogError($"Load failed with error {result2.Status}.");
            }
        }
        Debug.Log("ANCHOR: Unbound anchor count: " + _unboundFurnitureAnchors.Count + " " + _unboundRepresentationAnchors.Count + " furniture count: " + Furniture.Count + " representation count: " + Representations.Count);
    }

    /// <summary>
    /// Loads the furniture and representation instances to be bound to their spatial anchors.
    /// If that fails, loads the transforms of the furniture and representation instances in the room after those are instantiated.
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
                    RepresentationInstances[i].transform.localScale = new Vector3(RepresentationTransforms[i]._scale[0], RepresentationTransforms[i]._scale[1],RepresentationTransforms[i]._scale[2]);
                    RepresentationInstances[i].transform.rotation = new Quaternion(RepresentationTransforms[i]._rotation[1], RepresentationTransforms[i]._rotation[2], RepresentationTransforms[i]._rotation[3], RepresentationTransforms[i]._rotation[0]);
                }
            }
        }
    }

    public void LoadAnchors()
    {
        List<Guid> oldFurnitureAnchors = FurnitureAnchors;
        List<Guid> oldRepresentationAnchors = RepresentationAnchors;
        if (_unboundFurnitureAnchors.Count > 0)
        {
            FurnitureAnchors.Clear();
        }

        if (_unboundRepresentationAnchors.Count > 0)
        {
            RepresentationAnchors.Clear();
        }

        Debug.Log("ANCHOR: load transforms, furniture count: " + Furniture.Count + " furniture transforms count: " + FurnitureTransforms.Count + " furniture anchors count: " + _unboundFurnitureAnchors.Count);

        if (FurnitureInstances.Count != 0)
        {
            if (Furniture != null && Furniture.Count != 0 && FurnitureTransforms != null && FurnitureTransforms.Count != 0)
            {
                for (int i = 0; i < Furniture.Count; i++)
                {
                    if (_unboundFurnitureAnchors.Count > i && _unboundFurnitureAnchors[i].Localized)
                    {
                        OVRSpatialAnchor spatialAnchor = FurnitureInstances[i].AddComponent<OVRSpatialAnchor>();
                        foreach (OVRSpatialAnchor.UnboundAnchor anchor in _unboundFurnitureAnchors)
                        {
                            bool found = false;
                            if (oldFurnitureAnchors.Count > i && anchor.Uuid == oldFurnitureAnchors[i])
                            {
                                anchor.BindTo(spatialAnchor);
                                FurnitureAnchors.Add(spatialAnchor.Uuid);
                                found = true;
                                Debug.Log("ANCHOR: furniture success!)");
                                break;
                            }
                            if (!found)
                            {
                                Debug.Log($"ANCHOR: Localisation furniture {i} failed");
                            }
                        }
                    }
                }
                _unboundFurnitureAnchors.Clear();
            }
        }
        if (RepresentationInstances.Count != 0)
        {
            if (Representations != null && Representations.Count != 0 && RepresentationTransforms != null && RepresentationTransforms.Count != 0)
            {
                for (int i = 0; i < Representations.Count; i++)
                {
                    if (_unboundRepresentationAnchors.Count > i && _unboundRepresentationAnchors[i].Localized)
                    {
                        OVRSpatialAnchor spatialAnchor = RepresentationInstances[i].AddComponent<OVRSpatialAnchor>();
                        foreach (OVRSpatialAnchor.UnboundAnchor anchor in _unboundRepresentationAnchors)
                        {
                            bool found = false;
                            if (oldRepresentationAnchors.Count > i && anchor.Uuid == oldRepresentationAnchors[i])
                            {
                                anchor.BindTo(spatialAnchor);
                                RepresentationAnchors.Add(spatialAnchor.Uuid);
                                Debug.Log("ANCHOR: representation success!)");
                                found = true;
                                break;
                            }
                            if (!found)
                            {
                                Debug.LogError($"ANCHOR: representation {i} failed to localize");
                            }
                        }
                    }
                }
                _unboundRepresentationAnchors.Clear();
            }
        }
    }

    /// <summary>
    /// Loads the transforms of the loci after images to mark them as instantiated.
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
    /// Returns whether the room has furniture or not.
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
    /// Creates a SaveRoom object from the room and stores it in the SaveData property. Preface with an UpdateTransforms call.
    /// </summary>
    public void SaveRoom()
    {
        SaveRoom saveRoom = new SaveRoom();
        saveRoom.roomID = ID;
        saveRoom.furniture = Furniture;
        saveRoom.representations = Representations;
        saveRoom.furnitureTransforms = FurnitureTransforms;
        saveRoom.representationTransforms = RepresentationTransforms;
        saveRoom.furnitureAnchors = (FurnitureAnchors.Any()) ? FurnitureAnchors.ConvertAll(x => x.ToString()) : new List<String>();
        saveRoom.representationAnchors = (RepresentationAnchors.Any()) ? RepresentationAnchors.ConvertAll(x => x.ToString()) : new List<String>();
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
    /// Creates Spatial Anchors for furniture and representations, that persist across sessions.
    /// </summary>
    public async Task UpdateAnchors()
    {
        if(Loci.Count != 0 && RepresentationInstances.Count != 0)
        {
            // Do not change the anchors when reusing the room
        }
        else
        {
            if (FurnitureInstances.Count != 0)
            {
                FurnitureAnchors.Clear();
                foreach (var instance in FurnitureInstances)
                {
                    Debug.Log("UPDATE ANCHOR: Updating " + instance.name);
                    if (!instance.TryGetComponent<OVRSpatialAnchor>(out OVRSpatialAnchor outanchor))
                    {
                        OVRSpatialAnchor newAnchor = instance.AddComponent<OVRSpatialAnchor>();
                        while (!newAnchor.Created)
                        {
                            await Task.Delay(100);
                        }
                        var result = await newAnchor.SaveAnchorAsync();
                        if (result.Success)
                        {
                            Debug.Log($"ANCHOR: {newAnchor.Uuid} saved successfully to " + instance.name);
                            FurnitureAnchors.Add(newAnchor.Uuid);
                        }
                        else
                        {
                            Debug.LogError($"ANCHOR: {newAnchor.Uuid} failed to save with error {result.Status}");
                        }
                    }
                    else
                    {
                        if (outanchor.Uuid != Guid.Empty)
                        {
                            Debug.Log($"ANCHOR: furniture anchor {outanchor.Uuid} found and reused. Belonging to " + instance.name);
                            FurnitureAnchors.Add(outanchor.Uuid);
                        }

                    }
                }
            }
            if (RepresentationInstances.Count != 0)
            {
                RepresentationAnchors.Clear();
                foreach (var instance in RepresentationInstances)
                {
                    if (!instance.TryGetComponent<OVRSpatialAnchor>(out OVRSpatialAnchor outRepanchor))
                    {
                        OVRSpatialAnchor newRepAnchor = instance.AddComponent<OVRSpatialAnchor>();
                        while (!newRepAnchor.Created)
                        {
                            await Task.Delay(100);
                        }
                        var result = await newRepAnchor.SaveAnchorAsync();
                        if (result.Success)
                        {
                            Debug.Log($"ANCHOR: {newRepAnchor.Uuid} saved successfully." + instance.name);
                            RepresentationAnchors.Add(newRepAnchor.Uuid);
                        }
                        else
                        {
                            Debug.LogError($"ANCHOR: {newRepAnchor.Uuid} failed to save with error {result.Status}");
                        }
                    }
                    else
                    {
                        if (outRepanchor.Uuid != Guid.Empty)
                        {
                            Debug.Log($"ANCHOR: representation anchor {outRepanchor.Uuid} found and reused");
                            RepresentationAnchors.Add(outRepanchor.Uuid);
                        }
                    }
                }
            }
        }
        Debug.Log("ANCHOR: After update transforms: Furniture count: " + Furniture.Count + " FurnitureTransforms count: " + FurnitureTransforms.Count + " FurnitureAnchors count: " + FurnitureAnchors.Count);
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

