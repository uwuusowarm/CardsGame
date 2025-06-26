using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogError("OrthoUICameraAlign: No camera found or assigned. Please assign a target camera.", this);
            enabled = false; 
            return;
        }

        if (!targetCamera.orthographic)
        {
            Debug.LogWarning("OrthoUICameraAlign: The assigned target camera is not orthographic. This script is intended for orthographic cameras. The UI will still align to its rotation, but the 'billboarding' effect might not be what you expect for a perspective camera.", this);
        }
    }
    void LateUpdate()
    {
        if (targetCamera == null) return;
        
        transform.rotation = targetCamera.transform.rotation;
    }
}