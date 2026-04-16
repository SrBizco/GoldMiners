using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TerrainCostZone : MonoBehaviour
{
    [Header("Speed Modifier")]
    [SerializeField][Range(0.05f, 1f)] private float speedMultiplier = 0.5f;

    [Header("Affected Layers")]
    [SerializeField] private LayerMask affectedLayers;

    private int zoneId;

    private void Awake()
    {
        zoneId = GetInstanceID();

        Collider zoneCollider = GetComponent<Collider>();

        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsAffectedLayer(other.gameObject.layer))
        {
            return;
        }

        PathAgent pathAgent = other.GetComponentInParent<PathAgent>();

        if (pathAgent == null)
        {
            return;
        }

        pathAgent.ApplySpeedModifier(zoneId, speedMultiplier);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsAffectedLayer(other.gameObject.layer))
        {
            return;
        }

        PathAgent pathAgent = other.GetComponentInParent<PathAgent>();

        if (pathAgent == null)
        {
            return;
        }

        pathAgent.RemoveSpeedModifier(zoneId);
    }

    private bool IsAffectedLayer(int layerNumber)
    {
        int layerMask = 1 << layerNumber;
        return (affectedLayers.value & layerMask) != 0;
    }
}