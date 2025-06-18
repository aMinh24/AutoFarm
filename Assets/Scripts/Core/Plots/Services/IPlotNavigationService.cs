public interface IPlotNavigationService
{
    int CurrentPlotID { get; set; }
    bool SwitchToPlot(int plotID);
    bool NextPlot();
    bool PreviousPlot();
    event System.Action<int> OnCurrentPlotChanged;
}
