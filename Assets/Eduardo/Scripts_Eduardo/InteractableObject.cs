using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string ItemName;
    public bool playerInRange;

    public string GetItemName()
    {
        return ItemName;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && playerInRange &&
            SelectionManager.Instance != null && SelectionManager.Instance.selectedInteractable == this)
        {
            // --- L�GICA CORRIGIDA ---
            // N�o precisamos mais verificar se o invent�rio est� cheio aqui.
            // Apenas tentamos adicionar o item. O pr�prio InventorySystem far� a verifica��o interna.
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddToInventory(ItemName);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError("InventorySystem.Instance n�o encontrado!");
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
        }
    }
}