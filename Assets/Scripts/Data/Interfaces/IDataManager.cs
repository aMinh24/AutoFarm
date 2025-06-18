using System;
using System.Threading.Tasks;

public interface IDataManager
{
    // Save/Load operations
    Task<bool> SaveGameDataAsync(GameSaveData data);
    Task<GameSaveData> LoadGameDataAsync();
    bool SaveGameData(GameSaveData data);
    GameSaveData LoadGameData();
    GameSaveData GetCurrentGameData(); // Add this method
    
    // Data validation
    bool ValidateGameData(GameSaveData data);
    
    // File operations
    bool HasSaveFile();
    bool DeleteSaveFile();
    void BackupSaveFile();
    
    // Events
    event Action<bool> OnSaveCompleted;
    event Action<GameSaveData> OnLoadCompleted;
    event Action<string> OnError;
}

public interface IPlayerDataProvider
{
    // Player data access
    PlayerData GetPlayerData();
    void UpdatePlayerData(PlayerData data);
    
    // Resource management
    long GetGold();
    void AddGold(long amount);
    bool SpendGold(long amount);
    
    // Inventory management
    bool HasItem(ItemID itemID, int amount = 1);
    void AddItem(ItemID itemID, int amount);
    bool RemoveItem(ItemID itemID, int amount);
    int GetItemCount(ItemID itemID);
    
    // Worker management
    int GetAvailableWorkers();
    bool HasAvailableWorkers();
    void AssignWorker();
    void FreeWorker();
    
    // Progress tracking
    bool HasWonGame();
    void UpdatePlayTime(float deltaTime);
}

public interface IGameDataProvider
{
    // Plot management
    PlotData GetPlot(int plotID);
    void UpdatePlot(PlotData plotData);
    void AddPlot();
    
    // Farm entity management
    FarmEntityInstanceData GetFarmEntity(string instanceID);
    void AddFarmEntity(FarmEntityInstanceData entity);
    void RemoveFarmEntity(string instanceID);
    void UpdateFarmEntity(FarmEntityInstanceData entity);
    
    // Worker management
    WorkerData GetWorker(string workerID);
    void AddWorker();
    void UpdateWorker(WorkerData worker);
    
    // Game state
    void UpdateOfflineProgress();
    bool IsValidGameState();
}
