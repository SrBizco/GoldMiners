using UnityEngine;

public class BaseStorage : MonoBehaviour
{
    [SerializeField] private int totalStoredGold;

    public int TotalStoredGold => totalStoredGold;

    public void DepositGold(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        totalStoredGold += amount;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(transform.position + Vector3.up * 0.5f, new Vector3(0.8f, 0.8f, 0.8f));
    }
}