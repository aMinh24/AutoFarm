using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
[Serializable]
public class GameSaveData
{
    [Header("Core Data")]
    public PlayerData playerData;
    public List<PlotData> plotsData;
    public List<FarmEntityInstanceData> farmEntitiesData;
    public List<WorkerData> workersData;

    [Header("Meta Information")]
    public string saveVersion = "1.0";
    public long saveTimestamp;
    public float totalPlayTimeSeconds;

    [JsonConstructor]
    public GameSaveData()
    {
        playerData = new PlayerData();
        plotsData = new List<PlotData>();
        farmEntitiesData = new List<FarmEntityInstanceData>();
        workersData = new List<WorkerData>();
        saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        totalPlayTimeSeconds = 0f;
    }

    public void InitializeNewGame()
    {
        playerData = new PlayerData();
        playerData.InitializeForNewGame(); // Explicitly initialize for new game
        
        // Initialize plots based on starting land plots
        plotsData.Clear();
        for (int i = 0; i < playerData.totalLandPlots; i++)
        {
            plotsData.Add(new PlotData(i));
        }

        // Initialize workers
        workersData.Clear();
        for (int i = 0; i < playerData.totalWorkersHired; i++)
        {
            workersData.Add(new WorkerData($"worker_{i}"));
        }

        farmEntitiesData.Clear();
        saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        totalPlayTimeSeconds = 0f;
    }

    public void UpdateSaveTimestamp()
    {
        saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    
    // Add method to prepare data for save
    public void PrepareForSave()
    {
        UpdateSaveTimestamp();
        playerData?.PrepareForSave();
        
        // Update busy workers count to match current state
        if (playerData != null)
        {
            playerData.busyWorkersCount = GetBusyWorkers().Count;
        }
        
        // Ensure all farm entities have current timestamps
        foreach (var entity in farmEntitiesData)
        {
            entity.lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }

    // Add method to prepare data after load
    public void PrepareAfterLoad()
    {
        playerData?.PrepareAfterLoad();
        UpdateOfflineProgress();
    }

    public PlotData GetPlot(int plotID)
    {
        return plotsData.Find(p => p.plotID == plotID);
    }

    public FarmEntityInstanceData GetFarmEntity(string instanceID)
    {
        return farmEntitiesData.Find(e => e.instanceID == instanceID);
    }

    public WorkerData GetWorker(string workerID)
    {
        return workersData.Find(w => w.workerID == workerID);
    }

    public void AddPlot()
    {
        int newPlotID = plotsData.Count;
        var newPlot = new PlotData(newPlotID);
        plotsData.Add(newPlot);
        playerData.totalLandPlots = plotsData.Count;
        
        Debug.Log($"GameSaveData: Added plot {newPlotID}, total plots: {plotsData.Count}");
    }

    public void AddWorker()
    {
        string newWorkerID = $"worker_{workersData.Count}";
        workersData.Add(new WorkerData(newWorkerID));
        playerData.totalWorkersHired = workersData.Count;
    }

    public void AddFarmEntity(FarmEntityInstanceData entity)
    {
        if (entity != null && !string.IsNullOrEmpty(entity.instanceID))
        {
            farmEntitiesData.Add(entity);
        }
    }

    public void RemoveFarmEntity(string instanceID)
    {
        farmEntitiesData.RemoveAll(e => e.instanceID == instanceID);
    }

    public List<WorkerData> GetIdleWorkers()
    {
        return workersData.FindAll(w => w.IsIdle());
    }

    public List<WorkerData> GetBusyWorkers()
    {
        return workersData.FindAll(w => w.IsBusy());
    }

    public void UpdateOfflineProgress()
    {
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        // Update farm entities
        foreach (var entity in farmEntitiesData)
        {
            entity.UpdateFromOfflineTime(currentTimestamp);
        }

        // Update workers - they can complete tasks while offline
        foreach (var worker in workersData)
        {
            worker.UpdateFromOfflineTime(currentTimestamp);
        }

        // Update busy workers count to match actual state
        playerData.busyWorkersCount = GetBusyWorkers().Count;
    }

    public bool IsValidSave()
    {
        return playerData != null && 
               plotsData != null && 
               farmEntitiesData != null && 
               workersData != null &&
               plotsData.Count == playerData.totalLandPlots &&
               workersData.Count == playerData.totalWorkersHired;
    }
}
