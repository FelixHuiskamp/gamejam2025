using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Header")]
    public Transform playerBody;

    [Header("Settings")]
    public float mouseSensitivity = 100f;
    public bool lockCursor = true;

    private float xRotation = 0f;


    void Start()
    {

    }

   
    void Update()
    {
        HandleMouseLook();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
