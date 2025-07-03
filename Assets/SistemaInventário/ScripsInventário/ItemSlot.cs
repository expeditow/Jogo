using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    public string itemName = null;
    public int quantity = 0;
    public int maxStackSize = 1;

    [SerializeField] private TextMeshProUGUI quantityText;

    public GameObject Item
    {
        get
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("ItemIcon")) return child.gameObject;
            }
            return null;
        }
    }

    private void Awake()
    {
        if (quantityText == null)
        {
            quantityText = GetComponentInChildren<TextMeshProUGUI>();
            if (quantityText != null) quantityText.gameObject.SetActive(false);
        }
    }

    public void UpdateSlotData(string name, int amount, int maxStack)
    {
        this.itemName = name;
        this.quantity = amount;
        this.maxStackSize = maxStack;
        UpdateSlotUI();
    }

    public void ClearSlot()
    {
        UpdateSlotData(null, 0, 1);
        if (Item != null) Destroy(Item);
    }

    public void UpdateSlotUI()
    {
        if (quantityText == null) return;
        if (string.IsNullOrEmpty(itemName))
        {
            quantityText.gameObject.SetActive(false);
            return;
        }
        if (quantity > 1)
        {
            quantityText.text = quantity.ToString();
            quantityText.gameObject.SetActive(true);
        }
        else
        {
            quantityText.gameObject.SetActive(false);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"OnDrop was called on slot: {this.gameObject.name}");

        GameObject droppedObject = eventData.pointerDrag;
        DragDrop dragDropScript = droppedObject.GetComponent<DragDrop>();

        ItemSlot originSlot = dragDropScript.parentAfterDrag.GetComponent<ItemSlot>();

        if (this.Item == null)
        {
            Debug.Log("Dropping item into an empty slot.");

            dragDropScript.parentAfterDrag = this.transform;

            this.UpdateSlotData(originSlot.itemName, originSlot.quantity, originSlot.maxStackSize);

            originSlot.ClearSlot();
        }
        else
        {
            Debug.Log("Swapping items between slots.");

            GameObject itemA_dragged = droppedObject;
            GameObject itemB_stationary = this.Item;

            string originSlot_itemName = originSlot.itemName;
            int originSlot_quantity = originSlot.quantity;
            int originSlot_maxStackSize = originSlot.maxStackSize;

            itemB_stationary.transform.SetParent(originSlot.transform);
            itemB_stationary.transform.localPosition = Vector3.zero;
            originSlot.UpdateSlotData(this.itemName, this.quantity, this.maxStackSize);

            itemA_dragged.transform.SetParent(this.transform);
            itemA_dragged.transform.localPosition = Vector3.zero;
            this.UpdateSlotData(originSlot_itemName, originSlot_quantity, originSlot_maxStackSize);
        }
    }
}
