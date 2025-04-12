using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Guid = System.Guid;
using Object = UnityEngine.Object;

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
    [Tooltip("The colour of the walls in the room")]
    public Color WallColour { get; private set; } = Color.white;
    [Tooltip("The save data of the room")]
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
                foreach (var unboundAnchor in result.Value)
                {
                    await unboundAnchor.LocalizeAsync();
                }
                bool done = false;
                while (!done)
                {
                    done = true;
                    foreach (var anchor in _unboundFurnitureAnchors)
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
        }

        if (representationAnchors.Count != 0)
        {
            _unboundRepresentationAnchors.Clear();
            var result = await OVRSpatialAnchor.LoadUnboundAnchorsAsync(representationAnchors, _unboundRepresentationAnchors);
            if (result.Success)
            {
                foreach (var unboundAnchor in result.Value)
                {
                    var result2 = await unboundAnchor.LocalizeAsync();
                }
                bool done = false;
                while (!done)
                {
                    done = true;
                    foreach (var anchor in _unboundRepresentationAnchors)
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
        }
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

    /// <summary>
    /// Binds the unbound anchors to the furniture and representation instances in the room, which positions them correctly, regardless of the user's initial position.
    /// </summary>
    public async void LoadAnchors()
    {
        // We rebuild these lists to cross-reference the UUIDs, as the order of the unbound anchors is not guaranteed
        // Simply assigning doesn't work, as we need to copy by value, not by reference
        List<Guid> oldFurnitureAnchors = new List<Guid>();
        foreach (var guid in FurnitureAnchors)
        {
            oldFurnitureAnchors.Add(guid);
        }
        List<Guid> oldRepresentationAnchors = new List<Guid>();
        foreach (var guid in RepresentationAnchors)
        {
            oldRepresentationAnchors.Add(guid);
        }

        if (_unboundFurnitureAnchors.Count > 0)
        {
            FurnitureAnchors.Clear();
        }
        if (_unboundRepresentationAnchors.Count > 0)
        {
            RepresentationAnchors.Clear();
        }

        // save count, as list elements are removed while binding
        int unboundFurnitureCount = _unboundFurnitureAnchors.Count;
        if (FurnitureInstances.Count != 0 && _unboundFurnitureAnchors.Count != 0)
        {
            if (Furniture != null && Furniture.Count != 0 && FurnitureTransforms != null && FurnitureTransforms.Count != 0)
            {
                for (int i = 0; i < Furniture.Count; i++)
                {
                    if (unboundFurnitureCount > i) // && _unboundFurnitureAnchors[i].Localized
                    {
                        await DeleteAnchor(FurnitureInstances[i], false); // This acts as a check for an existing anchor
                        OVRSpatialAnchor spatialAnchor = FurnitureInstances[i].AddComponent<OVRSpatialAnchor>();
                        foreach (OVRSpatialAnchor.UnboundAnchor anchor in _unboundFurnitureAnchors)
                        {
                            if (oldFurnitureAnchors.Count > i && anchor.Uuid == oldFurnitureAnchors[i])
                            {
                                anchor.BindTo(spatialAnchor);
                                FurnitureAnchors.Add(spatialAnchor.Uuid);
                                // We destroy the anchor after binding, to make the object movable, grabbable and allow for teleportation
                                await DeleteAnchor(spatialAnchor.GameObject(), false);
                                break;
                            }
                        }
                    }
                }
                _unboundFurnitureAnchors.Clear();
            }
        }
        int unboundRepresentationCount = _unboundRepresentationAnchors.Count;

        if (RepresentationInstances.Count != 0 && _unboundRepresentationAnchors.Count != 0)
        {
            if (Representations != null && Representations.Count != 0 && RepresentationTransforms != null && RepresentationTransforms.Count != 0)
            {
                for (int i = 0; i < Representations.Count; i++)
                {
                    if (unboundRepresentationCount > i) // && _unboundRepresentationAnchors[i].Localized
                    {
                        await DeleteAnchor(RepresentationInstances[i], false); // This acts as a check for an existing anchor
                        OVRSpatialAnchor spatialAnchor = RepresentationInstances[i].AddComponent<OVRSpatialAnchor>();
                        foreach (OVRSpatialAnchor.UnboundAnchor anchor in _unboundRepresentationAnchors)
                        {
                            if (oldRepresentationAnchors.Count > i && anchor.Uuid == oldRepresentationAnchors[i])
                            {
                                anchor.BindTo(spatialAnchor);
                                RepresentationAnchors.Add(spatialAnchor.Uuid);
                                // We destroy the anchor after binding, to make the object movable, grabbable and allow for teleportation
                                await DeleteAnchor(spatialAnchor.GameObject(), false);
                                break;
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
    public async Task LoadTransformsLoci()
    {
        if (Loci.Count == 0) return;
        if (Representations == null || Representations.Count == 0 || RepresentationTransforms == null ||
            RepresentationTransforms.Count == 0) return;
        for (int i = 0; i < Loci.Count; i++)
        {
            Loci[i].transform.position = new Vector3(RepresentationTransforms[i]._position[0], RepresentationTransforms[i]._position[1], RepresentationTransforms[i]._position[2]);
            Loci[i].transform.rotation = new Quaternion(RepresentationTransforms[i]._rotation[1], RepresentationTransforms[i]._rotation[2], RepresentationTransforms[i]._rotation[3], RepresentationTransforms[i]._rotation[0]);
            Loci[i].transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            if (_unboundRepresentationAnchors.Count > i)
            {
                OVRSpatialAnchor spatialAnchor = Loci[i].AddComponent<OVRSpatialAnchor>();
                _unboundRepresentationAnchors[i].BindTo(spatialAnchor);
                await DeleteAnchor(spatialAnchor.GameObject(), false);
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
    /// Creates a SaveRoom object from the room and stores it in the SaveData property. Preface with an UpdateTransforms and UpdateAnchor call.
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
                foreach (GameObject instance in FurnitureInstances)
                {
                    SerializedTransform newTransform = new SerializedTransform(instance.transform);
                    FurnitureTransforms.Add(newTransform);
                }
            }
            if (RepresentationInstances.Count != 0)
            {
                RepresentationTransforms.Clear();
                foreach (GameObject instance in RepresentationInstances)
                {
                    SerializedTransform newRepTransform = new SerializedTransform(instance.transform);
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
                await OVRSpatialAnchor.EraseAnchorsAsync(uuids: FurnitureAnchors, anchors: null);
                FurnitureAnchors.Clear();
                foreach (var instance in FurnitureInstances)
                {
                    // We delete and recreate existing anchors
                    await DeleteAnchor(instance, true);
                    OVRSpatialAnchor newAnchor = instance.AddComponent<OVRSpatialAnchor>();
                    while (!newAnchor.Created)
                    {
                        await Task.Delay(100);
                    }
                    var result = await newAnchor.SaveAnchorAsync();
                    if (result.Success)
                    {
                        FurnitureAnchors.Add(newAnchor.Uuid);
                    }
                    await DeleteAnchor(instance, false);
                }
            }
            if (RepresentationInstances.Count != 0)
            {
                await OVRSpatialAnchor.EraseAnchorsAsync(uuids: RepresentationAnchors, anchors: null);
                RepresentationAnchors.Clear();
                foreach (var instance in RepresentationInstances)
                {
                    await DeleteAnchor(instance, true);
                    OVRSpatialAnchor newRepAnchor = instance.AddComponent<OVRSpatialAnchor>();
                    while (!newRepAnchor.Created)
                    {
                        await Task.Delay(100);
                    }
                    var result = await newRepAnchor.SaveAnchorAsync();
                    if (result.Success)
                    {
                        RepresentationAnchors.Add(newRepAnchor.Uuid);
                    }
                    await DeleteAnchor(instance, false);
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
    public void ReplaceRepresentation(GameObject newRepresentation, GameObject newRepresentationInstance)
    {
        Representations[ReplacingLocusIndex] = newRepresentation;
        RepresentationInstances.Add(newRepresentationInstance);
        ReplacingLocusIndex++;
    }

    /// <summary>
    /// Delete an anchor from a GameObject, if present, and optionally erase it from memory. Asynchronous.
    /// </summary>
    /// <param name="anchorObject">The GameObject holding an OVRSpatialAnchor component, that should be deleted. If this object doesn't hold an anchor, nothing happens.</param>
    /// <param name="erase">True, if the anchor should also be erased, i.e. its reference deleted from memory. False, if only the component should be destroyed, to free up resources, while still being able to access the anchor afterward.</param>
    private static async Task DeleteAnchor(GameObject anchorObject, bool erase = false)
    {
        if (anchorObject.GetComponents<OVRSpatialAnchor>().Length != 0)
        {
            OVRSpatialAnchor anchorComponent = anchorObject.GetComponent<OVRSpatialAnchor>();
            if (erase)
            {
                await anchorComponent.EraseAnchorAsync();
            }
            Object.Destroy(anchorComponent);
            // Destroying only occurs in the next update cycle, so we wait for that to finish
            await Awaitable.NextFrameAsync();
        }
    }

    /// <summary>
    /// Deletes an anchor from the list of anchors, and erases it from memory. Asynchronous.
    /// </summary>
    /// <param name="index">The index in the applicable list</param>
    /// <param name="isFurniture">If true deletes from furniture list, else from representation list</param>
    public async Task DeleteAnchorFromList(int index, bool isFurniture)
    {
        Guid anchor = isFurniture ? FurnitureAnchors[index] : RepresentationAnchors[index];
        List<Guid> anchorList = new List<Guid>();
        anchorList.Add(anchor);
        await OVRSpatialAnchor.EraseAnchorsAsync(null, anchorList);
        if (isFurniture)
        {
            FurnitureAnchors.RemoveAt(index);
        }
        else
        {
            RepresentationAnchors.RemoveAt(index);
        }
    }

}

