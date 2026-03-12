using UnityEngine;
using System.IO;
using Cinemachine;
using System.Collections.Generic;
using System.Linq;

public class SaveController : MonoBehaviour
{
    private string savePath;

    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;
    private CinemachineConfiner2D confiner;
    private Transform player;

    private void Awake()
    {
        savePath = Application.persistentDataPath + "/save.json";

        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();
        confiner = FindObjectOfType<CinemachineConfiner2D>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Start()
    {
        // מוצא את כל התיבות בסצנה
        chests = FindObjectsOfType<Chest>();
        LoadGame();
    }

    public void SaveGame()
    {
        Debug.Log("SAVE CALLED");

        // יצירת אובייקט מה-SaveData הקיים שלך
        SaveData data = new SaveData();

        // שמירת מיקום
        if (player != null)
        {
            data.playerPosX = player.position.x;
            data.playerPosY = player.position.y;
            data.playerPosZ = player.position.z;
        }

        // שמירת גבולות מצלמה
        if (confiner != null && confiner.m_BoundingShape2D != null)
        {
            data.mapBoundary = confiner.m_BoundingShape2D.gameObject.name;
        }

        // שמירת אינוונטורי
        if (inventoryController != null)
            data.inventorySaveData = inventoryController.GetInventoryItems();

        if (hotbarController != null)
            data.hotbarSaveData = hotbarController.GetInventoryItems();

        // שמירת תיבות
        data.chestSaveDatas = GetChestsState();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved Successfully");
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestList = new List<ChestSaveData>();
        if (chests == null) return chestList;

        foreach (Chest chest in chests)
        {
            chestList.Add(new ChestSaveData
            {
                chestID = chest.ChestID,
                isOpened = chest.IsOpened
            });
        }
        return chestList;
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // טעינת מיקום
        if (player != null)
            player.position = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);

        // טעינת מצלמה
        if (!string.IsNullOrEmpty(data.mapBoundary))
        {
            GameObject boundaryObject = GameObject.Find(data.mapBoundary);
            if (boundaryObject != null && confiner != null)
            {
                confiner.m_BoundingShape2D = boundaryObject.GetComponent<PolygonCollider2D>();
                confiner.InvalidateCache();
            }
        }

        // טעינת פריטים
        if (inventoryController != null && data.inventorySaveData != null)
            inventoryController.SetInventoryItems(data.inventorySaveData);

        if (hotbarController != null && data.hotbarSaveData != null)
            hotbarController.SetHotbarItems(data.hotbarSaveData);

        // טעינת תיבות
        if (data.chestSaveDatas != null)
            LoadChestStates(data.chestSaveDatas);
    }

    private void LoadChestStates(List<ChestSaveData> chestState)
    {
        if (chests == null) return;
        foreach (Chest chest in chests)
        {
            var state = chestState.FirstOrDefault(c => c.chestID == chest.ChestID);
            if (state != null) chest.SetOpened(state.isOpened);
        }
    }
}