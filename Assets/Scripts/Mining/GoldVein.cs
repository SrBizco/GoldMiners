using UnityEngine;

public class GoldVein : MonoBehaviour
{
    [Header("Gold Settings")]
    [SerializeField] private int currentGoldAmount = 20;

    private MinerController reservedBy;

    public int CurrentGoldAmount => currentGoldAmount;
    public bool HasGold => currentGoldAmount > 0;
    public bool IsReserved => reservedBy != null;
    public MinerController ReservedBy => reservedBy;

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

        return extractedAmount;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = HasGold ? Color.yellow : Color.gray;
        Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.35f);
    }
}