using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveRoom
{
    [Tooltip("Id of the room local to every user")]
    public int roomID;
    [Tooltip("A list of the furniture prefabs that are in the room")]
    public List<GameObject> furniture = new List<GameObject>();
    [Tooltip("A list of the representation prefabs that are in the room")]
    public List<GameObject> representations = new List<GameObject>();
    [Tooltip("A list of UUIDs of Anchors, bound to the furniture prefabs")]
    public List<String> furnitureAnchors = new List<String>();
    [Tooltip("A list of the serialized transforms to instantiate the furniture instances")]
    public List<SerializedTransform> furnitureTransforms = new List<SerializedTransform>();
    [Tooltip("A list of UUIDs of Anchors, bound to the representation prefabs")]
    public List<String> representationAnchors = new List<String>();
    [Tooltip("A list of the serialized transforms to instantiate the representation instances")]
    public List<SerializedTransform> representationTransforms = new List<SerializedTransform>();
    [Tooltip("The colour of the walls in the room")]
    public Color wallColour;
}
