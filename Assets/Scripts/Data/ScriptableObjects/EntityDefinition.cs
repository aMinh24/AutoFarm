using UnityEngine;

[CreateAssetMenu(fileName = "New EntityDefinition", menuName = "Farm Game/Entity Definition")]
public class EntityDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public EntityID entityID;
    public string entityName;
    public EntityType entityType;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon; // Added icon field

    [Header("Production Settings")]
    public float baseProductionTime = 10f; // minutes
    public int baseYieldAmount = 1;
    public int totalYieldsLimit = 40;
    public ItemID productProducedItemID;

    [Header("Planting Requirements")]
    public int quantityPerPlot = 1; // Number of entities required per plot

    [Header("Plant Specific (Only for Plants)")]
    public ItemID seedItemID;

    [Header("Animal Specific (Only for Animals)")]
    public int purchasePrice = 100;

    [Header("Decay Settings")]
    public float decayTimeAfterLastYield = 60f; // minutes
}
