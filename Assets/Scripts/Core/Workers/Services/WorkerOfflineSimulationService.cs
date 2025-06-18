using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkerOfflineSimulationService
{
    private readonly IWorkerService workerService;
    private readonly WorkerTaskAssignmentService taskAssignmentService;
    private readonly WorkerTaskProcessingService taskProcessingService;

    public WorkerOfflineSimulationService(IWorkerService workerService)
    {
        this.workerService = workerService;
        this.taskAssignmentService = new WorkerTaskAssignmentService(workerService);
        this.taskProcessingService = new WorkerTaskProcessingService();
    }

    public OfflineSimulationResult SimulateWorkerActivities(long offlineSeconds)
    {
        var result = new OfflineSimulationResult();
        result.totalOfflineTime = offlineSeconds;
        result.eventsProcessed = new List<OfflineWorkerEvent>();

        if (offlineSeconds <= 0) return result;

        // Get simulation parameters
        var gameSettings = GameDataManager.Instance?.gameSettings;
        float workerUpdateInterval = gameSettings?.workerUpdateFrequency ?? 1f;
        float taskAssignmentInterval = gameSettings?.taskAssignmentFrequency ?? 2f;

        // Simulate time in chunks
        float currentTime = 0f;
        float nextWorkerUpdate = workerUpdateInterval;
        float nextTaskAssignment = taskAssignmentInterval;

        while (currentTime < offlineSeconds)
        {
            float nextEventTime = Mathf.Min(nextWorkerUpdate, nextTaskAssignment);
            nextEventTime = Mathf.Min(nextEventTime, offlineSeconds);

            float deltaTime = nextEventTime - currentTime;
            currentTime = nextEventTime;

            // Process worker updates
            if (currentTime >= nextWorkerUpdate)
            {
                ProcessWorkerUpdates(deltaTime, currentTime, result);
                nextWorkerUpdate += workerUpdateInterval;
            }

            // Process task assignments
            if (currentTime >= nextTaskAssignment)
            {
                ProcessTaskAssignments(currentTime, result);
                nextTaskAssignment += taskAssignmentInterval;
            }
        }

        result.finalWorkerState = GetWorkerStateSnapshot();
        return result;
    }

    private void ProcessWorkerUpdates(float deltaTime, float currentTime, OfflineSimulationResult result)
    {
        var gameData = GameDataManager.Instance?.DataManager?.GetCurrentGameData();
        if (gameData?.workersData == null) return;

        foreach (var worker in gameData.workersData)
        {
            if (worker.IsBusy())
            {
                var taskResult = worker.UpdateTask(deltaTime);
                
                if (taskResult.success && taskResult.completedTask != WorkerTask.None)
                {
                    // Process completed task
                    taskProcessingService.ProcessCompletedWorkerTask(worker, taskResult);
                    
                    // Record event
                    result.eventsProcessed.Add(new OfflineWorkerEvent
                    {
                        timestamp = currentTime,
                        workerID = worker.workerID,
                        eventType = OfflineWorkerEventType.TaskCompleted,
                        taskType = taskResult.completedTask,
                        targetID = taskResult.targetInstanceID
                    });

                    result.tasksCompleted++;
                }
            }
        }
    }

    private void ProcessTaskAssignments(float currentTime, OfflineSimulationResult result)
    {
        var idleWorkersBefore = workerService.GetIdleWorkers().Count;
        taskAssignmentService.AssignWorkerTasks();
        var idleWorkersAfter = workerService.GetIdleWorkers().Count;

        int newAssignments = idleWorkersBefore - idleWorkersAfter;
        
        if (newAssignments > 0)
        {
            result.eventsProcessed.Add(new OfflineWorkerEvent
            {
                timestamp = currentTime,
                eventType = OfflineWorkerEventType.TaskAssigned,
                description = $"{newAssignments} workers assigned to tasks"
            });

            result.tasksAssigned += newAssignments;
        }
    }

    private WorkerStateSnapshot GetWorkerStateSnapshot()
    {
        return new WorkerStateSnapshot
        {
            totalWorkers = workerService.GetTotalWorkerCount(),
            idleWorkers = workerService.GetAvailableWorkerCount(),
            busyWorkers = workerService.GetBusyWorkers().Count,
            workerStates = workerService.GetAllWorkers().ToDictionary(w => w.workerID, w => w.state)
        };
    }
}

[System.Serializable]
public struct OfflineSimulationResult
{
    public long totalOfflineTime;
    public int tasksCompleted;
    public int tasksAssigned;
    public List<OfflineWorkerEvent> eventsProcessed;
    public WorkerStateSnapshot initialWorkerState;
    public WorkerStateSnapshot finalWorkerState;
}

[System.Serializable]
public struct OfflineWorkerEvent
{
    public float timestamp;
    public string workerID;
    public OfflineWorkerEventType eventType;
    public WorkerTask taskType;
    public string targetID;
    public string description;
}

[System.Serializable]
public struct WorkerStateSnapshot
{
    public int totalWorkers;
    public int idleWorkers;
    public int busyWorkers;
    public Dictionary<string, WorkerState> workerStates;
}

public enum OfflineWorkerEventType
{
    TaskAssigned,
    TaskCompleted,
    TaskCancelled
}
