using UnityEngine;

public class MinerController : MonoBehaviour
{
    private enum MinerState
    {
        Idle,
        GoingToMine,
        Mining,
        ReturningToBase,
        Depositing
    }

    [Header("References")]
    [SerializeField] private PathAgent pathAgent;
    [SerializeField] private BaseStorage baseStorage;
    [SerializeField] private GoldVein[] goldVeins;

    [Header("Mining Settings")]
    [SerializeField] private int carryCapacity = 5;
    [SerializeField] private float interactionDistance = 1.2f;
    [SerializeField] private float miningInterval = 1f;
    [SerializeField] private float depositInterval = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;

    private MinerState currentState = MinerState.Idle;
    private GoldVein currentTargetVein;
    private int carriedGold;
    private float stateTimer;

    public int CarriedGold => carriedGold;
    public int CarryCapacity => carryCapacity;
    public GoldVein CurrentTargetVein => currentTargetVein;

    private void Start()
    {
        ChangeState(MinerState.Idle);
    }

    private void Update()
    {
        switch (currentState)
        {
            case MinerState.Idle:
                UpdateIdle();
                break;

            case MinerState.GoingToMine:
                UpdateGoingToMine();
                break;

            case MinerState.Mining:
                UpdateMining();
                break;

            case MinerState.ReturningToBase:
                UpdateReturningToBase();
                break;

            case MinerState.Depositing:
                UpdateDepositing();
                break;
        }
    }

    private void UpdateIdle()
    {
        if (carriedGold >= carryCapacity)
        {
            GoToBase();
            return;
        }

        GoldVein bestVein = FindBestAvailableVein();

        if (bestVein == null)
        {
            return;
        }

        if (!bestVein.TryReserve(this))
        {
            return;
        }

        currentTargetVein = bestVein;
        pathAgent.SetDestination(currentTargetVein.transform.position);
        ChangeState(MinerState.GoingToMine);
    }

    private void UpdateGoingToMine()
    {
        if (currentTargetVein == null)
        {
            ChangeState(MinerState.Idle);
            return;
        }

        if (!currentTargetVein.HasGold)
        {
            currentTargetVein.ReleaseReservation(this);
            currentTargetVein = null;
            ChangeState(MinerState.Idle);
            return;
        }

        float distanceToVein = Vector3.Distance(transform.position, currentTargetVein.transform.position);

        if (distanceToVein <= interactionDistance || pathAgent.HasReachedDestination)
        {
            ChangeState(MinerState.Mining);
        }
    }

    private void UpdateMining()
    {
        if (currentTargetVein == null)
        {
            ChangeState(MinerState.Idle);
            return;
        }

        if (!currentTargetVein.HasGold)
        {
            currentTargetVein.ReleaseReservation(this);
            currentTargetVein = null;

            if (carriedGold > 0)
            {
                GoToBase();
            }
            else
            {
                ChangeState(MinerState.Idle);
            }

            return;
        }

        if (carriedGold >= carryCapacity)
        {
            currentTargetVein.ReleaseReservation(this);
            currentTargetVein = null;
            GoToBase();
            return;
        }

        stateTimer += Time.deltaTime;

        if (stateTimer < miningInterval)
        {
            return;
        }

        stateTimer = 0f;

        int missingCapacity = carryCapacity - carriedGold;
        int extractedAmount = currentTargetVein.ExtractGold(Mathf.Min(1, missingCapacity));

        if (extractedAmount > 0)
        {
            carriedGold += extractedAmount;
        }

        if (carriedGold >= carryCapacity || !currentTargetVein.HasGold)
        {
            currentTargetVein.ReleaseReservation(this);
            currentTargetVein = null;
            GoToBase();
        }
    }

    private void UpdateReturningToBase()
    {
        if (baseStorage == null)
        {
            return;
        }

        float distanceToBase = Vector3.Distance(transform.position, baseStorage.transform.position);

        if (distanceToBase <= interactionDistance || pathAgent.HasReachedDestination)
        {
            ChangeState(MinerState.Depositing);
        }
    }

    private void UpdateDepositing()
    {
        if (carriedGold <= 0)
        {
            ChangeState(MinerState.Idle);
            return;
        }

        stateTimer += Time.deltaTime;

        if (stateTimer < depositInterval)
        {
            return;
        }

        stateTimer = 0f;

        carriedGold -= 1;
        baseStorage.DepositGold(1);

        if (carriedGold <= 0)
        {
            carriedGold = 0;
            ChangeState(MinerState.Idle);
        }
    }

    private GoldVein FindBestAvailableVein()
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

    private void GoToBase()
    {
        if (baseStorage == null)
        {
            return;
        }

        pathAgent.SetDestination(baseStorage.transform.position);
        ChangeState(MinerState.ReturningToBase);
    }

    private void ChangeState(MinerState newState)
    {
        currentState = newState;
        stateTimer = 0f;

        if (logStateChanges)
        {
            Debug.Log($"[{name}] State changed to: {currentState}");
        }
    }

    private void OnDrawGizmos()
    {
        if (currentTargetVein != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.4f, currentTargetVein.transform.position + Vector3.up * 0.4f);
        }
    }
}