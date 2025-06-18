using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDefinitionCollection", menuName = "Farm Game/Item Definition Collection")]
public class ItemDefinitionCollection : ScriptableObject
{
    [Header("Item Definitions")]
    public List<ItemDefinition> itemDefinitions = new List<ItemDefinition>();

    // Helper methods for easy access
    public ItemDefinition GetItemDefinition(ItemID itemID)
    {
        return itemDefinitions.FirstOrDefault(i => i.itemID == itemID);
    }

    public List<ItemDefinition> GetItemsByType(ItemType itemType)
    {
        return itemDefinitions.Where(i => i.itemType == itemType).ToList();
    }

    public List<ItemDefinition> GetSeeds()
    {
        return GetItemsByType(ItemType.Seed);
    }

    public List<ItemDefinition> GetProducts()
    {
        return GetItemsByType(ItemType.Product);
    }

    public ItemDefinition GetCurrency()
    {
        return itemDefinitions.FirstOrDefault(i => i.itemType == ItemType.Currency);
    }
}
