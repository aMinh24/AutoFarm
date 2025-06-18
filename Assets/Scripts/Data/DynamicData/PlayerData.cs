using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

[Serializable]
public class PlayerData
{
    [Header("Resources")]
    public long currentGold;
    public long earnedGoldTotal;
    
    [Header("Inventory - Serialized as Lists for JSON")]
    public List<ItemID> inventoryKeys = new List<ItemID>();
    public List<int> inventoryValues = new List<int>();
    
    [Header("Upgrades")]
    public int equipmentLevel;
    public int totalWorkersHired;
    public int busyWorkersCount;
    public int totalLandPlots;

    // Non-serialized dictionary for runtime use
    [JsonIgnore]
    private Dictionary<ItemID, int> _inventory;
    public Dictionary<ItemID, int> Inventory
    {
        get
        {
            if (_inventory == null)
            {
                DeserializeInventory();
            }
            return _inventory;
        }
        set
        {
            _inventory = value;
            SerializeInventory();
        }
    }

    [JsonConstructor]
    public PlayerData()
    {
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        var settings = GameDataManager.Instance?.gameSettings;
        if (settings != null)
        {
            currentGold = settings.startingGold;
            equipmentLevel = settings.startingEquipmentLevel;
            totalWorkersHired = settings.startingWorkers;
            totalLandPlots = settings.startingPlots;
            Inventory = settings.GetStartingInventoryDictionary();
        }
        else
        {
            // Fallback defaults
            currentGold = 1000;
            equipmentLevel = 1;
            totalWorkersHired = 1;
            totalLandPlots = 3;
            Inventory = new Dictionary<ItemID, int>();
        }
        
        busyWorkersCount = 0;
        earnedGoldTotal = 0;
    }

    public void SerializeInventory()
    {
        if (_inventory == null) return;
        
        inventoryKeys.Clear();
        inventoryValues.Clear();
        
        foreach (var kvp in _inventory)
        {
            inventoryKeys.Add(kvp.Key);
            inventoryValues.Add(kvp.Value);
        }
    }

    public void DeserializeInventory()
    {
        _inventory = new Dictionary<ItemID, int>();
        
        for (int i = 0; i < inventoryKeys.Count && i < inventoryValues.Count; i++)
        {
            _inventory[inventoryKeys[i]] = inventoryValues[i];
        }
    }

    // Helper methods for inventory management
    public bool HasItem(ItemID itemID, int amount = 1)
    {
        return Inventory.ContainsKey(itemID) && Inventory[itemID] >= amount;
    }

    public void AddItem(ItemID itemID, int amount)
    {
        if (Inventory.ContainsKey(itemID))
        {
            Inventory[itemID] += amount;
        }
        else
        {
            Inventory[itemID] = amount;
        }
        SerializeInventory(); // Ensure this is called
        Debug.Log($"Added {amount} {itemID}. Total: {Inventory[itemID]}");
    }

    public bool RemoveItem(ItemID itemID, int amount)
    {
        if (!HasItem(itemID, amount))
        {
            Debug.LogWarning($"Cannot remove {amount} {itemID}. Available: {GetItemCount(itemID)}");
            return false;
        }
        
        Inventory[itemID] -= amount;
        if (Inventory[itemID] <= 0)
        {
            Inventory.Remove(itemID);
        }
        SerializeInventory(); // Ensure this is called
        Debug.Log($"Removed {amount} {itemID}. Remaining: {GetItemCount(itemID)}");
        return true;
    }
    
    // Add method to ensure serialization before save
    public void PrepareForSave()
    {
        SerializeInventory();
    }
    
    // Add method to ensure deserialization after load
    public void PrepareAfterLoad()
    {
        DeserializeInventory();
    }
    
    public int GetItemCount(ItemID itemID)
    {
        return Inventory.ContainsKey(itemID) ? Inventory[itemID] : 0;
    }
    public void AddGold(long amount)
    {
        currentGold += amount;
        earnedGoldTotal += amount;
    }

    public bool SpendGold(long amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            return true;
        }
        return false;
    }

    public int GetAvailableWorkers()
    {
        return totalWorkersHired - busyWorkersCount;
    }

    public bool HasAvailableWorkers()
    {
        return GetAvailableWorkers() > 0;
    }

    public void AssignWorker()
    {
        if (HasAvailableWorkers())
        {
            busyWorkersCount++;
        }
    }

    public void FreeWorker()
    {
        if (busyWorkersCount > 0)
        {
            busyWorkersCount--;
        }
    }

    public bool HasWonGame()
    {
        var settings = GameDataManager.Instance?.gameSettings;
        return settings != null && earnedGoldTotal >= settings.winConditionGold;
    }
}
