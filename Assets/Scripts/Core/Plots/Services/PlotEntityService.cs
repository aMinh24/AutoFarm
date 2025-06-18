using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlotEntityService : IPlotEntityService
{
    private readonly IPlotService plotService;
    
    public PlotEntityService(IPlotService plotService)
    {
        this.plotService = plotService;
    }
    
    public List<FarmEntityInstanceData> GetPlotEntities(int plotID)
    {
        if (GameDataManager.Instance?.DataManager == null)
            return new List<FarmEntityInstanceData>();
            
        var gameData = GameDataManager.Instance.DataManager.GetCurrentGameData();
        if (gameData?.farmEntitiesData == null)
            return new List<FarmEntityInstanceData>();
            
        return gameData.farmEntitiesData.FindAll(e => e.associatedPlotID == plotID);
    }
    
    public List<FarmEntityInstanceData> GetCurrentPlotEntities()
    {
        var currentPlot = plotService.GetCurrentPlot();
        if (currentPlot == null)
            return new List<FarmEntityInstanceData>();
            
        return GetPlotEntities(currentPlot.plotID);
    }
    
    public FarmEntityInstanceData GetPlotEntity(int plotID)
    {
        var entities = GetPlotEntities(plotID);
        return entities.FirstOrDefault();
    }
    
    public FarmEntityInstanceData GetCurrentPlotEntity()
    {
        var currentPlot = plotService.GetCurrentPlot();
        if (currentPlot == null)
            return null;
            
        return GetPlotEntity(currentPlot.plotID);
    }
    
    public FarmEntityInstanceData GetCurrentPlotEntityAtPosition(int positionIndex)
    {
        var entities = GetCurrentPlotEntities();
        return entities.Find(e => e.positionIndex == positionIndex);
    }
    
    public bool PlotHasEntity(int plotID)
    {
        return GetPlotEntities(plotID).Count > 0;
    }
    
    public bool CurrentPlotHasEntity()
    {
        return GetCurrentPlotEntities().Count > 0;
    }
    
    public bool PlotHasHarvestableEntities(int plotID)
    {
        var entities = GetPlotEntities(plotID);
        return entities.Any(e => e.CanHarvest() && e.accumulatedProducts > 0);
    }
    
    public bool AddEntityToPlot(int plotID, EntityID entityID, int positionIndex)
    {
        var plot = plotService.GetPlot(plotID);
        if (plot == null)
            return false;
            
        var validator = new PlotEntityValidator();
        if (!validator.CanAddEntity(plotID, entityID, positionIndex))
            return false;
        
        // Create new entity
        var newEntity = new FarmEntityInstanceData(entityID, plotID);
        newEntity.positionIndex = positionIndex;
        
        GameDataManager.Instance?.AddFarmEntity(newEntity);
        
        // Update plot status if this is the first entity
        var existingEntities = GetPlotEntities(plotID);
        if (existingEntities.Count == 1) // This entity is the first one
        {
            plot.plotState = PlotState.Occupied;
            plot.occupyingEntityInstanceID = newEntity.instanceID;
            GameDataManager.Instance?.UpdatePlot(plot);
        }
        
        return true;
    }
    
    public bool AddEntityToCurrentPlot(EntityID entityID)
    {
        var currentPlot = plotService.GetCurrentPlot();
        if (currentPlot == null)
            return false;
            
        var utilities = new PlotUtilities();
        int nextPosition = utilities.FindNextAvailablePosition(currentPlot.plotID);
        if (nextPosition == -1)
            return false;
            
        return AddEntityToPlot(currentPlot.plotID, entityID, nextPosition);
    }
    
    public bool RemoveEntityFromPlot(int plotID, int positionIndex)
    {
        var entities = GetPlotEntities(plotID);
        var entityToRemove = entities.Find(e => e.positionIndex == positionIndex);
        
        if (entityToRemove == null)
            return false;
            
        GameDataManager.Instance?.RemoveFarmEntity(entityToRemove.instanceID);
        
        // Update plot status if no entities remain
        var remainingEntities = GetPlotEntities(plotID);
        if (remainingEntities.Count == 0)
        {
            var plot = plotService.GetPlot(plotID);
            if (plot != null)
            {
                plot.ClearPlot();
                GameDataManager.Instance?.UpdatePlot(plot);
            }
        }
        
        return true;
    }
    
    public bool RemoveEntityFromCurrentPlot(int positionIndex)
    {
        var currentPlot = plotService.GetCurrentPlot();
        if (currentPlot == null)
            return false;
            
        return RemoveEntityFromPlot(currentPlot.plotID, positionIndex);
    }
    
    public HarvestResult HarvestAllEntitiesOnPlot(int plotID)
    {
        var entities = GetPlotEntities(plotID);
        var harvestableEntities = entities.Where(e => e.CanHarvest() && e.accumulatedProducts > 0).ToList();
        
        if (harvestableEntities.Count == 0)
        {
            return new HarvestResult { success = false };
        }
        
        var totalResult = new HarvestResult
        {
            success = true,
            itemProduced = ItemID.None,
            amountProduced = 0
        };
        
        var deadEntities = new List<string>();
        int totalBaseAmount = 0;
        
        foreach (var entity in harvestableEntities)
        {
            // Get base amount before harvest (to avoid double equipment bonus)
            int entityBaseAmount = entity.accumulatedProducts;
            
            // Harvest without equipment bonus first
            var result = entity.Harvest();
            if (result.success)
            {
                totalResult.itemProduced = result.itemProduced;
                totalBaseAmount += entityBaseAmount; // Use original accumulated amount
                
                GameDataManager.Instance?.UpdateFarmEntity(entity);
                
                // Check if entity died after harvest
                if (entity.IsDead())
                {
                    deadEntities.Add(entity.instanceID);
                }
            }
        }
        
        // Apply equipment bonus to total harvest amount (only once)
        totalResult.amountProduced = GameDataManager.Instance?.GetBonusAdjustedAmount(totalBaseAmount) ?? totalBaseAmount;
        
        // Add harvested items to inventory with equipment bonus
        if (totalResult.amountProduced > 0)
        {
            GameDataManager.Instance?.AddPlayerItem(totalResult.itemProduced, totalResult.amountProduced);
        }
        
        // Remove dead entities
        foreach (var deadEntityID in deadEntities)
        {
            GameDataManager.Instance?.RemoveFarmEntity(deadEntityID);
        }
        
        // Check if plot should be cleared
        var remainingEntities = GetPlotEntities(plotID);
        if (remainingEntities.Count == 0)
        {
            var plot = plotService.GetPlot(plotID);
            if (plot != null)
            {
                plot.ClearPlot();
                GameDataManager.Instance?.UpdatePlot(plot);
            }
        }
        
        // Save changes if harvest was successful
        if (totalResult.success)
        {
            GameDataManager.Instance?.SaveGame();
            Debug.Log($"Harvested {totalResult.amountProduced} {totalResult.itemProduced} from plot {plotID} (with equipment bonus)");
        }
        
        return totalResult;
    }
    
    public void HandleEntityDeath(int plotID)
    {
        var plot = plotService.GetPlot(plotID);
        if (plot != null)
        {
            var entity = GetCurrentPlotEntity();
            if (entity != null)
            {
                GameDataManager.Instance?.RemoveFarmEntity(entity.instanceID);
            }
            
            plot.ClearPlot();
            GameDataManager.Instance?.UpdatePlot(plot);
        }
    }
    
    public void OnEntityUpdated(FarmEntityInstanceData entity)
    {
        if (entity == null) return;
        
        if (entity.IsDead())
        {
            HandleEntityDeath(entity.associatedPlotID);
        }
    }
}
