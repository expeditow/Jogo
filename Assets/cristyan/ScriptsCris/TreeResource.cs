using UnityEngine;
using System.Collections.Generic;

public class TreeResource : InteractableObject
{
    [Header("Configura��es da �rvore")]
    [SerializeField] private int maxHealth = 6;
    private int currentHealth; // Vida atual da �rvore

    public GameObject woodPrefabToDrop;
    public int woodQuantityToDrop = 1;

    public string requiredToolName = "MachadoFicticio";

    [Header("Efeitos da �rvore")]
    public List<EmissorLagrima> emissorLagrimasDosOlhos; // Lista de emissores nos olhos

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth; // Inicializa a vida da �rvore
        InteractionPrompt = "Cortar";

        if (emissorLagrimasDosOlhos == null || emissorLagrimasDosOlhos.Count == 0)
        {
            Debug.LogWarning($"TreeResource: Lista de EmissorLagrimasDosOlhos vazia ou n�o atribu�da para {ItemName}. L�grimas n�o ser�o emitidas pelos olhos.");
        }
    }

    public override void Interact()
    {
        Debug.Log($"Interagindo com {ItemName}. Vida atual: {currentHealth}/{maxHealth}");

        string equippedTool = InventorySystem.Instance.GetEquippedItemName();

        if (string.IsNullOrEmpty(equippedTool) || equippedTool != requiredToolName)
        {
            Debug.LogWarning($"Voc� precisa de um '{requiredToolName}' equipado para cortar {ItemName}!");
            return;
        }

        currentHealth--; // Diminui a vida da �rvore
        Debug.Log($"Voc� cortou {ItemName}. Faltam {currentHealth} hits.");

        // --- NOVA L�GICA AQUI: Inicia a emiss�o de l�grimas no PRIMEIRO GOLPE ---
        if (currentHealth == maxHealth - 1) // Se a vida diminuiu 1 do m�ximo (primeiro golpe)
        {
            if (emissorLagrimasDosOlhos != null)
            {
                foreach (EmissorLagrima emissor in emissorLagrimasDosOlhos)
                {
                    if (emissor != null)
                    {
                        emissor.StartCrying(); // Inicia a emiss�o cont�nua em cada olho
                    }
                }
            }
        }
        // --- FIM DA NOVA L�GICA ---

        if (currentHealth <= 0)
        {
            BreakTree();
        }
    }

    void BreakTree()
    {
        Debug.Log($"{ItemName} quebrou!");

        // Opcional: Parar a emiss�o de l�grimas quando a �rvore quebra (antes de destruir)
        if (emissorLagrimasDosOlhos != null)
        {
            foreach (EmissorLagrima emissor in emissorLagrimasDosOlhos)
            {
                if (emissor != null)
                {
                    emissor.StopCrying(); // Para a emiss�o
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
            Debug.LogError($"Prefab da madeira para {ItemName} n�o atribu�do no TreeResource!");
        }

        Destroy(gameObject); // Destr�i a �rvore
    }
}