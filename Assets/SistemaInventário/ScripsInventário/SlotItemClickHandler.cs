using UnityEngine;
using UnityEngine.EventSystems; 

public class SlotItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    public string itemName; 

    private float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f;
    public void Initialize(string name)
    {
        itemName = name;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if ((Time.time - lastClickTime) < DOUBLE_CLICK_TIME)
            {

                Debug.Log("Duplo clique em: " + itemName);

                if (InventorySystem.Instance != null)
                {
                    InventorySystem.Instance.HandleItemEquipRequest(itemName);
                }
                lastClickTime = 0; 
            }
            else
            {
                lastClickTime = Time.time;
            }
        }
    }
}