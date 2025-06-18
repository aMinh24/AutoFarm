using System;
using Newtonsoft.Json;

public enum PlotState
{
    Empty,
    Occupied,
    Locked // Add for future plot unlocking system
}

[Serializable]
public class PlotData
{
    public int plotID;
    public PlotState plotState;
    public string occupyingEntityInstanceID;

    [JsonConstructor]
    public PlotData()
    {
        plotID = -1;
        plotState = PlotState.Empty;
        occupyingEntityInstanceID = string.Empty;
    }

    public PlotData(int id)
    {
        plotID = id;
        plotState = PlotState.Empty;
        occupyingEntityInstanceID = string.Empty;
    }

    public bool IsEmpty()
    {
        return plotState == PlotState.Empty;
    }

    public bool IsOccupied()
    {
        return plotState == PlotState.Occupied;
    }

    public void OccupyPlot(string entityInstanceID)
    {
        if (string.IsNullOrEmpty(entityInstanceID))
        {
            throw new ArgumentException("Entity instance ID cannot be null or empty");
        }

        plotState = PlotState.Occupied;
        occupyingEntityInstanceID = entityInstanceID;
    }

    public void ClearPlot()
    {
        plotState = PlotState.Empty;
        occupyingEntityInstanceID = string.Empty;
    }

    public bool HasEntity(string entityInstanceID)
    {
        return IsOccupied() && occupyingEntityInstanceID == entityInstanceID;
    }

    public void ValidateState()
    {
        // Ensure consistency between state and occupying entity
        if (plotState == PlotState.Empty && !string.IsNullOrEmpty(occupyingEntityInstanceID))
        {
            occupyingEntityInstanceID = string.Empty;
        }
        else if (plotState == PlotState.Occupied && string.IsNullOrEmpty(occupyingEntityInstanceID))
        {
            plotState = PlotState.Empty;
        }
    }
}
