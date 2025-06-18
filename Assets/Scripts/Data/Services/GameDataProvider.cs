using UnityEngine;

public class GameDataProvider : IGameDataProvider
{
    private GameSaveData gameData;
    
    public GameDataProvider(GameSaveData gameData)
    {
        this.gameData = gameData;
    }
    
    public void SetGameData(GameSaveData gameData)
    {
        this.gameData = gameData;
    }
    
    public PlotData GetPlot(int plotID)
    {
        return gameData?.GetPlot(plotID);
    }
    
    public void UpdatePlot(PlotData plotData)
    {
        var existingPlot = GetPlot(plotData.plotID);
        if (existingPlot != null)
        {
            existingPlot.plotState = plotData.plotState;
            existingPlot.occupyingEntityInstanceID = plotData.occupyingEntityInstanceID;
            existingPlot.ValidateState();
        }
    }
    
    public void AddPlot()
    {
        if (gameData != null)
        {
            int newPlotID = gameData.plotsData.Count;
            var newPlot = new PlotData(newPlotID);
            gameData.plotsData.Add(newPlot);
            gameData.playerData.totalLandPlots = gameData.plotsData.Count;
            
            Debug.Log($"Added plot with ID {newPlotID}. Total plots: {gameData.plotsData.Count}");
        }
    }
    
    public FarmEntityInstanceData GetFarmEntity(string instanceID)
    {
        return gameData?.GetFarmEntity(instanceID);
    }
    
    public void AddFarmEntity(FarmEntityInstanceData entity)
    {
        gameData?.AddFarmEntity(entity);
    }
    
    public void RemoveFarmEntity(string instanceID)
    {
        gameData?.RemoveFarmEntity(instanceID);
    }
    
    public void UpdateFarmEntity(FarmEntityInstanceData entity)
    {
        var existing = GetFarmEntity(entity.instanceID);
        if (existing != null)
        {
            existing.currentState = entity.currentState;
            existing.lastUpdateTimestamp = entity.lastUpdateTimestamp;
        }
    }
    
    public WorkerData GetWorker(string workerID)
    {
        return gameData?.GetWorker(workerID);
    }
    
    public void AddWorker()
    {
        gameData?.AddWorker();
    }
    
    public void UpdateWorker(WorkerData worker)
    {
        var existing = GetWorker(worker.workerID);
        if (existing != null)
        {
            existing.state = worker.state;
            existing.assignedTask = worker.assignedTask;
            existing.taskTargetInstanceID = worker.taskTargetInstanceID;
            existing.timeRemainingOnTask = worker.timeRemainingOnTask;
            existing.lastUpdateTimestamp = worker.lastUpdateTimestamp;
        }
    }
    
    public void UpdateOfflineProgress()
    {
        gameData?.UpdateOfflineProgress();
    }
    
    public bool IsValidGameState()
    {
        return gameData?.IsValidSave() ?? false;
    }
}
