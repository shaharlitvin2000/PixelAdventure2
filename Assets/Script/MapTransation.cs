using UnityEngine;
using Cinemachine;

public class MapTransation : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundery;

    private CinemachineConfiner2D confiner;

    public enum Direction { Up, Down, Right, Left }
    [SerializeField] private Direction direction;

    [SerializeField] private float offsetAmount = 2f;

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();

        if (confiner == null)
            Debug.LogError("CinemachineConfiner2D not found in scene!");

        if (mapBoundery == null)
            Debug.LogError("Map Boundary (PolygonCollider2D) not assigned!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (confiner != null && mapBoundery != null)
        {
            confiner.m_BoundingShape2D = mapBoundery;
            confiner.InvalidateCache();
        }

        UpdatePlayerPosition(collision.gameObject);
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += offsetAmount;
                break;

            case Direction.Down:
                newPos.y -= offsetAmount;
                break;

            case Direction.Right:
                newPos.x += offsetAmount;
                break;

            case Direction.Left:
                newPos.x -= offsetAmount;
                break;
        }

        player.transform.position = newPos;
    }
}