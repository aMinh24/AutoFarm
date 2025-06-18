using TMPro;
using UnityEngine;

public class DetailUI : MonoBehaviour
{
    public TextMeshProUGUI nameItem;
    public TextMeshProUGUI descriptionItem;
    public TextMeshProUGUI priceItem;
    public void Setup(string name, string description, string price)
    {
        if (nameItem != null)
        {
            nameItem.text = name;
        }

        if (descriptionItem != null)
        {
            descriptionItem.text = description;
        }

        if (priceItem != null)
        {
            priceItem.text = price;
        }
    }
}
