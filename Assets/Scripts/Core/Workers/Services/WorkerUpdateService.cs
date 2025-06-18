using UnityEngine;

public class WorkerUpdateService
{
    private readonly IWorkerService workerService;
    private readonly WorkerTaskProcessingService taskProcessingService;
    
    public WorkerUpdateService(IWorkerService workerService, WorkerTaskProcessingService taskProcessingService)
    {
        this.workerService = workerService;
        this.taskProcessingService = taskProcessingService;
    }
    
    public void UpdateAllWorkers()
    {
        if (GameDataManager.Instance == null) return;
        
        var gameData = GameDataManager.Instance.DataManager.GetCurrentGameData();
        if (gameData?.workersData == null) return;
        
        bool workersUpdated = false;
        float updateInterval = GameUpdateManager.Instance.workerUpdateInterval;
        
        foreach (var worker in gameData.workersData)
        {
            if (worker.IsBusy())
            {
                var previousState = worker.state;
                WorkerTaskResult result = worker.UpdateTask(updateInterval);
                
                // Check if task completed
                if (worker.IsIdle() && previousState == WorkerState.Busy)
                {
                    taskProcessingService.ProcessCompletedWorkerTask(worker, result);
                    workersUpdated = true;
                }
            }
        }
        
        if (workersUpdated)
        {
            workerService.UpdateWorkerCounts();
        }
    }
}
