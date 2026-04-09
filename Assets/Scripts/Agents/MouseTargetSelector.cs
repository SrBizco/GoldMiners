using UnityEngine;

public class MouseTargetSelector : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private PathAgent pathAgent;
    [SerializeField] private LayerMask clickableLayerMask;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        if (targetCamera == null || pathAgent == null)
        {
            return;
        }

        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 500f, clickableLayerMask))
        {
            pathAgent.SetDestination(hitInfo.point);
        }
    }
}