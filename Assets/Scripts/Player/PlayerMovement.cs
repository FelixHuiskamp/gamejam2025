using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f; 
    public float jumpHeight = 2f;

    private CharacterController controller;
    private Vector3 velocity; 
    private bool isGrounded;

    [Header("Ground Check")]
    public Transform groundCheck; 
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    
    public void MoveForward()
    {
        controller.Move(transform.forward * moveSpeed);
    }

    public void MoveBack()
    {
        controller.Move(-transform.forward * moveSpeed);
    }

    public void MoveLeft()
    {
        controller.Move(-transform.right * moveSpeed);
    }

    public void MoveRight()
    {
        controller.Move(transform.right * moveSpeed);
    }
}

