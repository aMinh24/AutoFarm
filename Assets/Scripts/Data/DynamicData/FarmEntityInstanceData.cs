using System;
using UnityEngine;
using Newtonsoft.Json;
using AutoFarm.Utilities;

public enum EntityState
{
    Growing,
    ReadyToHarvest,
    Decaying,
    Dead
}

[Serializable]
public class FarmEntityInstanceData
{
    [Header("Identification")]
    public string instanceID;
    public EntityID entityID;
    public int associatedPlotID;
    public int positionIndex; // New: position within the plot (0-9 for trees, 0 for cow)

    [Header("State")]
    public EntityState currentState;
    public int currentYieldsProduced;
    public int accumulatedProducts; // New: accumulated products ready for harvest
    public float timeUntilNextYield;
    public float timeUntilDecay;

    [Header("Timestamps for persistence")]
    public long lastUpdateTimestamp; // Unix timestamp when this was last updated

    [JsonConstructor]
    public FarmEntityInstanceData()
    {
        instanceID = System.Guid.NewGuid().ToString();
        entityID = EntityID.None;
        associatedPlotID = -1;
        positionIndex = 0;
        currentState = EntityState.Growing;
        currentYieldsProduced = 0;
        accumulatedProducts = 0;
        timeUntilNextYield = 0f;
        timeUntilDecay = 0f;
        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public FarmEntityInstanceData(EntityID entityType, int plotID)
    {
        instanceID = System.Guid.NewGuid().ToString();
        entityID = entityType;
        associatedPlotID = plotID;
        positionIndex = 0;
        currentState = EntityState.Growing;
        currentYieldsProduced = 0;
        accumulatedProducts = 0;
        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        InitializeFromDefinition();
    }

    public FarmEntityInstanceData(EntityID entityType, int plotID, int position = 0)
    {
        instanceID = System.Guid.NewGuid().ToString();
        entityID = entityType;
        associatedPlotID = plotID;
        positionIndex = position;
        currentState = EntityState.Growing;
        currentYieldsProduced = 0;
        accumulatedProducts = 0;
        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        InitializeFromDefinition();
    }

    private void InitializeFromDefinition()
    {
        var entityDef = GameDataManager.Instance?.GetEntity(entityID);
        if (entityDef != null)
        {
            timeUntilNextYield = entityDef.baseProductionTime * 60f; // Convert minutes to seconds
            timeUntilDecay = entityDef.decayTimeAfterLastYield * 60f; // Convert minutes to seconds
        }
        else
        {
            timeUntilNextYield = 600f; // 10 minutes default
            timeUntilDecay = 3600f; // 60 minutes default
        }
    }

    public void UpdateTimers(float deltaTime)
    {
        var entityDef = GameDataManager.Instance?.GetEntity(entityID);
        if (entityDef == null) return;

        switch (currentState)
        {
            case EntityState.Growing:
                timeUntilNextYield -= deltaTime;
                if (timeUntilNextYield <= 0)
                {
                    // Produce first yield
                    currentYieldsProduced++;
                    accumulatedProducts += entityDef.baseYieldAmount;
                    currentState = EntityState.ReadyToHarvest;
                    
                    // Start decay timer from first production
                    timeUntilDecay = entityDef.decayTimeAfterLastYield * 60f;
                    
                    // Reset production timer for continuous production
                    timeUntilNextYield = entityDef.baseProductionTime * 60f;
                }
                break;

            case EntityState.ReadyToHarvest:
                // Continue producing while waiting for harvest
                timeUntilNextYield -= deltaTime;
                timeUntilDecay -= deltaTime;
                
                // Check if can produce more
                if (timeUntilNextYield <= 0 && CanProduceMore())
                {
                    currentYieldsProduced++;
                    accumulatedProducts += entityDef.baseYieldAmount;
                    timeUntilNextYield = entityDef.baseProductionTime * 60f;
                }
                
                // Check if should start decaying (if reached max yields or decay time)
                if (timeUntilDecay <= 0 || !CanProduceMore())
                {
                    if (accumulatedProducts > 0)
                    {
                        currentState = EntityState.Decaying;
                        timeUntilDecay = entityDef.decayTimeAfterLastYield * 60f;
                    }
                    else
                    {
                        currentState = EntityState.Dead;
                    }
                }
                break;

            case EntityState.Decaying:
                timeUntilDecay -= deltaTime;
                if (timeUntilDecay <= 0)
                {
                    // Entity dies and loses all accumulated products
                    currentState = EntityState.Dead;
                    accumulatedProducts = 0;
                    timeUntilDecay = 0f;
                }
                break;

            case EntityState.Dead:
                // Entity should be removed from the game
                break;
        }

        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    
    // Add method to get time remaining for UI display
    public float GetTimeRemaining()
    {
        switch (currentState)
        {
            case EntityState.Growing:
                return timeUntilNextYield;
            case EntityState.ReadyToHarvest:
                return Mathf.Min(timeUntilNextYield, timeUntilDecay);
            case EntityState.Decaying:
                return timeUntilDecay;
            default:
                return 0f;
        }
    }
    
    // Add method to get time remaining as formatted string
    public string GetFormattedTimeRemaining()
    {
        switch (currentState)
        {
            case EntityState.Growing:
                return FormatUtilities.FormatEntityState("Growing", timeUntilNextYield);
            case EntityState.ReadyToHarvest:
                float nextProduction = CanProduceMore() ? timeUntilNextYield : float.MaxValue;
                if (nextProduction < timeUntilDecay)
                {
                    return FormatUtilities.FormatEntityState("Next", timeUntilNextYield);
                }
                else
                {
                    return FormatUtilities.FormatEntityState("Decay", timeUntilDecay);
                }
            case EntityState.Decaying:
                return FormatUtilities.FormatEntityState("Decay", timeUntilDecay);
            case EntityState.Dead:
                return "Dead";
            default:
                return "";
        }
    }

    public bool CanHarvest()
    {
        return (currentState == EntityState.ReadyToHarvest || currentState == EntityState.Decaying) 
               && accumulatedProducts > 0;
    }

    public bool CanProduceMore()
    {
        var entityDef = GameDataManager.Instance?.GetEntity(entityID);
        if (entityDef == null) return false;
        
        return currentYieldsProduced < entityDef.totalYieldsLimit;
    }

    public HarvestResult Harvest()
    {
        if (!CanHarvest())
        {
            return new HarvestResult { success = false };
        }

        var entityDef = GameDataManager.Instance?.GetEntity(entityID);
        if (entityDef == null)
        {
            return new HarvestResult { success = false };
        }

        // Return base amount without equipment bonus (bonus applied in PlotManager)
        int baseAmount = accumulatedProducts;

        // Harvest all accumulated products (base amount only)
        var result = new HarvestResult
        {
            success = true,
            itemProduced = entityDef.productProducedItemID,
            amountProduced = baseAmount
        };

        // Clear accumulated products
        accumulatedProducts = 0;

        // Check if entity should die after harvest
        if (currentState == EntityState.Decaying && !CanProduceMore())
        {
            // Entity is decaying and cannot produce more - it dies after harvest
            currentState = EntityState.Dead;
            timeUntilDecay = 0f;
            timeUntilNextYield = 0f;
        }
        else
        {
            // Reset decay timer after harvest
            timeUntilDecay = entityDef.decayTimeAfterLastYield * 60f;

            // Check if entity can continue producing
            if (CanProduceMore())
            {
                currentState = EntityState.ReadyToHarvest; // Stay ready to continue production
            }
            else
            {
                // Entity reached production limit, start final decay
                currentState = EntityState.Decaying;
            }
        }

        lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return result;
    }

    public bool IsDead()
    {
        return currentState == EntityState.Dead;
    }

    public void UpdateFromOfflineTime(long currentTimestamp)
    {
        if (lastUpdateTimestamp <= 0) return;

        long offlineSeconds = currentTimestamp - lastUpdateTimestamp;
        if (offlineSeconds > 0)
        {
            UpdateTimers(offlineSeconds);
        }
    }
}

[Serializable]
public struct HarvestResult
{
    public bool success;
    public ItemID itemProduced;
    public int amountProduced;
}