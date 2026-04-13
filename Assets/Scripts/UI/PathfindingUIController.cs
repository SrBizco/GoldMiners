using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PathfindingUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PathfindingManager pathfindingManager;
    [SerializeField] private Button depthFirstButton;
    [SerializeField] private Button breadthFirstButton;
    [SerializeField] private Button dijkstraButton;
    [SerializeField] private Button aStarButton;
    [SerializeField] private TMP_Text currentStrategyText;

    private void Start()
    {
        if (depthFirstButton != null)
        {
            depthFirstButton.onClick.AddListener(SetDepthFirst);
        }

        if (breadthFirstButton != null)
        {
            breadthFirstButton.onClick.AddListener(SetBreadthFirst);
        }

        if (dijkstraButton != null)
        {
            dijkstraButton.onClick.AddListener(SetDijkstra);
        }

        if (aStarButton != null)
        {
            aStarButton.onClick.AddListener(SetAStar);
        }

        RefreshStrategyLabel();
    }

    private void OnDestroy()
    {
        if (depthFirstButton != null)
        {
            depthFirstButton.onClick.RemoveListener(SetDepthFirst);
        }

        if (breadthFirstButton != null)
        {
            breadthFirstButton.onClick.RemoveListener(SetBreadthFirst);
        }

        if (dijkstraButton != null)
        {
            dijkstraButton.onClick.RemoveListener(SetDijkstra);
        }

        if (aStarButton != null)
        {
            aStarButton.onClick.RemoveListener(SetAStar);
        }
    }

    public void SetDepthFirst()
    {
        if (pathfindingManager == null)
        {
            return;
        }

        pathfindingManager.SetDepthFirstStrategy();
        RefreshStrategyLabel();
    }

    public void SetBreadthFirst()
    {
        if (pathfindingManager == null)
        {
            return;
        }

        pathfindingManager.SetBreadthFirstStrategy();
        RefreshStrategyLabel();
    }

    public void SetDijkstra()
    {
        if (pathfindingManager == null)
        {
            return;
        }

        pathfindingManager.SetDijkstraStrategy();
        RefreshStrategyLabel();
    }

    public void SetAStar()
    {
        if (pathfindingManager == null)
        {
            return;
        }

        pathfindingManager.SetAStarStrategy();
        RefreshStrategyLabel();
    }

    private void RefreshStrategyLabel()
    {
        if (currentStrategyText == null || pathfindingManager == null)
        {
            return;
        }

        currentStrategyText.text = $"Current Strategy: {pathfindingManager.CurrentStrategy}";
    }
}