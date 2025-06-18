using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System;

public class SlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Button button;
    public Image icon;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI priceText;
    
    [Header("Visual States")]
    public Color normalColor = Color.white;
    public Color hoveredColor = Color.yellow;
    public Color selectedColor = Color.green;
    public Color unavailableColor = Color.gray;
    
    private StoreItemDefinition currentItem;
    private bool isSelected = false;
    private bool isAvailable = true;
    
    // Events
    public event Action<SlotUI, StoreItemDefinition> OnSlotClicked;
    public event Action<SlotUI, StoreItemDefinition> OnSlotHovered;
    
    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    public void Setup(StoreItemDefinition item, bool available = true)
    {
        currentItem = item;
        isAvailable = available;
        
        if (item != null)
        {
            SetupIcon(item.icon);
            SetupAmount(item.amount);
            SetupPrice(item.price);
            UpdateVisualState();
        }
        else
        {
            ClearSlot();
        }
    }
    
    private void SetupIcon(Sprite iconSprite)
    {
        if (icon != null)
        {
            icon.sprite = iconSprite;
            icon.enabled = iconSprite != null;
        }
    }
    
    private void SetupAmount(int amount)
    {
        if (amountText != null)
        {
            amountText.text = amount > 1 ? amount.ToString() : string.Empty;
            amountText.enabled = amount > 1;
        }
    }
    
    private void SetupPrice(int price)
    {
        if (priceText != null)
        {
            priceText.text = price > 0 ? $"${price}" : string.Empty;
            priceText.enabled = price > 0;
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();
    }
    
    public void SetAvailable(bool available)
    {
        isAvailable = available;
        UpdateVisualState();
        
        if (button != null)
        {
            button.interactable = available;
        }
    }
    
    private void UpdateVisualState()
    {
        if (icon == null) return;
        
        Color targetColor = normalColor;
        
        if (!isAvailable)
            targetColor = unavailableColor;
        else if (isSelected)
            targetColor = selectedColor;
        
        icon.color = targetColor;
    }
    
    private void OnButtonClicked()
    {
        if (currentItem != null && isAvailable)
        {
            OnSlotClicked?.Invoke(this, currentItem);
        }
    }
    
    public void OnPointerEnter()
    {
        if (currentItem != null && isAvailable)
        {
            OnSlotHovered?.Invoke(this, currentItem);
            if (icon != null && !isSelected)
            {
                icon.color = hoveredColor;
            }
        }
    }
    
    public void OnPointerExit()
    {
        if (currentItem != null)
        {
            UpdateVisualState();
        }
    }
    
    private void ClearSlot()
    {
        SetupIcon(null);
        SetupAmount(0);
        SetupPrice(0);
        currentItem = null;
        isSelected = false;
        isAvailable = true;
        UpdateVisualState();
    }
    
    public StoreItemDefinition GetCurrentItem()
    {
        return currentItem;
    }
}
