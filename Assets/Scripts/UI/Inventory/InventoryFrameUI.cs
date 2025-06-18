using System.Collections.Generic;
using UnityEngine;

public class InventoryFrameUI : MonoBehaviour
{
    [Header("Slot Configuration")]
    public InventorySlotUI slotPrefab;
    public Transform slotContainer;
    public int maxSlots = 20;
    
    private List<InventorySlotUI> slots = new List<InventorySlotUI>();
    private InventorySlotUI currentSelectedSlot;
    private bool farmingModeFilter = false;
    
    // Events
    public System.Action<ItemDefinition, int> OnItemSelected;
    public System.Action<ItemDefinition, int> OnItemHovered;
    
    private void Awake()
    {
        InitializeSlots();
    }
    
    private void Start()
    {
        RefreshInventory();
    }
    
    private void InitializeSlots()
    {
        if (slotPrefab == null || slotContainer == null) return;
        
        // Clear existing slots
        ClearSlots();
        
        // Create new slots
        for (int i = 0; i < maxSlots; i++)
        {
            CreateSlot(i);
        }
    }
    
    private void CreateSlot(int index)
    {
        InventorySlotUI newSlot = Instantiate(slotPrefab, slotContainer);
        newSlot.name = $"InventorySlot_{index:00}";
        
        // Subscribe to slot events
        newSlot.OnSlotClicked += OnSlotClicked;
        newSlot.OnSlotHovered += OnSlotHovered;
        
        slots.Add(newSlot);
    }
    
    public void RefreshInventory()
    {
        var playerData = GameDataManager.Instance?.PlayerData?.GetPlayerData();
        var itemDefinitions = GameDataManager.Instance?.itemDefinitions;
        
        if (playerData != null && itemDefinitions != null)
        {
            // Ensure inventory dictionary is synchronized
            playerData.PrepareAfterLoad();
            PopulateWithInventoryItems(playerData.Inventory, itemDefinitions);
        }
    }
    
    public void SetFarmingModeFilter(bool enabled)
    {
        farmingModeFilter = enabled;
        RefreshInventory();
    }
    
    private void PopulateWithInventoryItems(Dictionary<ItemID, int> inventory, ItemDefinitionCollection itemDefinitions)
    {
        ClearAllSlots();
        
        int slotIndex = 0;
        foreach (var kvp in inventory)
        {
            if (slotIndex >= slots.Count) break;
            
            var itemDef = itemDefinitions.GetItemDefinition(kvp.Key);
            if (itemDef != null && kvp.Value > 0)
            {
                // Filter items if in farming mode
                if (farmingModeFilter && !CanUseForFarming(itemDef))
                    continue;
                    
                slots[slotIndex].Setup(itemDef, kvp.Value);
                slotIndex++;
            }
        }
    }
    
    private bool CanUseForFarming(ItemDefinition item)
    {
        if (item == null) return false;
        
        // Check if item is a seed or animal that can be placed
        return (item.itemType == ItemType.Seed && item.growsIntoEntityID != EntityID.None) ||
               (item.itemType == ItemType.Animal && item.growsIntoEntityID != EntityID.None);
    }
    
    private void OnSlotClicked(InventorySlotUI slot, ItemDefinition item, int amount)
    {
        // Deselect previous slot
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
        }
        
        // Select new slot
        currentSelectedSlot = slot;
        slot.SetSelected(true);
        
        OnItemSelected?.Invoke(item, amount);
    }
    
    private void OnSlotHovered(InventorySlotUI slot, ItemDefinition item, int amount)
    {
        OnItemHovered?.Invoke(item, amount);
    }
    
    public void ClearAllSlots()
    {
        foreach (InventorySlotUI slot in slots)
        {
            slot.Setup(null, 0);
        }
        
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
            currentSelectedSlot = null;
        }
    }
    
    private void ClearSlots()
    {
        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null)
            {
                slot.OnSlotClicked -= OnSlotClicked;
                slot.OnSlotHovered -= OnSlotHovered;
                DestroyImmediate(slot.gameObject);
            }
        }
        slots.Clear();
    }
    
    public void OnInventoryChanged()
    {
        RefreshInventory();
    }
    
    private void OnDestroy()
    {
        ClearSlots();
    }
}
