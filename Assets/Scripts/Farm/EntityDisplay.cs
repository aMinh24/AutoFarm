using UnityEngine;

public class EntityDisplay : MonoBehaviour
{
    [Header("Display Components")]
    public SpriteRenderer entitySprite;
    public SpriteRenderer productSprite;
    
    [Header("Entity Settings")]
    public EntityID currentEntityType = EntityID.None;
    public EntityState currentState = EntityState.Dead;
    
    [Header("Position Settings")]
    public int positionIndex = -1;
    
    public int PositionIndex => positionIndex;
    
    /// <summary>
    /// Initialize the entity display with position index
    /// </summary>
    public void Initialize(int position)
    {
        positionIndex = position;
        ClearDisplay();
        
        // Make this clickable for interactions
        var collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
    }
    
    /// <summary>
    /// Update the entity display based on data
    /// </summary>
    public void UpdateDisplay(FarmEntityInstanceData entityData)
    {
        if (entityData == null)
        {
            ClearDisplay();
            return;
        }
        
        currentEntityType = entityData.entityID;
        currentState = entityData.currentState;
        
        UpdateEntitySprite();
        UpdateProductSprite();
    }
    
    /// <summary>
    /// Update entity sprite based on current type
    /// </summary>
    private void UpdateEntitySprite()
    {
        if (entitySprite == null || GameDataManager.Instance == null) return;
        
        if (currentEntityType != EntityID.None)
        {
            var entityDef = GameDataManager.Instance.GetEntity(currentEntityType);
            if (entityDef != null && entityDef.icon != null)
            {
                entitySprite.sprite = entityDef.icon;
                entitySprite.gameObject.SetActive(true);
            }
            else
            {
                entitySprite.gameObject.SetActive(false);
            }
        }
        else
        {
            entitySprite.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update product sprite based on entity state
    /// </summary>
    private void UpdateProductSprite()
    {
        if (productSprite == null || GameDataManager.Instance == null) return;
        
        // Show product sprite if there are accumulated products ready for harvest
        var entity = PlotManager.Instance?.GetCurrentPlotEntityAtPosition(positionIndex);
        if (entity != null && entity.accumulatedProducts > 0 && currentEntityType != EntityID.None)
        {
            var entityDef = GameDataManager.Instance.GetEntity(currentEntityType);
            if (entityDef != null)
            {
                var itemDef = GameDataManager.Instance.GetItem(entityDef.productProducedItemID);
                if (itemDef != null && itemDef.icon != null)
                {
                    productSprite.sprite = itemDef.icon;
                    productSprite.gameObject.SetActive(true);
                    
                    // Add text component to show quantity with equipment bonus
                    var textComponent = productSprite.GetComponentInChildren<TMPro.TextMeshPro>();
                    if (textComponent != null)
                    {
                        int bonusAdjustedAmount = GameDataManager.Instance?.GetBonusAdjustedAmount(entity.accumulatedProducts) ?? entity.accumulatedProducts;
                        if (bonusAdjustedAmount > 1)
                        {
                            textComponent.text = $"x{bonusAdjustedAmount}";
                            textComponent.gameObject.SetActive(true);
                        }
                        else
                        {
                            textComponent.gameObject.SetActive(false);
                        }
                    }
                    return;
                }
            }
        }
        
        productSprite.gameObject.SetActive(false);
        
        // Hide quantity text if exists
        var textComp = productSprite.GetComponentInChildren<TMPro.TextMeshPro>();
        if (textComp != null)
        {
            textComp.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Clear the display
    /// </summary>
    public void ClearDisplay()
    {
        currentEntityType = EntityID.None;
        currentState = EntityState.Dead;
        
        if (entitySprite != null)
            entitySprite.gameObject.SetActive(false);
            
        if (productSprite != null)
            productSprite.gameObject.SetActive(false);
    }
}
