using System;
using UnityEngine;

public class PlayerDataService : IPlayerDataProvider
{
    private GameSaveData gameData;
    
    public PlayerDataService(GameSaveData gameData)
    {
        this.gameData = gameData;
    }
    
    public void SetGameData(GameSaveData gameData)
    {
        this.gameData = gameData;
    }
    
    public PlayerData GetPlayerData()
    {
        return gameData?.playerData;
    }
    
    public void UpdatePlayerData(PlayerData data)
    {
        if (gameData != null)
        {
            gameData.playerData = data;
        }
    }
    
    public long GetGold()
    {
        return gameData?.playerData?.currentGold ?? 0;
    }
    
    public void AddGold(long amount)
    {
        if (gameData?.playerData != null)
        {
            gameData.playerData.AddGold(amount);
        }
    }
    
    public bool SpendGold(long amount)
    {
        return gameData?.playerData?.SpendGold(amount) ?? false;
    }
    
    public bool HasItem(ItemID itemID, int amount = 1)
    {
        return gameData?.playerData?.HasItem(itemID, amount) ?? false;
    }
    
    public void AddItem(ItemID itemID, int amount)
    {
        gameData?.playerData?.AddItem(itemID, amount);
    }
    
    public bool RemoveItem(ItemID itemID, int amount)
    {
        return gameData?.playerData?.RemoveItem(itemID, amount) ?? false;
    }
    
    public int GetItemCount(ItemID itemID)
    {
        var inventory = gameData?.playerData?.Inventory;
        return inventory?.ContainsKey(itemID) == true ? inventory[itemID] : 0;
    }
    
    public int GetAvailableWorkers()
    {
        if (gameData?.workersData == null || gameData?.playerData == null) 
            return 0;
            
        int idleWorkers = gameData.GetIdleWorkers().Count;
        return idleWorkers;
    }
    
    public bool HasAvailableWorkers()
    {
        return GetAvailableWorkers() > 0;
    }
    
    public void AssignWorker()
    {
        if (gameData?.playerData != null && HasAvailableWorkers())
        {
            gameData.playerData.busyWorkersCount++;
        }
    }
    
    public void FreeWorker()
    {
        if (gameData?.playerData != null && gameData.playerData.busyWorkersCount > 0)
        {
            gameData.playerData.busyWorkersCount--;
        }
    }
    
    public bool HasWonGame()
    {
        return gameData?.playerData?.HasWonGame() ?? false;
    }
    
    public void UpdatePlayTime(float deltaTime)
    {
        if (gameData != null)
        {
            gameData.totalPlayTimeSeconds += deltaTime;
        }
    }
}
