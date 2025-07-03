using UnityEngine;

public class CraftingBench : InteractableObject // Herda de InteractableObject
{
    [Header("Configuração da Bancada")]
    public GameObject brokenVisual;   // O GameObject que representa a bancada quebrada
    public GameObject repairedVisual; // O GameObject que representa a bancada consertada

    public string repairItemName = "Madeira"; // Nome do item necessário para o conserto
    public int repairItemQuantity = 5;       // Quantidade necessária do item para o conserto

    private bool isRepaired = false; // Estado atual da bancada

    // Usamos 'protected override' para chamar o Awake da classe base e adicionar nossa lógica
    protected override void Awake()
    {
        base.Awake(); // Chama o Awake de InteractableObject
        UpdateVisuals(); // Define o visual inicial baseado no estado
    }

    // Sobrescreve o método Interact() da classe base InteractableObject
    public override void Interact()
    {
     // Se a bancada já estiver consertada, abre o painel de crafting
    if (isRepaired)
    {
        Debug.Log("Bancada de trabalho já está consertada. Abrindo painel de crafting...");
        // CHAMA O SISTEMA DE CRAFTING AQUI!
        if (CraftingSystem.Instance != null)
        {
            CraftingSystem.Instance.ToggleCraftingPanel();
        }
        else
        {
            Debug.LogError("CraftingSystem.Instance não encontrado!");
        }
        return;
    }

    // Se estiver quebrada, tenta consertar
    TryRepairBench();
    }

    void UpdateVisuals()
    {
        if (brokenVisual != null)
            brokenVisual.SetActive(!isRepaired); // Ativa o visual quebrado se não estiver consertada
        if (repairedVisual != null)
            repairedVisual.SetActive(isRepaired); // Ativa o visual consertado se estiver consertada
    }

    void TryRepairBench()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("InventorySystem.Instance não encontrado!");
            return;
        }

        // Verifica se o jogador tem itens suficientes para o conserto
        if (InventorySystem.Instance.HasItem(repairItemName, repairItemQuantity))
        {
            // Remove os itens do inventário
            InventorySystem.Instance.RemoveItem(repairItemName, repairItemQuantity);

            // Marca a bancada como consertada
            isRepaired = true;
            UpdateVisuals(); // Troca o visual

            Debug.Log($"Bancada de trabalho consertada com sucesso usando {repairItemQuantity} de {repairItemName}!");

            // Opcional: Mudar o prompt de interação após o conserto
            InteractionPrompt = "Usar"; // Altera o texto que aparece na UI
            ItemName = "Bancada Consertada"; // Altera o nome do item (se relevante para a UI)
        }
        else
        {
            Debug.LogWarning($"Você precisa de {repairItemQuantity} {repairItemName} para consertar a bancada!");
            // Poderia mostrar uma mensagem na tela para o jogador (ex: "Madeira insuficiente!")
        }
    }
}