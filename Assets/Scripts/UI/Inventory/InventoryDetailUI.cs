using AutoFarm.Utilities;
using TMPro;
using UnityEngine;

public class InventoryDetailUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI nameItem;
    public TextMeshProUGUI descriptionItem;
    public TextMeshProUGUI sellPriceItem;
    public TextMeshProUGUI totalValueItem;
    
    public void Setup(string name, string description, int sellPrice, int quantity)
    {
        if (nameItem != null)
        {
            nameItem.text = name;
        }

        if (descriptionItem != null)
        {
            descriptionItem.text = description;
        }

        if (sellPriceItem != null)
        {
            sellPriceItem.text = sellPrice > 0 ? $"Sell Price: {FormatUtilities.FormatCurrency(sellPrice)}" : "Cannot Sell";
        }
        
        
        if (totalValueItem != null)
        {
            int totalValue = sellPrice * quantity;
            totalValueItem.text = totalValue > 0 ? $"Total Value: {FormatUtilities.FormatCurrency(totalValue)}" : "No Value";
        }
    }
    
    public void Clear()
    {
        if (nameItem != null) nameItem.text = "";
        if (descriptionItem != null) descriptionItem.text = "";
        if (sellPriceItem != null) sellPriceItem.text = "";
        if (totalValueItem != null) totalValueItem.text = "";
    }
}
