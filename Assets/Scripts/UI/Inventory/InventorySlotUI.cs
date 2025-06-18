using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Button button;
    public Image icon;
    public TextMeshProUGUI amountText;
    
    [Header("Visual States")]
    public Color normalColor = Color.white;
    public Color hoveredColor = Color.yellow;
    public Color selectedColor = Color.green;
    public Color emptyColor = Color.gray;
    
    private ItemDefinition currentItem;
    private int currentAmount;
    private bool isSelected = false;
    private bool isEmpty = true;
    
    // Events
    public event Action<InventorySlotUI, ItemDefinition, int> OnSlotClicked;
    public event Action<InventorySlotUI, ItemDefinition, int> OnSlotHovered;
    
    private void Awake()
    {
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
        }
    }
    
    public void Setup(ItemDefinition item, int amount)
    {
        currentItem = item;
        currentAmount = amount;
        isEmpty = item == null || amount <= 0;
        
        if (!isEmpty)
        {
            SetupIcon(item.icon);
            SetupAmount(amount);
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
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();
    }
    
    private void UpdateVisualState()
    {
        if (icon == null) return;
        
        Color targetColor = normalColor;
        
        if (isEmpty)
            targetColor = emptyColor;
        else if (isSelected)
            targetColor = selectedColor;
        
        icon.color = targetColor;
    }
    
    private void OnButtonClicked()
    {
        if (!isEmpty)
        {
            OnSlotClicked?.Invoke(this, currentItem, currentAmount);
        }
    }
    
    public void OnPointerEnter()
    {
        if (!isEmpty)
        {
            OnSlotHovered?.Invoke(this, currentItem, currentAmount);
            if (icon != null && !isSelected)
            {
                icon.color = hoveredColor;
            }
        }
    }
    
    public void OnPointerExit()
    {
        if (!isEmpty)
        {
            UpdateVisualState();
        }
    }
    
    private void ClearSlot()
    {
        SetupIcon(null);
        SetupAmount(0);
        currentItem = null;
        currentAmount = 0;
        isEmpty = true;
        isSelected = false;
        UpdateVisualState();
    }
    
    public ItemDefinition GetCurrentItem()
    {
        return currentItem;
    }
    
    public int GetCurrentAmount()
    {
        return currentAmount;
    }
    
    public bool IsEmpty()
    {
        return isEmpty;
    }
}