using System.Collections;
using UnityEngine;
using Cinemachine;

public class MapTransation : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundery;
    private CinemachineConfiner2D confiner;

    public enum Direction { Up, Down, Right, Left, Teleport }
    [SerializeField] private Direction direction;
    [SerializeField] private Transform teleportTargetPosition;
    [SerializeField] private float offsetAmount = 2f;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private string targetAreaName; // ← הוסף את זה!

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();
        if (confiner == null)
            Debug.LogError("CinemachineConfiner2D not found in scene!");
        if (mapBoundery == null)
            Debug.LogError("Map Boundary not assigned!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;
        FadeTransition(collision.gameObject);
    }

    async void FadeTransition(GameObject player)
    {
        PauseController.SetPause(true);

        if (ScreenFader.instance == null)
        {
            Debug.LogError("ScreenFader not found!");
            if (confiner != null && mapBoundery != null)
            {
                confiner.m_BoundingShape2D = mapBoundery;
                confiner.InvalidateCache();
            }
            UpdatePlayerPosition(player);
            UpdateMap(); // ← הוסף גם כאן
            return;
        }

        await ScreenFader.instance.FadeOut(transitionDuration);

        if (confiner != null && mapBoundery != null)
        {
            confiner.enabled = false;
            confiner.m_BoundingShape2D = mapBoundery;
            confiner.InvalidateCache();
        }

        Vector3 oldPos = player.transform.position;
        UpdatePlayerPosition(player);
        Vector3 delta = player.transform.position - oldPos;

        UpdateMap(); // ← הוסף כאן!

        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
            vcam.OnTargetObjectWarped(player.transform, delta);

        if (confiner != null)
            confiner.enabled = true;

        await ScreenFader.instance.FadeIn(transitionDuration);
        PauseController.SetPause(false);
    }

    private void UpdateMap()
    {
        if (string.IsNullOrEmpty(targetAreaName)) return;

        if (MapController_Dynamic.Instance != null)
            MapController_Dynamic.Instance.UpdateCurrentArea(targetAreaName);
        else if (MapController_Manual.Instance != null)
            MapController_Manual.Instance.HighlightArea(targetAreaName);
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            player.transform.position = teleportTargetPosition.position;
            return;
        }

        Vector3 newPos = player.transform.position;
        switch (direction)
        {
            case Direction.Up: newPos.y += offsetAmount; break;
            case Direction.Down: newPos.y -= offsetAmount; break;
            case Direction.Right: newPos.x += offsetAmount; break;
            case Direction.Left: newPos.x -= offsetAmount; break;
        }
        player.transform.position = newPos;
    }
}