using UnityEngine;

public class MinerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PathAgent pathAgent;
    [SerializeField] private BaseStorage baseStorage;
    [SerializeField] private GoldVeinManager goldVeinManager;

    [Header("Mining Settings")]
    [SerializeField] private int carryCapacity = 5;
    [SerializeField] private float interactionDistance = 1.2f;
    [SerializeField] private float miningInterval = 1f;
    [SerializeField] private float depositInterval = 0.5f;

    [Header("Threat")]
    [SerializeField] private float safeDuration = 3f;

    [Header("Health")]
    [SerializeField] private int maxHealth = 10;

    [Header("UI")]
    [SerializeField] private string displayName = "Miner";

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;

    private FiniteStateMachine<MinerController> fsm;

    private MinerIdleState idleState;
    private MinerGoToMineState goToMineState;
    private MinerMiningState miningState;
    private MinerReturnToBaseState returnToBaseState;
    private MinerDepositingState depositingState;
    private MinerFleeState fleeState;
    private MinerSafeState safeState;
    private MinerDeadState deadState;

    private GoldVein currentTargetVein;
    private int carriedGold;
    private float actionTimer;
    private int currentHealth;
    private bool isDead;
    private bool isInSafeZone;
    private EnemyController currentThreat;

    public PathAgent PathAgent => pathAgent;
    public BaseStorage BaseStorage => baseStorage;
    public GoldVein CurrentTargetVein => currentTargetVein;

    public int CarriedGold => carriedGold;
    public int CarryCapacity => carryCapacity;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
    public bool IsInSafeZone => isInSafeZone;
    public EnemyController CurrentThreat => currentThreat;

    public bool HasCarriedGold => carriedGold > 0;
    public bool IsCarryFull => carriedGold >= carryCapacity;

    public MinerIdleState IdleState => idleState;
    public MinerGoToMineState GoToMineState => goToMineState;
    public MinerMiningState MiningState => miningState;
    public MinerReturnToBaseState ReturnToBaseState => returnToBaseState;
    public MinerDepositingState DepositingState => depositingState;
    public MinerFleeState FleeState => fleeState;
    public MinerSafeState SafeState => safeState;
    public MinerDeadState DeadState => deadState;

    private void Awake()
    {
        if (goldVeinManager == null)
        {
            goldVeinManager = GoldVeinManager.Instance;

            if (goldVeinManager == null)
            {
                goldVeinManager = FindFirstObjectByType<GoldVeinManager>();
            }
        }

        currentHealth = maxHealth;
        InitializeStates();
    }

    private void Update()
    {
        fsm?.Update();
    }

    private void OnDisable()
    {
        ReleaseReservationIfNeeded();
    }

    private void OnDestroy()
    {
        ReleaseReservationIfNeeded();
    }

    private void InitializeStates()
    {
        idleState = new MinerIdleState();
        goToMineState = new MinerGoToMineState();
        miningState = new MinerMiningState();
        returnToBaseState = new MinerReturnToBaseState();
        depositingState = new MinerDepositingState();
        fleeState = new MinerFleeState();
        safeState = new MinerSafeState();
        deadState = new MinerDeadState();

        idleState.Initialize(this);
        goToMineState.Initialize(this);
        miningState.Initialize(this);
        returnToBaseState.Initialize(this);
        depositingState.Initialize(this);
        fleeState.Initialize(this);
        safeState.Initialize(this);
        deadState.Initialize(this);

        fsm = new FiniteStateMachine<MinerController>();
        fsm.SetInitialState(idleState);
    }

    public void ChangeState(MinerStateBase newState)
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

    public void SetSafeZoneStatus(bool value)
    {
        isInSafeZone = value;
    }

    public bool HasActiveThreat()
    {
        if (isDead)
        {
            return false;
        }

        return currentThreat != null;
    }

    public bool ShouldFlee()
    {
        if (isDead || isInSafeZone)
        {
            return false;
        }

        return HasActiveThreat();
    }

    public void ClearThreat()
    {
        currentThreat = null;
    }

    public bool HasReachedSafeDuration()
    {
        return actionTimer >= safeDuration;
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, null);
    }

    public void TakeDamage(int damage, EnemyController attacker)
    {
        if (isDead || isInSafeZone || damage <= 0)
        {
            return;
        }

        if (attacker != null)
        {
            currentThreat = attacker;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            ChangeState(deadState);
            return;
        }

        if (!(fleeState.Equals(null)) && !(safeState.Equals(null)))
        {
            if (fsm != null && !ReferenceEquals(fleeState, deadState))
            {
                ChangeState(fleeState);
            }
        }
    }

    public void HandleDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        ReleaseReservationIfNeeded();
        ClearThreat();

        if (pathAgent != null)
        {
            pathAgent.StopMovement();
        }
    }

    public GoldVein FindBestAvailableVein()
    {
        if (isDead)
        {
            return null;
        }

        if (goldVeinManager == null)
        {
            goldVeinManager = GoldVeinManager.Instance;

            if (goldVeinManager == null)
            {
                goldVeinManager = FindFirstObjectByType<GoldVeinManager>();
            }
        }

        if (goldVeinManager == null)
        {
            Debug.LogWarning($"[{name}] No GoldVeinManager assigned or found in scene.");
            return null;
        }

        return goldVeinManager.FindBestAvailableVein(this, transform.position);
    }

    public void SetCurrentTargetVein(GoldVein vein)
    {
        currentTargetVein = vein;
    }

    public void ClearCurrentTargetVein()
    {
        if (currentTargetVein != null)
        {
            currentTargetVein.ReleaseReservation(this);
            currentTargetVein = null;
        }
    }

    public bool HasReachedCurrentVein()
    {
        if (currentTargetVein == null)
        {
            return false;
        }

        float distanceToVein = Vector3.Distance(transform.position, currentTargetVein.transform.position);
        return distanceToVein <= interactionDistance || pathAgent.HasReachedDestination;
    }

    public bool HasReachedBase()
    {
        if (baseStorage == null)
        {
            return false;
        }

        float distanceToBase = Vector3.Distance(transform.position, baseStorage.transform.position);
        return distanceToBase <= interactionDistance || pathAgent.HasReachedDestination;
    }

    public void GoToBase()
    {
        if (isDead || baseStorage == null)
        {
            return;
        }

        pathAgent.SetDestination(baseStorage.transform.position);
    }

    public void TryMineOneUnit()
    {
        if (isDead || currentTargetVein == null)
        {
            return;
        }

        if (IsCarryFull)
        {
            return;
        }

        int missingCapacity = carryCapacity - carriedGold;
        int extractedAmount = currentTargetVein.ExtractGold(Mathf.Min(1, missingCapacity));

        if (extractedAmount > 0)
        {
            carriedGold += extractedAmount;
        }
    }

    public void DepositOneUnitToBase()
    {
        if (isDead || baseStorage == null)
        {
            return;
        }

        if (carriedGold <= 0)
        {
            return;
        }

        carriedGold -= 1;
        baseStorage.DepositGold(1);

        if (carriedGold < 0)
        {
            carriedGold = 0;
        }
    }

    public void AddToActionTimer()
    {
        actionTimer += Time.deltaTime;
    }

    public void ResetActionTimer()
    {
        actionTimer = 0f;
    }

    public bool HasReachedMiningInterval()
    {
        return actionTimer >= miningInterval;
    }

    public bool HasReachedDepositInterval()
    {
        return actionTimer >= depositInterval;
    }

    private void ReleaseReservationIfNeeded()
    {
        if (currentTargetVein != null)
        {
            currentTargetVein.ReleaseReservation(this);
            currentTargetVein = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (currentTargetVein != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(
                transform.position + Vector3.up * 0.4f,
                currentTargetVein.transform.position + Vector3.up * 0.4f
            );
        }
    }
}