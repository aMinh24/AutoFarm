using System;
using System.Threading.Tasks;
using UnityEngine;

public class DataManagerService : IDataManager
{
    private readonly ISaveLoadService saveLoadService;
    private GameSaveData currentGameData;
    
    public event Action<bool> OnSaveCompleted;
    public event Action<GameSaveData> OnLoadCompleted;
    public event Action<string> OnError;
    
    public DataManagerService(ISaveLoadService saveLoadService)
    {
        this.saveLoadService = saveLoadService;
    }
    
    public GameSaveData GetCurrentGameData()
    {
        return currentGameData;
    }
    
    public async Task<GameSaveData> LoadGameDataAsync()
    {
        try
        {
            var data = await saveLoadService.LoadFromFileAsync<GameSaveData>(saveLoadService.SavePath);
            if (data != null)
            {
                data.PrepareAfterLoad(); // Important: prepare data after loading
                currentGameData = data;
                OnLoadCompleted?.Invoke(data);
                return data;
            }
            else
            {
                // No save file exists, create new game
                var newData = new GameSaveData();
                newData.InitializeNewGame();
                currentGameData = newData;
                OnLoadCompleted?.Invoke(newData);
                return newData;
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to load game data: {ex.Message}");
            return null;
        }
    }
    
    public GameSaveData LoadGameData()
    {
        try
        {
            var data = saveLoadService.LoadFromFile<GameSaveData>(saveLoadService.SavePath);
            if (data != null)
            {
                data.PrepareAfterLoad(); // Important: prepare data after loading
                currentGameData = data;
                OnLoadCompleted?.Invoke(data);
                return data;
            }
            else
            {
                // No save file exists, create new game
                var newData = new GameSaveData();
                newData.InitializeNewGame();
                currentGameData = newData;
                OnLoadCompleted?.Invoke(newData);
                return newData;
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to load game data: {ex.Message}");
            return null;
        }
    }
    
    public bool SaveGameData(GameSaveData data)
    {
        try
        {
            data.PrepareForSave(); // Prepare data before saving
            bool result = saveLoadService.SaveToFile(data, saveLoadService.SavePath);
            OnSaveCompleted?.Invoke(result);
            
            if (result)
            {
                currentGameData = data;
                // Create backup
                saveLoadService.CopyFile(saveLoadService.SavePath, saveLoadService.BackupPath);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to save game data: {ex.Message}");
            OnSaveCompleted?.Invoke(false);
            return false;
        }
    }
    
    public async Task<bool> SaveGameDataAsync(GameSaveData data)
    {
        try
        {
            data.PrepareForSave(); // Prepare data before saving
            bool result = await saveLoadService.SaveToFileAsync(data, saveLoadService.SavePath);
            OnSaveCompleted?.Invoke(result);
            
            if (result)
            {
                currentGameData = data;
                // Create backup
                saveLoadService.CopyFile(saveLoadService.SavePath, saveLoadService.BackupPath);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            OnError?.Invoke($"Failed to save game data: {ex.Message}");
            OnSaveCompleted?.Invoke(false);
            return false;
        }
    }
    
    public bool ValidateGameData(GameSaveData data)
    {
        return data?.IsValidSave() ?? false;
    }
    
    public bool HasSaveFile()
    {
        return saveLoadService.FileExists(saveLoadService.SavePath);
    }
    
    public bool DeleteSaveFile()
    {
        return saveLoadService.DeleteFile(saveLoadService.SavePath);
    }
    
    public void BackupSaveFile()
    {
        if (HasSaveFile())
        {
            saveLoadService.CopyFile(saveLoadService.SavePath, saveLoadService.BackupPath);
        }
    }
}