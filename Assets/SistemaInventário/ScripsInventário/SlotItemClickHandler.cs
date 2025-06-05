using UnityEngine;
using UnityEngine.EventSystems; // Necessário para IPointerClickHandler

public class SlotItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    public string itemName; // Será definido quando o item for adicionado ao slot

    private float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f; // Tempo em segundos para considerar um duplo clique (ajuste conforme necessário)

    // Este método será chamado pelo InventorySystem para configurar o nome do item
    public void Initialize(string name)
    {
        itemName = name;
        // Opcional: mudar o nome do GameObject para depuração
        // gameObject.name = itemName + "_IconInSlot";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se foi o botão esquerdo do mouse
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if ((Time.time - lastClickTime) < DOUBLE_CLICK_TIME)
            {
                // Duplo clique detectado!
                Debug.Log("Duplo clique em: " + itemName);

                if (InventorySystem.Instance != null)
                {
                    // Chama o InventorySystem para lidar com a tentativa de equipar
                    InventorySystem.Instance.HandleItemEquipRequest(itemName);
                }
                lastClickTime = 0; // Reseta para evitar que múltiplos cliques rápidos contem
            }
            else
            {
                // Primeiro clique (ou clique muito lento para ser duplo)
                lastClickTime = Time.time;
            }
        }
    }
}