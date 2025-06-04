using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string ItemName;
    public bool playerInRange; // Determinado pelo Trigger

    // Este método é usado pelo SelectionManager para mostrar o nome na UI
    public string GetItemName()
    {
        return ItemName;
    }

    void Update()
    {
        // Verifica o clique, se o jogador está no alcance do trigger DESTE item
        // E, crucialmente, se ESTE gameObject é o que o SelectionManager está selecionando
        if (Input.GetKeyDown(KeyCode.Mouse0) && playerInRange &&
            SelectionManager.Instance != null && SelectionManager.Instance.selectedInteractable == this)
        {
            if (InventorySystem.Instance != null && !InventorySystem.Instance.CheckIfFull())
            {
                InventorySystem.Instance.AddToInventory(ItemName);
                Debug.Log($"Pegou: {ItemName}");

                // Opcional: Se este item for destruído, o SelectionManager pode precisar ser notificado
                // ou simplesmente limpará a referência no próximo Update.
                // Se quiser limpar imediatamente:
                // if(SelectionManager.Instance.selectedInteractable == this)
                // {
                //     SelectionManager.Instance.ClearSelection(); // Um método que você criaria em SelectionManager
                // }
                Destroy(gameObject);
            }
            else
            {
                if (InventorySystem.Instance == null)
                {
                    Debug.LogError("InventorySystem.Instance não encontrado!");
                }
                else
                {
                    Debug.Log("Inventário cheio. Não foi possível pegar " + ItemName);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // Se o jogador sair do alcance e este era o objeto selecionado,
            // a UI de interação deve ser desativada. O Update do SelectionManager já cuida disso
            // pois reavalia 'interactableComponent.playerInRange'.
        }
    }
}