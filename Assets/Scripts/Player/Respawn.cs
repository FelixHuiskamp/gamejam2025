using UnityEngine;

public class Respawn : MonoBehaviour
{

    public Transform respawnPoint;
    public float fallThreshold = -10f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    
    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        if (controller != null)
        {
            controller.enabled = false;
            transform.position = respawnPoint.position;
            controller.enabled = true;
        }
        else 
        { 
            transform.position = respawnPoint.position;
        }
    }
}
