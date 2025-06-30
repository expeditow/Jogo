using UnityEngine;
using UnityEngine.EventSystems;

public class SlotItemClickHandler : MonoBehaviour, IPointerClickHandler
{
    public string itemName; // Esta vari�vel ser� preenchida pelo InventorySystem

    private float lastClickTime = 0f;
    private const float DOUBLE_CLICK_TIME = 0.3f;

    // --- O M�TODO QUE ESTAVA FALTANDO ---
    // Este m�todo permite que o InventorySystem "diga" a este script qual � o nome do item.
    public void Initialize(string name)
    {
        this.itemName = name;
    }

    // O m�todo OnPointerClick agora funcionar� corretamente
    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se o clique foi com o bot�o esquerdo
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // L�gica do clique duplo
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