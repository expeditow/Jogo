using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public int maxStackSize;

    [Header("Equippable Info")]
    public bool isEquippable; // É um item que pode ser equipado?
    public GameObject equippablePrefab; // O prefab que vai para a mão do jogador
}

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; }

    [Header("UI e Slots")]
    public GameObject inventoryScreenUI;
    private List<ItemSlot> slotList = new List<ItemSlot>();

    [Header("Item Database")]
    public List<ItemData> itemDatabase;
    private Dictionary<string, ItemData> itemDictionary = new Dictionary<string, ItemData>();

    [Header("Configurações de Equipamento")]
    public Transform playerHand;
    public GameObject currentlyEquippedItem = null;

    public bool isOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // --- SINAL DE VIDA ---
            Debug.LogWarning(">>> INSTÂNCIA DO INVENTORYSYSTEM FOI CRIADA COM SUCESSO! <<<");
        }

        // Popula o dicionário de itens
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
            isOpen = !isOpen;
            inventoryScreenUI.SetActive(isOpen);
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // ADICIONE ESTE BLOCO PARA LIMPEZA
        if (Input.GetKeyDown(KeyCode.F10)) // Usaremos a tecla F10 para limpar
        {
            Debug.LogWarning("--- LIMPANDO TODOS OS SLOTS DO INVENTÁRIO ---");
            foreach (var slot in slotList)
            {
                slot.ClearSlot();
            }
        }
    }

    private void PopulateSlotList()
    {
        if (inventoryScreenUI == null) return;
        slotList = inventoryScreenUI.GetComponentsInChildren<ItemSlot>().ToList();
        Debug.Log($"[InventorySystem] PopulateSlotList encontrou {slotList.Count} slots.");
    }

    public void AddToInventory(string itemName)
    {
        if (!itemDictionary.ContainsKey(itemName))
        {
            Debug.LogError($"Item '{itemName}' não existe no banco de dados!");
            return;
        }

        ItemData data = itemDictionary[itemName];
        GameObject iconPrefab = Resources.Load<GameObject>(itemName); // Carrega o prefab uma vez

        if (iconPrefab == null)
        {
            Debug.LogError($"Prefab do ícone '{itemName}' não encontrado na pasta Resources.");
            return;
        }

        ItemSlot existingStack = FindIncompleteStack(itemName);

        if (existingStack != null)
        {
            // Se encontramos um stack, primeiro garantimos que ele tenha um ícone visível.
            if (existingStack.Item == null)
            {
                // Este é o caso do "slot fantasma"! Criamos o ícone que faltava.
                GameObject newIconObject = Instantiate(iconPrefab, existingStack.transform);
                newIconObject.tag = "ItemIcon";
                // Adiciona o script de clique
                if (newIconObject.GetComponent<SlotItemClickHandler>() == null)
                {
                    var handler = newIconObject.AddComponent<SlotItemClickHandler>();
                    handler.Initialize(itemName);
                }
            }

            // Agora, com a certeza de que o ícone existe, apenas atualizamos os dados.
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
                    handler.Initialize(itemName);
                }

                emptySlot.UpdateSlotData(itemName, 1, data.maxStackSize);
            }
            else
            {
                Debug.Log("Inventário cheio! Não foi possível pegar " + itemName);
            }
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

    // Seu método de equipar item continua aqui
    public void HandleItemEquipRequest(string itemNameFromSlot)
    {
        Debug.Log($"[1] Pedido para equipar '{itemNameFromSlot}' recebido.");

        // Verificação 1: A mão do jogador está atribuída?
        if (playerHand == null)
        {
            Debug.LogError("[FALHA] O campo 'Player Hand' no InventorySystem não está atribuído!");
            return;
        }

        // Verificação 2: O item existe no banco de dados?
        if (itemDictionary.TryGetValue(itemNameFromSlot, out ItemData data))
        {
            Debug.Log($"[2] Dados para '{itemNameFromSlot}' encontrados.");

            // Verificação 3: O item é equipável?
            if (data.isEquippable)
            {
                Debug.Log($"[3] O item é equipável.");

                // Verificação 4: O prefab do item de mão existe?
                if (data.equippablePrefab == null)
                {
                    Debug.LogError($"[FALHA] O item '{itemNameFromSlot}' é equipável, mas o 'Equippable Prefab' não foi atribuído!");
                    return;
                }

                // Se o item já está equipado, desequipa.
                if (currentlyEquippedItem != null && currentlyEquippedItem.name.StartsWith(data.equippablePrefab.name))
                {
                    Debug.Log($"[AÇÃO] Desequipando item.");
                    Destroy(currentlyEquippedItem);
                    currentlyEquippedItem = null;
                    return;
                }

                // Se outro item estiver equipado, destrói ele primeiro.
                if (currentlyEquippedItem != null)
                {
                    Destroy(currentlyEquippedItem);
                }

                // --- PONTO CRÍTICO DA LÓGICA ---
                // Cria o item como filho da mão e zera sua posição e rotação locais.
                GameObject newEquippedItem = Instantiate(data.equippablePrefab, playerHand);
                newEquippedItem.transform.localPosition = Vector3.zero;
                newEquippedItem.transform.localRotation = Quaternion.identity;
                currentlyEquippedItem = newEquippedItem;
                // ------------------------------------

                Debug.Log($"[SUCESSO] '{currentlyEquippedItem.name}' foi criado na mão do jogador.");
            }
            else
            {
                Debug.LogWarning($"[INFO] O item '{itemNameFromSlot}' não está marcado como equipável.");
            }
        }
        else
        {
            Debug.LogError($"[FALHA] Não foi possível encontrar os dados para '{itemNameFromSlot}' no dicionário.");
        }
    }



} // <--- A CLASSE TERMINA AQUI. Verifique se não há nenhum código extra depois desta chave.