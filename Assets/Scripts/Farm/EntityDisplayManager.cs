using UnityEngine;
using System.Collections.Generic;

public class EntityDisplayManager : MonoBehaviour
{
    [Header("Entity Display Settings")]
    public GameObject entityDisplayPrefab;
    public Transform[] entityPositions; // Pre-defined positions for entities (10 for trees, 1 for cow)
    
    [Header("Current Plot Configuration")]
    public int maxEntitiesForCurrentPlot = 10; // Can be changed based on plot type
    
    private List<EntityDisplay> entityDisplays = new List<EntityDisplay>();
    
    private void Start()
    {
        InitializeEntityDisplays();
    }
    
    private void InitializeEntityDisplays()
    {
        ClearEntityDisplays();
        
        // Create entity displays for all positions
        for (int i = 0; i < entityPositions.Length && i < maxEntitiesForCurrentPlot; i++)
        {
            CreateEntityDisplay(i);
        }
    }
    
    private void CreateEntityDisplay(int positionIndex)
    {
        if (positionIndex >= entityPositions.Length || entityDisplayPrefab == null)
            return;
            
        GameObject displayObj = Instantiate(entityDisplayPrefab, entityPositions[positionIndex]);
        displayObj.transform.localPosition = Vector3.zero;
        
        EntityDisplay entityDisplay = displayObj.GetComponent<EntityDisplay>();
        if (entityDisplay == null)
        {
            entityDisplay = displayObj.AddComponent<EntityDisplay>();
        }
        
        entityDisplay.Initialize(positionIndex);
        entityDisplays.Add(entityDisplay);
    }
    
    public void UpdateEntityDisplays(List<FarmEntityInstanceData> entities)
    {
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
        
        // Ensure we only show displays up to the required quantity for current entity type
        if (entities.Count > 0)
        {
            var firstEntity = entities[0];
            var entityDef = GameDataManager.Instance?.GetEntity(firstEntity.entityID);
            if (entityDef != null)
            {
                maxEntitiesForCurrentPlot = entityDef.quantityPerPlot;
                
                // Hide excess displays if current entity type requires fewer slots
                for (int i = maxEntitiesForCurrentPlot; i < entityDisplays.Count; i++)
                {
                    if (entityDisplays[i] != null)
                    {
                        entityDisplays[i].gameObject.SetActive(false);
                    }
                }
                
                // Show required displays
                for (int i = 0; i < maxEntitiesForCurrentPlot && i < entityDisplays.Count; i++)
                {
                    if (entityDisplays[i] != null)
                    {
                        entityDisplays[i].gameObject.SetActive(true);
                    }
                }
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
    }

}
