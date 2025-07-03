using UnityEngine;
using System.Collections.Generic;
using TMPro; // Para TextMeshPro

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [Header("UI do Crafting")]
    public GameObject craftingPanelUI; // O GameObject do seu painel de crafting
    public Transform recipeListParent; // Onde os botões de receita serão instanciados
    public GameObject recipeButtonPrefab; // Prefab do botão que mostra a receita

    [Header("Receitas de Crafting")]
    public List<CraftingRecipe> recipes;

    private InventorySystem inventory;
    private bool isOpen = false; // Estado atual do painel de crafting

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        inventory = InventorySystem.Instance; // Pega a instância do InventorySystem
        if (inventory == null)
        {
            Debug.LogError("CraftingSystem: InventorySystem.Instance não encontrado!");
        }

        if (craftingPanelUI != null)
        {
            craftingPanelUI.SetActive(false); // Garante que o painel comece desativado
        }
    }

    // Método para abrir/fechar o painel de crafting
    public void ToggleCraftingPanel()
    {
        isOpen = !isOpen;
        if (craftingPanelUI != null)
        {
            craftingPanelUI.SetActive(isOpen);
            if (isOpen)
            {
                PopulateRecipes(); // Atualiza a lista de receitas ao abrir
                Cursor.lockState = CursorLockMode.None; // Libera o cursor
            }
            else
            {
                // Verifica se o inventário NÃO está aberto antes de travar o cursor
                // Isso evita que o cursor seja travado se o jogador fechar o crafting, mas o inventário ainda estiver aberto
                if (InventorySystem.Instance == null || !InventorySystem.Instance.isOpen)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
            
            // --- NOVA LINHA AQUI ---
            // Informa ao SelectionManager o estado da UI
            SelectionManager.IsAnyUIOpen = isOpen;
        }
    }

    // NOVO MÉTODO: Retorna se o painel de crafting está aberto
    public bool IsCraftingPanelOpen()
    {
        return isOpen;
    }

    // Preenche a UI com os botões de receita
    void PopulateRecipes()
    {
        // Limpa receitas antigas para evitar duplicação
        foreach (Transform child in recipeListParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var recipe in recipes)
        {
            if (recipeButtonPrefab == null)
            {
                Debug.LogError("CraftingSystem: recipeButtonPrefab não atribuído!");
                return;
            }

            GameObject buttonGO = Instantiate(recipeButtonPrefab, recipeListParent);
            // Configura o texto do botão (pode ser mais complexo)
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                string recipeDisplay = $"{recipe.craftedItemName} ({recipe.craftedItemQuantity}) - ";
                foreach (var ingredient in recipe.ingredients)
                {
                    recipeDisplay += $"{ingredient.itemName} x{ingredient.quantity} ";
                }
                buttonText.text = recipeDisplay;
            }

            // Adiciona a funcionalidade de clique ao botão
            // Passamos o nome do item a ser criado para o método de crafting
            buttonGO.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => TryCraftItem(recipe));
        }
    }

    // Tenta criar um item
    public void TryCraftItem(CraftingRecipe recipe)
    {
        Debug.Log($"Clicou na receita de: {recipe.craftedItemName}"); // Mensagem mais específica
        if (inventory == null) return;

        bool canCraft = true;
        foreach (var ingredient in recipe.ingredients)
        {
            if (!inventory.HasItem(ingredient.itemName, ingredient.quantity))
            {
                canCraft = false;
                Debug.LogWarning($"Você não tem ingredientes suficientes para criar {recipe.craftedItemName}. Falta {ingredient.itemName} (necessário: {ingredient.quantity}).");
                break;
            }
        }

        if (canCraft)
        {
            // Remove os ingredientes
            foreach (var ingredient in recipe.ingredients)
            {
                inventory.RemoveItem(ingredient.itemName, ingredient.quantity);
            }

            // Adiciona o item criado ao inventário
            // O AddToInventory do InventorySystem já adiciona 1 por vez, então chame-o 'craftedItemQuantity' vezes
            for (int i = 0; i < recipe.craftedItemQuantity; i++)
            {
                 inventory.AddToInventory(recipe.craftedItemName);
            }
            
            Debug.Log($"Você criou {recipe.craftedItemQuantity} {recipe.craftedItemName}!");
            // Opcional: Atualizar a UI das receitas para refletir as novas quantidades do inventário
            PopulateRecipes(); // Recarrega os botões para atualizar as informações de disponibilidade
        }
        else
        {
            Debug.Log("Não foi possível criar o item. Ingredientes insuficientes.");
        }
    }
}   