using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PathAgent pathAgent;
    [SerializeField] private SafeZone safeZone;
    [SerializeField] private EnemyAnimationController animationController;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private float interactionDistance = 1.2f;
    [SerializeField] private float repathInterval = 0.75f;
    [SerializeField] private float minTargetMovementBeforeRepath = 0.75f;

    [Header("Attack")]
    [SerializeField] private int damagePerHit = 1;
    [SerializeField] private float attackInterval = 1f;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;

    private FiniteStateMachine<EnemyController> fsm;

    private EnemyIdleState idleState;
    private EnemyChaseState chaseState;
    private EnemyAttackState attackState;
    private EnemyReturnState returnState;

    private MinerController currentTarget;
    private float repathTimer;
    private float attackTimer;
    private Vector3 spawnPosition;
    private Vector3 lastRequestedTargetPosition;
    private bool hasRequestedTargetPosition;
    private Vector3 lastAnimationPosition;

    public PathAgent PathAgent => pathAgent;
    public MinerController CurrentTarget => currentTarget;
    public Vector3 SpawnPosition => spawnPosition;
    public EnemyAnimationController AnimationController => animationController;

    public EnemyIdleState IdleState => idleState;
    public EnemyChaseState ChaseState => chaseState;
    public EnemyAttackState AttackState => attackState;
    public EnemyReturnState ReturnState => returnState;

    private void Awake()
    {
        if (safeZone == null)
        {
            safeZone = FindFirstObjectByType<SafeZone>();
        }

        if (animationController == null)
        {
            animationController = GetComponentInChildren<EnemyAnimationController>();
        }

        spawnPosition = transform.position;
        lastAnimationPosition = transform.position;

        InitializeStates();
    }

    private void Update()
    {
        fsm?.Update();
        UpdateAnimationParameters();
    }

    private void InitializeStates()
    {
        idleState = new EnemyIdleState();
        chaseState = new EnemyChaseState();
        attackState = new EnemyAttackState();
        returnState = new EnemyReturnState();

        idleState.Initialize(this);
        chaseState.Initialize(this);
        attackState.Initialize(this);
        returnState.Initialize(this);

        fsm = new FiniteStateMachine<EnemyController>();
        fsm.SetInitialState(idleState);
    }

    public void ChangeState(EnemyStateBase newState)
    {
        fsm.ChangeState(newState);
    }

    public void NotifyStateEntered(string stateName)
    {
        if (!logStateChanges)
        {
            return;
        }

        Debug.Log($"[{name}] State changed to: {stateName}");
    }

    public MinerController FindNearestTarget()
    {
        MinerController[] miners = FindObjectsByType<MinerController>(FindObjectsSortMode.None);

        MinerController bestTarget = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < miners.Length; i++)
        {
            MinerController miner = miners[i];

            if (miner == null || miner.IsDead)
            {
                continue;
            }

            if (IsTargetInsideSafeZone(miner))
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, miner.transform.position);

            if (distance > detectionRadius)
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTarget = miner;
            }
        }

        return bestTarget;
    }

    public void SetTarget(MinerController target)
    {
        currentTarget = target;
        hasRequestedTargetPosition = false;
    }

    public void ClearTarget()
    {
        currentTarget = null;
        hasRequestedTargetPosition = false;
    }

    public bool IsTargetInAttackRange()
    {
        if (currentTarget == null)
        {
            return false;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
        return distance <= attackRadius;
    }

    public bool IsTargetInsideSafeZone()
    {
        return IsTargetInsideSafeZone(currentTarget);
    }

    public bool IsTargetInsideSafeZone(MinerController target)
    {
        if (target == null)
        {
            return false;
        }

        return target.IsInSafeZone;
    }

    public void ResetRepathTimer()
    {
        repathTimer = 0f;
    }

    public void UpdateChasePath()
    {
        if (currentTarget == null)
        {
            return;
        }

        repathTimer += Time.deltaTime;

        if (repathTimer < repathInterval)
        {
            return;
        }

        Vector3 currentTargetPosition = currentTarget.transform.position;
        bool shouldRequestPath = !hasRequestedTargetPosition;

        if (!shouldRequestPath)
        {
            float movementSinceLastRequest = Vector3.Distance(lastRequestedTargetPosition, currentTargetPosition);
            shouldRequestPath = movementSinceLastRequest >= minTargetMovementBeforeRepath;
        }

        if (!shouldRequestPath && !pathAgent.HasDestination)
        {
            shouldRequestPath = true;
        }

        if (!shouldRequestPath)
        {
            return;
        }

        repathTimer = 0f;
        lastRequestedTargetPosition = currentTargetPosition;
        hasRequestedTargetPosition = true;
        pathAgent.SetDestination(currentTargetPosition);
    }

    public void GoToSpawn()
    {
        pathAgent.SetDestination(spawnPosition);
        ResetRepathTimer();
        hasRequestedTargetPosition = false;
    }

    public void UpdateReturnPath()
    {
        repathTimer += Time.deltaTime;

        if (repathTimer < repathInterval)
        {
            return;
        }

        repathTimer = 0f;
        pathAgent.SetDestination(spawnPosition);
    }

    public bool HasReachedSpawn()
    {
        float distanceToSpawn = Vector3.Distance(transform.position, spawnPosition);
        return distanceToSpawn <= interactionDistance || pathAgent.HasReachedDestination;
    }

    public void ResetAttackTimer()
    {
        attackTimer = 0f;
    }

    public void UpdateAttack()
    {
        if (currentTarget == null)
        {
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
        {
            return;
        }

        attackTimer = 0f;
        animationController?.TriggerAttack();
        currentTarget.TakeDamage(damagePerHit, this);
    }

    private void UpdateAnimationParameters()
    {
        if (animationController == null)
        {
            return;
        }

        float speed = Vector3.Distance(transform.position, lastAnimationPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastAnimationPosition = transform.position;

        animationController.SetSpeed(speed);
    }
}