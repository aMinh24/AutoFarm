using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlotDisplayManager : MonoBehaviour
{
    public EntityDisplayManager entityDisplayManager;
    
    [Header("Plot Info Display")]
    public PlotInfoDisplay plotInfoDisplay;
    
    [Header("Worker Display")]
    public WorkerDisplay workerDisplay;
    
    private int displayedPlotID = -1;
    
    private void Start()
    {
        // Subscribe to plot manager events
        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.OnCurrentPlotChanged += OnCurrentPlotChanged;
            PlotManager.Instance.OnPlotEntityStatusChanged += OnPlotEntityStatusChanged;
            PlotManager.Instance.OnPlotUpdated += OnPlotUpdated;
        }
        
        // Subscribe to game data loaded event
        this.Register(EventID.OnGameLoaded, OnGameDataLoaded);
        
        // Initialize plot info display
        InitializePlotInfoDisplay();
        
        // Initialize display
        UpdatePlotDisplay();
    }
    
    private void OnDestroy()
    {
        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.OnCurrentPlotChanged -= OnCurrentPlotChanged;
            PlotManager.Instance.OnPlotEntityStatusChanged -= OnPlotEntityStatusChanged;
            PlotManager.Instance.OnPlotUpdated -= OnPlotUpdated;
        }
        
        this.Unregister(EventID.OnGameLoaded, OnGameDataLoaded);
    }
    
    private void OnCurrentPlotChanged(int newPlotID)
    {
        UpdatePlotDisplay();
    }
    
    private void OnPlotEntityStatusChanged(int plotID, bool hasEntity)
    {
        if (plotID == displayedPlotID)
        {
            UpdatePlotDisplay();
        }
    }
    
    private void OnPlotUpdated(int plotID)
    {
        if (plotID == displayedPlotID)
        {
            UpdatePlotDisplay();
        }
    }
    
    private void OnGameDataLoaded(object data)
    {
        UpdatePlotDisplay();
    }
    
    private void InitializePlotInfoDisplay()
    {
        if (plotInfoDisplay != null && PlotManager.Instance != null)
        {
            int currentPlotID = PlotManager.Instance.GetCurrentPlotIndex();
            plotInfoDisplay.Initialize(currentPlotID);
        }
    }
    
    private void UpdatePlotDisplay()
    {
        if (PlotManager.Instance == null)
            return;
            
        int currentPlotID = PlotManager.Instance.GetCurrentPlotIndex();
        
        // Update plot info display (without worker status)
        if (plotInfoDisplay != null)
        {
            if (displayedPlotID != currentPlotID)
            {
                plotInfoDisplay.Initialize(currentPlotID);
            }
            else
            {
                plotInfoDisplay.UpdateDisplay();
            }
        }
        
        // Update worker display
        UpdateWorkerDisplay(currentPlotID);
        
        // Update entity displays
        if (entityDisplayManager != null)
        {
            var entities = PlotManager.Instance.GetCurrentPlotEntities();
            entityDisplayManager.UpdateEntityDisplays(entities);
        }
        
        displayedPlotID = currentPlotID;
    }
    
    private void UpdateWorkerDisplay(int plotID)
    {
        if (workerDisplay == null || WorkerManager.Instance == null)
            return;
            
        var busyWorkers = WorkerManager.Instance.GetBusyWorkers();
        var plotWorker = busyWorkers.FirstOrDefault(w => 
            int.TryParse(w.taskTargetInstanceID, out int targetPlotID) && 
            targetPlotID == plotID);
        
        if (plotWorker != null)
        {
            workerDisplay.SetWorker(plotWorker);
        }
        else
        {
            workerDisplay.ClearWorker();
        }
    }
    
    public void RefreshDisplay()
    {
        UpdatePlotDisplay();
    }
}