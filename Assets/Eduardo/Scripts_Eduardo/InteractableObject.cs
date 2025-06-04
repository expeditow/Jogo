using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string ItemName;
    public bool playerInRange; // Determinado pelo Trigger

    // Este m�todo � usado pelo SelectionManager para mostrar o nome na UI
    public string GetItemName()
    {
        return ItemName;
    }

    void Update()
    {
        // Verifica o clique, se o jogador est� no alcance do trigger DESTE item
        // E, crucialmente, se ESTE gameObject � o que o SelectionManager est� selecionando
        if (Input.GetKeyDown(KeyCode.Mouse0) && playerInRange &&
            SelectionManager.Instance != null && SelectionManager.Instance.selectedInteractable == this)
        {
            if (InventorySystem.Instance != null && !InventorySystem.Instance.CheckIfFull())
            {
                InventorySystem.Instance.AddToInventory(ItemName);
                Debug.Log($"Pegou: {ItemName}");

                // Opcional: Se este item for destru�do, o SelectionManager pode precisar ser notificado
                // ou simplesmente limpar� a refer�ncia no pr�ximo Update.
                // Se quiser limpar imediatamente:
                // if(SelectionManager.Instance.selectedInteractable == this)
                // {
                //     SelectionManager.Instance.ClearSelection(); // Um m�todo que voc� criaria em SelectionManager
                // }
                Destroy(gameObject);
            }
            else
            {
                if (InventorySystem.Instance == null)
                {
                    Debug.LogError("InventorySystem.Instance n�o encontrado!");
                }
                else
                {
                    Debug.Log("Invent�rio cheio. N�o foi poss�vel pegar " + ItemName);
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
            // a UI de intera��o deve ser desativada. O Update do SelectionManager j� cuida disso
            // pois reavalia 'interactableComponent.playerInRange'.
        }
    }
}