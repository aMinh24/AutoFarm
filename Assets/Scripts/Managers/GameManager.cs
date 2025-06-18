using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : BaseManager<GameManager>
{
    void Start()
    {
        UIManager.Instance.ShowScreen<MainUI>();
    }
    [Button("Open Shop")]
    public void OpenShop()
    {
        UIManager.Instance?.ShowPopup<Shop>(null);
    }
    [Button("Open Inventory")]
    public void OpenInventory()
    {
        UIManager.Instance?.ShowPopup<InventoryUI>(null);
    }
}
