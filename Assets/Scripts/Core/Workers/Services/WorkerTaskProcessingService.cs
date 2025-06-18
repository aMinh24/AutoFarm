using System.Linq;
using UnityEngine;

public class WorkerTaskProcessingService
{
    public void ProcessCompletedWorkerTask(WorkerData worker, WorkerTaskResult result)
    {
        if (!result.success) return;
        
        switch (result.completedTask)
        {
            case WorkerTask.Harvest:
                ProcessWorkerHarvest(result.targetInstanceID);
                break;
            case WorkerTask.Plant:
                ProcessWorkerPlant(result.targetInstanceID);
                break;
            case WorkerTask.Milk:
                ProcessWorkerMilk(result.targetInstanceID);
                break;
        }
        
        Debug.Log($"Worker {result.workerID} completed {result.completedTask}");
    }
    
    private void ProcessWorkerHarvest(string targetID)
    {
        // targetID is plotID for harvest tasks
        if (int.TryParse(targetID, out int plotID))
        {
            if (PlotManager.Instance?.PlotHasHarvestableEntities(plotID) == true)
            {
                // Delegate to PlotManager for centralized harvest logic
                var harvestResult = PlotManager.Instance.HarvestAllEntitiesOnPlot(plotID);
                
                if (harvestResult.success && harvestResult.amountProduced > 0)
                {
                    Debug.Log($"Worker completed plot harvest: {harvestResult.amountProduced} {harvestResult.itemProduced} from plot {plotID}");
                }
            }
        }
    }
    
    private void ProcessWorkerPlant(string plotInstanceID)
    {
        Debug.Log($"Worker completed planting on plot {plotInstanceID}");
    }
    
    private void ProcessWorkerMilk(string entityInstanceID)
    {
        // For milk task, targetID is plotID - delegate to PlotManager
        if (int.TryParse(entityInstanceID, out int plotID))
        {
            if (PlotManager.Instance?.PlotHasHarvestableEntities(plotID) == true)
            {
                // Use the same centralized harvest logic for milking
                var harvestResult = PlotManager.Instance.HarvestAllEntitiesOnPlot(plotID);
                
                if (harvestResult.success && harvestResult.amountProduced > 0)
                {
                    Debug.Log($"Worker milked all cows on plot {plotID}: {harvestResult.amountProduced} milk collected");
                }
            }
        }
    }
}