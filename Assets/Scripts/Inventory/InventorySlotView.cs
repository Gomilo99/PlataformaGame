using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotView : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI label;

    public void SetSlot(ItemData item, int count)
    {
        if (item != null)
        {
            if (icon)
            {
                icon.enabled = true;
                icon.sprite = item.icon;
            }
            if (label)
            {
                label.text = count > 1 ? $"{item.displayName} x{count}" : item.displayName;
            }
        }
        else
        {
            if (icon)
            {
                icon.enabled = false;
                icon.sprite = null;
            }
            if (label)
            {
                label.text = string.Empty;
            }
        }
    }
}
