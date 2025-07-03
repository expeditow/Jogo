using UnityEngine;
using System.Collections.Generic;

// Marca como serializável para aparecer no Inspector
[System.Serializable]
public class CraftingRecipe
{
    public string craftedItemName; // Nome do item que será criado
    public int craftedItemQuantity = 1; // Quantidade do item criado

    // Lista de ingredientes necessários
    public List<Ingredient> ingredients;

    // Estrutura para cada ingrediente
    [System.Serializable]
    public class Ingredient
    {
        public string itemName; // Nome do item ingrediente
        public int quantity; // Quantidade necessária
    }
}