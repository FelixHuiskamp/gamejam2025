using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitTime = 2f;

    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;
    private float waitTimer;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    
    void Update()
    {
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f) 
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime) 
            {
                GoToNextWaypoint();
                waitTimer = 0f;
            }
        }
    }

    void GoToNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex  + 1) % waypoints.Length;
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }
}
