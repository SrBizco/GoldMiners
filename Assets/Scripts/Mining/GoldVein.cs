using UnityEngine;

public class GoldVein : MonoBehaviour
{
    [Header("Gold Settings")]
    [SerializeField] private int currentGoldAmount = 20;

    private MinerController reservedBy;
    private GoldVeinManager cachedManager;
    private bool isRegistered;

    public int CurrentGoldAmount => currentGoldAmount;
    public bool HasGold => currentGoldAmount > 0;
    public bool IsReserved => reservedBy != null;
    public MinerController ReservedBy => reservedBy;

    private void OnEnable()
    {
        TryRegister();
    }

    private void Start()
    {
        TryRegister();
    }

    private void OnDisable()
    {
        Unregister();
        reservedBy = null;
    }

    private void OnDestroy()
    {
        Unregister();
    }

    private void TryRegister()
    {
        if (isRegistered)
        {
            return;
        }

        if (cachedManager == null)
        {
            cachedManager = GoldVeinManager.Instance;

            if (cachedManager == null)
            {
                cachedManager = FindFirstObjectByType<GoldVeinManager>();
            }
        }

        if (cachedManager == null)
        {
            Debug.LogWarning($"[GoldVein] No GoldVeinManager found for vein: {name}");
            return;
        }

        cachedManager.RegisterVein(this);
        isRegistered = true;
    }

    private void Unregister()
    {
        if (!isRegistered)
        {
            return;
        }

        if (cachedManager == null)
        {
            cachedManager = GoldVeinManager.Instance;

            if (cachedManager == null)
            {
                cachedManager = FindFirstObjectByType<GoldVeinManager>();
            }
        }

        if (cachedManager != null)
        {
            cachedManager.UnregisterVein(this);
        }

        isRegistered = false;
    }

    public bool IsAvailableFor(MinerController miner)
    {
        if (!HasGold)
        {
            return false;
        }

        return reservedBy == null || reservedBy == miner;
    }

    public bool TryReserve(MinerController miner)
    {
        if (miner == null)
        {
            return false;
        }

        if (!HasGold)
        {
            return false;
        }

        if (reservedBy != null && reservedBy != miner)
        {
            return false;
        }

        reservedBy = miner;
        return true;
    }

    public void ReleaseReservation(MinerController miner)
    {
        if (reservedBy == miner)
        {
            reservedBy = null;
        }
    }

    public int ExtractGold(int requestedAmount)
    {
        if (requestedAmount <= 0 || currentGoldAmount <= 0)
        {
            return 0;
        }

        int extractedAmount = Mathf.Min(requestedAmount, currentGoldAmount);
        currentGoldAmount -= extractedAmount;

        if (currentGoldAmount <= 0)
        {
            currentGoldAmount = 0;
            Destroy(gameObject);
        }

        return extractedAmount;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = HasGold ? Color.yellow : Color.gray;
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.35f);
    }
}