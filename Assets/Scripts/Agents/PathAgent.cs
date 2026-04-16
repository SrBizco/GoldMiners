using System.Collections.Generic;
using UnityEngine;

public class PathAgent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PathfindingManager pathfindingManager;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float nodeReachDistance = 0.1f;

    [Header("Terrain Sampling")]
    [SerializeField] private float terrainSampleHeight = 2f;
    [SerializeField] private float terrainSampleDistance = 10f;

    [Header("Debug")]
    [SerializeField] private bool drawCurrentPath = true;
    [SerializeField] private bool logRecalculation = false;
    [SerializeField] private bool logCurrentSpeed = false;

    private readonly Dictionary<int, float> externalSpeedModifiers = new Dictionary<int, float>();

    private List<PathNode> currentPath = new List<PathNode>();
    private int currentPathIndex;
    private bool hasDestination;
    private Vector3 currentWorldDestination;
    private float currentMovementSpeed;

    public bool HasReachedDestination { get; private set; } = true;
    public bool HasDestination => hasDestination;
    public Vector3 CurrentWorldDestination => currentWorldDestination;

    public PathfindingStrategyType CurrentStrategy => pathfindingManager != null
        ? pathfindingManager.CurrentStrategy
        : PathfindingStrategyType.BreadthFirst;

    private void Awake()
    {
        currentMovementSpeed = movementSpeed;
    }

    private void OnEnable()
    {
        if (pathfindingManager != null)
        {
            pathfindingManager.StrategyChanged += OnStrategyChanged;
        }
    }

    private void OnDisable()
    {
        if (pathfindingManager != null)
        {
            pathfindingManager.StrategyChanged -= OnStrategyChanged;
        }

        externalSpeedModifiers.Clear();
        currentMovementSpeed = movementSpeed;
    }

    private void Update()
    {
        UpdateCurrentMovementSpeed();
        MoveAlongPath();
    }

    public void SetDestination(Vector3 worldDestination)
    {
        currentWorldDestination = worldDestination;
        RecalculatePathToCurrentDestination();
    }

    private void OnStrategyChanged(PathfindingStrategyType newStrategy)
    {
        if (!hasDestination || HasReachedDestination)
        {
            return;
        }

        RecalculatePathToCurrentDestination();

        if (logRecalculation)
        {
            Debug.Log($"[{name}] Path recalculated using {newStrategy}");
        }
    }

    public void RecalculatePathToCurrentDestination()
    {
        if (pathfindingManager == null)
        {
            Debug.LogWarning("[PathAgent] PathfindingManager not assigned.");
            return;
        }

        PathNode startNode = pathfindingManager.GetClosestNode(transform.position);
        PathNode targetNode = pathfindingManager.GetClosestNode(currentWorldDestination);

        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning("[PathAgent] Start node or target node is null.");
            return;
        }

        List<PathNode> newPath = pathfindingManager.CreatePath(startNode, targetNode);

        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning($"[PathAgent] No valid path found using {CurrentStrategy}.");
            currentPath.Clear();
            currentPathIndex = 0;
            hasDestination = false;
            HasReachedDestination = true;
            return;
        }

        currentPath = newPath;
        currentPathIndex = 0;
        hasDestination = true;
        HasReachedDestination = false;

        if (currentPath.Count > 0)
        {
            if ((currentPath[0].Position - transform.position).sqrMagnitude <= nodeReachDistance * nodeReachDistance)
            {
                currentPathIndex = 1;
            }
        }

        if (currentPathIndex >= currentPath.Count)
        {
            HasReachedDestination = true;
            hasDestination = false;
        }
    }

    public void ApplySpeedModifier(int sourceId, float speedMultiplier)
    {
        float clampedMultiplier = Mathf.Clamp(speedMultiplier, 0.05f, 1f);
        externalSpeedModifiers[sourceId] = clampedMultiplier;
    }

    public void RemoveSpeedModifier(int sourceId)
    {
        externalSpeedModifiers.Remove(sourceId);
    }

    private void UpdateCurrentMovementSpeed()
    {
        float terrainMultiplier = 1f;

        if (pathfindingManager != null)
        {
            terrainMultiplier = pathfindingManager.SampleTerrainSpeedMultiplier(
                transform.position,
                terrainSampleHeight,
                terrainSampleDistance
            );
        }

        float externalMultiplier = 1f;

        foreach (KeyValuePair<int, float> modifier in externalSpeedModifiers)
        {
            if (modifier.Value < externalMultiplier)
            {
                externalMultiplier = modifier.Value;
            }
        }

        currentMovementSpeed = movementSpeed * terrainMultiplier * externalMultiplier;

        if (logCurrentSpeed)
        {
            Debug.Log($"[{name}] Current speed: {currentMovementSpeed}");
        }
    }

    private void MoveAlongPath()
    {
        if (!hasDestination || HasReachedDestination)
        {
            return;
        }

        if (currentPath == null || currentPath.Count == 0)
        {
            HasReachedDestination = true;
            hasDestination = false;
            return;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            HasReachedDestination = true;
            hasDestination = false;
            return;
        }

        Vector3 targetPosition = currentPath[currentPathIndex].Position;
        Vector3 flatTargetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);

        Vector3 direction = flatTargetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= nodeReachDistance * nodeReachDistance)
        {
            currentPathIndex++;

            if (currentPathIndex >= currentPath.Count)
            {
                HasReachedDestination = true;
                hasDestination = false;
            }

            return;
        }

        Vector3 moveDirection = direction.normalized;
        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            flatTargetPosition,
            currentMovementSpeed * Time.deltaTime
        );

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position = newPosition;
    }

    private void OnDrawGizmos()
    {
        if (!drawCurrentPath || currentPath == null || currentPath.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.cyan;

        for (int i = 0; i < currentPath.Count; i++)
        {
            Gizmos.DrawSphere(currentPath[i].Position + Vector3.up * 0.2f, 0.15f);

            if (i < currentPath.Count - 1)
            {
                Gizmos.DrawLine(
                    currentPath[i].Position + Vector3.up * 0.2f,
                    currentPath[i + 1].Position + Vector3.up * 0.2f
                );
            }
        }
    }
}