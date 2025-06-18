using UnityEngine;
using System.Linq;

public class WorkerManager : BaseManager<WorkerManager>
{
    [Header("Worker Management")]
    public bool enableAutoWorkers = true;
    
    // Services
    private IWorkerService workerService;
    private WorkerUpdateService workerUpdateService;
    private WorkerTaskAssignmentService taskAssignmentService;
    private WorkerTaskProcessingService taskProcessingService;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeServices();
        SubscribeToTimers();
    }
    
    private void InitializeServices()
    {
        workerService = new WorkerService();
        taskProcessingService = new WorkerTaskProcessingService();
        workerUpdateService = new WorkerUpdateService(workerService, taskProcessingService);
        taskAssignmentService = new WorkerTaskAssignmentService(workerService);
    }
    
    private void SubscribeToTimers()
    {
        if (GameUpdateManager.Instance != null)
        {
            GameUpdateManager.Instance.OnWorkerUpdate += UpdateAllWorkers;
            GameUpdateManager.Instance.OnTaskAssignment += AssignWorkerTasks;
        }
    }
    
    private void OnDestroy()
    {
        if (GameUpdateManager.Instance != null)
        {
            GameUpdateManager.Instance.OnWorkerUpdate -= UpdateAllWorkers;
            GameUpdateManager.Instance.OnTaskAssignment -= AssignWorkerTasks;
        }
    }
    
    #region Timer Event Handlers
    
    private void UpdateAllWorkers()
    {
        if (!enableAutoWorkers) return;
        workerUpdateService.UpdateAllWorkers();
    }
    
    private void AssignWorkerTasks()
    {
        if (!enableAutoWorkers) return;
        taskAssignmentService.AssignWorkerTasks();
    }
    
    #endregion
    
    #region Public API - Delegate to Services
    
    public System.Collections.Generic.List<WorkerData> GetAllWorkers()
    {
        return workerService.GetAllWorkers();
    }
    
    public System.Collections.Generic.List<WorkerData> GetIdleWorkers()
    {
        return workerService.GetIdleWorkers();
    }
    
    public System.Collections.Generic.List<WorkerData> GetBusyWorkers()
    {
        return workerService.GetBusyWorkers();
    }
    
    public int GetAvailableWorkerCount()
    {
        return workerService.GetAvailableWorkerCount();
    }
    
    public int GetTotalWorkerCount()
    {
        return workerService.GetTotalWorkerCount();
    }
    
    public WorkerData GetWorkerByID(string workerID)
    {
        return workerService.GetWorkerByID(workerID);
    }
    
    public bool HasAvailableWorkers()
    {
        return workerService.HasAvailableWorkers();
    }
    
    public void ForceWorkerUpdate()
    {
        workerService.UpdateWorkerCounts();
    }
    
    public string GetWorkerStatusSummary()
    {
        return workerService.GetWorkerStatusSummary();
    }
    
    #endregion
    
    #region Manual Assignment Methods
    
    public bool AssignWorkerToHarvestPlot(int plotID)
    {
        return taskAssignmentService.AssignWorkerToHarvestPlot(plotID);
    }
    
    [System.Obsolete("Use AssignWorkerToHarvestPlot instead")]
    public bool AssignWorkerToHarvest(string entityInstanceID)
    {
        var entity = GameDataManager.Instance.GetFarmEntity(entityInstanceID);
        if (entity != null)
        {
            return taskAssignmentService.AssignWorkerToHarvestPlot(entity.associatedPlotID);
        }
        return false;
    }
    
    public bool AssignWorkerToPlant(int plotID, ItemID itemID)
    {
        return taskAssignmentService.AssignWorkerToPlant(plotID, itemID);
    }
    
    public void CancelAllWorkerTasks()
    {
        foreach (var worker in GetBusyWorkers())
        {
            worker.CancelTask();
        }
        workerService.UpdateWorkerCounts();
        Debug.Log("Cancelled all worker tasks");
    }
    
    #endregion
}