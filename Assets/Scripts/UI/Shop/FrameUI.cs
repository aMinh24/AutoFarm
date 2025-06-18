using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AutoFarm.Utilities;

public class FrameUI : MonoBehaviour
{
    [Header("Slot Configuration")]
    public SlotUI slotPrefab;
    public Transform slotContainer;
    public int maxSlots = 20;
    
    private List<SlotUI> slots = new List<SlotUI>();
    private SlotUI currentSelectedSlot;
    
    // Events
    public System.Action<StoreItemDefinition> OnItemSelected;
    public System.Action<StoreItemDefinition> OnItemHovered;
    
    private void Awake()
    {
        InitializeSlots();
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
        SlotUI newSlot = Instantiate(slotPrefab, slotContainer);
        newSlot.name = $"Slot_{index:00}";
        
        // Subscribe to slot events
        newSlot.OnSlotClicked += OnSlotClicked;
        newSlot.OnSlotHovered += OnSlotHovered;
        
        slots.Add(newSlot);
    }
    
    public void PopulateWithItems(List<StoreItemDefinition> items)
    {
        ClearAllSlots();
        
        for (int i = 0; i < items.Count && i < slots.Count; i++)
        {
            if (items[i] != null)
            {
                bool isAvailable = CanAffordItem(items[i]);
                slots[i].Setup(items[i], isAvailable);
            }
        }
    }
    
    public void PopulateWithInventoryItems(Dictionary<ItemID, int> inventory, ItemDefinitionCollection itemDefinitions)
    {
        ClearAllSlots();
        
        int slotIndex = 0;
        foreach (var kvp in inventory)
        {
            if (slotIndex >= slots.Count) break;
            
            var itemDef = itemDefinitions.GetItemDefinition(kvp.Key);
            if (itemDef != null && kvp.Value > 0)
            {
                // Create a temporary store item for display
                var tempStoreItem = CreateTempStoreItem(itemDef, kvp.Value);
                slots[slotIndex].Setup(tempStoreItem, true);
                slotIndex++;
            }
        }
    }
    
    private StoreItemDefinition CreateTempStoreItem(ItemDefinition itemDef, int amount)
    {
        var tempItem = ScriptableObject.CreateInstance<StoreItemDefinition>();
        tempItem.displayName = itemDef.itemName;
        tempItem.description = itemDef.description;
        tempItem.icon = itemDef.icon;
        tempItem.amount = amount;
        tempItem.referencedItemID = itemDef.itemID;
        return tempItem;
    }
    
    public void AddItem(StoreItemDefinition item)
    {
        SlotUI emptySlot = FindEmptySlot();
        if (emptySlot != null)
        {
            bool isAvailable = CanAffordItem(item);
            emptySlot.Setup(item, isAvailable);
        }
    }
    
    public void RemoveItem(StoreItemDefinition item)
    {
        SlotUI targetSlot = FindSlotWithItem(item);
        if (targetSlot != null)
        {
            targetSlot.Setup(null);
        }
    }
    
    public void UpdateSlotAvailability()
    {
        foreach (SlotUI slot in slots)
        {
            var currentItem = slot.GetCurrentItem();
            if (currentItem != null)
            {
                bool isAvailable = CanAffordItem(currentItem);
                slot.SetAvailable(isAvailable);
            }
        }
    }
    
    private bool CanAffordItem(StoreItemDefinition item)
    {
        if (GameDataManager.Instance?.PlayerData?.GetPlayerData() != null)
        {
            var playerData = GameDataManager.Instance.PlayerData.GetPlayerData();
            bool canAfford = playerData.currentGold >= item.price;
            
            if (!canAfford)
            {
                Debug.Log($"Cannot afford {item.displayName}. Need {FormatUtilities.FormatCurrency(item.price)}, have {FormatUtilities.FormatCurrency(playerData.currentGold)}");
            }
            
            return canAfford;
        }
        return true;
    }
    
    private SlotUI FindEmptySlot()
    {
        foreach (SlotUI slot in slots)
        {
            if (slot.GetCurrentItem() == null)
            {
                return slot;
            }
        }
        return null;
    }
    
    private SlotUI FindSlotWithItem(StoreItemDefinition item)
    {
        foreach (SlotUI slot in slots)
        {
            if (slot.GetCurrentItem() == item)
            {
                return slot;
            }
        }
        return null;
    }
    
    private void OnSlotClicked(SlotUI slot, StoreItemDefinition item)
    {
        // Deselect previous slot
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
        }
        
        // Select new slot
        currentSelectedSlot = slot;
        slot.SetSelected(true);
        
        OnItemSelected?.Invoke(item);
    }
    
    private void OnSlotHovered(SlotUI slot, StoreItemDefinition item)
    {
        OnItemHovered?.Invoke(item);
    }
    
    public void ClearAllSlots()
    {
        foreach (SlotUI slot in slots)
        {
            slot.Setup(null);
        }
        
        if (currentSelectedSlot != null)
        {
            currentSelectedSlot.SetSelected(false);
            currentSelectedSlot = null;
        }
    }
    
    private void ClearSlots()
    {
        foreach (SlotUI slot in slots)
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
    
    public List<SlotUI> GetSlots()
    {
        return new List<SlotUI>(slots);
    }
    
    public SlotUI GetSelectedSlot()
    {
        return currentSelectedSlot;
    }
    
    private void OnDestroy()
    {
        ClearSlots();
    }
}
