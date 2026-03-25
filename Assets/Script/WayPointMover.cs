using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTime = 2f;
    public bool loopWypoints = true;

    private Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isWaiting;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        waypoints = new Transform[waypointParent.childCount];

        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PauseController.IsGamePaused || isWaiting)
        {
            animator.SetBool("isWalking", false);
            return;
        }

        MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];
        Vector2 direction = (target.position - transform.position).normalized;

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed*Time.deltaTime);
        animator.SetFloat("InputX", direction.x);
        animator.SetFloat("InputY", direction.y);
        animator.SetBool("isWalking", direction.magnitude > 0f);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        animator.SetBool("isWalking", false);

        yield return new WaitForSeconds(waitTime);

        currentWaypointIndex = loopWypoints ? (currentWaypointIndex + 1) % waypoints.Length : Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);

        isWaiting = false;
    }
}
