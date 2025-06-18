using System.Collections.Generic;

public interface IPlotEntityService
{
    List<FarmEntityInstanceData> GetPlotEntities(int plotID);
    List<FarmEntityInstanceData> GetCurrentPlotEntities();
    FarmEntityInstanceData GetPlotEntity(int plotID);
    FarmEntityInstanceData GetCurrentPlotEntity();
    FarmEntityInstanceData GetCurrentPlotEntityAtPosition(int positionIndex);
    bool PlotHasEntity(int plotID);
    bool CurrentPlotHasEntity();
    bool PlotHasHarvestableEntities(int plotID);
    bool AddEntityToPlot(int plotID, EntityID entityID, int positionIndex);
    bool AddEntityToCurrentPlot(EntityID entityID);
    bool RemoveEntityFromPlot(int plotID, int positionIndex);
    bool RemoveEntityFromCurrentPlot(int positionIndex);
    HarvestResult HarvestAllEntitiesOnPlot(int plotID);
    void HandleEntityDeath(int plotID);
    void OnEntityUpdated(FarmEntityInstanceData entity);
}
