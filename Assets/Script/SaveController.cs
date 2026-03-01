using UnityEngine;
using System.IO;
using Cinemachine;
using System.Collections.Generic;

public class SaveController : MonoBehaviour
{
    private string savePath;
    private InventoryController inventoryController;

    private void Awake()
    {

        savePath = Application.persistentDataPath + "/save.json";
    }

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
        LoadGame();
    }

    // =================================
    // SAVE
    // =================================
    public void SaveGame()
    {
        Debug.Log("SAVE CALLED");

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        SaveData saveData = new SaveData();

        if (player != null)
        {
            saveData.playerPosX = player.transform.position.x;
            saveData.playerPosY = player.transform.position.y;
            saveData.playerPosZ = player.transform.position.z;
        }

        CinemachineConfiner2D confiner = FindObjectOfType<CinemachineConfiner2D>();

        if (confiner != null && confiner.m_BoundingShape2D != null)
        {
            saveData.mapBoundary = confiner.m_BoundingShape2D.gameObject.name;
        }

        // 🔥 שמירת אינבנטורי
        if (inventoryController != null)
        {
            saveData.inventorySaveData = inventoryController.GetInventoryItems();
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Game Saved Successfully");
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
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            player.transform.position = new Vector3(
                saveData.playerPosX,
                saveData.playerPosY,
                saveData.playerPosZ
            );
        }

        if (!string.IsNullOrEmpty(saveData.mapBoundary))
        {
            GameObject boundaryObject = GameObject.Find(saveData.mapBoundary);

            if (boundaryObject != null)
            {
                PolygonCollider2D collider = boundaryObject.GetComponent<PolygonCollider2D>();
                CinemachineConfiner2D confiner = FindObjectOfType<CinemachineConfiner2D>();

                if (confiner != null && collider != null)
                {
                    confiner.m_BoundingShape2D = collider;
                    confiner.InvalidateCache();
                }
            }
        }

        // 🔥 טעינת אינבנטורי
        if (inventoryController != null && saveData.inventorySaveData != null)
        {
            inventoryController.SetInventoryItems(saveData.inventorySaveData);
        }

        Debug.Log("Game Loaded Successfully");
    }
}