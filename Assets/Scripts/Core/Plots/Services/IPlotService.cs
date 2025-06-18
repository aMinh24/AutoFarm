using System.Collections.Generic;

public interface IPlotService
{
    PlotData GetPlot(int plotID);
    List<PlotData> GetAllPlots();
    PlotData GetCurrentPlot();
    int GetCurrentPlotIndex();
    int GetTotalPlotsCount();
    PlotInfo GetCurrentPlotInfo();
    void RefreshPlotsFromGameData();
    void SetNavigationService(IPlotNavigationService navigationService);
    bool IsInitialized { get; }
}
