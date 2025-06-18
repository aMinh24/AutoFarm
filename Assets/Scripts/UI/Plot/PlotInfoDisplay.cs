using UnityEngine;
using TMPro;
using System.Linq;
using AutoFarm.Utilities;

public class PlotInfoDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshPro entityNameText;
    public TextMeshPro timerText;
    public TextMeshPro plotStatusText;
    
    [Header("Display Settings")]
    public bool showEntityName = true;
    public bool showTimer = true;
    public bool showPlotStatus = true;
    
    [Header("Color Settings")]
    public Color growingColor = Color.yellow;
    public Color readyColor = Color.green;
    public Color decayingColor = Color.red;
    public Color emptyColor = Color.gray;
    
    private int plotID = -1;
    private bool isInitialized = false;
    
    public void Initialize(int plotIndex)
    {
        plotID = plotIndex;
        isInitialized = true;
        
        // Subscribe to plot manager events
        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.OnCurrentPlotChanged += OnPlotChanged;
            PlotManager.Instance.OnPlotUpdated += OnPlotUpdated;
            PlotManager.Instance.OnPlotEntityStatusChanged += OnPlotEntityStatusChanged;
        }
        
        UpdateDisplay();
    }
    
    private void Update()
    {
        if (isInitialized && showTimer)
        {
            UpdateTimerDisplay();
        }
    }
    
    private void OnPlotChanged(int newPlotID)
    {
        if (newPlotID == plotID)
        {
            UpdateDisplay();
        }
    }
    
    private void OnPlotUpdated(int updatedPlotID)
    {
        if (updatedPlotID == plotID)
        {
            UpdateDisplay();
        }
    }
    
    private void OnPlotEntityStatusChanged(int updatedPlotID, bool hasEntity)
    {
        if (updatedPlotID == plotID)
        {
            UpdateDisplay();
        }
    }
    
    public void UpdateDisplay()
    {
        if (!isInitialized || PlotManager.Instance == null)
            return;
            
        var entities = PlotManager.Instance.GetPlotEntities(plotID);
        
        if (entities.Count == 0)
        {
            ShowEmptyPlotInfo();
        }
        else
        {
            // Since all entities in a plot are the same type, get the first one
            var representativeEntity = entities.First();
            ShowEntityInfo(representativeEntity);
        }
    }
    
    private void ShowEmptyPlotInfo()
    {
        if (showEntityName && entityNameText != null)
        {
            entityNameText.text = "Empty Plot";
            entityNameText.color = emptyColor;
        }
        
        if (showTimer && timerText != null)
        {
            timerText.text = "";
        }
        
        if (showPlotStatus && plotStatusText != null)
        {
            plotStatusText.text = "Ready to Plant";
            plotStatusText.color = emptyColor;
        }
    }
    
    private void ShowEntityInfo(FarmEntityInstanceData entity)
    {
        if (GameDataManager.Instance == null) return;
        
        var entityDef = GameDataManager.Instance.GetEntity(entity.entityID);
        if (entityDef == null) return;
        
        // Get all entities on the plot
        var allEntities = PlotManager.Instance.GetPlotEntities(plotID);
        int entityCount = allEntities.Count;
        
        // Update entity name with count
        if (showEntityName && entityNameText != null)
        {
            string displayName = $"{entityDef.entityName} ({entityCount}/{entityDef.quantityPerPlot})";
            entityNameText.text = displayName;
            entityNameText.color = GetStateColor(entity.currentState);
        }
        
        // Update status
        if (showPlotStatus && plotStatusText != null)
        {
            plotStatusText.text = GetStatusText(entity.currentState);
            plotStatusText.color = GetStateColor(entity.currentState);
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (!showTimer || timerText == null || PlotManager.Instance == null)
            return;
            
        var entities = PlotManager.Instance.GetPlotEntities(plotID);
        if (entities.Count == 0)
        {
            timerText.text = "";
            return;
        }
        
        var representativeEntity = entities.First();
        string timerDisplay = GetTimerText(representativeEntity);
        timerText.text = timerDisplay;
        timerText.color = GetStateColor(representativeEntity.currentState);
    }
    
    private string GetTimerText(FarmEntityInstanceData entity)
    {
        switch (entity.currentState)
        {
            case EntityState.Growing:
                return FormatUtilities.FormatTime(entity.timeUntilNextYield);
                
            case EntityState.ReadyToHarvest:
                if (entity.CanProduceMore())
                {
                    return FormatUtilities.FormatTime(entity.timeUntilNextYield);
                }
                else
                {
                    return FormatUtilities.FormatTime(entity.timeUntilDecay);
                }
                
            case EntityState.Decaying:
                return FormatUtilities.FormatTime(entity.timeUntilDecay);
                
            case EntityState.Dead:
                return "Dead";
                
            default:
                return "";
        }
    }

    private string GetStatusText(EntityState state)
    {
        switch (state)
        {
            case EntityState.Growing:
                return "Growing";
            case EntityState.ReadyToHarvest:
                return "Ready to Harvest";
            case EntityState.Decaying:
                return "Decaying - Harvest Soon!";
            case EntityState.Dead:
                return "Dead";
            default:
                return "";
        }
    }
    
    private Color GetStateColor(EntityState state)
    {
        switch (state)
        {
            case EntityState.Growing:
                return growingColor;
            case EntityState.ReadyToHarvest:
                return readyColor;
            case EntityState.Decaying:
                return decayingColor;
            case EntityState.Dead:
                return emptyColor;
            default:
                return emptyColor;
        }
    }
    
    private void OnDestroy()
    {
        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.OnCurrentPlotChanged -= OnPlotChanged;
            PlotManager.Instance.OnPlotUpdated -= OnPlotUpdated;
            PlotManager.Instance.OnPlotEntityStatusChanged -= OnPlotEntityStatusChanged;
        }
    }

}