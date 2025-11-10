using UnityEngine;

public class PlayerJump : MonoBehaviour
{
    public float jumpForce = 5f;
    private Rigidbody rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        
    }

    public void Jump()
    {
        Debug.Log("Player springt!");
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}

