using System.Collections.Generic;

public interface IPathfindingStrategy
{
    List<PathNode> CreatePath(PathNode startNode, PathNode targetNode, IReadOnlyList<PathNode> allNodes);
}