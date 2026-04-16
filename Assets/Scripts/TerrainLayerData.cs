using System;
using UnityEngine;

[Serializable]
public class TerrainLayerData
{
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private float costMultiplier = 1f;
    [SerializeField][Range(0.05f, 1f)] private float speedMultiplier = 1f;

    public LayerMask TerrainLayer => terrainLayer;
    public float CostMultiplier => costMultiplier;
    public float SpeedMultiplier => speedMultiplier;

    public bool MatchesLayer(int layerNumber)
    {
        int layerMask = 1 << layerNumber;
        return (terrainLayer.value & layerMask) != 0;
    }
}