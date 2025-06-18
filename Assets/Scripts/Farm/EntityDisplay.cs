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
    
    // Behavior system
    private IEntityBehavior currentBehavior;
    private FarmEntityInstanceData currentEntityData;
    
    public int PositionIndex => positionIndex;
    
    private void Update()
    {
        // Update behavior if active
        if (currentBehavior != null && currentEntityData != null)
        {
            currentBehavior.UpdateBehavior(Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Initialize the entity display with position index
    /// </summary>
    public void Initialize(int position)
    {
        positionIndex = position;
        
        // Find sprite renderers in the prefab if not assigned
        if (entitySprite == null)
        {
            entitySprite = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (productSprite == null)
        {
            // Look for a child object named "ProductSprite" or similar
            Transform productTransform = transform.Find("ProductSprite") ?? transform.Find("Product") ?? transform.Find("Circle");
            if (productTransform != null)
            {
                productSprite = productTransform.GetComponent<SpriteRenderer>();
            }
        }
        
        ClearDisplay();
        
        // Ensure this object has a collider for interactions
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
        
        bool entityTypeChanged = currentEntityType != entityData.entityID;
        bool stateChanged = currentState != entityData.currentState;
        
        currentEntityType = entityData.entityID;
        EntityState previousState = currentState;
        currentState = entityData.currentState;
        currentEntityData = entityData;
        
        // Handle entity type change (setup behavior)
        if (entityTypeChanged)
        {
            SetupEntityBehavior();
        }
        
        // Handle state change
        if (stateChanged && currentBehavior != null)
        {
            currentBehavior.OnStateChanged(currentState);
        }
        
        UpdateEntitySprite();
        UpdateProductSprite();
    }
    
    private void SetupEntityBehavior()
    {
        // Cleanup previous behavior
        if (currentBehavior != null)
        {
            currentBehavior.Cleanup();
            currentBehavior = null;
        }
        
        if (currentEntityType == EntityID.None) return;
        
        // Create appropriate behavior
        currentBehavior = EntityBehaviorFactory.CreateBehavior(currentEntityType);
        if (currentBehavior != null)
        {
            currentBehavior.Initialize(transform, currentEntityData);
        }
    }
    
    /// <summary>
    /// Update entity sprite based on current type
    /// </summary>
    private void UpdateEntitySprite()
    {
        if (entitySprite == null) return;
        
        if (currentEntityType != EntityID.None)
        {
            entitySprite.gameObject.SetActive(true);
            // The sprite should already be set correctly from the prefab
            // but we can override with icon if needed
            if (GameDataManager.Instance != null)
            {
                var entityDef = GameDataManager.Instance.GetEntity(currentEntityType);
                if (entityDef?.icon != null)
                {
                    entitySprite.sprite = entityDef.icon;
                }
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
        if (productSprite == null) return;
        
        // Show product sprite if there are accumulated products ready for harvest
        var entity = PlotManager.Instance?.GetCurrentPlotEntityAtPosition(positionIndex);
        if (entity != null && entity.accumulatedProducts > 0 && currentEntityType != EntityID.None)
        {
            if (GameDataManager.Instance != null)
            {
                var entityDef = GameDataManager.Instance.GetEntity(currentEntityType);
                if (entityDef != null)
                {
                    var itemDef = GameDataManager.Instance.GetItem(entityDef.productProducedItemID);
                    if (itemDef?.icon != null)
                    {
                        productSprite.sprite = itemDef.icon;
                        productSprite.gameObject.SetActive(true);
                        
                        // Add text component to show quantity with equipment bonus
                        var textComponent = productSprite.GetComponentInChildren<TMPro.TextMeshPro>();
                        if (textComponent != null)
                        {
                            int bonusAdjustedAmount = GameDataManager.Instance.GetBonusAdjustedAmount(entity.accumulatedProducts);
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
        // Cleanup behavior
        if (currentBehavior != null)
        {
            currentBehavior.Cleanup();
            currentBehavior = null;
        }
        
        currentState = EntityState.Dead;
        currentEntityData = null;
        
        if (entitySprite != null)
            entitySprite.gameObject.SetActive(false);
            
        if (productSprite != null)
            productSprite.gameObject.SetActive(false);
    }
    
    public void OnHarvested()
    {
        if (currentBehavior != null)
        {
            currentBehavior.OnHarvested();
        }
    }
    
    private void OnDestroy()
    {
        if (currentBehavior != null)
        {
            currentBehavior.Cleanup();
            currentBehavior = null;
        }
    }
}
       