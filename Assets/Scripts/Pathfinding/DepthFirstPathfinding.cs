using System.Collections.Generic;

public class DepthFirstPathfinding : IPathfindingStrategy
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

        Stack<PathNode> nodesToVisit = new Stack<PathNode>();
        HashSet<PathNode> visitedNodes = new HashSet<PathNode>();

        nodesToVisit.Push(startNode);

        while (nodesToVisit.Count > 0)
        {
            PathNode currentNode = nodesToVisit.Pop();

            if (visitedNodes.Contains(currentNode))
            {
                continue;
            }

            visitedNodes.Add(currentNode);
            currentNode.CurrentState = PathNodeState.Closed;

            if (currentNode == targetNode)
            {
                return PathfindingUtils.ReconstructPath(startNode, targetNode);
            }

            for (int i = currentNode.AdjacentNodes.Count - 1; i >= 0; i--)
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

                if (visitedNodes.Contains(adjacentNode))
                {
                    continue;
                }

                if (adjacentNode.Parent == null && adjacentNode != startNode)
                {
                    adjacentNode.Parent = currentNode;
                }

                adjacentNode.CurrentState = PathNodeState.Open;
                nodesToVisit.Push(adjacentNode);
            }
        }

        return emptyPath;
    }
}