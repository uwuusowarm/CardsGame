using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCoordinates : MonoBehaviour
{
    public static float xOffset = 2, yOffset = 1, zOffset = 1.73f;

    public Vector3Int GetHexCoords()
        => offsetCoordinates;

    [Header("OFfset coordinates")]
    [SerializeField] private Vector3Int offsetCoordinates;

    private void Awake()
    {
        offsetCoordinates = ConvertPositionToOffset(transform.position);
        offsetCoordinates.x = Mathf.RoundToInt(offsetCoordinates.x);
        offsetCoordinates.y = Mathf.RoundToInt(offsetCoordinates.y);
        offsetCoordinates.z = Mathf.RoundToInt(offsetCoordinates.z);
    }

    private Vector3Int ConvertPositionToOffset(Vector3 position)
    {
        int x = Mathf.CeilToInt(position.x / xOffset);
        int y = Mathf.RoundToInt(position.y / yOffset);
        int z = Mathf.RoundToInt(position.z / zOffset);
        return new Vector3Int(x, y, z);
    }
    
    public void UpdateHexCoords(Vector3Int newCoords)
    {
        offsetCoordinates = newCoords;
    }

}