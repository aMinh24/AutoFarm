using AutoFarm.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorkerDisplay : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject workerIcon;
    public Slider sliderTimer;
    public TextMeshPro textTimer;
    
    [Header("Display Settings")]
    public bool showIcon = true;
    public bool showSlider = true;
    public bool showTimer = true;
    
    private WorkerData currentWorker;
    private bool isActive = false;
    
    private void Start()
    {
        InitializeDisplay();
    }
    
    private void InitializeDisplay()
    {
        SetActive(false);
        
        if (sliderTimer != null)
        {
            sliderTimer.minValue = 0f;
            sliderTimer.maxValue = 1f;
            sliderTimer.value = 0f;
        }
    }
    
    private void Update()
    {
        if (isActive && currentWorker != null)
        {
            UpdateWorkerProgress();
        }
    }
    
    public void SetWorker(WorkerData worker)
    {
        currentWorker = worker;
        
        if (worker != null && worker.IsBusy())
        {
            SetActive(true);
            UpdateWorkerProgress();
        }
        else
        {
            SetActive(false);
        }
    }
    
    public void ClearWorker()
    {
        currentWorker = null;
        SetActive(false);
    }
    
    private void SetActive(bool active)
    {
        isActive = active;
        
        if (workerIcon != null && showIcon)
        {
            workerIcon.SetActive(active);
        }
        
        if (sliderTimer != null && showSlider)
        {
            sliderTimer.gameObject.SetActive(active);
        }
        
        if (textTimer != null && showTimer)
        {
            textTimer.gameObject.SetActive(active);
        }
    }
    
    private void UpdateWorkerProgress()
    {
        if (currentWorker == null || !currentWorker.IsBusy()) return;
        
        float progress = currentWorker.GetTaskProgress();
        string taskName = GetTaskDisplayName(currentWorker.assignedTask);
        
        // Update slider
        if (sliderTimer != null && showSlider)
        {
            sliderTimer.value = progress;
        }
        
        // Update timer text
        if (textTimer != null && showTimer)
        {
            string timeText = FormatUtilities.FormatTime(currentWorker.timeRemainingOnTask);
            textTimer.text = timeText;
        }
    }
    
    private string GetTaskDisplayName(WorkerTask task)
    {
        switch (task)
        {
            case WorkerTask.Harvest:
                return "Harvesting";
            case WorkerTask.Plant:
                return "Planting";
            case WorkerTask.Milk:
                return "Milking";
            default:
                return "Working";
        }
    }
}
   