using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemID itemID;
    public int amount;
}

[CreateAssetMenu(fileName = "GameSettings", menuName = "Farm Game/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Starting Values")]
    public int startingGold = 1000;
    public int startingPlots = 3;
    public List<InventoryItem> startingInventory = new List<InventoryItem>();
    public int startingWorkers = 1;
    public int startingEquipmentLevel = 1;

    [Header("Game Mechanics")]
    public float workerTaskDuration = 2f; // 2 minutes - workers complete entire plot harvest/milk in this time
    public float equipmentYieldBonusPerLevel = 0.1f; // 10% per level

    [Header("Win Condition")]
    public long winConditionGold = 1000000;

    [Header("Costs")]
    public int cost_HireWorker = 500;
    public int cost_UpgradeEquipment = 500;
    public int cost_BuyLandPlot = 500;

    // Helper method to get starting inventory as Dictionary
    public Dictionary<ItemID, int> GetStartingInventoryDictionary()
    {
        Dictionary<ItemID, int> inventory = new Dictionary<ItemID, int>();
        foreach (var item in startingInventory)
        {
            inventory[item.itemID] = item.amount;
        }
        return inventory;
    }
}
