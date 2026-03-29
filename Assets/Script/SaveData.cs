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
    public List<InventorySaveData> hotbarSaveData;
    public List<ChestSaveData> chestSaveDatas;
    public List<QuestSaveData> questProgressData; // FIX: קלאס נפרד לשמירה
    public List<string> handinQuestIDs;

}

[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool isOpened;
}

// FIX: קלאס נפרד לשמירת קווסט — JsonUtility לא יכול לשמור ScriptableObject
[System.Serializable]
public class QuestSaveData
{
    public string questID;
    public List<ObjectiveSaveData> objectives;
}

[System.Serializable]
public class ObjectiveSaveData
{
    public string objectiveID;
    public int currentAmount;
}