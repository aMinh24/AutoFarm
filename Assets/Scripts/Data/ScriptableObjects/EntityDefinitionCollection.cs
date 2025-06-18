using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EntityDefinitionCollection", menuName = "Farm Game/Entity Definition Collection")]
public class EntityDefinitionCollection : ScriptableObject
{
    [Header("Entity Definitions")]
    public List<EntityDefinition> entityDefinitions = new List<EntityDefinition>();

    // Helper methods for easy access
    public EntityDefinition GetEntityDefinition(EntityID entityID)
    {
        return entityDefinitions.FirstOrDefault(e => e.entityID == entityID);
    }

    public List<EntityDefinition> GetEntitiesByType(EntityType entityType)
    {
        return entityDefinitions.Where(e => e.entityType == entityType).ToList();
    }

    public List<EntityDefinition> GetPlants()
    {
        return GetEntitiesByType(EntityType.Plant);
    }

    public List<EntityDefinition> GetAnimals()
    {
        return GetEntitiesByType(EntityType.Animal);
    }
}
