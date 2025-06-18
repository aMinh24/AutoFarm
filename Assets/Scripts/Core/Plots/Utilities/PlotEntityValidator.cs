using System.Linq;

public class PlotEntityValidator
{
    public bool CanAddEntity(int plotID, EntityID entityID, int positionIndex)
    {
        var entityService = new PlotEntityService(null);
        var existingEntities = entityService.GetPlotEntities(plotID);
        
        // Check if position is already occupied
        if (existingEntities.Any(e => e.positionIndex == positionIndex))
            return false;
        
        // Check entity-specific rules
        var entityDef = GameDataManager.Instance?.GetEntity(entityID);
        if (entityDef == null)
            return false;
        
        // Check quantity limit
        var sameTypeEntities = existingEntities.Where(e => e.entityID == entityID).Count();
        if (sameTypeEntities >= entityDef.quantityPerPlot)
            return false;
            
        // Check for mixed entity types
        if (existingEntities.Count > 0 && existingEntities.Any(e => e.entityID != entityID))
            return false;
        
        return true;
    }
}
