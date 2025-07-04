using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    public LayerMask selectionMask;

    public UnityEvent<GameObject> OnUnitySelected;
    public UnityEvent<GameObject> TerrainSelected;


    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }
    
    private bool IsUnit(GameObject obj)
    {
        return obj.GetComponent<Unit>() != null;
    }

    private bool IsEnemy(GameObject obj)
    {
        return obj.GetComponent<EnemyUnit>() != null;
    }

    public void HandleClick(Vector3 mousePosition)
    {
        GameObject result;
        if (FindTarget(mousePosition, out result))
        {
            if (!IsUnit(result) && !IsEnemy(result))
            {
                TerrainSelected.Invoke(result);
            }
        }
    }

    private bool UnitySelected(GameObject result)
    {
        return result.GetComponent<Unit>() != null &&
           result.GetComponent<EnemyUnit>() == null;
    }

    private bool FindTarget(Vector3 mousePosition, out GameObject result)
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out hit, selectionMask))
        {
            result = hit.collider.gameObject;
            return true;
        }
        result = null;
        return false;
    }
}