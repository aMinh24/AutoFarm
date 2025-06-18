using Sirenix.OdinInspector;
using UnityEngine;

public class GameManager : BaseManager<GameManager>
{
    void Start()
    {
        UIManager.Instance.ShowScreen<MainUI>();
    }
}
