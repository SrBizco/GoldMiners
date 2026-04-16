using System.Collections.Generic;
using UnityEngine;

public class GoldVeinManager : MonoBehaviour
{
    private static GoldVeinManager instance;

    [SerializeField] private bool logRegistration = false;

    private readonly List<GoldVein> registeredVeins = new List<GoldVein>();

    public static GoldVeinManager Instance => instance;
    public IReadOnlyList<GoldVein> RegisteredVeins => registeredVeins;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[GoldVeinManager] More than one instance detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void RegisterVein(GoldVein vein)
    {
        if (vein == null)
        {
            return;
        }

        if (registeredVeins.Contains(vein))
        {
            return;
        }

        registeredVeins.Add(vein);

        if (logRegistration)
        {
            Debug.Log($"[GoldVeinManager] Registered vein: {vein.name}");
        }
    }

    public void UnregisterVein(GoldVein vein)
    {
        if (vein == null)
        {
            return;
        }

        if (registeredVeins.Remove(vein) && logRegistration)
        {
            Debug.Log($"[GoldVeinManager] Unregistered vein: {vein.name}");
        }
    }

    public GoldVein FindBestAvailableVein(MinerController miner, Vector3 requesterPosition)
    {
        GoldVein bestVein = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < registeredVeins.Count; i++)
        {
            GoldVein vein = registeredVeins[i];

            if (vein == null)
            {
                continue;
            }

            if (!vein.IsAvailableFor(miner))
            {
                continue;
            }

            if (!vein.HasGold)
            {
                continue;
            }

            float distance = Vector3.Distance(requesterPosition, vein.transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestVein = vein;
            }
        }

        return bestVein;
    }
}