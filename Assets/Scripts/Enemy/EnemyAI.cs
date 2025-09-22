using UnityEngine;
using UnityEngine.AI; 

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;              
    public PlayerHealth playerHealth;     

    [Header("Stats")]
    public float lookRadius = 10f;        
    public float attackRange = 2f;        
    public int damage = 10;               
    public float attackRate = 1f;         

    private NavMeshAgent agent;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();



        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                player = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= lookRadius)
        {
            
            agent.SetDestination(player.position);

            
            if (distance <= attackRange)
            {
                AttackPlayer();
            }
        }
    }

    void AttackPlayer()
    {
        
        if (Time.time >= nextAttackTime && playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

