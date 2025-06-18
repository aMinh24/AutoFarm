using System;
using System.Threading.Tasks;

public interface ISaveLoadService
{
    string SavePath { get; }
    string BackupPath { get; }
    
    // Synchronous operations
    bool SaveToFile<T>(T data, string filePath);
    T LoadFromFile<T>(string filePath) where T : class;
    
    // Asynchronous operations
    Task<bool> SaveToFileAsync<T>(T data, string filePath);
    Task<T> LoadFromFileAsync<T>(string filePath) where T : class;
    
    // File management
    bool FileExists(string filePath);
    bool DeleteFile(string filePath);
    bool CopyFile(string sourcePath, string destinationPath);
    
    // Validation
    bool ValidateFileIntegrity(string filePath);
}
