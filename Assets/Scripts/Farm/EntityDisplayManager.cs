using UnityEngine;
using System.Collections.Generic;

public class EntityDisplayManager : MonoBehaviour
{
    [Header("Entity Display Settings")]
    public Transform[] entityPositions; // Pre-defined positions for entities (10 for trees, 1 for cow)
    
    [Header("Current Plot Configuration")]
    public int maxEntitiesForCurrentPlot = 10; // Can be changed based on plot type
    
    private List<EntityDisplay> entityDisplays = new List<EntityDisplay>();
    private EntityID currentEntityType = EntityID.None;
    
    private void Start()
    {
        InitializeEntityDisplays();
    }
    
    private void InitializeEntityDisplays()
    {
        ClearEntityDisplays();
        
        // Wait for entity data to determine what prefabs to use
        // This will be called from UpdateEntityDisplays when we have entity data
    }
    
    private void CreateEntityDisplaysForType(EntityID entityType, int quantity)
    {
        if (entityType == EntityID.None || GameDataManager.Instance == null)
            return;
            
        var entityDef = GameDataManager.Instance.GetEntity(entityType);
        if (entityDef?.entityPrefab == null)
            return;
        
        currentEntityType = entityType;
        maxEntitiesForCurrentPlot = quantity;
        
        // Create entity displays using the prefab
        for (int i = 0; i < quantity && i < entityPositions.Length; i++)
        {
            CreateEntityDisplayFromPrefab(entityDef.entityPrefab, i);
        }
    }
    
    private void CreateEntityDisplayFromPrefab(GameObject prefab, int positionIndex)
    {
        if (positionIndex >= entityPositions.Length)
            return;
            
        // Instantiate the prefab which should already have EntityDisplay component
        GameObject displayObj = Instantiate(prefab, entityPositions[positionIndex]);
        displayObj.transform.localPosition = Vector3.zero;
        
        // Get the EntityDisplay component from the prefab
        EntityDisplay entityDisplay = displayObj.GetComponent<EntityDisplay>();
        if (entityDisplay == null)
        {
            Debug.LogError($"Prefab {prefab.name} doesn't have EntityDisplay component!");
            DestroyImmediate(displayObj);
            return;
        }
        
        // Initialize the display
        entityDisplay.Initialize(positionIndex);
        entityDisplays.Add(entityDisplay);
    }
    
    public void UpdateEntityDisplays(List<FarmEntityInstanceData> entities)
    {
        // Determine if we need to recreate displays for a different entity type
        if (entities.Count > 0)
        {
            var firstEntity = entities[0];
            var entityDef = GameDataManager.Instance?.GetEntity(firstEntity.entityID);
            
            if (entityDef != null && (currentEntityType != firstEntity.entityID || entityDisplays.Count == 0))
            {
                // Clear and recreate displays for new entity type
                ClearEntityDisplays();
                CreateEntityDisplaysForType(firstEntity.entityID, entityDef.quantityPerPlot);
            }
        }
        
        // Clear all displays first
        foreach (var display in entityDisplays)
        {
            display.ClearDisplay();
        }
        
        // Update displays with entity data
        foreach (var entity in entities)
        {
            if (entity.positionIndex >= 0 && entity.positionIndex < entityDisplays.Count)
            {
                entityDisplays[entity.positionIndex].UpdateDisplay(entity);
            }
        }
    }
    
    public EntityDisplay GetEntityDisplayAtPosition(int positionIndex)
    {
        if (positionIndex >= 0 && positionIndex < entityDisplays.Count)
        {
            return entityDisplays[positionIndex];
        }
        return null;
    }
    
    public void SetMaxEntitiesForPlot(int maxEntities)
    {
        maxEntitiesForCurrentPlot = Mathf.Min(maxEntities, entityPositions.Length);
        InitializeEntityDisplays();
    }
    
    private void ClearEntityDisplays()
    {
        foreach (var display in entityDisplays)
        {
            if (display != null)
            {
                DestroyImmediate(display.gameObject);
            }
        }
        entityDisplays.Clear();
        currentEntityType = EntityID.None;
    }

}