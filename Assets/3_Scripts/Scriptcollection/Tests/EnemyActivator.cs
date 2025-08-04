using System.Collections.Generic;
using UnityEngine;

public class EnemyActivator : MonoBehaviour
{
    public static EnemyActivator Instance { get; private set; }
    private List<EnemyUnit> allEnemies = new List<EnemyUnit>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterEnemy(EnemyUnit enemy)
    {
        if (!allEnemies.Contains(enemy))
            allEnemies.Add(enemy);
    }

    public void ActivateEnemiesInRoom(int roomID)
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy.currentHex != null && enemy.currentHex.RoomID == roomID)
            {
                enemy.gameObject.SetActive(true);
                Debug.Log($"Gegner {enemy.name} in Raum {roomID} aktiviert!");
            }
        }
    }

    public void DeactivateEnemiesBehindClosedDoors()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy.currentHex != null && !IsRoomOpen(enemy.currentHex.RoomID))
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }

    private bool IsRoomOpen(int roomID)
    {
        foreach (var door in FindObjectsOfType<OpenDoor>())
        {
            if (door.roomID == roomID && door.isOpen)
                return true;
        }
        return false;
    }
}
