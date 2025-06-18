using System.Collections.Generic;

public interface IWorkerService
{
    List<WorkerData> GetAllWorkers();
    List<WorkerData> GetIdleWorkers();
    List<WorkerData> GetBusyWorkers();
    WorkerData GetWorkerByID(string workerID);
    int GetAvailableWorkerCount();
    int GetTotalWorkerCount();
    bool HasAvailableWorkers();
    void UpdateWorkerCounts();
    string GetWorkerStatusSummary();
}
