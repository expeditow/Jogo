using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour, IDropHandler // Certifique-se de que IDropHandler está aqui
{
    // --- DADOS E REFERÊNCIAS ---
    public string itemName = null;
    public int quantity = 0;
    public int maxStackSize = 1;

    [SerializeField] private TextMeshProUGUI quantityText;

    // --- PROPRIEDADE PARA ENCONTRAR O ÍCONE ---
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

    // --- MÉTODOS DE INICIALIZAÇÃO E DADOS ---
    private void Awake()
    {
        // Garante que a referência do texto seja encontrada
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

    // --- LÓGICA FINAL DO DRAG AND DROP ---
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"OnDrop foi chamado no slot: {this.gameObject.name}");

        // Pega o GameObject do item que foi solto aqui
        GameObject droppedObject = eventData.pointerDrag;
        DragDrop dragDropScript = droppedObject.GetComponent<DragDrop>();

        // Pega o slot de onde o item veio
        ItemSlot originSlot = dragDropScript.parentAfterDrag.GetComponent<ItemSlot>();

        // Caso 1: Se este slot (o de destino) estiver VAZIO
        if (this.Item == null)
        {
            Debug.Log("Soltando item em um slot vazio.");

            // Move o item para este slot
            dragDropScript.parentAfterDrag = this.transform;

            // Atualiza os dados: este slot recebe os dados do slot de origem
            this.UpdateSlotData(originSlot.itemName, originSlot.quantity, originSlot.maxStackSize);

            // Limpa o slot de origem
            originSlot.ClearSlot();
        }
        // Caso 2: Se este slot (o de destino) estiver OCUPADO (Troca de itens)
        else
        {
            Debug.Log("Trocando itens entre slots.");

            // Pega as referências dos dois itens (A = o que foi arrastado, B = o que já estava aqui)
            GameObject itemA_arrastado = droppedObject;
            GameObject itemB_parado = this.Item;

            // Guarda os dados do slot de origem (Slot A)
            string originSlot_itemName = originSlot.itemName;
            int originSlot_quantity = originSlot.quantity;
            int originSlot_maxStackSize = originSlot.maxStackSize;

            // Move o item B (que estava parado) para o Slot A (de origem)
            itemB_parado.transform.SetParent(originSlot.transform);
            itemB_parado.transform.localPosition = Vector3.zero;
            // Atualiza os dados do Slot A com os dados do item B
            originSlot.UpdateSlotData(this.itemName, this.quantity, this.maxStackSize);

            // Move o item A (arrastado) para este Slot B (de destino)
            itemA_arrastado.transform.SetParent(this.transform);
            itemA_arrastado.transform.localPosition = Vector3.zero;
            // Atualiza os dados do Slot B com os dados do item A
            this.UpdateSlotData(originSlot_itemName, originSlot_quantity, originSlot_maxStackSize);
        }
    }
}