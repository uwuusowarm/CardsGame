using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInput : MonoBehaviour
{
    public UnityEvent<Vector3> PointerClick;
    public UnityEvent PointerRightClick;

    private void Update()
    {
        DetectInput();
    }

    private void DetectInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pointerPos = Input.mousePosition;
            PointerClick?.Invoke(pointerPos);
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector3 touchPos = Input.GetTouch(0).position;
            PointerClick?.Invoke(touchPos);
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            PointerRightClick?.Invoke();
        }
    }

}