using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AutoFarm.Utilities;

public class MainUI : BaseScreen
{
    [Header("UI Buttons")]
    public Button shopButton;
    public Button inventoryButton;
    
    [Header("Plot Navigation")]
    public Button previousPlotButton;
    public Button nextPlotButton;
    public TMPro.TextMeshProUGUI plotInfoText;
    
    [Header("Plot Action")]
    public Button plotActionButton;
    public TMPro.TextMeshProUGUI plotActionText;

    [Header("Resource Displays")]
    public TextMeshProUGUI workerCountText;
    public TextMeshProUGUI goldCountText;
    public TextMeshProUGUI equipmentLevelText;
    
    [Header("Resource Display Settings")]
    public bool useEvents = true;
    public float fallbackUpdateInterval = 2f;
    
    private float fallbackTimer;

    public override void Init()
    {
        base.Init();
        SetupButtons();
    }
    
    private void SetupButtons()
    {
        if (shopButton != null)
            shopButton.onClick.AddListener(OpenShop);
            
        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(OpenInventory);
            
        if (previousPlotButton != null)
            previousPlotButton.onClick.AddListener(GoToPreviousPlot);
            
        if (nextPlotButton != null)
            nextPlotButton.onClick.AddListener(GoToNextPlot);
            
        if (plotActionButton != null)
            plotActionButton.onClick.AddListener(OnPlotActionClicked);
    }
    
    private void SetupResourceDisplays()
    {
        if (useEvents && GameDataManager.Instance != null)
        {
            // Subscribe to events for real-time updates
            GameDataManager.Instance.OnGoldChanged += UpdateGoldDisplay;
            GameDataManager.Instance.OnWorkerCountChanged += UpdateWorkerDisplay;
            GameDataManager.Instance.OnInventoryChanged += RefreshAllDisplays;
            GameDataManager.Instance.OnEquipmentLevelChanged += UpdateEquipmentLevelDisplay;
        }
        
        // Subscribe to plot changes
        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.OnCurrentPlotChanged += UpdatePlotInfo;
            PlotManager.Instance.OnCurrentPlotChanged += UpdatePlotActionButton;
            PlotManager.Instance.OnPlotUpdated += OnPlotUpdated;
            PlotManager.Instance.OnPlotEntityStatusChanged += OnPlotEntityStatusChanged;
        }
    }
    
    private void Update()
    {
        // Fallback update mechanism if events are not used
        if (!useEvents)
        {
            fallbackTimer += Time.deltaTime;
            if (fallbackTimer >= fallbackUpdateInterval)
            {
                UpdateResourceDisplays();
                fallbackTimer = 0f;
            }
        }
    }
    
    private void UpdateResourceDisplays()
    {
        UpdateGoldDisplay();
        UpdateWorkerDisplay();
        UpdateEquipmentLevelDisplay();
    }
    
    private void UpdateGoldDisplay()
    {
        if (goldCountText != null && GameDataManager.Instance != null)
        {
            long currentGold = GameDataManager.Instance.GetPlayerGold();
            goldCountText.text = FormatUtilities.FormatNumber(currentGold);
            
            // Check win condition
            CheckWinCondition(currentGold);
        }
    }
    
    private void CheckWinCondition(long currentGold)
    {
        var gameSettings = GameDataManager.Instance?.gameSettings;
        if (gameSettings != null && currentGold >= gameSettings.winConditionGold)
        {
            Debug.Log($"🎉 VICTORY! Player has reached win condition! Current Gold: {FormatUtilities.FormatCurrency(currentGold)} >= Target: {FormatUtilities.FormatCurrency(gameSettings.winConditionGold)}");
            
            // Optional: Show victory UI or trigger win event
            // You can add more win condition logic here
        }
    }
    
    private void UpdateWorkerDisplay()
    {
        if (workerCountText != null && GameDataManager.Instance != null)
        {
            int availableWorkers = GameDataManager.Instance.GetAvailableWorkers();
            int totalWorkers = GameDataManager.Instance.PlayerData?.GetPlayerData()?.totalWorkersHired ?? 0;
            
            // Enhanced display to show busy workers
            int busyWorkers = totalWorkers - availableWorkers;
            workerCountText.text = $"{availableWorkers}/{totalWorkers}";
            
            // Change color based on availability
            if (availableWorkers == 0 && totalWorkers > 0)
            {
                workerCountText.color = UnityEngine.Color.red; // All busy
            }
            else if (availableWorkers == totalWorkers)
            {
                workerCountText.color = UnityEngine.Color.green; // All available
            }
            else
            {
                workerCountText.color = UnityEngine.Color.yellow; // Some busy
            }
        }
    }
    
    private void UpdateEquipmentLevelDisplay()
    {
        if (equipmentLevelText != null && GameDataManager.Instance != null)
        {
            int equipmentLevel = GameDataManager.Instance.PlayerData?.GetPlayerData()?.equipmentLevel ?? 1;
            equipmentLevelText.text = $"Level {equipmentLevel}";
        }
    }
    
    private string FormatNumber(long number)
    {
        return FormatUtilities.FormatNumber(number);
    }
    
    // Public methods to manually trigger updates
    public void RefreshGoldDisplay()
    {
        UpdateGoldDisplay();
    }
    
    public void RefreshWorkerDisplay()
    {
        UpdateWorkerDisplay();
    }
    
    public void RefreshEquipmentLevelDisplay()
    {
        UpdateEquipmentLevelDisplay();
    }
    
    public void RefreshAllDisplays()
    {
        UpdateResourceDisplays();
    }
    
    private void OpenShop()
    {
        UIManager.Instance?.ShowPopup<Shop>(null);
    }
    
    private void OpenInventory()
    {
        UIManager.Instance?.ShowPopup<InventoryUI>(null);
    }

    private void GoToPreviousPlot()
    {
        PlotManager.Instance?.PreviousPlot();
    }
    
    private void GoToNextPlot()
    {
        PlotManager.Instance?.NextPlot();
    }
    
    private void UpdatePlotInfo(int plotID)
    {
        if (plotInfoText != null && PlotManager.Instance != null)
        {
            var plotInfo = PlotManager.Instance.GetCurrentPlotInfo();
            int displayPlotNumber = plotID + 1;
            int totalPlots = plotInfo.totalPlots;
            plotInfoText.text = $"Plot {displayPlotNumber}/{totalPlots}";
        }
    }
    
    private void UpdatePlotActionButton(int plotID)
    {
        if (plotActionButton == null || PlotManager.Instance == null) return;
        
        var plotInfo = PlotManager.Instance.GetCurrentPlotInfo();
        var entities = PlotManager.Instance.GetCurrentPlotEntities();
        
        // Check if worker is working on this plot
        bool hasWorkerWorking = false;
        if (WorkerManager.Instance != null)
        {
            var busyWorkers = WorkerManager.Instance.GetBusyWorkers();
            hasWorkerWorking = busyWorkers.Any(w => 
                int.TryParse(w.taskTargetInstanceID, out int targetPlotID) && 
                targetPlotID == plotID);
        }
        
        if (hasWorkerWorking)
        {
            // Worker is working - show worker status
            SetPlotActionButton("Worker Busy", false);
        }
        else if (plotInfo.isEmpty)
        {
            // Plot is empty - show Farm button
            SetPlotActionButton("Farm", true);
        }
        else if (entities.Count > 0)
        {
            // Check if any entity can be harvested
            bool canHarvest = false;
            int totalProducts = 0;
            
            foreach (var entity in entities)
            {
                if (entity.CanHarvest() && entity.accumulatedProducts > 0)
                {
                    canHarvest = true;
                    totalProducts += entity.accumulatedProducts;
                }
            }
            
            if (canHarvest)
            {
                // Apply equipment bonus to display
                int bonusAdjustedTotal = GameDataManager.Instance?.GetBonusAdjustedAmount(totalProducts) ?? totalProducts;
                string buttonText = bonusAdjustedTotal > 1 ? $"Harvest (x{bonusAdjustedTotal})" : "Harvest";
                SetPlotActionButton(buttonText, true);
            }
            else
            {
                // Entities exist but can't harvest - hide button
                SetPlotActionButton("", false);
            }
        }
        else
        {
            // Fallback - hide button
            SetPlotActionButton("", false);
        }
    }
    
    private void SetPlotActionButton(string text, bool visible)
    {
        if (plotActionButton != null)
        {
            plotActionButton.gameObject.SetActive(visible);
        }
        
        if (plotActionText != null)
        {
            plotActionText.text = text;
        }
    }
    
    private void OnPlotUpdated(int plotID)
    {
        if (PlotManager.Instance != null && plotID == PlotManager.Instance.GetCurrentPlotIndex())
        {
            UpdatePlotActionButton(plotID);
        }
    }
    
    private void OnPlotEntityStatusChanged(int plotID, bool hasEntity)
    {
        if (PlotManager.Instance != null && plotID == PlotManager.Instance.GetCurrentPlotIndex())
        {
            UpdatePlotActionButton(plotID);
        }
    }
    
    private void OnPlotActionClicked()
    {
        if (PlotManager.Instance == null) return;
        
        var plotInfo = PlotManager.Instance.GetCurrentPlotInfo();
        
        if (plotInfo.isEmpty)
        {
            // Open inventory for farming
            OpenFarmingInventory();
        }
        else
        {
            // Try to harvest
            HarvestCurrentPlot();
        }
    }
    
    private void OpenFarmingInventory()
    {
        // Open inventory in farming mode
        UIManager.Instance?.ShowPopup<InventoryUI>("farming");
    }
    
    private void HarvestCurrentPlot()
    {
        if (PlotManager.Instance == null) return;
        
        var result = PlotManager.Instance.HarvestCurrentPlotEntities();
        
        if (result.success)
        {
            Debug.Log($"Harvested {result.totalAmount} {result.itemName}");
            // UI could show harvest result notification here if needed
        }
    }

    public override void Show(object data = null)
    {
        base.Show(data);
        SetupResourceDisplays();
        UpdateResourceDisplays();
        
        // Update plot info on show
        if (PlotManager.Instance != null)
        {
            UpdatePlotInfo(PlotManager.Instance.GetCurrentPlotIndex());
        }
    }
    
    public override void Hide()
    {
        base.Hide();
        // Unsubscribe from events when hiding
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
            GameDataManager.Instance.OnWorkerCountChanged -= UpdateWorkerDisplay;
            GameDataManager.Instance.OnInventoryChanged -= RefreshAllDisplays;
            GameDataManager.Instance.OnEquipmentLevelChanged -= UpdateEquipmentLevelDisplay;
        }
        
        // Unsubscribe from plot events
        if (PlotManager.Instance != null)
        {
            PlotManager.Instance.OnCurrentPlotChanged -= UpdatePlotInfo;
            PlotManager.Instance.OnCurrentPlotChanged -= UpdatePlotActionButton;
            PlotManager.Instance.OnPlotUpdated -= OnPlotUpdated;
            PlotManager.Instance.OnPlotEntityStatusChanged -= OnPlotEntityStatusChanged;
        }
    }
}