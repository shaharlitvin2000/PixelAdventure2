using UnityEngine;
using System.IO;
using Cinemachine;
using System.Collections.Generic;

public class SaveController : MonoBehaviour
{
    private string savePath;

    private InventoryController inventoryController;
    private HotbarController hotbarController;
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
        LoadGame();
    }

    // =================================
    // SAVE
    // =================================
    public void SaveGame()
    {
        Debug.Log("SAVE CALLED");

        SaveData saveData = new SaveData();

        // Player Position
        if (player != null)
        {
            saveData.playerPosX = player.position.x;
            saveData.playerPosY = player.position.y;
            saveData.playerPosZ = player.position.z;
        }

        // Camera Boundary
        if (confiner != null && confiner.m_BoundingShape2D != null)
        {
            saveData.mapBoundary = confiner.m_BoundingShape2D.gameObject.name;
        }

        // Inventory Save
        if (inventoryController != null)
        {
            saveData.inventorySaveData = inventoryController.GetInventoryItems();
        }

        // Hotbar Save
        if (hotbarController != null)
        {
            saveData.hotbarSaveData = hotbarController.GetInventoryItems();
        }

        string json = JsonUtility.ToJson(saveData, true);

        try
        {
            File.WriteAllText(savePath, json);
            Debug.Log("Game Saved Successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Save failed: " + e.Message);
        }
    }

    // =================================
    // LOAD
    // =================================
    public void LoadGame()
    {
        Debug.Log("LOAD FUNCTION CALLED");

        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found!");
            return;
        }

        string json = File.ReadAllText(savePath);

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("Save file corrupted!");
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        // Player Position
        if (player != null)
        {
            player.position = new Vector3(
                saveData.playerPosX,
                saveData.playerPosY,
                saveData.playerPosZ
            );
        }

        // Load Camera Boundary
        if (!string.IsNullOrEmpty(saveData.mapBoundary))
        {
            GameObject boundaryObject = GameObject.Find(saveData.mapBoundary);

            if (boundaryObject != null)
            {
                PolygonCollider2D collider = boundaryObject.GetComponent<PolygonCollider2D>();

                if (confiner != null && collider != null)
                {
                    confiner.m_BoundingShape2D = collider;
                    confiner.InvalidateCache();
                }
            }
        }

        // Load Inventory
        if (inventoryController != null && saveData.inventorySaveData != null)
        {
            inventoryController.SetInventoryItems(saveData.inventorySaveData);
        }

        // Load Hotbar
        if (hotbarController != null && saveData.hotbarSaveData != null)
        {
            hotbarController.SetHotbarItems(saveData.hotbarSaveData);
        }

        Debug.Log("Game Loaded Successfully");
    }
}