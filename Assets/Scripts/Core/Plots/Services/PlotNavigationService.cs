using UnityEngine;

public class PlotNavigationService : IPlotNavigationService
{
    private readonly IPlotService plotService;
    
    public int CurrentPlotID { get; set; }
    public event System.Action<int> OnCurrentPlotChanged;
    
    public PlotNavigationService(IPlotService plotService)
    {
        this.plotService = plotService;
        this.CurrentPlotID = 0;
    }
    
    public bool SwitchToPlot(int plotID)
    {
        if (!plotService.IsInitialized)
        {
            Debug.LogWarning("PlotService not initialized");
            return false;
        }
        
        var totalPlots = plotService.GetTotalPlotsCount();
        if (plotID < 0 || plotID >= totalPlots)
        {
            Debug.LogWarning($"Invalid plot ID: {plotID}. Available plots: {totalPlots}");
            return false;
        }
        
        if (CurrentPlotID != plotID)
        {
            CurrentPlotID = plotID;
            OnCurrentPlotChanged?.Invoke(CurrentPlotID);
            Debug.Log($"Switched to plot {CurrentPlotID}");
        }
        
        return true;
    }
    
    public bool NextPlot()
    {
        if (!plotService.IsInitialized)
            return false;
            
        var totalPlots = plotService.GetTotalPlotsCount();
        if (totalPlots == 0)
            return false;
            
        int nextPlotID = (CurrentPlotID + 1) % totalPlots;
        return SwitchToPlot(nextPlotID);
    }
    
    public bool PreviousPlot()
    {
        if (!plotService.IsInitialized)
            return false;
            
        var totalPlots = plotService.GetTotalPlotsCount();
        if (totalPlots == 0)
            return false;
            
        int prevPlotID = (CurrentPlotID - 1 + totalPlots) % totalPlots;
        return SwitchToPlot(prevPlotID);
    }
}
