using System;
using System.Collections.Generic;
using System.Text;
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
    [SerializeField] private LayerMask defaultGroundLayer;

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
    [SerializeField] private bool logTerrainSampling = false;
    [SerializeField] private bool logNodeGenerationSummary = true;

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

        Dictionary<string, int> generatedNodesByLayer = new Dictionary<string, int>();

        float startX = Mathf.Min(lowerLeftBound.position.x, upperRightBound.position.x);
        float endX = Mathf.Max(lowerLeftBound.position.x, upperRightBound.position.x);
        float startZ = Mathf.Min(lowerLeftBound.position.z, upperRightBound.position.z);
        float endZ = Mathf.Max(lowerLeftBound.position.z, upperRightBound.position.z);

        for (float x = startX; x <= endX; x += nodeSeparation)
        {
            for (float z = startZ; z <= endZ; z += nodeSeparation)
            {
                Vector3 rayOrigin = new Vector3(x, raycastHeight, z);

                RaycastHit[] hits = Physics.RaycastAll(
                    rayOrigin,
                    Vector3.down,
                    raycastHeight * 2f,
                    terrainLayerMask,
                    QueryTriggerInteraction.Collide
                );

                RaycastHit? selectedTerrainHit = GetPreferredTerrainHit(hits);

                if (!selectedTerrainHit.HasValue)
                {
                    continue;
                }

                RaycastHit hitInfo = selectedTerrainHit.Value;

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

                int terrainLayerNumber = hitInfo.collider.gameObject.layer;
                float nodeCost = GetTerrainCost(terrainLayerNumber);
                PathNode newNode = new PathNode(nodePosition, nodeCost);
                nodes.Add(newNode);

                string terrainLayerName = LayerMask.LayerToName(terrainLayerNumber);

                if (!generatedNodesByLayer.ContainsKey(terrainLayerName))
                {
                    generatedNodesByLayer[terrainLayerName] = 0;
                }

                generatedNodesByLayer[terrainLayerName]++;
            }
        }

        ConnectAdjacentNodes();


        if (logNodeGenerationSummary)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("[PathfindingManager] Node generation summary:");

            foreach (KeyValuePair<string, int> pair in generatedNodesByLayer)
            {
                builder.Append("- ");
                builder.Append(pair.Key);
                builder.Append(": ");
                builder.Append(pair.Value);
                builder.AppendLine(" nodes");
            }

            Debug.Log(builder.ToString());
        }
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

    public float GetTerrainCost(int terrainLayerNumber)
    {
        for (int i = 0; i < terrainLayers.Count; i++)
        {
            if (terrainLayers[i].MatchesLayer(terrainLayerNumber))
            {
                return terrainLayers[i].CostMultiplier;
            }
        }

        return 1f;
    }

    public float GetTerrainSpeedMultiplier(int terrainLayerNumber)
    {
        for (int i = 0; i < terrainLayers.Count; i++)
        {
            if (terrainLayers[i].MatchesLayer(terrainLayerNumber))
            {
                return terrainLayers[i].SpeedMultiplier;
            }
        }

        return 1f;
    }

    public float SampleTerrainSpeedMultiplier(Vector3 worldPosition, float sampleHeight = 2f, float maxDistance = 10f)
    {
        Vector3 rayOrigin = worldPosition + Vector3.up * sampleHeight;

        RaycastHit[] hits = Physics.RaycastAll(
            rayOrigin,
            Vector3.down,
            maxDistance,
            terrainLayerMask,
            QueryTriggerInteraction.Collide
        );

        RaycastHit? selectedTerrainHit = GetPreferredTerrainHit(hits);

        if (!selectedTerrainHit.HasValue)
        {
            if (logTerrainSampling)
            {
                Debug.Log("[PathfindingManager] No terrain hit found under agent.");
            }

            return 1f;
        }

        RaycastHit selectedHit = selectedTerrainHit.Value;
        int selectedLayer = selectedHit.collider.gameObject.layer;
        float selectedSpeedMultiplier = GetTerrainSpeedMultiplier(selectedLayer);

        if (logTerrainSampling)
        {
            Debug.Log(
                $"[PathfindingManager] Terrain sampled: {selectedHit.collider.name} | " +
                $"Layer: {LayerMask.LayerToName(selectedLayer)} | " +
                $"SpeedMultiplier: {selectedSpeedMultiplier}"
            );
        }

        return selectedSpeedMultiplier;
    }

    public string BuildPathDebugSummary(List<PathNode> path)
    {
        if (path == null || path.Count == 0)
        {
            return "[PathfindingManager] Path summary: empty path";
        }

        float totalCost = 0f;
        Dictionary<string, int> nodesByTerrain = new Dictionary<string, int>();
        Dictionary<float, int> nodesByCost = new Dictionary<float, int>();

        for (int i = 0; i < path.Count; i++)
        {
            PathNode node = path[i];

            if (i > 0)
            {
                totalCost += PathfindingUtils.GetTraversalCost(path[i - 1], node);
            }

            string terrainName = GetTerrainLayerNameAtPosition(node.Position);

            if (!nodesByTerrain.ContainsKey(terrainName))
            {
                nodesByTerrain[terrainName] = 0;
            }

            nodesByTerrain[terrainName]++;

            if (!nodesByCost.ContainsKey(node.CostMultiplier))
            {
                nodesByCost[node.CostMultiplier] = 0;
            }

            nodesByCost[node.CostMultiplier]++;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("[PathfindingManager] Path summary:");
        builder.Append("Strategy: ");
        builder.AppendLine(CurrentStrategy.ToString());
        builder.Append("Node count: ");
        builder.AppendLine(path.Count.ToString());
        builder.Append("Estimated total cost: ");
        builder.AppendLine(totalCost.ToString("F2"));

        builder.AppendLine("Nodes by terrain:");
        foreach (KeyValuePair<string, int> pair in nodesByTerrain)
        {
            builder.Append("- ");
            builder.Append(pair.Key);
            builder.Append(": ");
            builder.Append(pair.Value);
            builder.AppendLine();
        }

        builder.AppendLine("Nodes by cost:");
        foreach (KeyValuePair<float, int> pair in nodesByCost)
        {
            builder.Append("- Cost ");
            builder.Append(pair.Key);
            builder.Append(": ");
            builder.Append(pair.Value);
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private string GetTerrainLayerNameAtPosition(Vector3 worldPosition)
    {
        Vector3 rayOrigin = worldPosition + Vector3.up * 2f;

        RaycastHit[] hits = Physics.RaycastAll(
            rayOrigin,
            Vector3.down,
            10f,
            terrainLayerMask,
            QueryTriggerInteraction.Collide
        );

        RaycastHit? selectedTerrainHit = GetPreferredTerrainHit(hits);

        if (!selectedTerrainHit.HasValue)
        {
            return "Unknown";
        }

        return LayerMask.LayerToName(selectedTerrainHit.Value.collider.gameObject.layer);
    }

    private RaycastHit? GetPreferredTerrainHit(RaycastHit[] hits)
    {
        if (hits == null || hits.Length == 0)
        {
            return null;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        List<RaycastHit> nonDefaultHits = new List<RaycastHit>();
        List<RaycastHit> defaultHits = new List<RaycastHit>();

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            int hitLayer = hit.collider.gameObject.layer;

            if (IsDefaultGroundLayer(hitLayer))
            {
                defaultHits.Add(hit);
            }
            else
            {
                nonDefaultHits.Add(hit);
            }
        }

        if (nonDefaultHits.Count > 0)
        {
            RaycastHit bestHit = nonDefaultHits[0];
            float bestSpeedMultiplier = GetTerrainSpeedMultiplier(bestHit.collider.gameObject.layer);

            for (int i = 1; i < nonDefaultHits.Count; i++)
            {
                RaycastHit candidateHit = nonDefaultHits[i];
                float candidateSpeedMultiplier = GetTerrainSpeedMultiplier(candidateHit.collider.gameObject.layer);

                if (candidateSpeedMultiplier < bestSpeedMultiplier)
                {
                    bestHit = candidateHit;
                    bestSpeedMultiplier = candidateSpeedMultiplier;
                }
            }

            return bestHit;
        }

        return defaultHits.Count > 0 ? defaultHits[0] : hits[0];
    }

    private bool IsDefaultGroundLayer(int layerNumber)
    {
        int layerMask = 1 << layerNumber;
        return (defaultGroundLayer.value & layerMask) != 0;
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