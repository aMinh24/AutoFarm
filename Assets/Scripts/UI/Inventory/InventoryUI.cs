using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : BasePopup
{
    [Header("UI References")]
    public InventoryFrameUI inventoryFrame;
    public InventoryDetailUI detailUI;
    public Button sellButton;
    public Button closeButton;
    
    private ItemDefinition selectedItem;
    private int selectedAmount;
    private int sellQuantity = 1;
    private bool isFarmingMode = false;
    
    public override void Init()
    {
        base.Init();
    }

    public override void Show(object data)
    {
        base.Show(data);
        
        // Check if we're in farming mode
        isFarmingMode = data != null && data.ToString() == "farming";
        SetupButtons();
        SetupFrameEvents();
        // Set farming mode filter on inventory frame
        if (inventoryFrame != null)
        {
            inventoryFrame.SetFarmingModeFilter(isFarmingMode);
        }
        
        // Subscribe to inventory changes
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnInventoryChanged += RefreshInventoryDisplay;
        }
        
        RefreshInventoryDisplay();
        ClearSelection();
    }
    
    private void SetupButtons()
    {
        if (sellButton != null)
        {
            if (isFarmingMode)
                sellButton.onClick.AddListener(UseSelectedItem);
            else
                sellButton.onClick.AddListener(SellSelectedItems);
        }
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }
    
    private void SetupFrameEvents()
    {
        if (inventoryFrame != null)
        {
            inventoryFrame.OnItemSelected += OnItemSelected;
            inventoryFrame.OnItemHovered += OnItemHovered;
        }
    }
    
    private void RefreshInventoryDisplay()
    {
        if (inventoryFrame != null)
        {
            inventoryFrame.RefreshInventory();
        }
    }
    
    private void OnItemSelected(ItemDefinition item, int amount)
    {
        selectedItem = item;
        selectedAmount = amount;
        sellQuantity = amount; // Sell all by default
        UpdateDetailUI(item, amount);
        UpdateSellButtons(item);
    }
    
    private void OnItemHovered(ItemDefinition item, int amount)
    {
        if (selectedItem == null)
        {
            UpdateDetailUI(item, amount);
        }
    }
    
    private void UpdateDetailUI(ItemDefinition item, int amount)
    {
        if (detailUI != null && item != null)
        {
            detailUI.Setup(
                item.itemName,
                item.description,
                item.baseSalePrice,
                amount
            );
        }
    }
    
    private void UpdateSellButtons(ItemDefinition item)
    {
        if (sellButton == null) return;
        
        var buttonText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
        
        if (isFarmingMode)
        {
            // Farming mode - check if item can be used for farming
            bool canUse = CanUseForFarming(item);
            sellButton.interactable = canUse;
            
            if (buttonText != null)
            {
                if (canUse)
                {
                    var entityDef = GameDataManager.Instance?.GetEntity(item.growsIntoEntityID);
                    if (entityDef != null)
                    {
                        buttonText.text = $"Use ({entityDef.quantityPerPlot})";
                    }
                    else
                    {
                        buttonText.text = "Use";
                    }
                }
                else
                {
                    buttonText.text = "Cannot Use";
                }
            }
        }
        else
        {
            // Normal mode - selling
            bool canSell = item != null && item.baseSalePrice > 0;
            sellButton.interactable = canSell;
            
            if (buttonText != null)
            {
                buttonText.text = canSell ? "Sell All" : "Cannot Sell";
            }
        }
    }
    
    private bool CanUseForFarming(ItemDefinition item)
    {
        if (item == null) return false;
        
        // Check if item is a seed or animal that can be placed
        if (!((item.itemType == ItemType.Seed && item.growsIntoEntityID != EntityID.None) ||
              (item.itemType == ItemType.Animal && item.growsIntoEntityID != EntityID.None)))
        {
            return false;
        }
        
        // Check if player has enough quantity
        var entityDef = GameDataManager.Instance?.GetEntity(item.growsIntoEntityID);
        if (entityDef != null)
        {
            return GameDataManager.Instance.HasPlayerItem(item.itemID, entityDef.quantityPerPlot);
        }
        
        return false;
    }
    
    private void SellSelectedItems()
    {
        if (selectedItem == null || selectedItem.baseSalePrice <= 0) return;
        
        // Use all available quantity
        sellQuantity = selectedAmount;
        
        // Check if player has enough items using GameDataManager
        if (!GameDataManager.Instance.HasPlayerItem(selectedItem.itemID, sellQuantity)) return;
        
        // Calculate total sell value
        int totalValue = selectedItem.baseSalePrice * sellQuantity;
        
        // Remove items and add gold using GameDataManager methods
        if (GameDataManager.Instance.RemovePlayerItem(selectedItem.itemID, sellQuantity))
        {
            GameDataManager.Instance.AddPlayerGold(totalValue);
            
            // Save changes
            GameDataManager.Instance.SaveGame();
            
            Debug.Log($"Sold {sellQuantity} {selectedItem.itemName} for ${totalValue}");
            // Refresh display
            RefreshInventoryDisplay();
            ClearSelection();
            
        }
    }
    
    private void UseSelectedItem()
    {
        if (selectedItem == null || !CanUseForFarming(selectedItem)) return;
        
        var entityDef = GameDataManager.Instance?.GetEntity(selectedItem.growsIntoEntityID);
        if (entityDef == null) return;
        
        int requiredQuantity = entityDef.quantityPerPlot;
        
        // Check if player has enough items
        if (!GameDataManager.Instance.HasPlayerItem(selectedItem.itemID, requiredQuantity)) return;
        
        // Try to place entities on current plot
        if (PlotManager.Instance != null)
        {
            var plotInfo = PlotManager.Instance.GetCurrentPlotInfo();
            if (plotInfo.isEmpty)
            {
                bool allPlaced = true;
                
                // Place entities based on required quantity
                for (int i = 0; i < requiredQuantity; i++)
                {
                    if (!PlotManager.Instance.AddEntityToCurrentPlot(selectedItem.growsIntoEntityID, i))
                    {
                        allPlaced = false;
                        // Remove any entities that were already placed if failed
                        for (int j = 0; j < i; j++)
                        {
                            PlotManager.Instance.RemoveEntityFromCurrentPlot(j);
                        }
                        break;
                    }
                }
                
                if (allPlaced)
                {
                    // Remove items from inventory
                    GameDataManager.Instance.RemovePlayerItem(selectedItem.itemID, requiredQuantity);
                    
                    // Save changes
                    GameDataManager.Instance.SaveGame();
                    
                    Debug.Log($"Placed {requiredQuantity} {selectedItem.itemName} on current plot");
                    
                    // Close inventory after successful use
                    CloseShop();
                }
                else
                {
                    Debug.LogWarning("Failed to place all entities on plot");
                }
            }
            else
            {
                Debug.LogWarning("Plot is not empty");
            }
        }
    }
    
    private void ClearSelection()
    {
        selectedItem = null;
        selectedAmount = 0;
        sellQuantity = 1;
        
        if (detailUI != null)
            detailUI.Clear();
            
        UpdateSellButtons(null);
    }
    
    public void CloseShop()
    {
        this.Hide();
    }
    public override void Hide()
    {
        base.Hide();
        ClearSelection();
        
        // Reset farming mode filter
        if (inventoryFrame != null)
        {
            inventoryFrame.SetFarmingModeFilter(false);
        }
        
        // Properly unsubscribe from events
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnInventoryChanged -= RefreshInventoryDisplay;
        }
        
        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
        }
            
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        if (inventoryFrame != null)
        {
            inventoryFrame.OnItemSelected -= OnItemSelected;
            inventoryFrame.OnItemHovered -= OnItemHovered;
        }
    }
}