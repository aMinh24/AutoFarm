using System.Linq;
using AutoFarm.Utilities;
using UnityEngine;

public class WorkerTaskAssignmentService
{
    private readonly IWorkerService workerService;
    
    public WorkerTaskAssignmentService(IWorkerService workerService)
    {
        this.workerService = workerService;
    }
    
    public void AssignWorkerTasks()
    {
        if (GameDataManager.Instance == null) return;
        
        var gameData = GameDataManager.Instance.DataManager.GetCurrentGameData();
        if (gameData?.workersData == null) return;
        
        var idleWorkers = gameData.GetIdleWorkers();
        if (idleWorkers.Count == 0) return;
        
        // Priority 1: Harvest ready entities
        foreach (var worker in idleWorkers.ToList())
        {
            if (AssignHarvestTask(worker))
            {
                idleWorkers.Remove(worker);
                workerService.UpdateWorkerCounts();
                if (idleWorkers.Count == 0) break;
            }
        }
        
        // Priority 2: Plant on empty plots
        foreach (var worker in idleWorkers.ToList())
        {
            if (AssignPlantTask(worker))
            {
                idleWorkers.Remove(worker);
                workerService.UpdateWorkerCounts();
                if (idleWorkers.Count == 0) break;
            }
        }
    }
    
    private bool AssignHarvestTask(WorkerData worker)
    {
        var gameData = GameDataManager.Instance.DataManager.GetCurrentGameData();
        
        // Check each plot instead of each entity
        foreach (var plot in gameData.plotsData)
        {
            if (PlotManager.Instance?.PlotHasHarvestableEntities(plot.plotID) == true)
            {
                // Check if no other worker is already assigned to this plot for harvesting
                bool isAlreadyAssigned = gameData.workersData.Any(w => 
                    w.IsBusy() && 
                    (w.assignedTask == WorkerTask.Harvest || w.assignedTask == WorkerTask.Milk) && 
                    w.taskTargetInstanceID == plot.plotID.ToString());
                
                if (!isAlreadyAssigned)
                {
                    // Determine task type based on entities on the plot
                    var entities = PlotManager.Instance.GetPlotEntities(plot.plotID);
                    var firstEntity = entities.FirstOrDefault();
                    if (firstEntity != null)
                    {
                        var entityDef = GameDataManager.Instance.GetEntity(firstEntity.entityID);
                        var taskType = (entityDef?.entityType == EntityType.Animal) ? WorkerTask.Milk : WorkerTask.Harvest;
                        
                        worker.AssignTask(taskType, plot.plotID.ToString());
                        Debug.Log($"Assigned {taskType} task to worker {worker.workerID} for plot {plot.plotID}");
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    private bool AssignPlantTask(WorkerData worker)
    {
        var gameData = GameDataManager.Instance.DataManager.GetCurrentGameData();
        
        foreach (var plot in gameData.plotsData)
        {
            if (plot.IsEmpty())
            {
                // Check if no other worker is already assigned to this plot
                bool isAlreadyAssigned = gameData.workersData.Any(w => 
                    w.IsBusy() && 
                    w.assignedTask == WorkerTask.Plant && 
                    w.taskTargetInstanceID == plot.plotID.ToString());
                
                if (!isAlreadyAssigned)
                {
                    var itemToPlant = GetBestItemToPlant();
                    if (itemToPlant != ItemID.None && PlotManager.Instance?.CanPlantOnPlot(plot.plotID, itemToPlant) == true)
                    {
                        // Start planting task
                        worker.AssignTask(WorkerTask.Plant, plot.plotID.ToString());
                        
                        // Use PlotManager's centralized planting logic
                        var result = PlotManager.Instance.PlantItemOnPlot(plot.plotID, itemToPlant);
                        
                        if (result.success)
                        {
                            Debug.Log($"Worker {worker.workerID} planted {result.quantityPlanted} items on plot {plot.plotID}");
                            return true;
                        }
                        else
                        {
                            // Revert worker assignment if planting failed
                            worker.CancelTask();
                            Debug.LogWarning($"Worker planting failed: {result.errorMessage}");
                        }
                    }
                }
            }
        }
        
        return false;
    }
    
    private ItemID GetBestItemToPlant()
    {
        var playerData = GameDataManager.Instance.PlayerData.GetPlayerData();
        if (playerData?.Inventory == null) return ItemID.None;
        
        ItemID bestItem = ItemID.None;
        int highestValue = 0;
        
        foreach (var inventoryItem in playerData.Inventory)
        {
            if (inventoryItem.Value <= 0) continue;
            
            var itemDef = GameDataManager.Instance.GetItem(inventoryItem.Key);
            if (itemDef == null) continue;
            
            // Check if item can be planted (has growsIntoEntityID)
            if (itemDef.growsIntoEntityID != EntityID.None)
            {
                var entityDef = GameDataManager.Instance.GetEntity(itemDef.growsIntoEntityID);
                if (entityDef != null)
                {
                    // Calculate value based on production
                    var productDef = GameDataManager.Instance.GetItem(entityDef.productProducedItemID);
                    if (productDef != null)
                    {
                        int totalValue = productDef.baseSalePrice * entityDef.baseYieldAmount * entityDef.totalYieldsLimit;
                        if (totalValue > highestValue)
                        {
                            highestValue = totalValue;
                            bestItem = inventoryItem.Key;
                            Debug.Log($"Best planting option: {itemDef.itemName} with total value {FormatUtilities.FormatCurrency(totalValue)}");
                        }
                    }
                }
            }
        }
        
        return bestItem;
    }
    
    public bool AssignWorkerToHarvestPlot(int plotID)
    {
        var idleWorkers = workerService.GetIdleWorkers();
        if (idleWorkers.Count == 0) return false;
        
        if (PlotManager.Instance?.PlotHasHarvestableEntities(plotID) != true) return false;
        
        var worker = idleWorkers.First();
        
        // Determine task type based on entities on the plot
        var entities = PlotManager.Instance.GetPlotEntities(plotID);
        var firstEntity = entities.FirstOrDefault();
        if (firstEntity != null)
        {
            var entityDef = GameDataManager.Instance.GetEntity(firstEntity.entityID);
            var taskType = (entityDef?.entityType == EntityType.Animal) ? WorkerTask.Milk : WorkerTask.Harvest;
            
            worker.AssignTask(taskType, plotID.ToString());
            workerService.UpdateWorkerCounts();
            
            Debug.Log($"Manually assigned {taskType} task to worker {worker.workerID} for plot {plotID}");
            return true;
        }
        
        return false;
    }
    
    public bool AssignWorkerToPlant(int plotID, ItemID itemID)
    {
        var idleWorkers = workerService.GetIdleWorkers();
        if (idleWorkers.Count == 0) return false;
        
        if (!PlotManager.Instance?.CanPlantOnPlot(plotID, itemID) == true) return false;
        
        var worker = idleWorkers.First();
        worker.AssignTask(WorkerTask.Plant, plotID.ToString());
        workerService.UpdateWorkerCounts();
        
        // Use PlotManager's centralized planting logic
        var result = PlotManager.Instance.PlantItemOnPlot(plotID, itemID);
        
        if (result.success)
        {
            Debug.Log($"Manually assigned plant task to worker {worker.workerID} - planted {result.quantityPlanted} items");
            return true;
        }
        else
        {
            // Revert worker assignment if planting failed
            worker.CancelTask();
            workerService.UpdateWorkerCounts();
            Debug.LogWarning($"Manual worker planting failed: {result.errorMessage}");
            return false;
        }
    }
}