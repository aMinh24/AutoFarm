using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OfflineEventSystem
{
    private readonly GameUpdateManager gameUpdateManager;
    private readonly WorkerTaskAssignmentService taskAssignmentService;
    private readonly WorkerTaskProcessingService taskProcessingService;
    private readonly IWorkerService workerService;

    public OfflineEventSystem()
    {
        gameUpdateManager = GameUpdateManager.Instance;
        workerService = new WorkerService();
        taskProcessingService = new WorkerTaskProcessingService();
        taskAssignmentService = new WorkerTaskAssignmentService(workerService);
    }

    public OfflineSimulationResult SimulateOfflineTime(long offlineSeconds)
    {
        if (offlineSeconds <= 0) return new OfflineSimulationResult();

        Debug.Log($"Starting offline simulation for {offlineSeconds} seconds ({offlineSeconds / 60f:F1} minutes)");

        var result = new OfflineSimulationResult
        {
            totalOfflineTime = offlineSeconds,
            eventsProcessed = new List<OfflineWorkerEvent>()
        };

        // Get update intervals from GameUpdateManager
        float workerUpdateInterval = gameUpdateManager?.workerUpdateInterval ?? 1f;
        float taskAssignmentInterval = gameUpdateManager?.taskAssignmentInterval ?? 2f;
        float entityUpdateInterval = gameUpdateManager?.farmEntityUpdateInterval ?? 1f;

        // Create event timeline including entity updates
        var events = CreateEventTimeline(offlineSeconds, workerUpdateInterval, taskAssignmentInterval, entityUpdateInterval);
        
        // Process events in chronological order
        foreach (var eventData in events.OrderBy(e => e.timestamp))
        {
            ProcessOfflineEvent(eventData, result);
        }

        Debug.Log($"Offline simulation completed. Processed {events.Count} events, {result.tasksCompleted} tasks completed, {result.tasksAssigned} tasks assigned");
        return result;
    }

    private List<OfflineEvent> CreateEventTimeline(long totalOfflineSeconds, float workerInterval, float taskInterval, float entityInterval)
    {
        var events = new List<OfflineEvent>();

        // Add worker update events
        for (float time = workerInterval; time <= totalOfflineSeconds; time += workerInterval)
        {
            events.Add(new OfflineEvent
            {
                timestamp = time,
                eventType = OfflineEventType.WorkerUpdate
            });
        }

        // Add task assignment events
        for (float time = taskInterval; time <= totalOfflineSeconds; time += taskInterval)
        {
            events.Add(new OfflineEvent
            {
                timestamp = time,
                eventType = OfflineEventType.TaskAssignment
            });
        }

        // Add entity update events
        for (float time = entityInterval; time <= totalOfflineSeconds; time += entityInterval)
        {
            events.Add(new OfflineEvent
            {
                timestamp = time,
                eventType = OfflineEventType.EntityUpdate
            });
        }

        return events;
    }

    private void ProcessOfflineEvent(OfflineEvent eventData, OfflineSimulationResult result)
    {
        switch (eventData.eventType)
        {
            case OfflineEventType.WorkerUpdate:
                ProcessWorkerUpdateEvent(eventData, result);
                break;
            case OfflineEventType.TaskAssignment:
                ProcessTaskAssignmentEvent(eventData, result);
                break;
            case OfflineEventType.EntityUpdate:
                ProcessEntityUpdateEvent(eventData, result);
                break;
        }
    }

    private void ProcessWorkerUpdateEvent(OfflineEvent eventData, OfflineSimulationResult result)
    {
        var gameData = GameDataManager.Instance?.DataManager?.GetCurrentGameData();
        if (gameData?.workersData == null) return;

        var completedTasks = new List<WorkerTaskResult>();

        // Update all workers
        foreach (var worker in gameData.workersData)
        {
            if (worker.IsBusy())
            {
                float deltaTime = gameUpdateManager?.workerUpdateInterval ?? 1f;
                var taskResult = worker.UpdateTask(deltaTime);

                if (taskResult.success && taskResult.completedTask != WorkerTask.None)
                {
                    completedTasks.Add(taskResult);
                    result.tasksCompleted++;
                    Debug.Log($"Offline: Worker {worker.workerID} completed {taskResult.completedTask} at timestamp {eventData.timestamp}");
                }
            }
        }

        // Process completed tasks
        foreach (var taskResult in completedTasks)
        {
            var worker = gameData.workersData.Find(w => w.workerID == taskResult.workerID);
            if (worker != null)
            {
                taskProcessingService.ProcessCompletedWorkerTask(worker, taskResult);
            }
        }

        // Update worker counts
        workerService.UpdateWorkerCounts();
    }

    private void ProcessTaskAssignmentEvent(OfflineEvent eventData, OfflineSimulationResult result)
    {
        var idleWorkersBefore = GameDataManager.Instance?.GetAvailableWorkers() ?? 0;
        taskAssignmentService.AssignWorkerTasks();
        var idleWorkersAfter = GameDataManager.Instance?.GetAvailableWorkers() ?? 0;

        int newAssignments = idleWorkersBefore - idleWorkersAfter;
        if (newAssignments > 0)
        {
            result.tasksAssigned += newAssignments;
            Debug.Log($"Offline: {newAssignments} workers assigned to tasks at timestamp {eventData.timestamp}");
        }
    }

    private void ProcessEntityUpdateEvent(OfflineEvent eventData, OfflineSimulationResult result)
    {
        var gameData = GameDataManager.Instance?.DataManager?.GetCurrentGameData();
        if (gameData?.farmEntitiesData == null) return;

        float deltaTime = gameUpdateManager?.farmEntityUpdateInterval ?? 1f;
        var entitiesToRemove = new List<string>();

        foreach (var entity in gameData.farmEntitiesData)
        {
            var previousState = entity.currentState;
            var previousProducts = entity.accumulatedProducts;
            
            // Update entity timers
            entity.UpdateTimers(deltaTime);

            // Check if entity state changed or products accumulated
            if (entity.currentState != previousState || entity.accumulatedProducts != previousProducts)
            {
                // Notify PlotManager of entity changes if available
                PlotManager.Instance?.OnEntityUpdated(entity);
            }

            // Mark dead entities for removal
            if (entity.IsDead())
            {
                entitiesToRemove.Add(entity.instanceID);
            }
        }

        // Remove dead entities
        foreach (var instanceID in entitiesToRemove)
        {
            RemoveDeadEntity(instanceID);
        }
    }

    private void RemoveDeadEntity(string instanceID)
    {
        var entity = GameDataManager.Instance.GetFarmEntity(instanceID);
        if (entity == null) return;

        // Update plot to be empty if this was the main entity
        var plot = GameDataManager.Instance.GetPlot(entity.associatedPlotID);
        if (plot != null && plot.occupyingEntityInstanceID == instanceID)
        {
            plot.plotState = PlotState.Empty;
            plot.occupyingEntityInstanceID = string.Empty;
            GameDataManager.Instance.UpdatePlot(plot);
        }

        // Remove the entity
        GameDataManager.Instance.RemoveFarmEntity(instanceID);

        Debug.Log($"Offline: Removed dead entity {instanceID} from plot {entity.associatedPlotID}");
    }
}

[Serializable]
public struct OfflineEvent
{
    public float timestamp;
    public OfflineEventType eventType;
}

public enum OfflineEventType
{
    WorkerUpdate,
    TaskAssignment,
    EntityUpdate
}
