public enum EntityID
{
    None, // Added
    TOMATO_PLANT,
    BLUEBERRY_PLANT,
    STRAWBERRY_PLANT,
    MILK_COW
}

public enum EntityType
{
    Plant,
    Animal
}

public enum ItemID
{
    None,
    Gold,
    TomatoSeed,
    BlueberrySeed,
    StrawberrySeed,
    Tomato,
    Blueberry,
    Strawberry,
    Milk,
    Cow // Added for starting inventory cow item
}

public enum ItemType
{
    Currency,
    Seed,
    Product,
    Animal // Added to match individual file
}

public enum StoreID
{
    None, // Added
    TomatoSeedSale,
    BlueberrySeedSale,
    StrawberrySeedPackSale, // For strawberry seed packs
    DairyCowSale,
    HireWorker,
    UpgradeEquipment,
    BuyLandPlot
}

public enum PurchaseType
{
    Seed,
    Animal,
    Land,
    EquipmentUpgrade,
    Worker
}
