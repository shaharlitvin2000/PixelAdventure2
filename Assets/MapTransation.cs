using Cinemachine;
using UnityEngine;

public class MapTransation : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundery;
    CinemachineConfiner2D confiner;
    [SerializeField] Direction direction;
    enum Direction { UP, Down, Left, Right}
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.m_BoundingShape2D = mapBoundery;
            UpdatePlayerPosition(collision.gameObject);
        }
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector3 newPos = player.transform.position;

        switch (direction)
        { 
        case Direction.UP:
                newPos.y += 2;
                break;


            case Direction.Down:
                newPos.y -= 2;
                break;

            case Direction.Right:
                newPos.x -= 2;
                break;

            case Direction.Left:
                newPos.x += 2;
                break;
        }

        player.transform.position = newPos;
    }

}

