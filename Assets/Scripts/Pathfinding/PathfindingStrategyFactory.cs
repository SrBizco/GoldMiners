public static class PathfindingStrategyFactory
{
    private static readonly IPathfindingStrategy depthFirstStrategy = new DepthFirstPathfinding();
    private static readonly IPathfindingStrategy breadthFirstStrategy = new BreadthFirstPathfinding();
    private static readonly IPathfindingStrategy dijkstraStrategy = new DijkstraPathfinding();
    private static readonly IPathfindingStrategy aStarStrategy = new AStarPathfinding();

    public static IPathfindingStrategy GetStrategy(PathfindingStrategyType strategyType)
    {
        switch (strategyType)
        {
            case PathfindingStrategyType.DepthFirst:
                return depthFirstStrategy;

            case PathfindingStrategyType.BreadthFirst:
                return breadthFirstStrategy;

            case PathfindingStrategyType.Dijkstra:
                return dijkstraStrategy;

            case PathfindingStrategyType.AStar:
                return aStarStrategy;

            default:
                return breadthFirstStrategy;
        }
    }
}