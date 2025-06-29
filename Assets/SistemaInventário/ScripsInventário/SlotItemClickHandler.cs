using UnityEngine;
using UnityEngine.EventSystems;

public class SlotItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    public string itemName; // Esta variável será preenchida pelo InventorySystem

    private float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f;

    // --- O MÉTODO QUE ESTAVA FALTANDO ---
    // Este método permite que o InventorySystem "diga" a este script qual é o nome do item.
    public void Initialize(string name)
    {
        this.itemName = name;
    }

    // O método OnPointerClick agora funcionará corretamente
    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se o clique foi com o botão esquerdo
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Lógica do clique duplo
            if ((Time.time - lastClickTime) < DOUBLE_CLICK_TIME)
            {
                Debug.Log("Duplo clique em: " + itemName); // Agora vai mostrar "Pedra"

                if (InventorySystem.Instance != null && !string.IsNullOrEmpty(itemName))
                {
                    // Passa o nome correto para o sistema de equipar
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