using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

// A CLASSE ITEMDATA ESTÁ DEFINIDA AQUI, DENTRO DESTE MESMO ARQUIVO!
// Ela é [System.Serializable] para aparecer no Inspector do Unity.
[System.Serializable]
public class ItemData
{
    public string itemName;
    public int maxStackSize;

    [Header("Equippable Info")]
    public bool isEquippable;
    public GameObject equippablePrefab;
    public Vector3 equipPositionOffset = Vector3.zero;
    public Vector3 equipRotationOffset = Vector3.zero;

    public WeaponStats weaponStats;

    // --- NOVAS VARIÁVEIS PARA ITENS CONSUMÍVEIS ---
    [Header("Consumable Info")]
    public bool isConsumable;       // Pode ser consumido?
    public float hungerRestoration; // Quanto de fome restaura (se aplicável)
    public float healthRestoration; // Quanto de vida restaura (se aplicável)
    public AudioClip consumeSound;  // Som ao consumir
    // --- FIM DAS NOVAS VARIÁVEIS ---
}

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; }

    [Header("UI e Slots")]
    public GameObject inventoryScreenUI;
    private List<ItemSlot> slotList = new List<ItemSlot>();

    [Header("Item Database")]
    public List<ItemData> itemDatabase; // Esta lista usa a classe ItemData definida acima
    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    [Header("Configurações de Equipamento")]
    public Transform playerHand;
    public GameObject currentlyEquippedItem = null;

    [Header("Controlador de Ataque do Jogador")] // Novo cabeçalho para organizar no Inspector
    public CylinderMeleeAttack playerAttackController;

    public bool isOpen; // Indicates if the inventory is currently open

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            Debug.LogWarning(">>> INSTÂNCIA DO INVENTORYSYSTEM FOI CRIADA COM SUCESSO! <<<");
        }

        // Populate the item dictionary
        foreach (var itemData in itemDatabase)
        {
            if (!itemDictionary.ContainsKey(itemData.itemName))
            {
                itemDictionary.Add(itemData.itemName, itemData);
            }
        }
    }

    void Start()
    {
        isOpen = false;
        if (inventoryScreenUI != null)
        {
            inventoryScreenUI.SetActive(false);
            PopulateSlotList();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        // Block for cleaning (keep for debugging)
        if (Input.GetKeyDown(KeyCode.F10))
        {
            Debug.LogWarning("--- CLEARING ALL INVENTORY SLOTS ---");
            foreach (var slot in slotList)
            {
                slot.ClearSlot();
            }
        }
    }

    // New method to open/close the inventory
    public void ToggleInventory()
    {
        isOpen = !isOpen;
        if (inventoryScreenUI != null)
        {
            inventoryScreenUI.SetActive(isOpen);
            if (isOpen)
            {
                Cursor.lockState = CursorLockMode.None; // Unlock cursor
            }
            else
            {
                // Check if the crafting panel is NOT open before locking the cursor
                // This prevents the cursor from being locked if the player closes the inventory, but crafting is still open
                if (CraftingSystem.Instance == null || !CraftingSystem.Instance.IsCraftingPanelOpen()) // Use a getter method for IsCraftingPanelOpen
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }

            // Notify SelectionManager about UI state
            SelectionManager.IsAnyUIOpen = isOpen;
        }
    }

    private void PopulateSlotList()
    {
        if (inventoryScreenUI == null) return;
        slotList = inventoryScreenUI.GetComponentsInChildren<ItemSlot>().ToList();
        Debug.Log($"[InventorySystem] PopulateSlotList found {slotList.Count} slots.");
    }

    public void AddToInventory(string itemName)
    {
        if (!itemDictionary.ContainsKey(itemName))
        {
            Debug.LogError($"Item '{itemName}' does not exist in the database!");
            return;
        }

        ItemData data = itemDictionary[itemName]; // Usa a classe ItemData
        GameObject iconPrefab = Resources.Load<GameObject>(itemName); // Load the prefab once

        if (iconPrefab == null)
        {
            Debug.LogError($"Icon prefab '{itemName}' not found in the Resources folder.");
            return;
        }

        ItemSlot existingStack = FindIncompleteStack(itemName);

        if (existingStack != null)
        {
            if (existingStack.Item == null)
            {
                GameObject newIconObject = Instantiate(iconPrefab, existingStack.transform);
                newIconObject.tag = "ItemIcon";
                if (newIconObject.GetComponent<SlotItemClickHandler>() == null)
                {
                    var handler = newIconObject.AddComponent<SlotItemClickHandler>();
                    // ATUALIZAÇÃO AQUI: Passando existingStack como o ItemSlot pai
                    handler.Initialize(itemName, existingStack);
                }
            }
            existingStack.UpdateSlotData(itemName, existingStack.quantity + 1, data.maxStackSize);
        }
        else
        {
            ItemSlot emptySlot = FindNextEmptySlot();
            if (emptySlot != null)
            {
                GameObject newIconObject = Instantiate(iconPrefab, emptySlot.transform);
                newIconObject.tag = "ItemIcon";

                if (newIconObject.GetComponent<SlotItemClickHandler>() == null)
                {
                    var handler = newIconObject.AddComponent<SlotItemClickHandler>();
                    // ATUALIZAÇÃO AQUI: Passando emptySlot como o ItemSlot pai
                    handler.Initialize(itemName, emptySlot);
                }

                emptySlot.UpdateSlotData(itemName, 1, data.maxStackSize);
            }
            else
            {
                Debug.Log("Inventory full! Could not pick up " + itemName);
            }
        }
    }
    public void UseEquippedItem()
    {
        if (currentlyEquippedItem == null)
        {
            Debug.LogWarning("Nenhum item equipado para usar.");
            return;
        }

        string equippedItemName = GetEquippedItemName(); // Pega o nome "limpo" do item equipado

        if (string.IsNullOrEmpty(equippedItemName))
        {
            Debug.LogWarning("Nome do item equipado é nulo ou vazio.");
            return;
        }

        if (itemDictionary.TryGetValue(equippedItemName, out ItemData data))
        {
            if (data.isConsumable)
            {
                Debug.Log($"Usando item: {equippedItemName}");

                // Aplicar efeitos de consumo
                if (data.hungerRestoration > 0 && PlayerHunger.Instance != null)
                {
                    PlayerHunger.Instance.AddHunger(data.hungerRestoration);
                }
                if (data.healthRestoration > 0 && PlayerHealth.Instance != null) // Certifique-se que PlayerHealth é um Singleton ou referencia ele
                {
                    // Se PlayerHealth não for um Singleton, você precisará de uma referência pública para ele
                    // Ex: public PlayerHealth playerHealth; e arrastar no Inspector, ou GetComponent no Awake
                    PlayerHealth.Instance.Heal(data.healthRestoration); // Chame o método Heal
                }

                // Tocar som de consumo (se tiver um AudioSource e AudioClip)
                // Se o player tiver um AudioSource, pode ser usado:
                // var playerAudioSource = FindObjectOfType<AudioSource>(); // Ou referência mais específica
                // if (data.consumeSound != null && playerAudioSource != null)
                // {
                //     playerAudioSource.PlayOneShot(data.consumeSound);
                // }

                // Remover o item consumido do slot equipado
                // Isso é mais complexo: precisamos saber de qual slot o item veio para remover 1 unidade
                // Por agora, vamos simplificar: se o item é consumível e está equipado, ele é removido.
                // Uma implementação mais robusta envolveria passar o ItemSlot de origem do item equipado
                // para o HandleItemEquipRequest ou ter uma forma de remover 1 unidade do slot.

                // Para uma solução rápida: desequipe o item (ele será destruído)
                // E, se for um item stackable, idealmente você removeria 1 da pilha no inventário
                // Por enquanto, vamos assumir que consumir remove o item equipado e o destruiria.
                // Você pode adicionar lógica aqui para remover 1 da pilha no inventário.

                // --- REMOÇÃO DO ITEM CONSUMIDO (SIMPLIFICADO PARA TESTE) ---
                // Para uma remoção correta: Encontre o slot que contém o item equipado
                ItemSlot equippedSlot = slotList.Find(slot => slot.itemName == equippedItemName);
                if (equippedSlot != null)
                {
                    equippedSlot.quantity--;
                    if (equippedSlot.quantity <= 0)
                    {
                        HandleItemEquipRequest(equippedItemName); // Desequipa e destrói o visual da mão
                        equippedSlot.ClearSlot(); // Limpa o slot do inventário
                        Debug.Log($"Item '{equippedItemName}' consumido e removido do inventário.");
                    }
                    else
                    {
                        equippedSlot.UpdateSlotUI(); // Atualiza a quantidade no slot
                        Debug.Log($"Item '{equippedItemName}' consumido. Quantidade restante: {equippedSlot.quantity}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Não foi possível encontrar o slot para remover '{equippedItemName}' após o consumo.");
                }

            }
            else
            {
                Debug.LogWarning($"Item '{equippedItemName}' não é consumível.");
            }
        }
        else
        {
            Debug.LogError($"Dados para '{equippedItemName}' não encontrados no dicionário de itens.");
        }
    }

    private ItemSlot FindIncompleteStack(string itemName)
    {
        foreach (ItemSlot slot in slotList)
        {
            if (slot != null && slot.itemName == itemName && slot.quantity < slot.maxStackSize)
            {
                return slot;
            }
        }
        return null;
    }

    private ItemSlot FindNextEmptySlot()
    {
        foreach (ItemSlot slot in slotList)
        {
            if (slot != null && string.IsNullOrEmpty(slot.itemName))
            {
                return slot;
            }
        }
        return null;
    }

    public void HandleItemEquipRequest(string itemNameFromSlot)
    {
        Debug.Log($"[1] Pedido para equipar '{itemNameFromSlot}' recebido.");

        if (playerHand == null)
        {
            Debug.LogError("[FALHA] O campo 'Player Hand' no InventorySystem não está atribuído!");
            return;
        }
        // ---> NOVA LINHA: Verificação crucial para o controlador de ataque <---
        if (playerAttackController == null)
        {
            Debug.LogError("[FALHA] O campo 'Player Attack Controller' no InventorySystem não está atribuído! Não é possível atualizar a lógica da arma.");
            return;
        }

        if (itemDictionary.TryGetValue(itemNameFromSlot, out ItemData data))
        {
            Debug.Log($"[2] Dados para '{itemNameFromSlot}' encontrados.");

            if (data.isEquippable)
            {
                Debug.Log($"[3] O item é equipável.");

                if (data.equippablePrefab == null)
                {
                    Debug.LogError($"[FALHA] O item '{itemNameFromSlot}' é equipável, mas o 'Equippable Prefab' não foi atribuído!");
                    return;
                }

                // Lógica para DESEQUIPAR o item atual
                if (currentlyEquippedItem != null && currentlyEquippedItem.name.StartsWith(data.equippablePrefab.name))
                {
                    Debug.Log($"[AÇÃO] Desequipando item.");
                    Destroy(currentlyEquippedItem);
                    currentlyEquippedItem = null;

                    // ---> NOVA LINHA: Informa ao controlador de ataque que nenhuma arma está equipada <---
                    playerAttackController.SetCurrentWeaponStats(null);
                    return;
                }

                // Lógica para EQUIPAR um novo item (destruindo o antigo se houver)
                if (currentlyEquippedItem != null)
                {
                    Destroy(currentlyEquippedItem);
                }

                GameObject newEquippedItem = Instantiate(data.equippablePrefab, playerHand);

                newEquippedItem.transform.localPosition = Vector3.zero;
                newEquippedItem.transform.localRotation = Quaternion.identity;

                newEquippedItem.transform.localPosition += data.equipPositionOffset;
                newEquippedItem.transform.localRotation *= Quaternion.Euler(data.equipRotationOffset);

                currentlyEquippedItem = newEquippedItem;
                Debug.Log($"[SUCESSO] '{currentlyEquippedItem.name}' foi criado na mão do jogador com offsets.");

                // ---> NOVA LINHA: Envia os stats da nova arma para o controlador de ataque! <---
                playerAttackController.SetCurrentWeaponStats(data.weaponStats);
            }
            else
            {
                Debug.LogWarning($"[INFO] O item '{itemNameFromSlot}' não está marcado como equipável.");
                // ---> NOVA LINHA (Opcional, mas bom): Se o item não é equipável, garante que a lógica de ataque seja nula <---
                playerAttackController.SetCurrentWeaponStats(null);
            }
        }
        else
        {
            Debug.LogError($"[FALHA] Não foi possível encontrar os dados para '{itemNameFromSlot}' no dicionário.");
        }
    }

    public bool HasItem(string itemName, int quantity)
    {
        int currentQuantity = 0;
        foreach (var slot in slotList)
        {
            if (slot.itemName == itemName)
            {
                currentQuantity += slot.quantity;
            }
        }
        return currentQuantity >= quantity;
    }

    public void RemoveItem(string itemName, int quantityToRemove)
    {
        if (!HasItem(itemName, quantityToRemove))
        {
            Debug.LogWarning($"Tentando remover {quantityToRemove} de {itemName}, mas o jogador não tem o suficiente.");
            return;
        }

        int removedCount = 0;
        for (int i = 0; i < slotList.Count && removedCount < quantityToRemove; i++)
        {
            ItemSlot slot = slotList[i];
            if (slot.itemName == itemName)
            {
                int canRemoveFromSlot = Mathf.Min(slot.quantity, quantityToRemove - removedCount);
                slot.quantity -= canRemoveFromSlot;
                removedCount += canRemoveFromSlot;

                if (slot.quantity <= 0)
                {
                    slot.ClearSlot();
                }
                else
                {
                    slot.UpdateSlotUI();
                }
            }
        }
        Debug.Log($"Removidos {quantityToRemove} de {itemName} do inventário.");
    }
    // Dentro da classe InventorySystem
    public string GetEquippedItemName()
    {
        if (currentlyEquippedItem != null)
        {
            // O nome do prefab pode ter "(Clone)" no final.
            // Precisamos do nome base para comparar.
            return currentlyEquippedItem.name.Replace("(Clone)", "").Trim();
        }
        return null; // Retorna null se não houver nada equipado
    }

    // Você também pode querer um método para verificar o ItemData do item equipado
    public ItemData GetEquippedItemData()
    {
        string equippedName = GetEquippedItemName();
        if (!string.IsNullOrEmpty(equippedName) && itemDictionary.ContainsKey(equippedName))
        {
            return itemDictionary[equippedName];
        }
        return null;
    }

}