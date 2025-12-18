using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
public class Node {
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX, gridY;

    public int gCost;
    public int hCost;
    public Node parent;

    public List<Node> neighbours = new List<Node>();
    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY) {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
    public int fCost {
        get { return gCost + hCost; }
    }
}
public class GridMap : MonoBehaviour {
    [Header("Grid Settings")]
    [SerializeField] private LayerMask worldMask;
    [SerializeField] private Vector2 gridWorldSize;
    [SerializeField] private float nodeRadius;

    [Header("Max Slope")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float maxStepHeight = 0.5f;

    List<Node>[,] grid;

    private float nodeDiameter;
    private int gridSizeX, gridSizeY;

    private void Awake() {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    private void CreateGrid() {
        grid = new List<Node>[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                grid[x, y] = new List<Node>();
            }
        }

        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                Vector3 rayOrigin = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                rayOrigin.y = 500f;

                RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 1000f, worldMask);

                foreach (RaycastHit hit in hits) {
                    float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                    if (slopeAngle > maxSlopeAngle) continue;

                    Vector3 checkPosition = hit.point + Vector3.up * nodeRadius;
                    Collider[] colliders = Physics.OverlapSphere(checkPosition, nodeRadius * 0.9f, worldMask);

                    bool isBlocked = false;
                    foreach (Collider collider in colliders) {
                        if (collider.gameObject != hit.collider) {
                            isBlocked = true;
                            break;
                        }
                    }

                    bool walkable = !isBlocked;

                    Node newNode = new Node(walkable, hit.point, x, y);
                    grid[x, y].Add(newNode);
                }
            }
        }
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                foreach (Node node in grid[x, y]) {
                    if (node.walkable) CalculateNeighbours(node);
                }
            }
        }
    }

    private void CalculateNeighbours(Node node) {
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
                    List<Node> potentialNeighbours = grid[checkX, checkY];

                    foreach (Node potentialNeighbour in potentialNeighbours) {
                        if (!potentialNeighbour.walkable) continue;

                        float heightDiff = Mathf.Abs(node.worldPosition.y - potentialNeighbour.worldPosition.y);

                        if (heightDiff <= maxStepHeight) {
                            node.neighbours.Add(potentialNeighbour);
                        }
                    }
                }
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition) {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        List<Node> columnNodes = grid[x, y];

        Node closestNode = null;
        float minHeightDistance = float.MaxValue;

        foreach (Node node in columnNodes) {
            float distance = Mathf.Abs(worldPosition.y - node.worldPosition.y);
            if (distance < minHeightDistance) {
                minHeightDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    public List<Node> GetNeighbours(Node node) {
        return node.neighbours;
    }

    public List<Node> path;
    private void OnDrawGizmos() {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        if (grid != null) {
            foreach (var list in grid) {
                foreach (Node node in list) {
                    Gizmos.color = (node.walkable) ? new Color(1, 1, 1, 0.5f) : Color.red;
                    if (path != null && path.Contains(node)) Gizmos.color = Color.green;

                    Gizmos.DrawCube(node.worldPosition, new Vector3(nodeDiameter - .1f, 0.1f, nodeDiameter - 0.1f));
                }
            }
        }
    }
}
