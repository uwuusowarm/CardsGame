using UnityEngine;

[SelectionBase]
public class EnemyUnit : Unit  
{
    private void Start()
    {
        GetComponentInChildren<Renderer>().material.color = Color.red;

        HexGrid hexGrid = FindObjectOfType<HexGrid>();
        if (hexGrid != null)
        {
            Hex startHex = hexGrid.GetTileAt(hexGrid.GetClosestHex(transform.position));
            if (startHex != null) startHex.SetUnit(this);
        }
    }

    /*public override void MoveTroughPath(List<Vector3> currentPath)
    {
        Debug.Log("No Move");
    }*/
}