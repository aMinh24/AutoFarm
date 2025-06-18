using UnityEngine;
using System.Collections.Generic;

public class FarmEntityUpdateManager : MonoBehaviour
{
    [Header("Update Settings")]
    public float updateSpeed = 1f; // Speed of updates, can be adjusted for faster/slower updates
    
    private bool isInitialized = false;

    private void Start()
    {
        SubscribeToTimers();
        isInitialized = true;
    }
    
    private void SubscribeToTimers()
    {
        if (GameUpdateManager.Instance != null)
        {
            GameUpdateManager.Instance.OnFarmEntityUpdate += UpdateAllFarmEntities;
        }
    }
    
    private void OnDestroy()
    {
        if (GameUpdateManager.Instance != null)
        {
            GameUpdateManager.Instance.OnFarmEntityUpdate -= UpdateAllFarmEntities;
        }
    }
    
    private void UpdateAllFarmEntities()
    {
        if (!isInitialized || GameDataManager.Instance == null) return;
        
        var gameData = GameDataManager.Instance.DataManager.GetCurrentGameData();
        if (gameData?.farmEntitiesData == null) return;

        bool entitiesUpdated = false;
        var entitiesToRemove = new List<string>();
        float updateInterval = GameUpdateManager.Instance.farmEntityUpdateInterval;

        foreach (var entity in gameData.farmEntitiesData)
        {
            var previousState = entity.currentState;
            var previousProducts = entity.accumulatedProducts;
            
            entity.UpdateTimers(updateInterval * updateSpeed);

            // Check if entity state changed or products accumulated
            if (entity.currentState != previousState || entity.accumulatedProducts != previousProducts)
            {
                entitiesUpdated = true;
                
                // Notify PlotManager of entity changes
                PlotManager.Instance?.OnEntityUpdated(entity);
            }

            // Mark dead entities for removal
            if (entity.IsDead())
            {
                entitiesToRemove.Add(entity.instanceID);
                entitiesUpdated = true;
            }
        }

        // Remove dead entities
        foreach (var instanceID in entitiesToRemove)
        {
            RemoveDeadEntity(instanceID);
        }

        // Save game data if entities were updated
        if (entitiesUpdated)
        {
            GameDataManager.Instance.SaveGame();
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

        // Update plot display
        PlotManager.Instance?.UpdatePlotDisplay(entity.associatedPlotID);

        Debug.Log($"Removed dead entity {instanceID} from plot {entity.associatedPlotID}");
    }

    // Public method to force update entities (for testing or special cases)
    public void ForceUpdateEntities()
    {
        UpdateAllFarmEntities();
    }

    // Public method to get entity update status
    public string GetEntityUpdateSummary()
    {
        var gameData = GameDataManager.Instance?.DataManager.GetCurrentGameData();
        if (gameData?.farmEntitiesData == null) return "No entities";

        int growing = 0, ready = 0, decaying = 0, dead = 0;

        foreach (var entity in gameData.farmEntitiesData)
        {
            switch (entity.currentState)
            {
                case EntityState.Growing: growing++; break;
                case EntityState.ReadyToHarvest: ready++; break;
                case EntityState.Decaying: decaying++; break;
                case EntityState.Dead: dead++; break;
            }
        }

        return $"Entities: {growing} growing, {ready} ready, {decaying} decaying, {dead} dead";
    }
}
