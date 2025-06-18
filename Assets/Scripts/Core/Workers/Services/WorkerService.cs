using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkerService : IWorkerService
{
    public List<WorkerData> GetAllWorkers()
    {
        var gameData = GameDataManager.Instance?.DataManager.GetCurrentGameData();
        return gameData?.workersData ?? new List<WorkerData>();
    }
    
    public List<WorkerData> GetIdleWorkers()
    {
        return GetAllWorkers().Where(w => w.IsIdle()).ToList();
    }
    
    public List<WorkerData> GetBusyWorkers()
    {
        return GetAllWorkers().Where(w => w.IsBusy()).ToList();
    }
    
    public WorkerData GetWorkerByID(string workerID)
    {
        return GetAllWorkers().FirstOrDefault(w => w.workerID == workerID);
    }
    
    public int GetAvailableWorkerCount()
    {
        return GetIdleWorkers().Count;
    }
    
    public int GetTotalWorkerCount()
    {
        return GetAllWorkers().Count;
    }
    
    public bool HasAvailableWorkers()
    {
        return GetAvailableWorkerCount() > 0;
    }
    
    public void UpdateWorkerCounts()
    {
        var gameData = GameDataManager.Instance.DataManager.GetCurrentGameData();
        if (gameData?.playerData != null)
        {
            int busyCount = gameData.GetBusyWorkers().Count;
            if (gameData.playerData.busyWorkersCount != busyCount)
            {
                gameData.playerData.busyWorkersCount = busyCount;
                GameDataManager.Instance.OnWorkerCountChanged?.Invoke();
            }
        }
    }
    
    public string GetWorkerStatusSummary()
    {
        var idleCount = GetAvailableWorkerCount();
        var busyCount = GetBusyWorkers().Count;
        var totalCount = GetTotalWorkerCount();
        
        return $"Workers: {idleCount} available, {busyCount} busy (Total: {totalCount})";
    }
}
