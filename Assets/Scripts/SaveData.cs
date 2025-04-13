using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class SaveData
{
    [Tooltip("The list of rooms saved in the user's save file")]
    public List<SaveRoom> saveRooms = new List<SaveRoom>();
}
