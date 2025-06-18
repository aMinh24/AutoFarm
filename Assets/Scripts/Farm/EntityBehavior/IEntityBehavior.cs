using UnityEngine;

public interface IEntityBehavior
{
    void Initialize(Transform entityTransform, FarmEntityInstanceData entityData);
    void UpdateBehavior(float deltaTime);
    void OnStateChanged(EntityState newState);
    void OnHarvested();
    void Cleanup();
}
