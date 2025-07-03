using UnityEngine;
using UnityEngine.EventSystems; // Necess�rio para IPointerClickHandler

public class SlotItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    // O nome do item que este �cone representa
    private string itemName;

    // Tempo m�ximo entre cliques para ser considerado um clique duplo
    private const float DOUBLE_CLICK_TIME = 0.3f; // 0.3 segundos
    private float lastClickTime = 0f;

    // Refer�ncia ao ItemSlot pai para obter os dados do slot
    private ItemSlot parentItemSlot;

    // M�TODO DE INICIALIZA��O ATUALIZADO:
    // Agora recebe a refer�ncia do ItemSlot diretamente
    public void Initialize(string name, ItemSlot slot)
    {
        itemName = name;
        parentItemSlot = slot; // Atribui a refer�ncia diretamente
        if (parentItemSlot == null)
        {
            Debug.LogError($"[SlotItemClickHandler] A refer�ncia do ItemSlot passada para {gameObject.name} � nula.");
        }
    }

    // Este m�todo � chamado quando o GameObject � clicado
    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se o clique foi com o bot�o esquerdo do mouse
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            float clickTime = Time.time;

            // Se o tempo entre o clique atual e o �ltimo clique for menor que DOUBLE_CLICK_TIME
            if (clickTime - lastClickTime < DOUBLE_CLICK_TIME)
            {
                // � um clique duplo!
                Debug.Log($"Clique duplo detectado no item: {itemName}");

                // Verifica se o InventorySystem existe e se o item � equip�vel
                if (InventorySystem.Instance != null && parentItemSlot != null)
                {
                    // Chama o m�todo para equipar o item
                    InventorySystem.Instance.HandleItemEquipRequest(parentItemSlot.itemName);
                }
                else
                {
                    Debug.LogWarning("[SlotItemClickHandler] InventorySystem.Instance ou parentItemSlot � nulo. N�o foi poss�vel equipar o item.");
                }

                // Reseta o tempo do �ltimo clique para evitar m�ltiplos cliques duplos
                lastClickTime = 0f;
            }
            else
            {
                // � um clique simples, apenas registra o tempo
                lastClickTime = clickTime;
            }
        }
    }
}