using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SafeZone : MonoBehaviour
{
    private readonly Dictionary<MinerController, int> minersInsideCounts = new Dictionary<MinerController, int>();
    private Collider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();

        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[SafeZone] OnTriggerEnter from collider: {other.name}");

        MinerController miner = other.GetComponentInParent<MinerController>();

        if (miner == null)
        {
            Debug.Log("[SafeZone] Enter ignored: no MinerController found in parent.");
            return;
        }

        if (!minersInsideCounts.ContainsKey(miner))
        {
            minersInsideCounts[miner] = 0;
        }

        minersInsideCounts[miner]++;
        miner.SetSafeZoneStatus(true);

        Debug.Log($"[SafeZone] Miner entered: {miner.name} | Count: {minersInsideCounts[miner]}");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[SafeZone] OnTriggerExit from collider: {other.name}");

        MinerController miner = other.GetComponentInParent<MinerController>();

        if (miner == null)
        {
            Debug.Log("[SafeZone] Exit ignored: no MinerController found in parent.");
            return;
        }

        if (!minersInsideCounts.ContainsKey(miner))
        {
            Debug.Log($"[SafeZone] Exit ignored: miner {miner.name} was not registered inside.");
            return;
        }

        minersInsideCounts[miner]--;

        if (minersInsideCounts[miner] <= 0)
        {
            minersInsideCounts.Remove(miner);
            miner.SetSafeZoneStatus(false);
            Debug.Log($"[SafeZone] Miner exited completely: {miner.name}");
        }
        else
        {
            Debug.Log($"[SafeZone] Miner partial exit: {miner.name} | Count: {minersInsideCounts[miner]}");
        }
    }
}