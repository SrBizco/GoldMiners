using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GoldUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BaseStorage baseStorage;
    [SerializeField] private MinerController[] miners;

    [Header("Top Bar")]
    [SerializeField] private Button totalGoldButton;
    [SerializeField] private TMP_Text totalGoldText;

    [Header("Details Panel")]
    [SerializeField] private GameObject minersDetailsPanel;
    [SerializeField] private TMP_Text minersGoldText;

    [Header("Labels")]
    [SerializeField] private string minersGoldTitle = "Miners";

    private readonly StringBuilder stringBuilder = new StringBuilder();

    private void Start()
    {
        if (totalGoldButton != null)
        {
            totalGoldButton.onClick.AddListener(ToggleDetailsPanel);
        }

        if (minersDetailsPanel != null)
        {
            minersDetailsPanel.SetActive(false);
        }

        RefreshUI();
    }

    private void Update()
    {
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (totalGoldButton != null)
        {
            totalGoldButton.onClick.RemoveListener(ToggleDetailsPanel);
        }
    }

    private void RefreshUI()
    {
        RefreshTotalGoldText();
        RefreshMinersGoldText();
    }

    private void RefreshTotalGoldText()
    {
        if (totalGoldText == null)
        {
            return;
        }

        int totalGold = baseStorage != null ? baseStorage.TotalStoredGold : 0;
        totalGoldText.text = totalGold.ToString();
    }

    private void RefreshMinersGoldText()
    {
        if (minersGoldText == null)
        {
            return;
        }

        stringBuilder.Clear();
        stringBuilder.AppendLine(minersGoldTitle);

        if (miners == null || miners.Length == 0)
        {
            stringBuilder.Append("No miners assigned.");
            minersGoldText.text = stringBuilder.ToString();
            return;
        }

        for (int i = 0; i < miners.Length; i++)
        {
            MinerController miner = miners[i];

            if (miner == null)
            {
                continue;
            }

            stringBuilder.Append(miner.DisplayName);
            stringBuilder.Append(": ");
            stringBuilder.Append(miner.CarriedGold);
            stringBuilder.Append(" / ");
            stringBuilder.Append(miner.CarryCapacity);

            if (i < miners.Length - 1)
            {
                stringBuilder.AppendLine();
            }
        }

        minersGoldText.text = stringBuilder.ToString();
    }

    public void ToggleDetailsPanel()
    {
        if (minersDetailsPanel == null)
        {
            return;
        }

        minersDetailsPanel.SetActive(!minersDetailsPanel.activeSelf);
    }

    public void ShowDetailsPanel()
    {
        if (minersDetailsPanel == null)
        {
            return;
        }

        minersDetailsPanel.SetActive(true);
    }

    public void HideDetailsPanel()
    {
        if (minersDetailsPanel == null)
        {
            return;
        }

        minersDetailsPanel.SetActive(false);
    }
}