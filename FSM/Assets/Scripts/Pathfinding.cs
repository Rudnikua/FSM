using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour {

    [SerializeField] private GridMap grid;
    [SerializeField] Transform[] seeker, target;

    public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos) {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        if (startNode == null || targetNode == null) return null;

        if (!targetNode.walkable) {
            targetNode = FindClosestWalkableNode(targetNode);
        }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0) {
            Node currentNode = openSet[0];

            for (int i = 1; i < openSet.Count; i++) {
                if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost  && openSet[i].hCost < currentNode.hCost) {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode) {
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
                if (!neighbour.walkable || closedSet.Contains(neighbour)) {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);

                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour)) {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        return null;
    }

    private List<Vector3> RetracePath(Node startNode, Node endNode) {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        grid.path = path;

        List<Vector3> waypoints = new List<Vector3>();
        foreach (Node node in path) waypoints.Add(node.worldPosition);

        return waypoints;
    }

    private Node FindClosestWalkableNode(Node targetNode) {
        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        queue.Enqueue(targetNode);
        visited.Add(targetNode);

        while (queue.Count > 0) {
            Node currentNode = queue.Dequeue();

            if (currentNode.walkable) {
                return currentNode;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
                if (!visited.Contains(neighbour)) {
                    visited.Add(neighbour);
                    queue.Enqueue(neighbour);
                }
            }
        }
        return null;
    }

    private int GetDistance(Node nodeA, Node nodeB) {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distanceX > distanceY) return 14 * distanceY + 10 * (distanceX - distanceY);
        return 14 * distanceX + 10 * (distanceY - distanceX);
    }
}
