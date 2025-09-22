using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Refrences")]
    public Transform target;
    public Transform followPoint;

    [Header("Settings")]
    public float distance = 4f;
    public float height = 2f;
    public float rotationSpeed = 150f;
    public float smoothTime = 0.1f;
    public float minY = -30f;
    public float maxY = 60f;

    public float yaw;
    public float pitch;
    private Vector3 currentVelocity;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    
    void LateUpdate()
    {
        if (target != null) return;
        
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * distance + Vector3.up * height;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
         
        transform.LookAt(target.position + Vector3.up * 1.5f);                        

    }
}
