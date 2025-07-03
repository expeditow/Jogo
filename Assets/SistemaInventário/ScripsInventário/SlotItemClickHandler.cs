using UnityEngine;
using UnityEngine.EventSystems; // Necessário para IPointerClickHandler

public class SlotItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    // O nome do item que este ícone representa
    private string itemName;

    // Tempo máximo entre cliques para ser considerado um clique duplo
    private const float DOUBLE_CLICK_TIME = 0.3f; // 0.3 segundos
    private float lastClickTime = 0f;

    // Referência ao ItemSlot pai para obter os dados do slot
    private ItemSlot parentItemSlot;

    // MÉTODO DE INICIALIZAÇÃO ATUALIZADO:
    // Agora recebe a referência do ItemSlot diretamente
    public void Initialize(string name, ItemSlot slot)
    {
        itemName = name;
        parentItemSlot = slot; // Atribui a referência diretamente
        if (parentItemSlot == null)
        {
            Debug.LogError($"[SlotItemClickHandler] A referência do ItemSlot passada para {gameObject.name} é nula.");
        }
    }

    // Este método é chamado quando o GameObject é clicado
    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se o clique foi com o botão esquerdo do mouse
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            float clickTime = Time.time;

            // Se o tempo entre o clique atual e o último clique for menor que DOUBLE_CLICK_TIME
            if (clickTime - lastClickTime < DOUBLE_CLICK_TIME)
            {
                // É um clique duplo!
                Debug.Log($"Clique duplo detectado no item: {itemName}");

                // Verifica se o InventorySystem existe e se o item é equipável
                if (InventorySystem.Instance != null && parentItemSlot != null)
                {
                    // Chama o método para equipar o item
                    InventorySystem.Instance.HandleItemEquipRequest(parentItemSlot.itemName);
                }
                else
                {
                    Debug.LogWarning("[SlotItemClickHandler] InventorySystem.Instance ou parentItemSlot é nulo. Não foi possível equipar o item.");
                }

                // Reseta o tempo do último clique para evitar múltiplos cliques duplos
                lastClickTime = 0f;
            }
            else
            {
                // É um clique simples, apenas registra o tempo
                lastClickTime = clickTime;
            }
        }
    }
}