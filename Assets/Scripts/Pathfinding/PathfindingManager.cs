using System;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    [Header("Map Bounds")]
    [SerializeField] private Transform lowerLeftBound;
    [SerializeField] private Transform upperRightBound;

    [Header("Grid Settings")]
    [SerializeField] private float nodeSeparation = 1f;
    [SerializeField] private float raycastHeight = 10f;
    [SerializeField] private float obstacleCheckRadius = 0.35f;

    [Header("Layers")]
    [SerializeField] private LayerMask terrainLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Terrain Costs")]
    [SerializeField] private List<TerrainLayerData> terrainLayers = new List<TerrainLayerData>();

    [Header("Pathfinding")]
    [SerializeField] private PathfindingStrategyType currentStrategy = PathfindingStrategyType.BreadthFirst;

    [Header("Debug")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool drawNodeSpheres = true;
    [SerializeField] private bool drawConnections = true;
    [SerializeField] private bool drawOnlyWalkableNodes = true;
    [SerializeField] private float gizmoSphereRadius = 0.12f;

    private readonly List<PathNode> nodes = new List<PathNode>();

    public event Action<PathfindingStrategyType> StrategyChanged;

    public IReadOnlyList<PathNode> Nodes => nodes;
    public PathfindingStrategyType CurrentStrategy => currentStrategy;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateNodes();
        }
    }

    [ContextMenu("Generate Nodes")]
    public void GenerateNodes()
    {
        nodes.Clear();

        if (lowerLeftBound == null || upperRightBound == null)
        {
            Debug.LogWarning("[PathfindingManager] Bounds not assigned.");
            return;
        }

        float startX = Mathf.Min(lowerLeftBound.position.x, upperRightBound.position.x);
        float endX = Mathf.Max(lowerLeftBound.position.x, upperRightBound.position.x);
        float startZ = Mathf.Min(lowerLeftBound.position.z, upperRightBound.position.z);
        float endZ = Mathf.Max(lowerLeftBound.position.z, upperRightBound.position.z);

        for (float x = startX; x <= endX; x += nodeSeparation)
        {
            for (float z = startZ; z <= endZ; z += nodeSeparation)
            {
                Vector3 rayOrigin = new Vector3(x, raycastHeight, z);

                if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, raycastHeight * 2f, terrainLayerMask))
                {
                    continue;
                }

                Vector3 nodePosition = hitInfo.point + Vector3.up * 0.05f;
                bool isBlocked = Physics.CheckSphere(nodePosition, obstacleCheckRadius, obstacleLayerMask);

                if (isBlocked)
                {
                    if (!drawOnlyWalkableNodes)
                    {
                        PathNode blockedNode = new PathNode(nodePosition, 9999f);
                        blockedNode.CurrentState = PathNodeState.Blocked;
                        nodes.Add(blockedNode);
                    }

                    continue;
                }

                float nodeCost = GetTerrainCost(hitInfo.collider.tag);
                PathNode newNode = new PathNode(nodePosition, nodeCost);
                nodes.Add(newNode);
            }
        }

        ConnectAdjacentNodes();

        Debug.Log($"[PathfindingManager] Nodes generated: {nodes.Count}");
    }

    public void SetPathfindingStrategy(PathfindingStrategyType strategyType)
    {
        if (currentStrategy == strategyType)
        {
            return;
        }

        currentStrategy = strategyType;
        Debug.Log($"[PathfindingManager] Strategy changed to: {currentStrategy}");
        StrategyChanged?.Invoke(currentStrategy);
    }

    public void SetDepthFirstStrategy()
    {
        SetPathfindingStrategy(PathfindingStrategyType.DepthFirst);
    }

    public void SetBreadthFirstStrategy()
    {
        SetPathfindingStrategy(PathfindingStrategyType.BreadthFirst);
    }

    public void SetDijkstraStrategy()
    {
        SetPathfindingStrategy(PathfindingStrategyType.Dijkstra);
    }

    public void SetAStarStrategy()
    {
        SetPathfindingStrategy(PathfindingStrategyType.AStar);
    }

    public List<PathNode> CreatePath(PathNode startNode, PathNode targetNode)
    {
        IPathfindingStrategy strategy = PathfindingStrategyFactory.GetStrategy(currentStrategy);
        return strategy.CreatePath(startNode, targetNode, nodes);
    }

    private float GetTerrainCost(string terrainTag)
    {
        for (int i = 0; i < terrainLayers.Count; i++)
        {
            if (terrainLayers[i].TagName == terrainTag)
            {
                return terrainLayers[i].CostMultiplier;
            }
        }

        return 1f;
    }

    private void ConnectAdjacentNodes()
    {
        float maxDistance = nodeSeparation * 1.5f;
        float maxDistanceSqr = maxDistance * maxDistance;

        for (int i = 0; i < nodes.Count; i++)
        {
            PathNode currentNode = nodes[i];
            currentNode.AdjacentNodes.Clear();

            if (currentNode.CurrentState == PathNodeState.Blocked)
            {
                continue;
            }

            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                PathNode candidateNode = nodes[j];

                if (candidateNode.CurrentState == PathNodeState.Blocked)
                {
                    continue;
                }

                float distanceSqr = (currentNode.Position - candidateNode.Position).sqrMagnitude;

                if (distanceSqr <= maxDistanceSqr)
                {
                    currentNode.AdjacentNodes.Add(candidateNode);
                }
            }
        }
    }

    public PathNode GetClosestNode(Vector3 worldPosition)
    {
        PathNode closestNode = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < nodes.Count; i++)
        {
            PathNode currentNode = nodes[i];

            if (currentNode.CurrentState == PathNodeState.Blocked)
            {
                continue;
            }

            float distanceSqr = (currentNode.Position - worldPosition).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestNode = currentNode;
            }
        }

        return closestNode;
    }

    public void ResetNodeStates()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].CurrentState == PathNodeState.Blocked)
            {
                continue;
            }

            nodes[i].CurrentState = PathNodeState.Pending;
            nodes[i].AccumulatedCost = 0f;
            nodes[i].Parent = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (nodes == null || nodes.Count == 0)
        {
            return;
        }

        if (drawConnections)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                PathNode node = nodes[i];

                if (node.CurrentState == PathNodeState.Blocked)
                {
                    continue;
                }

                Gizmos.color = Color.gray;

                for (int j = 0; j < node.AdjacentNodes.Count; j++)
                {
                    Gizmos.DrawLine(node.Position, node.AdjacentNodes[j].Position);
                }
            }
        }

        if (drawNodeSpheres)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                PathNode node = nodes[i];

                switch (node.CurrentState)
                {
                    case PathNodeState.Blocked:
                        Gizmos.color = Color.red;
                        break;
                    case PathNodeState.Open:
                        Gizmos.color = Color.yellow;
                        break;
                    case PathNodeState.Closed:
                        Gizmos.color = Color.black;
                        break;
                    default:
                        Gizmos.color = Color.green;
                        break;
                }

                Gizmos.DrawSphere(node.Position, gizmoSphereRadius);
            }
        }
    }
}