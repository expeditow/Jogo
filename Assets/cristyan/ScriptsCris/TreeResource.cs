using UnityEngine;
using System.Collections.Generic;

public class TreeResource : InteractableObject
{
    [Header("Configurações da Árvore")]
    [SerializeField] private int maxHealth = 6;
    private int currentHealth; // Vida atual da árvore

    public GameObject woodPrefabToDrop;
    public int woodQuantityToDrop = 1;

    public string requiredToolName = "MachadoFicticio";

    [Header("Efeitos da Árvore")]
    public List<EmissorLagrima> emissorLagrimasDosOlhos; // Lista de emissores nos olhos

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth; // Inicializa a vida da árvore
        InteractionPrompt = "Cortar";

        if (emissorLagrimasDosOlhos == null || emissorLagrimasDosOlhos.Count == 0)
        {
            Debug.LogWarning($"TreeResource: Lista de EmissorLagrimasDosOlhos vazia ou não atribuída para {ItemName}. Lágrimas não serão emitidas pelos olhos.");
        }
    }

    public override void Interact()
    {
        Debug.Log($"Interagindo com {ItemName}. Vida atual: {currentHealth}/{maxHealth}");

        string equippedTool = InventorySystem.Instance.GetEquippedItemName();

        if (string.IsNullOrEmpty(equippedTool) || equippedTool != requiredToolName)
        {
            Debug.LogWarning($"Você precisa de um '{requiredToolName}' equipado para cortar {ItemName}!");
            return;
        }

        currentHealth--; // Diminui a vida da árvore
        Debug.Log($"Você cortou {ItemName}. Faltam {currentHealth} hits.");

        // --- NOVA LÓGICA AQUI: Inicia a emissão de lágrimas no PRIMEIRO GOLPE ---
        if (currentHealth == maxHealth - 1) // Se a vida diminuiu 1 do máximo (primeiro golpe)
        {
            if (emissorLagrimasDosOlhos != null)
            {
                foreach (EmissorLagrima emissor in emissorLagrimasDosOlhos)
                {
                    if (emissor != null)
                    {
                        emissor.StartCrying(); // Inicia a emissão contínua em cada olho
                    }
                }
            }
        }
        // --- FIM DA NOVA LÓGICA ---

        if (currentHealth <= 0)
        {
            BreakTree();
        }
    }

    void BreakTree()
    {
        Debug.Log($"{ItemName} quebrou!");

        // Opcional: Parar a emissão de lágrimas quando a árvore quebra (antes de destruir)
        if (emissorLagrimasDosOlhos != null)
        {
            foreach (EmissorLagrima emissor in emissorLagrimasDosOlhos)
            {
                if (emissor != null)
                {
                    emissor.StopCrying(); // Para a emissão
                }
            }
        }

        if (woodPrefabToDrop != null)
        {
            for (int i = 0; i < woodQuantityToDrop; i++)
            {
                Vector3 dropPosition = transform.position + Vector3.up * 0.5f + Random.insideUnitSphere * 0.5f;
                dropPosition.y = transform.position.y + 0.2f; // Offset na altura
                Instantiate(woodPrefabToDrop, dropPosition, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogError($"Prefab da madeira para {ItemName} não atribuído no TreeResource!");
        }

        Destroy(gameObject); // Destrói a árvore
    }
}