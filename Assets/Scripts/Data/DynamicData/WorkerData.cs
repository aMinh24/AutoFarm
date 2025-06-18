using System;
using UnityEngine;

public enum WorkerState
{
    Idle,
    Busy
}

public enum WorkerTask
{
    None,
    Plant,
    Harvest,
    Milk
}

[Serializable]
public class WorkerData
{
    [Header("Identification")]
    public string workerID;
    
    [Header("State")]
    public WorkerState state;
    public WorkerTask assignedTask;
    public string taskTargetInstanceID;
    public float timeRemainingOnTask;

    [Header("Timestamps")]
    public long lastUpdateTimestamp;

    public WorkerData()
    {
        workerID = System.Guid.NewGuid().ToString();
        state = WorkerState.Idle;
        assignedTask = WorkerTask.None;
        taskTargetInstanceID = string.Empty;
        timeRemainingOnTask = 0f;
        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public WorkerData(string id)
    {
        workerID = id;
        state = WorkerState.Idle;
        assignedTask = WorkerTask.None;
        taskTargetInstanceID = string.Empty;
        timeRemainingOnTask = 0f;
        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public bool IsIdle()
    {
        return state == WorkerState.Idle;
    }

    public bool IsBusy()
    {
        return state == WorkerState.Busy;
    }

    public void AssignTask(WorkerTask task, string targetInstanceID = "")
    {
        if (state == WorkerState.Busy)
        {
            Debug.LogWarning($"Worker {workerID} is already busy with task {assignedTask}");
            return;
        }

        state = WorkerState.Busy;
        assignedTask = task;
        taskTargetInstanceID = targetInstanceID;
        
        // Set task duration from game settings - worker will complete ALL entities on plot in this time
        var settings = GameDataManager.Instance?.gameSettings;
        timeRemainingOnTask = settings != null ? settings.workerTaskDuration * 60f : 120f; // Default 2 minutes
        
        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public WorkerTaskResult UpdateTask(float deltaTime)
    {
        var result = new WorkerTaskResult
        {
            workerID = workerID,
            completedTask = WorkerTask.None,
            targetInstanceID = taskTargetInstanceID,
            success = false
        };
        if (state != WorkerState.Busy) return result;

        timeRemainingOnTask -= deltaTime;
        if (timeRemainingOnTask <= 0)
        {
            result = CompleteTask();
        }

        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return result;
    }

    public WorkerTaskResult CompleteTask()
    {
        var result = new WorkerTaskResult
        {
            workerID = workerID,
            completedTask = assignedTask,
            targetInstanceID = taskTargetInstanceID,
            success = true
        };

        // Reset worker to idle
        state = WorkerState.Idle;
        assignedTask = WorkerTask.None;
        taskTargetInstanceID = string.Empty;
        timeRemainingOnTask = 0f;
        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return result;
    }

    public void CancelTask()
    {
        state = WorkerState.Idle;
        assignedTask = WorkerTask.None;
        taskTargetInstanceID = string.Empty;
        timeRemainingOnTask = 0f;
        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public void UpdateFromOfflineTime(long currentTimestamp)
    {
        if (lastUpdateTimestamp <= 0 || state != WorkerState.Busy) return;

        long offlineSeconds = currentTimestamp - lastUpdateTimestamp;
        if (offlineSeconds > 0)
        {
            UpdateTask(offlineSeconds);
        }
    }

    public float GetTaskProgress()
    {
        if (state != WorkerState.Busy) return 0f;

        var settings = GameDataManager.Instance?.gameSettings;
        float totalTaskTime = settings != null ? settings.workerTaskDuration * 60f : 120f;
        
        return 1f - (timeRemainingOnTask / totalTaskTime);
    }
}

[Serializable]
public struct WorkerTaskResult
{
    public string workerID;
    public WorkerTask completedTask;
    public string targetInstanceID;
    public bool success;
}
