using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class SaveRoom
{
    public int roomID;
    public List<GameObject> furniture = new List<GameObject>();
    public List<GameObject> representations = new List<GameObject>();
    public List<SerializedTransform> furnitureTransforms = new List<SerializedTransform>();
    public List<SerializedTransform> representationTransforms = new List<SerializedTransform>();

}
