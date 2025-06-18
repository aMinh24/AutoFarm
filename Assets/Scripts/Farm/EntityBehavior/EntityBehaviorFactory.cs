using UnityEngine;

public static class EntityBehaviorFactory
{
    public static IEntityBehavior CreateBehavior(EntityType entityType)
    {
        switch (entityType)
        {
            case EntityType.Plant:
                return new PlantBehavior();
            case EntityType.Animal:
                return new AnimalBehavior();
            default:
                return new PlantBehavior(); // Default fallback
        }
    }
    
    public static IEntityBehavior CreateBehavior(EntityID entityID)
    {
        var entityDef = GameDataManager.Instance?.GetEntity(entityID);
        if (entityDef != null)
        {
            return CreateBehavior(entityDef.entityType);
        }
        
        return new PlantBehavior(); // Default fallback
    }
}
