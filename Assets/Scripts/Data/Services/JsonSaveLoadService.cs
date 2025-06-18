using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class JsonSaveLoadService : ISaveLoadService
{
    public string SavePath { get; private set; }
    public string BackupPath { get; private set; }
    
    private const string SAVE_FILE_NAME = "gamedata.json";
    private const string BACKUP_FILE_NAME = "gamedata_backup.json";
    
    public JsonSaveLoadService()
    {
        string saveDirectory = Path.Combine(Application.persistentDataPath, "SaveData");
        
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
        
        SavePath = Path.Combine(saveDirectory, SAVE_FILE_NAME);
        BackupPath = Path.Combine(saveDirectory, BACKUP_FILE_NAME);
    }
    
    private static JsonSerializerSettings GetJsonSettings()
    {
        return new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.Auto
        };
    }
    
    public bool SaveToFile<T>(T data, string filePath)
    {
        try
        {
            if (data == null)
            {
                Debug.LogError("Cannot save null data");
                return false;
            }
            
            string json = JsonConvert.SerializeObject(data, GetJsonSettings());
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Failed to serialize data to JSON");
                return false;
            }
            
            File.WriteAllText(filePath, json);
            Debug.Log($"Successfully saved data to {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save file {filePath}: {ex.Message}");
            return false;
        }
    }
    
    public T LoadFromFile<T>(string filePath) where T : class
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Save file not found: {filePath}");
                return null;
            }
            
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json, GetJsonSettings());
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load file {filePath}: {ex.Message}");
            return null;
        }
    }
    
    public async Task<bool> SaveToFileAsync<T>(T data, string filePath)
    {
        try
        {
            if (data == null)
            {
                Debug.LogError("Cannot save null data");
                return false;
            }
            
            string json = JsonConvert.SerializeObject(data, GetJsonSettings());
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("Failed to serialize data to JSON");
                return false;
            }
            
            await File.WriteAllTextAsync(filePath, json);
            Debug.Log($"Successfully saved data to {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save file async {filePath}: {ex.Message}");
            return false;
        }
    }
    
    public async Task<T> LoadFromFileAsync<T>(string filePath) where T : class
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Save file not found: {filePath}");
                return null;
            }
            
            string json = await File.ReadAllTextAsync(filePath);
            return JsonConvert.DeserializeObject<T>(json, GetJsonSettings());
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load file async {filePath}: {ex.Message}");
            return null;
        }
    }
    
    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }
    
    public bool DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete file {filePath}: {ex.Message}");
            return false;
        }
    }
    
    public bool CopyFile(string sourcePath, string destinationPath)
    {
        try
        {
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, destinationPath, true);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to copy file from {sourcePath} to {destinationPath}: {ex.Message}");
            return false;
        }
    }
    
    public bool ValidateFileIntegrity(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return false;
                
            string json = File.ReadAllText(filePath);
            JsonConvert.DeserializeObject<GameSaveData>(json, GetJsonSettings());
            return true;
        }
        catch
        {
            return false;
        }
    }
}
