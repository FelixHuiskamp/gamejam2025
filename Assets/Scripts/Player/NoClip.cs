using Unity.VisualScripting;
using UnityEngine;

public class NoClip : MonoBehaviour
{
    public float normalSpeed = 5f;
    public float flySpeed = 10f;
    public bool noclipActive = false;

    private CharacterController controller;
    private bool wasGravityEnabled;
    private Rigidbody rb;
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();

        if (rb != null) wasGravityEnabled = rb.useGravity;
    }


    void Update()
    {
        if (noclipActive)
        {
            FlyMovement();
        }
    }

   
    public void ToggleNoClip()
    {
        noclipActive = !noclipActive;
        if (noclipActive)
        {
            controller.enabled = false;
        }
        else 
        { 
            controller.enabled = true;
        }
    }

    private void FlyMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        float moveY = 0f;

        if (Input.GetKey(KeyCode.Space)) moveY = 1f; 
        if (Input.GetKey(KeyCode.LeftControl)) moveY = -1f;

        Vector3 move = transform.right * moveX + transform.forward * moveZ + Vector3.up * moveY;
        transform.position += move * flySpeed * Time.deltaTime;
    }
}