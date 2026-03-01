using UnityEngine;
using System;
using System.Collections.Generic;


[System.Serializable]
public class SaveData
{
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public string mapBoundary;

    public List<InventorySaveData> inventorySaveData;
}