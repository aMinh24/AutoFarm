using UnityEngine;
using System.Collections;

public class AnimalBehavior : IEntityBehavior
{
    private Transform entityTransform;
    private Animator animator;
    private FarmEntityInstanceData entityData;
    private Vector3 plotCenter;
    private Vector3 targetPosition;
    private bool isMoving;
    private bool facingRight = true; // Track current facing direction
    
    [Header("Animal Settings")]
    private float moveSpeed = 1f;
    private float roamRadius = 1.5f;
    private float minMoveInterval = 2f;
    private float maxMoveInterval = 5f;
    
    private Coroutine movementCoroutine;
    private MonoBehaviour coroutineRunner;
    
    public void Initialize(Transform entityTransform, FarmEntityInstanceData entityData)
    {
        this.entityTransform = entityTransform;
        this.entityData = entityData;
        this.plotCenter = entityTransform.position;
        this.targetPosition = plotCenter;
        
        // Get animator if available
        animator = entityTransform.GetComponent<Animator>();
        
        // Get coroutine runner from the EntityDisplay component
        coroutineRunner = entityTransform.GetComponent<EntityDisplay>();
        if (coroutineRunner == null)
        {
            coroutineRunner = entityTransform.GetComponent<MonoBehaviour>();
        }
        
        // Start movement behavior
        StartMovementBehavior();
    }
    
    public void UpdateBehavior(float deltaTime)
    {
        if (entityTransform == null) return;
        
        // Handle movement
        if (isMoving)
        {
            MoveTowardsTarget(deltaTime);
        }
        
        // Update animator
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            // animator.SetBool("IsReady", entityData.currentState == EntityState.ReadyToHarvest);
            // animator.SetFloat("ProductionProgress", GetProductionProgress());
        }
    }
    
    public void OnStateChanged(EntityState newState)
    {
        if (animator != null)
        {
            // switch (newState)
            // {
            //     case EntityState.ReadyToHarvest:
            //         animator.SetTrigger("ReadyToProduce");
            //         break;
            //     case EntityState.Decaying:
            //         animator.SetTrigger("StartDecaying");
            //         // Reduce movement when decaying
            //         moveSpeed *= 0.5f;
            //         break;
            // }
        }
    }
    
    public void OnHarvested()
    {
        if (animator != null)
        {
            animator.SetTrigger("Produced");
        }
        
        // Add production effect
        CreateProductionEffect();
    }
    
    public void Cleanup()
    {
        if (movementCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(movementCoroutine);
        }
    }
    
    private void StartMovementBehavior()
    {
        if (coroutineRunner != null)
        {
            movementCoroutine = GameManager.Instance.StartCoroutine(MovementLoop());
        }
    }
    
    private IEnumerator MovementLoop()
    {
        Debug.Log($"Starting movement loop for {entityTransform.name}");
        while (entityTransform != null)
        {
            // Wait for random interval
            float waitTime = Random.Range(minMoveInterval, maxMoveInterval);
            yield return new WaitForSeconds(waitTime);

            // Choose new target position within roam radius
            Vector2 randomDirection = Random.insideUnitCircle * roamRadius;
            targetPosition = plotCenter + new Vector3(randomDirection.x, randomDirection.y, 0);

            isMoving = true;

            // Wait until reached target or timeout
            float moveTimeout = 10f;
            float moveTimer = 0f;

            while (Vector3.Distance(entityTransform.position, targetPosition) > 0.1f && moveTimer < moveTimeout)
            {
                moveTimer += Time.deltaTime;
                yield return null;
            }

            isMoving = false;

            // Small pause at destination
            yield return new WaitForSeconds(Random.Range(1f, 3f));
        }
    }
    
    private void MoveTowardsTarget(float deltaTime)
    {
        Vector3 direction = (targetPosition - entityTransform.position).normalized;
        entityTransform.position = Vector3.MoveTowards(entityTransform.position, targetPosition, moveSpeed * deltaTime);
        
        // Only flip left/right based on horizontal movement direction
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            bool shouldFaceRight = direction.x > 0;
            
            if (shouldFaceRight != facingRight)
            {
                facingRight = shouldFaceRight;
                // Flip the sprite by inverting the x scale
                Vector3 scale = entityTransform.localScale;
                scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
                entityTransform.localScale = scale;
            }
        }
    }
    
    private float GetProductionProgress()
    {
        if (entityData == null) return 0f;
        
        var entityDef = GameDataManager.Instance?.GetEntity(entityData.entityID);
        if (entityDef == null) return 0f;
        
        float totalProductionTime = entityDef.baseProductionTime * 60f;
        float elapsed = totalProductionTime - entityData.timeUntilNextYield;
        
        return Mathf.Clamp01(elapsed / totalProductionTime);
    }
    
    private void CreateProductionEffect()
    {
        // TODO: Add particle effects, sound effects for production
        Debug.Log($"Animal produced at position {entityTransform.position}");
    }
}
