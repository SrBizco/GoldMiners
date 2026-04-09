using System;
using UnityEngine;

[Serializable]
public class TerrainLayerData
{
    [SerializeField] private string tagName = "Untagged";
    [SerializeField] private float costMultiplier = 1f;

    public string TagName => tagName;
    public float CostMultiplier => costMultiplier;
}