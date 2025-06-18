using System;
using System.Threading.Tasks;
using UnityEngine;

public class GameDataService : IDataManager, IPlayerDataProvider, IGameDataProvider
{
    private DataManagerService dataManager;
    private PlayerDataService playerDataService;
    private GameDataProvider gameDataProvider;
    
    // Events - delegated from DataManagerService
    public event Action<bool> OnSaveCompleted
    {
        add { dataManager.OnSaveCompleted += value; }
        remove { dataManager.OnSaveCompleted -= value; }
    }
    
    public event Action<GameSaveData> OnLoadCompleted
    {
        add { dataManager.OnLoadCompleted += value; }
        remove { dataManager.OnLoadCompleted -= value; }
    }
    
    public event Action<string> OnError
    {
        add { dataManager.OnError += value; }
        remove { dataManager.OnError -= value; }
    }
    
    public GameDataService(ISaveLoadService saveLoadService)
    {
        var initialData = new GameSaveData();
        
        dataManager = new DataManagerService(saveLoadService);
        playerDataService = new PlayerDataService(initialData);
        gameDataProvider = new GameDataProvider(initialData);
        
        // Subscribe to data changes to update services
        dataManager.OnLoadCompleted += OnGameDataLoaded;
    }
    
    private void OnGameDataLoaded(GameSaveData data)
    {
        playerDataService.SetGameData(data);
        gameDataProvider.SetGameData(data);
    }
    
    public GameSaveData GetCurrentGameData()
    {
        return dataManager.GetCurrentGameData();
    }
    
    #region IDataManager Implementation - Delegated to DataManagerService
    
    public async Task<GameSaveData> LoadGameDataAsync()
    {
        var result = await dataManager.LoadGameDataAsync();
        OnGameDataLoaded(result);
        return result;
    }
    
    public bool SaveGameData(GameSaveData data)
    {
        return dataManager.SaveGameData(data);
    }
    
    public async Task<bool> SaveGameDataAsync(GameSaveData data)
    {
        return await dataManager.SaveGameDataAsync(data);
    }
    
    public GameSaveData LoadGameData()
    {
        var result = dataManager.LoadGameData();
        OnGameDataLoaded(result);
        return result;
    }
    
    public bool ValidateGameData(GameSaveData data)
    {
        return dataManager.ValidateGameData(data);
    }
    
    public bool HasSaveFile()
    {
        return dataManager.HasSaveFile();
    }
    
    public bool DeleteSaveFile()
    {
        return dataManager.DeleteSaveFile();
    }
    
    public void BackupSaveFile()
    {
        dataManager.BackupSaveFile();
    }
    
    #endregion
    
    #region IPlayerDataProvider Implementation - Delegated to PlayerDataService
    
    public PlayerData GetPlayerData() => playerDataService.GetPlayerData();
    public void UpdatePlayerData(PlayerData data) => playerDataService.UpdatePlayerData(data);
    public long GetGold() => playerDataService.GetGold();
    public void AddGold(long amount) => playerDataService.AddGold(amount);
    public bool SpendGold(long amount) => playerDataService.SpendGold(amount);
    public bool HasItem(ItemID itemID, int amount = 1) => playerDataService.HasItem(itemID, amount);
    public void AddItem(ItemID itemID, int amount) => playerDataService.AddItem(itemID, amount);
    public bool RemoveItem(ItemID itemID, int amount) => playerDataService.RemoveItem(itemID, amount);
    public int GetItemCount(ItemID itemID) => playerDataService.GetItemCount(itemID);
    public int GetAvailableWorkers() => playerDataService.GetAvailableWorkers();
    public bool HasAvailableWorkers() => playerDataService.HasAvailableWorkers();
    public void AssignWorker() => playerDataService.AssignWorker();
    public void FreeWorker() => playerDataService.FreeWorker();
    public bool HasWonGame() => playerDataService.HasWonGame();
    public void UpdatePlayTime(float deltaTime) => playerDataService.UpdatePlayTime(deltaTime);
    
    #endregion
    
    #region IGameDataProvider Implementation - Delegated to GameDataProvider
    
    public PlotData GetPlot(int plotID) => gameDataProvider.GetPlot(plotID);
    public void UpdatePlot(PlotData plotData) => gameDataProvider.UpdatePlot(plotData);
    public void AddPlot() => gameDataProvider.AddPlot();
    public FarmEntityInstanceData GetFarmEntity(string instanceID) => gameDataProvider.GetFarmEntity(instanceID);
    public void AddFarmEntity(FarmEntityInstanceData entity) => gameDataProvider.AddFarmEntity(entity);
    public void RemoveFarmEntity(string instanceID) => gameDataProvider.RemoveFarmEntity(instanceID);
    public void UpdateFarmEntity(FarmEntityInstanceData entity) => gameDataProvider.UpdateFarmEntity(entity);
    public WorkerData GetWorker(string workerID) => gameDataProvider.GetWorker(workerID);
    public void AddWorker() => gameDataProvider.AddWorker();
    public void UpdateWorker(WorkerData worker) => gameDataProvider.UpdateWorker(worker);
    public void UpdateOfflineProgress() => gameDataProvider.UpdateOfflineProgress();
    public bool IsValidGameState() => gameDataProvider.IsValidGameState();
    
    #endregion
}