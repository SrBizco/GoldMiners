using System.Collections.Generic;
using UnityEngine;

public static class PathfindingUtils
{
    public static void ResetNodes(IReadOnlyList<PathNode> nodes, PathNode startNode)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            PathNode node = nodes[i];

            if (node.CurrentState == PathNodeState.Blocked)
            {
                continue;
            }

            node.CurrentState = PathNodeState.Pending;
            node.AccumulatedCost = float.MaxValue;
            node.Parent = null;
        }

        if (startNode != null && startNode.CurrentState != PathNodeState.Blocked)
        {
            startNode.AccumulatedCost = 0f;
            startNode.CurrentState = PathNodeState.Open;
        }
    }

    public static List<PathNode> ReconstructPath(PathNode startNode, PathNode targetNode)
    {
        List<PathNode> path = new List<PathNode>();

        if (startNode == null || targetNode == null)
        {
            return path;
        }

        PathNode currentNode = targetNode;

        while (currentNode != null)
        {
            path.Add(currentNode);

            if (currentNode == startNode)
            {
                break;
            }

            currentNode = currentNode.Parent;
        }

        path.Reverse();

        if (path.Count == 0 || path[0] != startNode)
        {
            return new List<PathNode>();
        }

        return path;
    }

    public static float EstimateCost(PathNode a, PathNode b)
    {
        return (a.Position - b.Position).sqrMagnitude;
    }

    public static float GetTraversalCost(PathNode fromNode, PathNode toNode)
    {
        if (fromNode == null || toNode == null)
        {
            return float.MaxValue;
        }

        float distanceCost = Vector3.Distance(fromNode.Position, toNode.Position);
        return distanceCost * toNode.CostMultiplier;
    }
}