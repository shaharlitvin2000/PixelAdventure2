using Cinemachine;
using System.IO;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(saveLocation);

        //Define save location
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

        LoadGame();
    }

    public void SaveGame()
    {
        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            SaveGame();
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = saveData.playerPosition;

        CinemachineConfiner confiner = FindObjectOfType<CinemachineConfiner>();
        GameObject boundary = GameObject.Find(saveData.mapBoundary);

        if (confiner != null && boundary != null)
        {
            PolygonCollider2D collider = boundary.GetComponent<PolygonCollider2D>();
            if (collider != null)
                confiner.m_BoundingShape2D = collider;
        }
    }
}