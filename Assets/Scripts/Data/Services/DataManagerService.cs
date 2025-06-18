using System;
using System.Threading.Tasks;
using UnityEngine;

public class DataManagerService : IDataManager
{
    private ISaveLoadService saveLoadService;
    private GameSaveData currentGameData;
    
    public event Action<bool> OnSaveCompleted;
    public event Action<GameSaveData> OnLoadCompleted;
    public event Action<string> OnError;
    
    public DataManagerService(ISaveLoadService saveLoadService)
    {
        this.saveLoadService = saveLoadService;
        this.currentGameData = new GameSaveData();
    }
    
    public GameSaveData GetCurrentGameData()
    {
        return currentGameData;
    }
    
    public async Task<GameSaveData> LoadGameDataAsync()
    {
        try
        {
            GameSaveData data = await saveLoadService.LoadFromFileAsync<GameSaveData>(saveLoadService.SavePath);
            
            if (data == null && saveLoadService.FileExists(saveLoadService.BackupPath))
            {
                Debug.LogWarning("Main save corrupted, loading from backup");
                data = await saveLoadService.LoadFromFileAsync<GameSaveData>(saveLoadService.BackupPath);
            }
            
            if (data != null && ValidateGameData(data))
            {
                data.PrepareAfterLoad();
                currentGameData = data;
                OnLoadCompleted?.Invoke(data);
                Debug.Log("Game data loaded successfully using Newtonsoft.Json");
            }
            else
            {
                OnError?.Invoke("Failed to load valid game data");
                currentGameData = new GameSaveData();
                currentGameData.InitializeNewGame();
            }
            
            return currentGameData;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Load error: {ex.Message}");
            currentGameData = new GameSaveData();
            currentGameData.InitializeNewGame();
            return currentGameData;
        }
    }
    
    public bool SaveGameData(GameSaveData data)
    {
        try
        {
            if (data == null)
            {
                OnError?.Invoke("Cannot save null game data");
                return false;
            }
            
            data.PrepareForSave();
            
            if (saveLoadService.FileExists(saveLoadService.SavePath))
            {
                saveLoadService.CopyFile(saveLoadService.SavePath, saveLoadService.BackupPath);
            }
            
            bool success = saveLoadService.SaveToFile(data, saveLoadService.SavePath);
            OnSaveCompleted?.Invoke(success);
            
            if (success)
            {
                Debug.Log("Game data saved successfully");
            }
            else
            {
                OnError?.Invoke("Failed to save game data");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Save error: {ex.Message}");
            return false;
        }
    }
    
    public async Task<bool> SaveGameDataAsync(GameSaveData data)
    {
        try
        {
            if (data == null)
            {
                OnError?.Invoke("Cannot save null game data");
                return false;
            }
            
            data.PrepareForSave();
            
            if (saveLoadService.FileExists(saveLoadService.SavePath))
            {
                saveLoadService.CopyFile(saveLoadService.SavePath, saveLoadService.BackupPath);
            }
            
            bool success = await saveLoadService.SaveToFileAsync(data, saveLoadService.SavePath);
            OnSaveCompleted?.Invoke(success);
            
            if (success)
            {
                Debug.Log("Game data saved successfully");
            }
            else
            {
                OnError?.Invoke("Failed to save game data");
            }
            
            return success;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Save error: {ex.Message}");
            return false;
        }
    }
    
    public GameSaveData LoadGameData()
    {
        try
        {
            GameSaveData data = saveLoadService.LoadFromFile<GameSaveData>(saveLoadService.SavePath);
            
            if (data == null && saveLoadService.FileExists(saveLoadService.BackupPath))
            {
                Debug.LogWarning("Main save corrupted, loading from backup");
                data = saveLoadService.LoadFromFile<GameSaveData>(saveLoadService.BackupPath);
            }
            
            if (data != null && ValidateGameData(data))
            {
                data.PrepareAfterLoad();
                currentGameData = data;
                OnLoadCompleted?.Invoke(data);
                Debug.Log("Game data loaded successfully using Newtonsoft.Json");
            }
            else
            {
                OnError?.Invoke("Failed to load valid game data");
                currentGameData = new GameSaveData();
                currentGameData.InitializeNewGame();
            }
            
            return currentGameData;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Load error: {ex.Message}");
            currentGameData = new GameSaveData();
            currentGameData.InitializeNewGame();
            return currentGameData;
        }
    }
    
    public bool ValidateGameData(GameSaveData data)
    {
        return data != null && data.IsValidSave();
    }
    
    public bool HasSaveFile()
    {
        return saveLoadService.FileExists(saveLoadService.SavePath) || 
               saveLoadService.FileExists(saveLoadService.BackupPath);
    }
    
    public bool DeleteSaveFile()
    {
        bool mainDeleted = saveLoadService.DeleteFile(saveLoadService.SavePath);
        bool backupDeleted = saveLoadService.DeleteFile(saveLoadService.BackupPath);
        return mainDeleted || backupDeleted;
    }
    
    public void BackupSaveFile()
    {
        if (saveLoadService.FileExists(saveLoadService.SavePath))
        {
            saveLoadService.CopyFile(saveLoadService.SavePath, saveLoadService.BackupPath);
        }
    }
}
