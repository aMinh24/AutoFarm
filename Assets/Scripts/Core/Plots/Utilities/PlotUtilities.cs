using System.Collections.Generic;
using System.Linq;

public class PlotUtilities
{
    public int FindNextAvailablePosition(int plotID)
    {
        var entityService = new PlotEntityService(null); // Will be injected properly in full implementation
        var existingEntities = entityService.GetPlotEntities(plotID);
        var occupiedPositions = new HashSet<int>();
        
        foreach (var entity in existingEntities)
        {
            occupiedPositions.Add(entity.positionIndex);
        }
        
        int maxPositions = 10; // Configurable based on plot type
        
        for (int i = 0; i < maxPositions; i++)
        {
            if (!occupiedPositions.Contains(i))
            {
                return i;
            }
        }
        
        return -1;
    }
    
    public bool IsPositionOccupied(int plotID, int positionIndex)
    {
        var entityService = new PlotEntityService(null);
        var entities = entityService.GetPlotEntities(plotID);
        return entities.Any(e => e.positionIndex == positionIndex);
    }
    
    public int GetMaxPositionsForPlot(int plotID)
    {
        // This could be made configurable based on plot type or upgrades
        return 10;
    }
}
