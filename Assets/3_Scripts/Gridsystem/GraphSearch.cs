using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Analytics;

public class GraphSearch
{
    public static BFSResult BFSGetRange(HexGrid hexGrid, Vector3Int startPoint, int movementPoints)
    {
        Dictionary<Vector3Int, Vector3Int?> visitedNodes = new Dictionary<Vector3Int, Vector3Int?>();
        Dictionary<Vector3Int, int> costSoFar = new Dictionary<Vector3Int, int>();
        Queue<Vector3Int> nodesToVisitQueue = new Queue<Vector3Int>();

        nodesToVisitQueue.Enqueue(startPoint);
        costSoFar.Add(startPoint, 0);
        visitedNodes.Add(startPoint, null);

        while (nodesToVisitQueue.Count > 0)
        {
            Vector3Int currentNode = nodesToVisitQueue.Dequeue();
            foreach (Vector3Int neighbourPosition in hexGrid.GetNeighborsFor(currentNode))
            {
                Hex neighborHex = hexGrid.GetTileAt(neighbourPosition);
                if (neighborHex.IsObstacle() || neighborHex.IsOccupied())
                {
                    continue;
                }
                int nodeCost = hexGrid.GetTileAt(neighbourPosition).GetCost();
                int currentCost = costSoFar[currentNode];
                int newCost = currentCost + nodeCost;

                if (newCost <= movementPoints)
                {
                    if (!visitedNodes.ContainsKey(neighbourPosition))
                    {
                        visitedNodes[neighbourPosition] = currentNode;
                        costSoFar[neighbourPosition] = newCost;
                        nodesToVisitQueue.Enqueue(neighbourPosition);
                    }
                    else if (costSoFar[neighbourPosition] > newCost)
                    {
                        costSoFar[neighbourPosition] = newCost;
                        visitedNodes[neighbourPosition] = currentNode;
                    }
                }
            }
        }
        return new BFSResult { visitedNodesDict = visitedNodes };
    }

    internal static List<Vector3Int> GeneratePathBFS(Vector3Int current, Dictionary<Vector3Int, Vector3Int?> visitedNodesDict)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        path.Add(current);
        while (visitedNodesDict[current] != null)
        {
            path.Add(visitedNodesDict[current].Value);
            current = visitedNodesDict[current].Value;
        }
        path.Reverse();
        return path.Skip(1).ToList();
    }
}

public struct BFSResult
{
    public Dictionary<Vector3Int, Vector3Int?> visitedNodesDict;

    public BFSResult(Dictionary<Vector3Int, Vector3Int?> nodes)
    {
        visitedNodesDict = nodes ?? new Dictionary<Vector3Int, Vector3Int?>();
    }

    public List<Vector3Int> GetPathTo(Vector3Int destination)
    {
        if (visitedNodesDict == null || !visitedNodesDict.ContainsKey(destination))
            return new List<Vector3Int>();
        return GraphSearch.GeneratePathBFS(destination, visitedNodesDict);
    }

    public bool IsHexPositionInRange(Vector3Int position)
    {
        return visitedNodesDict != null && visitedNodesDict.ContainsKey(position);
    }

    public IEnumerable<Vector3Int> GetRangePositions()
    {
        return visitedNodesDict != null ? visitedNodesDict.Keys : Enumerable.Empty<Vector3Int>();
    }
}