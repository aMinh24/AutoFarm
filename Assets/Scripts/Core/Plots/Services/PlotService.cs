using System.Collections.Generic;
using UnityEngine;

public class PlotService : IPlotService
{
    private List<PlotData> availablePlots;
    private IPlotNavigationService navigationService;
    
    public bool IsInitialized { get; private set; }
    
    public PlotService()
    {
        this.availablePlots = new List<PlotData>();
    }
    
    public void SetNavigationService(IPlotNavigationService navigationService)
    {
        this.navigationService = navigationService;
    }
    
    public void RefreshPlotsFromGameData()
    {
        availablePlots.Clear();
        
        var gameData = GameDataManager.Instance?.DataManager?.GetCurrentGameData();
        if (gameData?.plotsData != null)
        {
            availablePlots.AddRange(gameData.plotsData);
            
            // Ensure current plot is valid
            if (navigationService != null && navigationService.CurrentPlotID >= availablePlots.Count)
            {
                navigationService.CurrentPlotID = Mathf.Max(0, availablePlots.Count - 1);
            }
        }
        
        IsInitialized = true;
        Debug.Log($"PlotService: Loaded {availablePlots.Count} plots");
    }
    
    public PlotData GetPlot(int plotID)
    {
        if (!IsInitialized || availablePlots == null)
            return null;
            
        return availablePlots.Find(p => p.plotID == plotID);
    }
    
    public List<PlotData> GetAllPlots()
    {
        return availablePlots ?? new List<PlotData>();
    }
    
    public PlotData GetCurrentPlot()
    {
        if (!IsInitialized || availablePlots == null || navigationService.CurrentPlotID >= availablePlots.Count)
            return null;
            
        return availablePlots[navigationService.CurrentPlotID];
    }
    
    public int GetCurrentPlotIndex()
    {
        return navigationService.CurrentPlotID;
    }
    
    public int GetTotalPlotsCount()
    {
        return availablePlots?.Count ?? 0;
    }
    
    public PlotInfo GetCurrentPlotInfo()
    {
        var currentPlot = GetCurrentPlot();
        var entityService = new PlotEntityService(this);
        var entity = entityService.GetCurrentPlotEntity();
        
        return new PlotInfo
        {
            plotID = navigationService.CurrentPlotID,
            totalPlots = availablePlots?.Count ?? 0,
            isEmpty = currentPlot?.IsEmpty() ?? true,
            hasEntity = currentPlot?.IsOccupied() ?? false,
            entityType = entity?.entityID ?? EntityID.None,
            entityState = entity?.currentState ?? EntityState.Dead
        };
    }
}
