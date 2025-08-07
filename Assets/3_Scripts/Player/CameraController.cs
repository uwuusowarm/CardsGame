using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player;  
    public float rotationSpeed = 100f; 
    
    private Vector3 offset;
    private float rotationY;
    
    void Start()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }
        
        if (player != null)
        {
            offset = transform.position - player.position;
            rotationY = 0f; 
        }
    }
    
    void LateUpdate()
    {
        if (player == null) return;
        
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            rotationY += mouseX;
        }
        
        Quaternion rotation = Quaternion.Euler(0, rotationY, 0);
        Vector3 rotatedOffset = rotation * offset;
        
        transform.position = player.position + rotatedOffset;
        transform.LookAt(player.position);
    }
}