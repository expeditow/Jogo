using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Mantido caso use outros componentes UI, mas não diretamente para TMP
using TMPro;         // Necessário para usar TextMeshProUGUI

public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI; // O GameObject que contém o TextMeshProUGUI

    // Propriedade Singleton
    public static SelectionManager Instance { get; private set; } // Alterado para private set

    public bool onTarget { get; private set; } // Pode ser útil para outros sistemas saberem se algo está na mira

    // NOVA ADIÇÃO: Referência ao objeto interativo especificamente selecionado
    public InteractableObject selectedInteractable { get; private set; }

    // RECOMENDAÇÕES: Para melhor controle e desempenho do Raycast
    [Header("Raycast Settings")]
    public float interactionDistance = 10f; // Distância máxima que o raio alcança
    public LayerMask interactableLayerMask;  // Máscara de layer para o raio atingir apenas interativos

    private TextMeshProUGUI interaction_text_component; // Renomeado para clareza

    private void Awake() // Awake é chamado antes de Start
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

    private void Start()
    {
        onTarget = false;
        selectedInteractable = null; // Inicializa a nova variável

        if (interaction_Info_UI != null)
        {
            // Pega o componente de texto da UI. É mais seguro procurar nos filhos também se a estrutura for complexa.
            interaction_text_component = interaction_Info_UI.GetComponentInChildren<TextMeshProUGUI>();

            if (interaction_text_component != null)
            {
                interaction_Info_UI.SetActive(false); // Esconde a UI inicialmente
            }
            else
            {
                Debug.LogError("SelectionManager: Nenhum componente TextMeshProUGUI encontrado no 'interaction_Info_UI' ou em seus filhos.");
                interaction_Info_UI.SetActive(false); // Garante que está desligado se o texto não for encontrado
            }
        }
        else
        {
            Debug.LogWarning("SelectionManager: 'interaction_Info_UI' não está atribuído no Inspector.");
        }
    }

    void Update()
    {
        // Limpa a seleção anterior no início de cada frame.
        // Se o objeto ainda estiver sendo mirado, será reatribuído.
        selectedInteractable = null;
        onTarget = false; // Assume que nada está na mira até que o Raycast prove o contrário

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Certifique-se que sua câmera principal tem a tag "MainCamera"
        RaycastHit hit;

        // Dispara o raio com distância e máscara de layer
        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayerMask))
        {
            var selectionTransform = hit.transform;
            var interactableComponent = selectionTransform.GetComponent<InteractableObject>();

            // Se atingiu um interativo E o jogador está no alcance dele (para fins de UI e interação)
            if (interactableComponent != null && interactableComponent.playerInRange)
            {
                selectedInteractable = interactableComponent; // ARMAZENA O OBJETO ESPECÍFICO
                onTarget = true;

                if (interaction_text_component != null)
                {
                    interaction_text_component.text = selectedInteractable.GetItemName(); // Usa o GetItemName do objeto específico
                    interaction_Info_UI.SetActive(true);
                }
            }
        }

        // Se, após o Raycast, nenhum interativo válido foi selecionado (ou o que foi mirado não está com playerInRange),
        // garante que a UI de interação esteja desligada.
        if (selectedInteractable == null && interaction_Info_UI != null && interaction_Info_UI.activeSelf)
        {
            interaction_Info_UI.SetActive(false);
        }
    }
}