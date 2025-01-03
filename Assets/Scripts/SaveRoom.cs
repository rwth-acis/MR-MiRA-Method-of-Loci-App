using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class SaveRoom
{
    public int roomID;
    public List<GameObject> furniture;
    public List<GameObject> representations;

}
