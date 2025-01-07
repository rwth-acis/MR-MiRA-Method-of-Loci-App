using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    [Tooltip("The list of rooms saved in the user's save file")]
    public List<SaveRoom> SaveRooms = new List<SaveRoom>();
}
