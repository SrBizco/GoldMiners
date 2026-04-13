using System.Collections.Generic;
using System.Linq;

public class AStarPathfinding : IPathfindingStrategy
{
    public List<PathNode> CreatePath(PathNode startNode, PathNode targetNode, IReadOnlyList<PathNode> allNodes)
    {
        List<PathNode> emptyPath = new List<PathNode>();

        if (startNode == null || targetNode == null)
        {
            return emptyPath;
        }

        if (startNode == targetNode)
        {
            return new List<PathNode> { startNode };
        }

        PathfindingUtils.ResetNodes(allNodes, startNode);

        List<PathNode> openNodes = new List<PathNode> { startNode };
        HashSet<PathNode> closedNodes = new HashSet<PathNode>();

        while (openNodes.Count > 0)
        {
            PathNode currentNode = openNodes
                .OrderBy(node => node.AccumulatedCost + PathfindingUtils.EstimateCost(node, targetNode))
                .First();

            openNodes.Remove(currentNode);

            if (closedNodes.Contains(currentNode))
            {
                continue;
            }

            currentNode.CurrentState = PathNodeState.Closed;
            closedNodes.Add(currentNode);

            if (currentNode == targetNode)
            {
                return PathfindingUtils.ReconstructPath(startNode, targetNode);
            }

            for (int i = 0; i < currentNode.AdjacentNodes.Count; i++)
            {
                PathNode adjacentNode = currentNode.AdjacentNodes[i];

                if (adjacentNode == null)
                {
                    continue;
                }

                if (adjacentNode.CurrentState == PathNodeState.Blocked)
                {
                    continue;
                }

                if (closedNodes.Contains(adjacentNode))
                {
                    continue;
                }

                float newCost = currentNode.AccumulatedCost + PathfindingUtils.GetTraversalCost(currentNode, adjacentNode);

                if (newCost < adjacentNode.AccumulatedCost)
                {
                    adjacentNode.AccumulatedCost = newCost;
                    adjacentNode.Parent = currentNode;
                }

                if (!openNodes.Contains(adjacentNode))
                {
                    adjacentNode.CurrentState = PathNodeState.Open;
                    openNodes.Add(adjacentNode);
                }
            }
        }

        return emptyPath;
    }
}