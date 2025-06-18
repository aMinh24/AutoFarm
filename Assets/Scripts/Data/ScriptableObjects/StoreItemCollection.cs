using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreItemCollection", menuName = "Farm Game/Store Item Collection")]
public class StoreItemCollection : ScriptableObject
{
    [Header("Store Item Definitions")]
    public List<StoreItemDefinition> storeItems = new List<StoreItemDefinition>();

    // Helper methods for easy access
    public StoreItemDefinition GetStoreItem(StoreID storeID)
    {
        return storeItems.FirstOrDefault(s => s.storeID == storeID);
    }

    public List<StoreItemDefinition> GetItemsByPurchaseType(PurchaseType purchaseType)
    {
        return storeItems.Where(s => s.purchaseType == purchaseType).ToList();
    }

    public List<StoreItemDefinition> GetSeedItems()
    {
        return GetItemsByPurchaseType(PurchaseType.Seed);
    }

    public List<StoreItemDefinition> GetAnimalItems()
    {
        return GetItemsByPurchaseType(PurchaseType.Animal);
    }

    public List<StoreItemDefinition> GetUpgradeItems()
    {
        return storeItems.Where(s => s.purchaseType == PurchaseType.EquipmentUpgrade || 
                                   s.purchaseType == PurchaseType.Worker || 
                                   s.purchaseType == PurchaseType.Land).ToList();
    }
}
