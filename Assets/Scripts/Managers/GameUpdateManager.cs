using UnityEngine;
using System;
using System.Collections.Generic;

public class GameUpdateManager : BaseManager<GameUpdateManager>
{
    [Header("Update Intervals")]
    public float farmEntityUpdateInterval = 1f;
    public float workerUpdateInterval = 1f;
    public float taskAssignmentInterval = 2f;
    public float autoSaveUpdateInterval = 30f; // Save every 30 seconds instead of 5
    
    [Header("Speed Settings")]
    public float gameSpeed = 1f;
    
    // Internal timers
    private float farmEntityTimer;
    private float workerTimer;
    private float taskAssignmentTimer;
    private float autoSaveTimer;
    
    // Events for subsystems
    public event Action OnFarmEntityUpdate;
    public event Action OnWorkerUpdate;
    public event Action OnTaskAssignment;
    public event Action OnAutoSave;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeTimers();
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        float deltaTime = Time.deltaTime * gameSpeed;
        
        // Update all timers
        UpdateFarmEntityTimer(deltaTime);
        UpdateWorkerTimer(deltaTime);
        UpdateTaskAssignmentTimer(deltaTime);
        UpdateAutoSaveTimer(deltaTime);
    }
    
    private void InitializeTimers()
    {
        farmEntityTimer = 0f;
        workerTimer = 0f;
        taskAssignmentTimer = 0f;
        autoSaveTimer = 0f;
        isInitialized = true;
    }
    
    #region Timer Updates
    
    private void UpdateFarmEntityTimer(float deltaTime)
    {
        farmEntityTimer += deltaTime;
        if (farmEntityTimer >= farmEntityUpdateInterval)
        {
            OnFarmEntityUpdate?.Invoke();
            farmEntityTimer = 0f;
        }
    }
    
    private void UpdateWorkerTimer(float deltaTime)
    {
        workerTimer += deltaTime;
        if (workerTimer >= workerUpdateInterval)
        {
            OnWorkerUpdate?.Invoke();
            workerTimer = 0f;
        }
    }
    
    private void UpdateTaskAssignmentTimer(float deltaTime)
    {
        taskAssignmentTimer += deltaTime;
        if (taskAssignmentTimer >= taskAssignmentInterval)
        {
            OnTaskAssignment?.Invoke();
            taskAssignmentTimer = 0f;
        }
    }
    
    private void UpdateAutoSaveTimer(float deltaTime)
    {
        autoSaveTimer += deltaTime;
        if (autoSaveTimer >= autoSaveUpdateInterval)
        {
            OnAutoSave?.Invoke();
            autoSaveTimer = 0f;
        }
    }
    
    #endregion
    
    #region Public API
    
    public void SetGameSpeed(float speed)
    {
        gameSpeed = Mathf.Max(0f, speed);
    }
    
    public void ResetTimer(TimerType timerType)
    {
        switch (timerType)
        {
            case TimerType.FarmEntity:
                farmEntityTimer = 0f;
                break;
            case TimerType.Worker:
                workerTimer = 0f;
                break;
            case TimerType.TaskAssignment:
                taskAssignmentTimer = 0f;
                break;
            case TimerType.AutoSave:
                autoSaveTimer = 0f;
                break;
        }
    }
    
    public float GetTimerProgress(TimerType timerType)
    {
        switch (timerType)
        {
            case TimerType.FarmEntity:
                return farmEntityTimer / farmEntityUpdateInterval;
            case TimerType.Worker:
                return workerTimer / workerUpdateInterval;
            case TimerType.TaskAssignment:
                return taskAssignmentTimer / taskAssignmentInterval;
            case TimerType.AutoSave:
                return autoSaveTimer / autoSaveUpdateInterval;
            default:
                return 0f;
        }
    }
    
    public void ForceUpdate(TimerType timerType)
    {
        switch (timerType)
        {
            case TimerType.FarmEntity:
                OnFarmEntityUpdate?.Invoke();
                farmEntityTimer = 0f;
                break;
            case TimerType.Worker:
                OnWorkerUpdate?.Invoke();
                workerTimer = 0f;
                break;
            case TimerType.TaskAssignment:
                OnTaskAssignment?.Invoke();
                taskAssignmentTimer = 0f;
                break;
            case TimerType.AutoSave:
                OnAutoSave?.Invoke();
                autoSaveTimer = 0f;
                break;
        }
    }
    
    public string GetTimerStatus()
    {
        return $"Timers - Farm: {farmEntityTimer:F1}s, Worker: {workerTimer:F1}s, Task: {taskAssignmentTimer:F1}s, Save: {autoSaveTimer:F1}s";
    }
    
    #endregion
    
    private bool isInitialized = false;
}

public enum TimerType
{
    FarmEntity,
    Worker,
    TaskAssignment,
    AutoSave
}
