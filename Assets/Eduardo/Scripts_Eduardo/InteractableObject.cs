using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string ItemName; // Nome do item que este objeto representa (se for pegável)
    public bool playerInRange;

    [Header("Configuração de Interação")]
    public string InteractionPrompt = "Pegar"; // Texto que aparecerá na UI (ex: "Pegar", "Consertar", "Abrir")

    public string GetItemName()
    {
        return ItemName; // Ainda usado para identificar o item em si
    }

    // Novo método para obter o texto de prompt de interação
    public string GetInteractionPrompt()
    {
        return InteractionPrompt;
    }

    // Método VIRTUAL que pode ser sobrescrito por classes filhas
    public virtual void Interact()
    {
        // Lógica padrão de interação: adicionar o item ao inventário
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddToInventory(ItemName);
            Destroy(gameObject); // Destrói o item após ser pego
            Debug.Log($"[InteractableObject] Item '{ItemName}' pego.");
        }
        else
        {
            Debug.LogError("InventorySystem.Instance não encontrado!");
        }
    }

    // Mude o Update para protected virtual para que classes filhas possam sobrescrever se necessário
    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && playerInRange &&
            SelectionManager.Instance != null && SelectionManager.Instance.selectedInteractable == this)
        {
            Interact(); // Chama o método Interact (que será o da classe filha se sobrescrito)
        }
    }

    // Usamos 'protected virtual void Awake()' caso classes filhas precisem sobrescrever também.
    // Se você não tem um Awake em InteractableObject e não planeja ter, pode ignorar esta parte.
    protected virtual void Awake()
    {
        // Seu código de Awake, se houver
    }
    protected virtual void Start()
    {
        
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