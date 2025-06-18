using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Sirenix.OdinInspector;

public class GameDataManager : BaseManager<GameDataManager>
{
    [Header("Data Collections")]
    public EntityDefinitionCollection entityDefinitions;
    public ItemDefinitionCollection itemDefinitions;
    public StoreItemCollection storeItems;
    public GameSettings gameSettings;

    [Header("Auto Save Settings")]
    public bool enableAutoSave = true;
    
    private GameDataService gameDataService;
    private bool isInitialized = false;
    
    // Quick access properties
    public IPlayerDataProvider PlayerData => gameDataService;
    public IGameDataProvider GameData => gameDataService;
    public IDataManager DataManager => gameDataService;
    
    // Events for UI updates
    public System.Action OnGoldChanged;
    public System.Action OnWorkerCountChanged;
    public System.Action OnInventoryChanged;
    public System.Action OnEquipmentLevelChanged;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeDataService();
    }
    
    private void Start()
    {
        LoadGameData();
        SubscribeToTimers();
    }
    
    private void SubscribeToTimers()
    {
        if (GameUpdateManager.Instance != null)
        {
            GameUpdateManager.Instance.OnAutoSave += AutoSave;
        }
    }
    

    private void Update()
    {
        if (isInitialized)
        {
            // Update play time
            gameDataService?.UpdatePlayTime(Time.deltaTime);
        }
    }
    
    private void InitializeDataService()
    {
        var saveLoadService = new JsonSaveLoadService();
        gameDataService = new GameDataService(saveLoadService);
        
        // Subscribe to events
        gameDataService.OnSaveCompleted += OnSaveCompleted;
        gameDataService.OnLoadCompleted += OnLoadCompleted;
        gameDataService.OnError += OnDataError;
    }
    
    #region Public API Methods
    
    // Quick access methods for definitions
    public EntityDefinition GetEntity(EntityID entityID) => entityDefinitions.GetEntityDefinition(entityID);
    public ItemDefinition GetItem(ItemID itemID) => itemDefinitions.GetItemDefinition(itemID);
    public StoreItemDefinition GetStoreItem(StoreID storeID) => storeItems.GetStoreItem(storeID);
    
    // Save/Load operations
    public async Task<bool> SaveGameAsync()
    {
        if (!isInitialized) return false;
        
        return await gameDataService.SaveGameDataAsync(gameDataService.GetCurrentGameData());
    }
    
    public bool SaveGame()
    {
        if (!isInitialized) return false;
        
        return gameDataService.SaveGameData(gameDataService.GetCurrentGameData());
    }
    
    public async Task LoadGameDataAsync()
    {
        var data = await gameDataService.LoadGameDataAsync();
        isInitialized = true;
        
        // Trigger game loaded event
        this.Broadcast(EventID.OnGameLoaded, data);
    }
    
    public void LoadGameData()
    {
        var data = gameDataService.LoadGameData();
        isInitialized = true;
        
        // Trigger game loaded event
        this.Broadcast(EventID.OnGameLoaded, data);
    }
    [Button]
    public void NewGame()
    {
        gameDataService.DeleteSaveFile();
        var newData = new GameSaveData();
        newData.InitializeNewGame();
        gameDataService.SaveGameData(newData);
        LoadGameData();
    }

    [Button]
    public void SimulateOfflineTimeMinutes(int minutes)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Game not initialized. Cannot simulate offline time.");
            return;
        }

        long offlineSeconds = minutes * 60;
        Debug.Log($"Simulating {minutes} minutes ({offlineSeconds} seconds) of offline time...");

        // Create offline event system and simulate
        var offlineEventSystem = new OfflineEventSystem();
        offlineEventSystem.SimulateOfflineTime(offlineSeconds);

        // Force save game data after simulation
        SaveGame();

        // Update all UI elements
        OnGoldChanged?.Invoke();
        OnWorkerCountChanged?.Invoke();
        OnInventoryChanged?.Invoke();
        OnEquipmentLevelChanged?.Invoke();

        // Force update plot displays
        PlotManager.Instance?.RefreshPlotsFromGameData();
        
        // Broadcast game updated event
        this.Broadcast(EventID.OnGameLoaded, gameDataService.GetCurrentGameData());

        Debug.Log($"Offline simulation complete! Simulated {minutes} minutes of offline progress.");
    }
    
    public bool HasSaveFile()
    {
        return gameDataService?.HasSaveFile() ?? false;
    }
    
    public void DeleteSave()
    {
        gameDataService?.DeleteSaveFile();
        isInitialized = false;
    }
    
    #endregion
    
    #region Player Data API
    
    // Gold management with events
    public long GetPlayerGold() => gameDataService?.GetGold() ?? 0;
    
    public void AddPlayerGold(long amount) 
    {
        gameDataService?.AddGold(amount);
        OnGoldChanged?.Invoke();
    }
    
    public bool SpendPlayerGold(long amount) 
    {
        bool result = gameDataService?.SpendGold(amount) ?? false;
        if (result)
        {
            OnGoldChanged?.Invoke();
        }
        return result;
    }
    
    // Inventory management with events
    public bool HasPlayerItem(ItemID itemID, int amount = 1) => gameDataService?.HasItem(itemID, amount) ?? false;

    public void AddPlayerItem(ItemID itemID, int amount) 
    {
        gameDataService?.AddItem(itemID, amount);
        OnInventoryChanged?.Invoke();
        Debug.Log($"Added {amount} {itemID} to inventory. Event triggered: {OnInventoryChanged != null}");
    }

    public bool RemovePlayerItem(ItemID itemID, int amount) 
    {
        bool result = gameDataService?.RemoveItem(itemID, amount) ?? false;
        if (result)
        {
            OnInventoryChanged?.Invoke();
            Debug.Log($"Removed {amount} {itemID} from inventory. Event triggered: {OnInventoryChanged != null}");
        }
        return result;
    }
    
    public int GetPlayerItemCount(ItemID itemID) => gameDataService?.GetItemCount(itemID) ?? 0;
    
    // Worker management with events
    public int GetAvailableWorkers() => gameDataService?.GetAvailableWorkers() ?? 0;
    public bool HasAvailableWorkers() => gameDataService?.HasAvailableWorkers() ?? false;
    
    public void AssignWorker() 
    {
        gameDataService?.AssignWorker();
        OnWorkerCountChanged?.Invoke();
    }
    
    public void FreeWorker() 
    {
        gameDataService?.FreeWorker();
        OnWorkerCountChanged?.Invoke();
    }
    
    public void AddNewWorkerToPlayer()
    {
        var playerData = gameDataService?.GetPlayerData();
        if (playerData != null)
        {
            playerData.totalWorkersHired++;
            OnWorkerCountChanged?.Invoke();
            
            // Also add to workers data
            AddNewWorker();
        }
    }
    
    // Enhanced worker methods
    public void UpdateWorkerCounts()
    {
        var gameData = gameDataService?.GetCurrentGameData();
        if (gameData?.playerData != null)
        {
            gameData.playerData.busyWorkersCount = gameData.GetBusyWorkers().Count;
            OnWorkerCountChanged?.Invoke();
        }
    }
    
    public void ProcessOfflineWorkerProgress()
    {
        var gameData = gameDataService?.GetCurrentGameData();
        if (gameData == null) return;
        
        // Process any completed worker tasks during offline time
        foreach (var worker in gameData.workersData.ToList())
        {
            if (worker.IsBusy())
            {
                worker.UpdateFromOfflineTime(System.DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                
                // If task completed during offline time
                if (worker.IsIdle())
                {
                    // Process completed task results
                    Debug.Log($"Worker {worker.workerID} completed offline task");
                }
            }
        }
        
        // Update worker counts
        UpdateWorkerCounts();
    }
    
    // Plot management
    public PlotData GetPlot(int plotID) => gameDataService?.GetPlot(plotID);
    public void UpdatePlot(PlotData plotData) => gameDataService?.UpdatePlot(plotData);
    
    public void AddNewPlot() 
    {
        // Add plot to game data
        gameDataService?.AddPlot();
        
        // Update player data
        var playerData = gameDataService?.GetPlayerData();
        
        Debug.Log($"Added new plot. Total plots: {playerData?.totalLandPlots}");
    }
    
    // Progress
    public bool HasPlayerWon() => gameDataService?.HasWonGame() ?? false;
    
    #endregion
    
    #region Game Data API
    
    // Farm entity management
    public FarmEntityInstanceData GetFarmEntity(string instanceID) => gameDataService?.GetFarmEntity(instanceID);
    public void AddFarmEntity(FarmEntityInstanceData entity) => gameDataService?.AddFarmEntity(entity);
    public void RemoveFarmEntity(string instanceID) => gameDataService?.RemoveFarmEntity(instanceID);
    public void UpdateFarmEntity(FarmEntityInstanceData entity) => gameDataService?.UpdateFarmEntity(entity);
    
    // Worker management
    public WorkerData GetWorker(string workerID) => gameDataService?.GetWorker(workerID);
    public void AddNewWorker() => gameDataService?.AddWorker();
    public void UpdateWorker(WorkerData worker) => gameDataService?.UpdateWorker(worker);
    
    #endregion
    
    #region Auto Save
    
    private void AutoSave()
    {
        if (!enableAutoSave || !isInitialized) return;
        
        bool result = SaveGame();
        if (result)
        {
            Debug.Log("Auto-save completed successfully");
        }
        else
        {
            Debug.LogWarning("Auto-save failed");
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnSaveCompleted(bool success)
    {
        if (success)
        {
            Debug.Log("Game saved successfully");
        }
        else
        {
            Debug.LogError("Failed to save game");
        }
    }
    
    private void OnLoadCompleted(GameSaveData data)
    {
        Debug.Log("Game loaded successfully");
        
        // Ensure data is properly prepared after loading
        data?.PrepareAfterLoad();
        
        // Process offline worker progress
        ProcessOfflineWorkerProgress();
        
        // Trigger game loaded event
        this.Broadcast(EventID.OnGameLoaded, data);
    }
    
    private void OnDataError(string error)
    {
        Debug.LogError($"Data error: {error}");
    }
    
    #endregion
    
    #region Application Events
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isInitialized)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && isInitialized)
        {
            SaveGame();
        }
    }
    
    private void OnDestroy()
    {
        if (isInitialized)
        {
            SaveGame();
        }
        
        if (gameDataService != null)
        {
            gameDataService.OnSaveCompleted -= OnSaveCompleted;
            gameDataService.OnLoadCompleted -= OnLoadCompleted;
            gameDataService.OnError -= OnDataError;
        }
    }
    
    #endregion
    
    #region Equipment API
    
    public void UpgradePlayerEquipment()
    {
        var playerData = gameDataService?.GetPlayerData();
        if (playerData != null)
        {
            playerData.equipmentLevel++;
            OnEquipmentLevelChanged?.Invoke();
        }
    }
    
    public int GetPlayerEquipmentLevel()
    {
        return gameDataService?.GetPlayerData()?.equipmentLevel ?? 1;
    }
    
    public float GetEquipmentYieldMultiplier()
    {
        int equipmentLevel = GetPlayerEquipmentLevel();
        float bonusPerLevel = gameSettings?.equipmentYieldBonusPerLevel ?? 0.1f;
        return 1f + ((equipmentLevel - 1) * bonusPerLevel);
    }
    
    public int GetBonusAdjustedAmount(int baseAmount)
    {
        float multiplier = GetEquipmentYieldMultiplier();
        return Mathf.RoundToInt(baseAmount * multiplier);
    }
    
    #endregion

    #region Offline Simulation

    public void ProcessOfflineProgress()
    {
        if (DataManager?.GetCurrentGameData() != null)
        {
            // This will now include worker simulation
            DataManager.GetCurrentGameData().UpdateOfflineProgress();
            
            // Trigger events to update UI
            this.Broadcast(EventID.OnGameLoaded, null);
            
            Debug.Log("Offline progress processed including worker simulation");
        }
    }

    public OfflineSimulationResult GetLastOfflineSimulationResult()
    {
        // This could be stored if we want to show offline summary to player
        return new OfflineSimulationResult();
    }

    #endregion
}
