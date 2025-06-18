using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class DataGenerator
{
    private const string GameDataPath = "Assets/Resources/GameData";
    private const string ItemsPath = GameDataPath + "/Items";
    private const string EntitiesPath = GameDataPath + "/Entities";
    private const string StoreItemsPath = GameDataPath + "/StoreItems";

    [MenuItem("Farm Game/Generate Default Game Data")]
    public static void GenerateDefaultData()
    {
        CreateDirectories();

        // Create GameSettings
        GameSettings gameSettings = CreateGameSettings();

        // Create ItemDefinitions
        ItemDefinition goldItem = CreateItemDefinition("Gold", ItemID.Gold, ItemType.Currency, "The primary currency.", 0, 0);
        ItemDefinition tomatoSeed = CreateItemDefinition("TomatoSeed", ItemID.TomatoSeed, ItemType.Seed, "Grows into a tomato plant.", 0, 30, 1, EntityID.TOMATO_PLANT);
        ItemDefinition blueberrySeed = CreateItemDefinition("BlueberrySeed", ItemID.BlueberrySeed, ItemType.Seed, "Grows into a blueberry plant.", 0, 50, 1, EntityID.BLUEBERRY_PLANT);
        ItemDefinition strawberrySeed = CreateItemDefinition("StrawberrySeed", ItemID.StrawberrySeed, ItemType.Seed, "Grows into a strawberry plant.", 0, 40, 10, EntityID.STRAWBERRY_PLANT); // Price per seed, pack size 10
        ItemDefinition cowItem = CreateItemDefinition("CowItem", ItemID.Cow, ItemType.Animal, "A dairy cow, ready to be placed.", 0, 100, 1, EntityID.MILK_COW);


        ItemDefinition tomatoProduct = CreateItemDefinition("Tomato", ItemID.Tomato, ItemType.Product, "A ripe, juicy tomato.", 5, 0, 1, EntityID.None, EntityID.TOMATO_PLANT);
        ItemDefinition blueberryProduct = CreateItemDefinition("Blueberry", ItemID.Blueberry, ItemType.Product, "A sweet, ripe blueberry.", 8, 0, 1, EntityID.None, EntityID.BLUEBERRY_PLANT);
        ItemDefinition strawberryProduct = CreateItemDefinition("Strawberry", ItemID.Strawberry, ItemType.Product, "A delicious, red strawberry.", 6, 0, 1, EntityID.None, EntityID.STRAWBERRY_PLANT); // Assumed sale price 6
        ItemDefinition milkProduct = CreateItemDefinition("Milk", ItemID.Milk, ItemType.Product, "A gallon of fresh milk.", 15, 0, 1, EntityID.None, EntityID.MILK_COW);

        // Create EntityDefinitions
        EntityDefinition tomatoPlant = CreateEntityDefinition("TomatoPlant", EntityID.TOMATO_PLANT, "Tomato Plant", EntityType.Plant, "Grows delicious tomatoes.", 10f, 1, 40, ItemID.Tomato, ItemID.TomatoSeed, 0, 60f);
        EntityDefinition blueberryPlant = CreateEntityDefinition("BlueberryPlant", EntityID.BLUEBERRY_PLANT, "Blueberry Plant", EntityType.Plant, "Grows sweet blueberries.", 15f, 1, 40, ItemID.Blueberry, ItemID.BlueberrySeed, 0, 60f);
        EntityDefinition strawberryPlant = CreateEntityDefinition("StrawberryPlant", EntityID.STRAWBERRY_PLANT, "Strawberry Plant", EntityType.Plant, "Grows juicy strawberries.", 5f, 1, 20, ItemID.Strawberry, ItemID.StrawberrySeed, 0, 60f);
        EntityDefinition milkCow = CreateEntityDefinition("MilkCow", EntityID.MILK_COW, "Dairy Cow", EntityType.Animal, "Produces fresh milk.", 30f, 1, 100, ItemID.Milk, ItemID.None, 100, 60f);

        // Create StoreItemDefinitions
        StoreItemDefinition tomatoSeedSale = CreateStoreItem("TomatoSeedSale", StoreID.TomatoSeedSale, "Tomato Seeds", "Buy tomato seeds.", PurchaseType.Seed, 1, 30, tomatoSeed.itemID, EntityID.None);
        StoreItemDefinition blueberrySeedSale = CreateStoreItem("BlueberrySeedSale", StoreID.BlueberrySeedSale, "Blueberry Seeds", "Buy blueberry seeds.", PurchaseType.Seed, 1, 50, blueberrySeed.itemID, EntityID.None);
        StoreItemDefinition strawberrySeedPackSale = CreateStoreItem("StrawberrySeedPackSale", StoreID.StrawberrySeedPackSale, "Strawberry Seed Pack (10)", "Buy a pack of 10 strawberry seeds.", PurchaseType.Seed, 10, 400, strawberrySeed.itemID, EntityID.None);
        StoreItemDefinition dairyCowSale = CreateStoreItem("DairyCowSale", StoreID.DairyCowSale, "Dairy Cow", "Buy a dairy cow.", PurchaseType.Animal, 1, 100, ItemID.Cow, milkCow.entityID);
        StoreItemDefinition hireWorker = CreateStoreItem("HireWorker", StoreID.HireWorker, "Hire Worker", "Hire an additional worker.", PurchaseType.Worker, 1, gameSettings.cost_HireWorker, ItemID.None, EntityID.None);
        StoreItemDefinition upgradeEquipment = CreateStoreItem("UpgradeEquipment", StoreID.UpgradeEquipment, "Upgrade Equipment", "Upgrade farm equipment.", PurchaseType.EquipmentUpgrade, 1, gameSettings.cost_UpgradeEquipment, ItemID.None, EntityID.None);
        StoreItemDefinition buyLandPlot = CreateStoreItem("BuyLandPlot", StoreID.BuyLandPlot, "Buy Land Plot", "Expand your farm.", PurchaseType.Land, 1, gameSettings.cost_BuyLandPlot, ItemID.None, EntityID.None);

        // Create Collections
        ItemDefinitionCollection itemCollection = CreateCollection<ItemDefinitionCollection>("ItemDefinitionCollection", ItemsPath);
        itemCollection.itemDefinitions = new List<ItemDefinition> { goldItem, tomatoSeed, blueberrySeed, strawberrySeed, cowItem, tomatoProduct, blueberryProduct, strawberryProduct, milkProduct };
        EditorUtility.SetDirty(itemCollection);

        EntityDefinitionCollection entityCollection = CreateCollection<EntityDefinitionCollection>("EntityDefinitionCollection", EntitiesPath);
        entityCollection.entityDefinitions = new List<EntityDefinition> { tomatoPlant, blueberryPlant, strawberryPlant, milkCow };
        EditorUtility.SetDirty(entityCollection);

        StoreItemCollection storeItemCollection = CreateCollection<StoreItemCollection>("StoreItemCollection", StoreItemsPath);
        storeItemCollection.storeItems = new List<StoreItemDefinition> { tomatoSeedSale, blueberrySeedSale, strawberrySeedPackSale, dairyCowSale, hireWorker, upgradeEquipment, buyLandPlot };
        EditorUtility.SetDirty(storeItemCollection);
        
        // Create GameDataManager
        // GameDataManager gameDataManager = CreateAsset<GameDataManager>(GameDataPath, "GameDataManager");
        // gameDataManager.entityDefinitions = entityCollection;
        // gameDataManager.itemDefinitions = itemCollection;
        // gameDataManager.storeItems = storeItemCollection;
        // gameDataManager.gameSettings = gameSettings;
        // EditorUtility.SetDirty(gameDataManager);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Default game data generated successfully!");
    }

    private static void CreateDirectories()
    {
        if (!Directory.Exists(Application.dataPath + "/Resources")) Directory.CreateDirectory(Application.dataPath + "/Resources");
        if (!Directory.Exists(GameDataPath)) Directory.CreateDirectory(GameDataPath);
        if (!Directory.Exists(ItemsPath)) Directory.CreateDirectory(ItemsPath);
        if (!Directory.Exists(EntitiesPath)) Directory.CreateDirectory(EntitiesPath);
        if (!Directory.Exists(StoreItemsPath)) Directory.CreateDirectory(StoreItemsPath);
    }

    private static T CreateAsset<T>(string path, string name) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        string assetPath = $"{path}/{name}.asset";
        AssetDatabase.CreateAsset(asset, assetPath);
        return asset;
    }
    
    private static T CreateCollection<T>(string name, string referencePath) where T : ScriptableObject
    {
        return CreateAsset<T>(GameDataPath, name);
    }

    private static GameSettings CreateGameSettings()
    {
        GameSettings settings = CreateAsset<GameSettings>(GameDataPath, "GameSettings");
        settings.startingGold = 1000;
        settings.startingPlots = 3;
        settings.startingInventory = new List<InventoryItem>
        {
            new InventoryItem { itemID = ItemID.TomatoSeed, amount = 10 },
            new InventoryItem { itemID = ItemID.BlueberrySeed, amount = 10 },
            new InventoryItem { itemID = ItemID.Cow, amount = 2 } // Using ItemID.Cow
        };
        settings.startingWorkers = 1;
        settings.startingEquipmentLevel = 1;
        settings.workerTaskDuration = 2f; // 2 minutes to complete entire plot harvest
        settings.equipmentYieldBonusPerLevel = 0.1f;
        settings.winConditionGold = 1000000;
        settings.cost_HireWorker = 500;
        settings.cost_UpgradeEquipment = 500;
        settings.cost_BuyLandPlot = 500;
        EditorUtility.SetDirty(settings);
        return settings;
    }

    private static ItemDefinition CreateItemDefinition(string assetName, ItemID id, ItemType type, string desc, int salePrice, int purchasePrice, int packSize = 1, EntityID growsInto = EntityID.None, EntityID producedBy = EntityID.None)
    {
        ItemDefinition itemDef = CreateAsset<ItemDefinition>(ItemsPath, assetName);
        itemDef.name = assetName + "ItemDef"; // ScriptableObject.name
        itemDef.itemID = id;
        itemDef.itemName = assetName.Replace("Item", ""); // User-friendly name
        itemDef.itemType = type;
        itemDef.description = desc;
        itemDef.baseSalePrice = salePrice;
        itemDef.basePurchasePrice = purchasePrice;
        itemDef.purchasePackSize = packSize;
        itemDef.growsIntoEntityID = growsInto;
        itemDef.producedByEntityID = producedBy;
        // itemDef.icon = null; // Default
        EditorUtility.SetDirty(itemDef);
        return itemDef;
    }

    private static EntityDefinition CreateEntityDefinition(string assetName, EntityID id, string entityName, EntityType type, string desc, float prodTime, int yieldAmount, int yieldLimit, ItemID productID, ItemID seedID, int purchasePrice, float decayTime)
    {
        EntityDefinition entityDef = CreateAsset<EntityDefinition>(EntitiesPath, assetName);
        entityDef.name = assetName + "EntityDef"; // ScriptableObject.name
        entityDef.entityID = id;
        entityDef.entityName = entityName;
        entityDef.entityType = type;
        entityDef.description = desc;
        entityDef.baseProductionTime = prodTime;
        entityDef.baseYieldAmount = yieldAmount;
        entityDef.totalYieldsLimit = yieldLimit;
        entityDef.productProducedItemID = productID;
        entityDef.seedItemID = seedID;
        entityDef.purchasePrice = purchasePrice;
        entityDef.decayTimeAfterLastYield = decayTime;
        
        // Set default quantity per plot based on entity type
        entityDef.quantityPerPlot = (type == EntityType.Plant) ? 10 : 1;
        
        // entityDef.icon = null; // Default
        EditorUtility.SetDirty(entityDef);
        return entityDef;
    }

    private static StoreItemDefinition CreateStoreItem(string assetName, StoreID id, string displayName, string desc, PurchaseType type, int amount, int price, ItemID refItemID, EntityID refEntityID)
    {
        StoreItemDefinition storeItem = CreateAsset<StoreItemDefinition>(StoreItemsPath, assetName);
        storeItem.name = assetName + "StoreItemDef"; // ScriptableObject.name
        storeItem.storeID = id;
        storeItem.displayName = displayName;
        storeItem.description = desc;
        storeItem.purchaseType = type;
        storeItem.amount = amount;
        storeItem.price = price;
        storeItem.referencedItemID = refItemID;
        storeItem.referencedEntityID = refEntityID;
        // storeItem.icon = null; // Default
        EditorUtility.SetDirty(storeItem);
        return storeItem;
    }
}
