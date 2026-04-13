using System.Collections.Generic;

public class BreadthFirstPathfinding : IPathfindingStrategy
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

        Queue<PathNode> nodesToVisit = new Queue<PathNode>();
        HashSet<PathNode> visitedNodes = new HashSet<PathNode>();

        nodesToVisit.Enqueue(startNode);
        visitedNodes.Add(startNode);

        while (nodesToVisit.Count > 0)
        {
            PathNode currentNode = nodesToVisit.Dequeue();
            currentNode.CurrentState = PathNodeState.Closed;

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

                if (visitedNodes.Contains(adjacentNode))
                {
                    continue;
                }

                visitedNodes.Add(adjacentNode);
                adjacentNode.Parent = currentNode;
                adjacentNode.CurrentState = PathNodeState.Open;
                nodesToVisit.Enqueue(adjacentNode);
            }
        }

        return emptyPath;
    }
}