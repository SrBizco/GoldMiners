using UnityEngine;

public class MinerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PathAgent pathAgent;
    [SerializeField] private BaseStorage baseStorage;
    [SerializeField] private GoldVein[] goldVeins;

    [Header("Mining Settings")]
    [SerializeField] private int carryCapacity = 5;
    [SerializeField] private float interactionDistance = 1.2f;
    [SerializeField] private float miningInterval = 1f;
    [SerializeField] private float depositInterval = 0.5f;

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

    private GoldVein currentTargetVein;
    private int carriedGold;
    private float actionTimer;

    public PathAgent PathAgent => pathAgent;
    public BaseStorage BaseStorage => baseStorage;
    public GoldVein CurrentTargetVein => currentTargetVein;

    public int CarriedGold => carriedGold;
    public int CarryCapacity => carryCapacity;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

    public bool HasCarriedGold => carriedGold > 0;
    public bool IsCarryFull => carriedGold >= carryCapacity;

    public MinerIdleState IdleState => idleState;
    public MinerGoToMineState GoToMineState => goToMineState;
    public MinerMiningState MiningState => miningState;
    public MinerReturnToBaseState ReturnToBaseState => returnToBaseState;
    public MinerDepositingState DepositingState => depositingState;

    private void Awake()
    {
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

        idleState.Initialize(this);
        goToMineState.Initialize(this);
        miningState.Initialize(this);
        returnToBaseState.Initialize(this);
        depositingState.Initialize(this);

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

    public GoldVein FindBestAvailableVein()
    {
        GoldVein bestVein = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < goldVeins.Length; i++)
        {
            GoldVein vein = goldVeins[i];

            if (vein == null)
            {
                continue;
            }

            if (!vein.IsAvailableFor(this))
            {
                continue;
            }

            if (!vein.HasGold)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, vein.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestVein = vein;
            }
        }

        return bestVein;
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
        if (baseStorage == null)
        {
            return;
        }

        pathAgent.SetDestination(baseStorage.transform.position);
    }

    public void TryMineOneUnit()
    {
        if (currentTargetVein == null)
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
        if (baseStorage == null)
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