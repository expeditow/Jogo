using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; set; } 

    public GameObject inventoryScreenUI;
    public List<GameObject> slotList = new List<GameObject>();
    public List<string> itemList = new List<string>(); 

    [Header("Configurações de Equipamento")]
    public Transform playerHand; 
    public GameObject currentlyEquippedItem = null; 

    private GameObject itemToAdd;
    private GameObject whatSlotToEquip; 

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
        else
        {
            Debug.LogError("InventorySystem: inventoryScreenUI não está atribuído no Inspector!");
        }
    }

    private void PopulateSlotList()
    {
        if (inventoryScreenUI == null) return; 

        foreach (Transform child in inventoryScreenUI.transform)
        {
            if (child.CompareTag("Slot"))
            {
                slotList.Add(child.gameObject);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) 
        {
            isOpen = !isOpen; 
            inventoryScreenUI.SetActive(isOpen);
            Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
            if (isOpen) Debug.Log("Inventário aberto"); else Debug.Log("Inventário fechado");
        }
    }

    public void AddToInventory(string itemName) 
    {
        if (CheckIfFull())
        {
            Debug.LogWarning("Inventário cheio. Não foi possível pegar " + itemName);
            return;
        }

        whatSlotToEquip = FindNextEmptySlot();

        if (whatSlotToEquip == null) 
        {
            Debug.LogError("Não foi encontrado slot vazio para adicionar " + itemName);
            return;
        }

        GameObject iconPrefab = Resources.Load<GameObject>(itemName);

        if (iconPrefab == null)
        {
            Debug.LogError($"Prefab do ícone do item '{itemName}' não encontrado na pasta Resources.");
            return;
        }

        itemToAdd = Instantiate(iconPrefab, whatSlotToEquip.transform.position, Quaternion.identity);
        itemToAdd.transform.SetParent(whatSlotToEquip.transform, false); 
        itemToAdd.transform.localPosition = Vector3.zero;
        itemToAdd.transform.localScale = Vector3.one; 


        SlotItemClickHandler clickHandler = itemToAdd.GetComponent<SlotItemClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = itemToAdd.AddComponent<SlotItemClickHandler>();
            Debug.LogWarning($"SlotItemClickHandler adicionado dinamicamente a {itemName}. É melhor adicioná-lo ao prefab do ícone.");
        }
        clickHandler.Initialize(itemName);

        itemList.Add(itemName);
        Debug.Log($"Pegou: {itemName} e adicionou ao inventário.");
    }

    public bool CheckIfFull()
    {
        int counter = 0;
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount > 0)
            {
                counter++;
            }
        }
        return counter >= slotList.Count;
    }

    private GameObject FindNextEmptySlot()
    {
        foreach (GameObject slot in slotList)
        {
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        Debug.LogWarning("Nenhum slot vazio encontrado.");
        return null; 
    }
    public void HandleItemEquipRequest(string itemNameFromSlot)
    {
        Debug.Log($"InventorySystem recebeu pedido para equipar: {itemNameFromSlot}");

        if (playerHand == null)
        {
            Debug.LogError("Player Hand transform não está atribuído no InventorySystem! Não é possível equipar.");
            return;
        }

        if (itemNameFromSlot == "Pedra")
        {
            string equippablePrefabName = "pedraMao"; 

            // 1. Desequipar item atual (se houver)
            if (currentlyEquippedItem != null)
            {
                Destroy(currentlyEquippedItem);
                currentlyEquippedItem = null;
                Debug.Log("Item anterior desequipado.");
            }

            GameObject prefabToEquip = Resources.Load<GameObject>(equippablePrefabName);

            if (prefabToEquip != null)
            {
                currentlyEquippedItem = Instantiate(prefabToEquip, playerHand.position, playerHand.rotation);
                currentlyEquippedItem.transform.SetParent(playerHand);
                currentlyEquippedItem.transform.localPosition = Vector3.zero;
                currentlyEquippedItem.transform.localRotation = Quaternion.identity;
                currentlyEquippedItem.transform.localScale = Vector3.one; 

                Debug.Log($"'{equippablePrefabName}' equipado na mão do jogador.");
            }
            else
            {
                Debug.LogError($"Prefab equipável '{equippablePrefabName}' não encontrado na pasta Resources.");
            }
        }
        else
        {
            Debug.LogWarning($"Nenhuma ação de equipamento definida para: {itemNameFromSlot}");
        }
    }
}