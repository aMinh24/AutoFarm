using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class CSVDataManager : EditorWindow
{
    private const string GameDataPath = "Assets/Resources/GameData";
    private string csvExportPath = "Assets/CSVData"; // Made non-const and configurable
    
    private Vector2 scrollPosition;
    
    [MenuItem("Farm Game/CSV Data Manager")]
    public static void ShowWindow()
    {
        GetWindow<CSVDataManager>("CSV Data Manager");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("CSV Data Export/Import", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // Path Selection Section
        GUILayout.Label("Export/Import Path Settings", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("CSV Path:", GUILayout.Width(80));
        csvExportPath = EditorGUILayout.TextField(csvExportPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("Select CSV Export/Import Folder", csvExportPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // Convert absolute path to relative if it's within the project
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    csvExportPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    csvExportPath = selectedPath;
                }
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Export Section
        GUILayout.Label("Export to CSV", EditorStyles.boldLabel);
        if (GUILayout.Button("Export All Data to CSV"))
        {
            ExportAllDataToCSV();
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Export Items Only"))
        {
            ExportItemsToCSV();
        }
        if (GUILayout.Button("Export Entities Only"))
        {
            ExportEntitiesToCSV();
        }
        if (GUILayout.Button("Export Store Items Only"))
        {
            ExportStoreItemsToCSV();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        
        // Import Section
        GUILayout.Label("Import from CSV", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Warning: Importing will overwrite existing data!", MessageType.Warning);
        
        if (GUILayout.Button("Import All Data from CSV"))
        {
            ImportAllDataFromCSV();
        }
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Import Items Only"))
        {
            ImportItemsFromCSV();
        }
        if (GUILayout.Button("Import Entities Only"))
        {
            ImportEntitiesFromCSV();
        }
        if (GUILayout.Button("Import Store Items Only"))
        {
            ImportStoreItemsFromCSV();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        
        // Utility Section
        GUILayout.Label("Utilities", EditorStyles.boldLabel);
        if (GUILayout.Button("Open CSV Export Folder"))
        {
            EditorUtility.RevealInFinder(csvExportPath);
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    #region Export Methods
    
    private void ExportAllDataToCSV()
    {
        CreateCSVDirectory();
        ExportItemsToCSV();
        ExportEntitiesToCSV();
        ExportStoreItemsToCSV();
        Debug.Log("All data exported to CSV successfully!");
    }
    
    private void ExportItemsToCSV()
    {
        CreateCSVDirectory();
        
        ItemDefinitionCollection itemCollection = Resources.Load<ItemDefinitionCollection>("GameData/ItemDefinitionCollection");
        if (itemCollection == null || itemCollection.itemDefinitions == null)
        {
            Debug.LogError("Could not load ItemDefinitionCollection!");
            return;
        }
        
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("ItemID,ItemName,ItemType,Description,BaseSalePrice,BasePurchasePrice,PurchasePackSize,GrowsIntoEntityID,ProducedByEntityID");
        
        foreach (var item in itemCollection.itemDefinitions)
        {
            csv.AppendLine($"{item.itemID},{EscapeCSV(item.itemName)},{item.itemType},{EscapeCSV(item.description)},{item.baseSalePrice},{item.basePurchasePrice},{item.purchasePackSize},{item.growsIntoEntityID},{item.producedByEntityID}");
        }
        
        File.WriteAllText($"{csvExportPath}/ItemDefinitions.csv", csv.ToString());
        Debug.Log("ItemDefinitions exported to CSV successfully!");
    }
    
    private void ExportEntitiesToCSV()
    {
        CreateCSVDirectory();
        
        EntityDefinitionCollection entityCollection = Resources.Load<EntityDefinitionCollection>("GameData/EntityDefinitionCollection");
        if (entityCollection == null || entityCollection.entityDefinitions == null)
        {
            Debug.LogError("Could not load EntityDefinitionCollection!");
            return;
        }
        
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("EntityID,EntityName,EntityType,Description,BaseProductionTime,BaseYieldAmount,TotalYieldsLimit,ProductProducedItemID,SeedItemID,PurchasePrice,DecayTimeAfterLastYield");
        
        foreach (var entity in entityCollection.entityDefinitions)
        {
            csv.AppendLine($"{entity.entityID},{EscapeCSV(entity.entityName)},{entity.entityType},{EscapeCSV(entity.description)},{entity.baseProductionTime},{entity.baseYieldAmount},{entity.totalYieldsLimit},{entity.productProducedItemID},{entity.seedItemID},{entity.purchasePrice},{entity.decayTimeAfterLastYield}");
        }
        
        File.WriteAllText($"{csvExportPath}/EntityDefinitions.csv", csv.ToString());
        Debug.Log("EntityDefinitions exported to CSV successfully!");
    }
    
    private void ExportStoreItemsToCSV()
    {
        CreateCSVDirectory();
        
        StoreItemCollection storeCollection = Resources.Load<StoreItemCollection>("GameData/StoreItemCollection");
        if (storeCollection == null || storeCollection.storeItems == null)
        {
            Debug.LogError("Could not load StoreItemCollection!");
            return;
        }
        
        StringBuilder csv = new StringBuilder();
        csv.AppendLine("StoreID,DisplayName,Description,PurchaseType,Amount,Price,ReferencedItemID,ReferencedEntityID");
        
        foreach (var storeItem in storeCollection.storeItems)
        {
            csv.AppendLine($"{storeItem.storeID},{EscapeCSV(storeItem.displayName)},{EscapeCSV(storeItem.description)},{storeItem.purchaseType},{storeItem.amount},{storeItem.price},{storeItem.referencedItemID},{storeItem.referencedEntityID}");
        }
        
        File.WriteAllText($"{csvExportPath}/StoreItemDefinitions.csv", csv.ToString());
        Debug.Log("StoreItemDefinitions exported to CSV successfully!");
    }
    
    #endregion
    
    #region Import Methods
    
    private void ImportAllDataFromCSV()
    {
        if (!EditorUtility.DisplayDialog("Import Warning", "This will overwrite all existing game data. Are you sure?", "Yes", "Cancel"))
            return;
            
        ImportItemsFromCSV();
        ImportEntitiesFromCSV();
        ImportStoreItemsFromCSV();
        Debug.Log("All data imported from CSV successfully!");
    }
    
    private void ImportItemsFromCSV()
    {
        string csvPath = $"{csvExportPath}/ItemDefinitions.csv";
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found: {csvPath}");
            return;
        }
        
        ItemDefinitionCollection itemCollection = Resources.Load<ItemDefinitionCollection>("GameData/ItemDefinitionCollection");
        if (itemCollection == null)
        {
            Debug.LogError("Could not load ItemDefinitionCollection!");
            return;
        }
        
        string[] lines = File.ReadAllLines(csvPath);
        List<ItemDefinition> newItems = new List<ItemDefinition>();
        
        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            string[] values = ParseCSVLine(lines[i]);
            if (values.Length >= 9)
            {
                ItemDefinition existingItem = itemCollection.itemDefinitions.Find(item => item.itemID.ToString() == values[0]);
                if (existingItem != null)
                {
                    UpdateItemFromCSV(existingItem, values);
                    newItems.Add(existingItem);
                }
                else
                {
                    Debug.LogWarning($"Item with ID {values[0]} not found in collection. Skipping.");
                }
            }
        }
        
        EditorUtility.SetDirty(itemCollection);
        AssetDatabase.SaveAssets();
        Debug.Log("Items imported from CSV successfully!");
    }
    
    private void ImportEntitiesFromCSV()
    {
        string csvPath = $"{csvExportPath}/EntityDefinitions.csv";
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found: {csvPath}");
            return;
        }
        
        EntityDefinitionCollection entityCollection = Resources.Load<EntityDefinitionCollection>("GameData/EntityDefinitionCollection");
        if (entityCollection == null)
        {
            Debug.LogError("Could not load EntityDefinitionCollection!");
            return;
        }
        
        string[] lines = File.ReadAllLines(csvPath);
        
        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            string[] values = ParseCSVLine(lines[i]);
            if (values.Length >= 11)
            {
                EntityDefinition existingEntity = entityCollection.entityDefinitions.Find(entity => entity.entityID.ToString() == values[0]);
                if (existingEntity != null)
                {
                    UpdateEntityFromCSV(existingEntity, values);
                }
                else
                {
                    Debug.LogWarning($"Entity with ID {values[0]} not found in collection. Skipping.");
                }
            }
        }
        
        EditorUtility.SetDirty(entityCollection);
        AssetDatabase.SaveAssets();
        Debug.Log("Entities imported from CSV successfully!");
    }
    
    private void ImportStoreItemsFromCSV()
    {
        string csvPath = $"{csvExportPath}/StoreItemDefinitions.csv";
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found: {csvPath}");
            return;
        }
        
        StoreItemCollection storeCollection = Resources.Load<StoreItemCollection>("GameData/StoreItemCollection");
        if (storeCollection == null)
        {
            Debug.LogError("Could not load StoreItemCollection!");
            return;
        }
        
        string[] lines = File.ReadAllLines(csvPath);
        
        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            string[] values = ParseCSVLine(lines[i]);
            if (values.Length >= 8)
            {
                StoreItemDefinition existingStoreItem = storeCollection.storeItems.Find(item => item.storeID.ToString() == values[0]);
                if (existingStoreItem != null)
                {
                    UpdateStoreItemFromCSV(existingStoreItem, values);
                }
                else
                {
                    Debug.LogWarning($"Store item with ID {values[0]} not found in collection. Skipping.");
                }
            }
        }
        
        EditorUtility.SetDirty(storeCollection);
        AssetDatabase.SaveAssets();
        Debug.Log("Store items imported from CSV successfully!");
    }
    
    #endregion
    
    #region Helper Methods
    
    private void CreateCSVDirectory()
    {
        if (!Directory.Exists(csvExportPath))
        {
            Directory.CreateDirectory(csvExportPath);
            AssetDatabase.Refresh();
        }
    }
    
    private string EscapeCSV(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";
            
        if (text.Contains(",") || text.Contains("\"") || text.Contains("\n"))
        {
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }
        return text;
    }
    
    private string[] ParseCSVLine(string line)
    {
        List<string> values = new List<string>();
        bool inQuotes = false;
        StringBuilder currentValue = new StringBuilder();
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentValue.Append('"');
                    i++; // Skip the next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }
        
        values.Add(currentValue.ToString());
        return values.ToArray();
    }
    
    private void UpdateItemFromCSV(ItemDefinition item, string[] values)
    {
        item.itemName = values[1];
        if (Enum.TryParse<ItemType>(values[2], out ItemType itemType))
            item.itemType = itemType;
        item.description = values[3];
        if (int.TryParse(values[4], out int salePrice))
            item.baseSalePrice = salePrice;
        if (int.TryParse(values[5], out int purchasePrice))
            item.basePurchasePrice = purchasePrice;
        if (int.TryParse(values[6], out int packSize))
            item.purchasePackSize = packSize;
        if (Enum.TryParse<EntityID>(values[7], out EntityID growsInto))
            item.growsIntoEntityID = growsInto;
        if (Enum.TryParse<EntityID>(values[8], out EntityID producedBy))
            item.producedByEntityID = producedBy;
            
        EditorUtility.SetDirty(item);
    }
    
    private void UpdateEntityFromCSV(EntityDefinition entity, string[] values)
    {
        entity.entityName = values[1];
        if (Enum.TryParse<EntityType>(values[2], out EntityType entityType))
            entity.entityType = entityType;
        entity.description = values[3];
        if (float.TryParse(values[4], out float productionTime))
            entity.baseProductionTime = productionTime;
        if (int.TryParse(values[5], out int yieldAmount))
            entity.baseYieldAmount = yieldAmount;
        if (int.TryParse(values[6], out int yieldsLimit))
            entity.totalYieldsLimit = yieldsLimit;
        if (Enum.TryParse<ItemID>(values[7], out ItemID productID))
            entity.productProducedItemID = productID;
        if (Enum.TryParse<ItemID>(values[8], out ItemID seedID))
            entity.seedItemID = seedID;
        if (int.TryParse(values[9], out int purchasePrice))
            entity.purchasePrice = purchasePrice;
        if (float.TryParse(values[10], out float decayTime))
            entity.decayTimeAfterLastYield = decayTime;
            
        EditorUtility.SetDirty(entity);
    }
    
    private void UpdateStoreItemFromCSV(StoreItemDefinition storeItem, string[] values)
    {
        storeItem.displayName = values[1];
        storeItem.description = values[2];
        if (Enum.TryParse<PurchaseType>(values[3], out PurchaseType purchaseType))
            storeItem.purchaseType = purchaseType;
        if (int.TryParse(values[4], out int amount))
            storeItem.amount = amount;
        if (int.TryParse(values[5], out int price))
            storeItem.price = price;
        if (Enum.TryParse<ItemID>(values[6], out ItemID itemID))
            storeItem.referencedItemID = itemID;
        if (Enum.TryParse<EntityID>(values[7], out EntityID entityID))
            storeItem.referencedEntityID = entityID;
            
        EditorUtility.SetDirty(storeItem);
    }
    
    #endregion
}
