using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : BasePopup
{
    [Header("UI References")]
    public FrameUI shopFrame;
    public DetailUI detailUI;
    public Button purchaseButton;
    public Button closeButton;
    
    private StoreItemCollection storeItemCollection;
    
    private StoreItemDefinition selectedItem;
    public override void Init()
    {
        base.Init();
        SetupButtons();
        SetupFrameEvents();
    }
    public override void Show(object data)
    {
        base.Show(data);
        storeItemCollection = GameDataManager.Instance?.storeItems;
        ShowAllItems();
    }
    
    private void SetupButtons()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(PurchaseSelectedItem);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
    }
    
    private void SetupFrameEvents()
    {
        if (shopFrame != null)
        {
            shopFrame.OnItemSelected += OnItemSelected;
            shopFrame.OnItemHovered += OnItemHovered;
        }
    }
    
    private void ShowAllItems()
    {
        if (storeItemCollection == null) return;
        
        shopFrame.PopulateWithItems(storeItemCollection.storeItems);
    }
    
    private void OnItemSelected(StoreItemDefinition item)
    {
        selectedItem = item;
        UpdateDetailUI(item);
        UpdatePurchaseButton(item);
    }
    
    private void OnItemHovered(StoreItemDefinition item)
    {
        if (selectedItem == null)
        {
            UpdateDetailUI(item);
        }
    }
    
    private void UpdateDetailUI(StoreItemDefinition item)
    {
        if (detailUI != null && item != null)
        {
            detailUI.Setup(
                item.displayName,
                item.description,
                $"${item.price}"
            );
        }
    }
    
    private void UpdatePurchaseButton(StoreItemDefinition item)
    {
        if (purchaseButton != null)
        {
            bool canAfford = CanAffordItem(item);
            purchaseButton.interactable = canAfford;
            
            var buttonText = purchaseButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = canAfford ? "Purchase" : "Not Enough Gold";
            }
        }
    }
    
    private bool CanAffordItem(StoreItemDefinition item)
    {
        if (item == null) return false;
        
        return GameDataManager.Instance?.GetPlayerGold() >= item.price;
    }
    
    private void PurchaseSelectedItem()
    {
        if (selectedItem == null || !CanAffordItem(selectedItem)) return;
        
        // Use GameDataManager methods that trigger events
        if (GameDataManager.Instance.SpendPlayerGold(selectedItem.price))
        {
            // Add item to inventory based on type
            ProcessPurchase(selectedItem);
            
            // Refresh the shop display
            shopFrame.UpdateSlotAvailability();
            UpdatePurchaseButton(selectedItem);
            
            Debug.Log($"Purchased {selectedItem.displayName} for ${selectedItem.price}");
        }
    }
    
    private void ProcessPurchase(StoreItemDefinition item)
    {
        switch (item.purchaseType)
        {
            case PurchaseType.Seed:
            case PurchaseType.Animal:
                if (item.referencedItemID != ItemID.None)
                {
                    Debug.Log($"Adding {item.amount} {item.referencedItemID} to inventory");
                    GameDataManager.Instance.AddPlayerItem(item.referencedItemID, item.amount);
                }
                break;
                
            case PurchaseType.Worker:
                GameDataManager.Instance.AddNewWorkerToPlayer();
                break;
                
            case PurchaseType.Land:
                GameDataManager.Instance.AddNewPlot();
                // Refresh plot manager to recognize new plot
                PlotManager.Instance?.RefreshPlotsFromGameData();
                break;
                
            case PurchaseType.EquipmentUpgrade:
                // Update equipment level through GameDataManager
                GameDataManager.Instance.UpgradePlayerEquipment();
                break;
        }
        
        // Save the changes
        GameDataManager.Instance.SaveGame();
    }
    
    public void OpenShop()
    {
        gameObject.SetActive(true);
        ShowAllItems();
    }
    
    public void CloseShop()
    {
        this.Hide();
        selectedItem = null;
    }
    
    private void OnDestroy()
    {
        if (shopFrame != null)
        {
            shopFrame.OnItemSelected -= OnItemSelected;
            shopFrame.OnItemHovered -= OnItemHovered;
        }
    }
}
