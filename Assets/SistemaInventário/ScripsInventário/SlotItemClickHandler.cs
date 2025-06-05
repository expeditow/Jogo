using UnityEngine;
using UnityEngine.EventSystems; // Necess�rio para IPointerClickHandler

public class SlotItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    public string itemName; // Ser� definido quando o item for adicionado ao slot

    private float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f; // Tempo em segundos para considerar um duplo clique (ajuste conforme necess�rio)

    // Este m�todo ser� chamado pelo InventorySystem para configurar o nome do item
    public void Initialize(string name)
    {
        itemName = name;
        // Opcional: mudar o nome do GameObject para depura��o
        // gameObject.name = itemName + "_IconInSlot";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se foi o bot�o esquerdo do mouse
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
                lastClickTime = 0; // Reseta para evitar que m�ltiplos cliques r�pidos contem
            }
            else
            {
                // Primeiro clique (ou clique muito lento para ser duplo)
                lastClickTime = Time.time;
            }
        }
    }
}