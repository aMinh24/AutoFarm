using UnityEngine;

[CreateAssetMenu(fileName = "New StoreItemDefinition", menuName = "Farm Game/Store Item Definition")]
public class StoreItemDefinition : ScriptableObject
{
    [Header("Store Info")]
    public StoreID storeID;
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public PurchaseType purchaseType;
    public Sprite icon; // Added icon field

    [Header("Purchase Details")]
    public int amount = 1;
    public int price = 0;

    [Header("References")]
    public ItemID referencedItemID;
    public EntityID referencedEntityID;
}
