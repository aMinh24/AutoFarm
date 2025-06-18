using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PlotManager : BaseManager<PlotManager>
{
    [Header("Events")]
    public System.Action<int> OnCurrentPlotChanged;
    public System.Action<int, bool> OnPlotEntityStatusChanged;
    public System.Action<int> OnPlotUpdated;
    public System.Action<int> OnEntityDied;
    
    // Services
    private IPlotService plotService;
    private IPlotNavigationService navigationService;
    private IPlotEntityService entityService;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeServices();
    }
    
    private void Start()
    {
        InitializePlots();
    }
    
    private void InitializeServices()
    {
        // Create services with proper dependency injection
        plotService = new PlotService();
        navigationService = new PlotNavigationService(plotService);
        plotService.SetNavigationService(navigationService);
        entityService = new PlotEntityService(plotService);
        
        // Subscribe to navigation events
        navigationService.OnCurrentPlotChanged += (plotID) => OnCurrentPlotChanged?.Invoke(plotID);
    }
    
    private void InitializePlots()
    {
        if (GameDataManager.Instance?.DataManager != null)
        {
            plotService.RefreshPlotsFromGameData();
            
            if (GameDataManager.Instance != null)
            {
                this.Register(EventID.OnGameLoaded, OnGameDataLoaded);
            }
        }
    }
    
    private void OnGameDataLoaded(object data)
    {
        plotService.RefreshPlotsFromGameData();
        OnCurrentPlotChanged?.Invoke(navigationService.CurrentPlotID);
    }
    
    #region Public API - Delegated to Services
    
    // Plot Service API
    public PlotInfo GetCurrentPlotInfo() => plotService.GetCurrentPlotInfo();
    public int GetTotalPlotsCount() => plotService.GetTotalPlotsCount();
    public int GetCurrentPlotIndex() => plotService.GetCurrentPlotIndex();
    
    // Navigation Service API
    public bool SwitchToPlot(int plotID) => navigationService.SwitchToPlot(plotID);
    public bool NextPlot() => navigationService.NextPlot();
    public bool PreviousPlot() => navigationService.PreviousPlot();
    
    // Entity Service API
    public System.Collections.Generic.List<FarmEntityInstanceData> GetCurrentPlotEntities() => entityService.GetCurrentPlotEntities();
    public System.Collections.Generic.List<FarmEntityInstanceData> GetPlotEntities(int plotID) => entityService.GetPlotEntities(plotID);
    public FarmEntityInstanceData GetCurrentPlotEntityAtPosition(int positionIndex) => entityService.GetCurrentPlotEntityAtPosition(positionIndex);
    public bool PlotHasHarvestableEntities(int plotID) => entityService.PlotHasHarvestableEntities(plotID);
    
    public bool AddEntityToCurrentPlot(EntityID entityID, int positionIndex)
    {
        var result = entityService.AddEntityToPlot(navigationService.CurrentPlotID, entityID, positionIndex);
        if (result)
        {
            OnPlotEntityStatusChanged?.Invoke(navigationService.CurrentPlotID, true);
            OnPlotUpdated?.Invoke(navigationService.CurrentPlotID);
        }
        return result;
    }
    
    public bool RemoveEntityFromCurrentPlot(int positionIndex)
    {
        var result = entityService.RemoveEntityFromCurrentPlot(positionIndex);
        if (result)
        {
            OnPlotEntityStatusChanged?.Invoke(navigationService.CurrentPlotID, !entityService.CurrentPlotHasEntity());
            OnPlotUpdated?.Invoke(navigationService.CurrentPlotID);
        }
        return result;
    }
    
    public HarvestResult HarvestAllEntitiesOnPlot(int plotID)
    {
        var result = entityService.HarvestAllEntitiesOnPlot(plotID);
        
        if (result.success)
        {
            // Trigger events after successful harvest
            OnPlotUpdated?.Invoke(plotID);
            
            // Check if plot is now empty and trigger status change
            if (!entityService.PlotHasEntity(plotID))
            {
                OnPlotEntityStatusChanged?.Invoke(plotID, false);
            }
        }
        
        return result;
    }
    
    public void HandleEntityDeath(int plotID)
    {
        entityService.HandleEntityDeath(plotID);
        OnEntityDied?.Invoke(plotID);
        OnPlotEntityStatusChanged?.Invoke(plotID, false);
        OnPlotUpdated?.Invoke(plotID);
    }
    
    public void OnEntityUpdated(FarmEntityInstanceData entity)
    {
        if (entity == null) return;
        
        // Update plot display when entity changes
        OnPlotUpdated?.Invoke(entity.associatedPlotID);
        
        // If entity died, handle death
        if (entity.IsDead())
        {
            HandleEntityDeath(entity.associatedPlotID);
        }
    }
    
    /// <summary>
    /// Update plot and trigger visual update event
    /// </summary>
    public void UpdatePlotDisplay(int plotID)
    {
        OnPlotUpdated?.Invoke(plotID);
    }
    
    
    /// <summary>
    /// Refresh plots from game data (call after adding new plots)
    /// </summary>
    public void RefreshPlotsFromGameData()
    {
        plotService?.RefreshPlotsFromGameData();
        
        // Trigger events to update UI
        OnCurrentPlotChanged?.Invoke(navigationService.CurrentPlotID);
        
        Debug.Log($"PlotManager: Refreshed plots, total: {GetTotalPlotsCount()}");
    }
    
    /// <summary>
    /// Harvest all entities on the current plot and handle cleanup
    /// </summary>
    public HarvestCurrentPlotResult HarvestCurrentPlotEntities()
    {
        var currentPlotID = navigationService.CurrentPlotID;
        var harvestResult = HarvestAllEntitiesOnPlot(currentPlotID);
        
        if (harvestResult.success)
        {
            var itemDef = GameDataManager.Instance?.GetItem(harvestResult.itemProduced);
            string itemName = itemDef?.itemName ?? "items";
            
            return new HarvestCurrentPlotResult
            {
                success = true,
                totalAmount = harvestResult.amountProduced,
                itemID = harvestResult.itemProduced,
                itemName = itemName
            };
        }
        
        return new HarvestCurrentPlotResult { success = false };
    }
    
    #endregion
    
    #region Cleanup
    
    private void OnDestroy()
    {
        if (GameDataManager.HasInstance)
        {
            this.Unregister(EventID.OnGameLoaded, OnGameDataLoaded);
        }
    }
    
    #endregion
}

/// <summary>
/// Data structure for plot information
/// </summary>
[System.Serializable]
public struct PlotInfo
{
    public int plotID;
    public int totalPlots;
    public bool isEmpty;
    public bool hasEntity;
    public EntityID entityType;
    public EntityState entityState;
}

/// <summary>
/// Result data structure for harvesting the current plot
/// </summary>
[System.Serializable]
public struct HarvestCurrentPlotResult
{
    public bool success;
    public int totalAmount;
    public ItemID itemID;
    public string itemName;
}