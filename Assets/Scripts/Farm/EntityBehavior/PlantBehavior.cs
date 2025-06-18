using UnityEngine;

public class PlantBehavior : IEntityBehavior
{
    private Transform entityTransform;
    private Animator animator;
    private FarmEntityInstanceData entityData;
    private Vector3 originalPosition;
    
    [Header("Plant Settings")]
    private float idleAmplitude = 0.05f;
    private float idleFrequency = 1f;
    private float time;
    
    public void Initialize(Transform entityTransform, FarmEntityInstanceData entityData)
    {
        this.entityTransform = entityTransform;
        this.entityData = entityData;
        this.originalPosition = entityTransform.position;
        
        // Get animator if available
        animator = entityTransform.GetComponent<Animator>();
        
        // Start idle animation
        StartIdleAnimation();
    }
    
    public void UpdateBehavior(float deltaTime)
    {
        // if (entityTransform == null) return;
        
        // time += deltaTime;
        
        // // Gentle swaying animation for plants
        // Vector3 newPosition = originalPosition;
        // newPosition.x += Mathf.Sin(time * idleFrequency) * idleAmplitude;
        // newPosition.y += Mathf.Sin(time * idleFrequency * 0.7f) * idleAmplitude * 0.5f;
        
        // entityTransform.position = newPosition;
        
        // Update animator if available
        if (animator != null)
        {
            // animator.SetFloat("GrowthProgress", GetGrowthProgress());
            // animator.SetBool("IsReady", entityData.currentState == EntityState.ReadyToHarvest);
        }
    }
    
    public void OnStateChanged(EntityState newState)
    {
        if (animator != null)
        {
            // switch (newState)
            // {
            //     case EntityState.Growing:
            //         animator.SetTrigger("StartGrowing");
            //         break;
            //     case EntityState.ReadyToHarvest:
            //         animator.SetTrigger("ReadyToHarvest");
            //         break;
            //     case EntityState.Decaying:
            //         animator.SetTrigger("StartDecaying");
            //         break;
            // }
        }
    }
    
    public void OnHarvested()
    {
        if (animator != null)
        {
            animator.SetTrigger("Harvested");
        }
        
        // Add harvest effect (particles, sound, etc.)
        CreateHarvestEffect();
    }
    
    public void Cleanup()
    {
        // Clean up any effects or animations
    }
    
    private void StartIdleAnimation()
    {
        if (animator != null)
        {
            // animator.SetBool("IsIdle", true);
        }
    }
    
    private float GetGrowthProgress()
    {
        if (entityData == null) return 0f;
        
        var entityDef = GameDataManager.Instance?.GetEntity(entityData.entityID);
        if (entityDef == null) return 0f;
        
        float totalGrowthTime = entityDef.baseProductionTime * 60f;
        float elapsed = totalGrowthTime - entityData.timeUntilNextYield;
        
        return Mathf.Clamp01(elapsed / totalGrowthTime);
    }
    
    private void CreateHarvestEffect()
    {
        // TODO: Add particle effects, sound effects for harvest
        Debug.Log($"Plant harvested at position {entityTransform.position}");
    }
}
