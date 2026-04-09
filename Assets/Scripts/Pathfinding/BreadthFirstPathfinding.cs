using System.Collections.Generic;
using System.Linq;

public static class BreadthFirstPathfinding
{
    public static List<PathNode> CreatePath(PathNode startNode, PathNode targetNode)
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

        Queue<PathNode> nodesToVisit = new Queue<PathNode>();
        HashSet<PathNode> visitedNodes = new HashSet<PathNode>();
        Dictionary<PathNode, PathNode> previousNodes = new Dictionary<PathNode, PathNode>();

        nodesToVisit.Enqueue(startNode);
        visitedNodes.Add(startNode);

        while (nodesToVisit.Count > 0)
        {
            PathNode currentNode = nodesToVisit.Dequeue();

            if (currentNode == targetNode)
            {
                return ReconstructPath(startNode, targetNode, previousNodes);
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
                previousNodes[adjacentNode] = currentNode;
                nodesToVisit.Enqueue(adjacentNode);
            }
        }

        return emptyPath;
    }

    private static List<PathNode> ReconstructPath(
        PathNode startNode,
        PathNode targetNode,
        Dictionary<PathNode, PathNode> previousNodes)
    {
        List<PathNode> path = new List<PathNode>();
        PathNode currentNode = targetNode;

        while (currentNode != null)
        {
            path.Add(currentNode);

            if (currentNode == startNode)
            {
                break;
            }

            if (!previousNodes.TryGetValue(currentNode, out currentNode))
            {
                return new List<PathNode>();
            }
        }

        path.Reverse();
        return path;
    }
}