using UnityEngine;

[CreateAssetMenu(fileName = "New ItemDefinition", menuName = "Farm Game/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Basic Info")]
    public ItemID itemID;
    public string itemName;
    public ItemType itemType;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon; // Added icon field

    [Header("Economic")]
    public int baseSalePrice;
    public int basePurchasePrice;
    public int purchasePackSize = 1;

    [Header("Relationships")]
    public EntityID growsIntoEntityID = EntityID.None;
    public EntityID producedByEntityID = EntityID.None;
}
