using UnityEngine;

public class PlotPlantingService
{
    private readonly IPlotService plotService;
    private readonly IPlotEntityService entityService;
    
    public PlotPlantingService(IPlotService plotService, IPlotEntityService entityService)
    {
        this.plotService = plotService;
        this.entityService = entityService;
    }
    
    public PlantingResult PlantItemOnPlot(int plotID, ItemID itemID)
    {
        // Validate plot
        var plot = plotService.GetPlot(plotID);
        if (plot == null || !plot.IsEmpty())
        {
            return new PlantingResult { success = false, errorMessage = "Plot is not empty or invalid" };
        }
        
        // Validate item
        var itemDef = GameDataManager.Instance?.GetItem(itemID);
        if (itemDef == null || itemDef.growsIntoEntityID == EntityID.None)
        {
            return new PlantingResult { success = false, errorMessage = "Item cannot be planted" };
        }
        
        // Validate entity definition
        var entityDef = GameDataManager.Instance?.GetEntity(itemDef.growsIntoEntityID);
        if (entityDef == null)
        {
            return new PlantingResult { success = false, errorMessage = "Invalid entity definition" };
        }
        
        int requiredQuantity = entityDef.quantityPerPlot;
        
        // Check if player has enough items
        if (!GameDataManager.Instance.HasPlayerItem(itemID, requiredQuantity))
        {
            return new PlantingResult 
            { 
                success = false, 
                errorMessage = $"Not enough items. Required: {requiredQuantity}" 
            };
        }
        
        // Remove items from inventory first
        if (!GameDataManager.Instance.RemovePlayerItem(itemID, requiredQuantity))
        {
            return new PlantingResult { success = false, errorMessage = "Failed to remove items from inventory" };
        }
        
        // Create entities
        bool allEntitiesPlaced = true;
        var placedEntities = new System.Collections.Generic.List<string>();
        
        for (int i = 0; i < requiredQuantity; i++)
        {
            var newEntity = new FarmEntityInstanceData(itemDef.growsIntoEntityID, plotID, i);
            GameDataManager.Instance.AddFarmEntity(newEntity);
            placedEntities.Add(newEntity.instanceID);
            
            // Update plot state for first entity
            if (i == 0)
            {
                plot.occupyingEntityInstanceID = newEntity.instanceID;
                plot.plotState = PlotState.Occupied;
                GameDataManager.Instance.UpdatePlot(plot);
            }
        }
        
        if (allEntitiesPlaced)
        {
            // Save changes
            GameDataManager.Instance.SaveGame();
            
            Debug.Log($"Successfully planted {requiredQuantity} {itemDef.growsIntoEntityID} on plot {plotID}");
            
            return new PlantingResult 
            { 
                success = true, 
                plotID = plotID,
                entityID = itemDef.growsIntoEntityID,
                quantityPlanted = requiredQuantity
            };
        }
        else
        {
            // Rollback if failed
            foreach (var entityID in placedEntities)
            {
                GameDataManager.Instance.RemoveFarmEntity(entityID);
            }
            
            // Add items back to inventory
            GameDataManager.Instance.AddPlayerItem(itemID, requiredQuantity);
            
            return new PlantingResult { success = false, errorMessage = "Failed to place all entities" };
        }
    }
    
    public PlantingResult PlantItemOnCurrentPlot(ItemID itemID)
    {
        var currentPlot = plotService.GetCurrentPlot();
        if (currentPlot == null)
        {
            return new PlantingResult { success = false, errorMessage = "No current plot available" };
        }
        
        return PlantItemOnPlot(currentPlot.plotID, itemID);
    }
    
    public bool CanPlantOnPlot(int plotID, ItemID itemID)
    {
        var plot = plotService.GetPlot(plotID);
        if (plot == null || !plot.IsEmpty()) return false;
        
        var itemDef = GameDataManager.Instance?.GetItem(itemID);
        if (itemDef?.growsIntoEntityID == EntityID.None) return false;
        
        var entityDef = GameDataManager.Instance?.GetEntity(itemDef.growsIntoEntityID);
        if (entityDef == null) return false;
        
        return GameDataManager.Instance.HasPlayerItem(itemID, entityDef.quantityPerPlot);
    }
}

[System.Serializable]
public struct PlantingResult
{
    public bool success;
    public string errorMessage;
    public int plotID;
    public EntityID entityID;
    public int quantityPlanted;
}
