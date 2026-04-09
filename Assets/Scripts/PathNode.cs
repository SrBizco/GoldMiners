using System.Collections.Generic;
using UnityEngine;

public enum PathNodeState
{
    Pending,
    Open,
    Closed,
    Blocked
}

public class PathNode
{
    public Vector3 Position { get; set; }
    public PathNodeState CurrentState { get; set; } = PathNodeState.Pending;
    public float CostMultiplier { get; set; } = 1f;
    public float AccumulatedCost { get; set; } = 0f;
    public PathNode Parent { get; set; }
    public List<PathNode> AdjacentNodes { get; set; } = new List<PathNode>();

    public PathNode(Vector3 position, float costMultiplier)
    {
        Position = position;
        CostMultiplier = costMultiplier;
    }
}