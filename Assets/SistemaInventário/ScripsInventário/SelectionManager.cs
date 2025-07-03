using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI;

    // Propriedade Singleton
    public static SelectionManager Instance { get; private set; }

    // NOVA VARIÁVEL: Indica se alguma UI de tela cheia está aberta
    public static bool IsAnyUIOpen = false;

    // 'onTarget' e 'selectedInteractable' agora são públicas para acesso externo se necessário.
    // Embora o IsAnyUIOpen resolva a interação indesejada, eles podem ser úteis para debug.
    public bool onTarget { get; private set; }
    public InteractableObject selectedInteractable { get; private set; }

    [Header("Raycast Settings")]
    public float interactionDistance = 10f;
    public LayerMask interactableLayerMask;

    private TextMeshProUGUI interaction_text_component;
    private Camera mainCamera; // Adicionado para cache da câmera

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

    private void Start()
    {
        onTarget = false;
        selectedInteractable = null;
        mainCamera = Camera.main; // Cache da câmera no Start

        if (interaction_Info_UI != null)
        {
            interaction_text_component = interaction_Info_UI.GetComponentInChildren<TextMeshProUGUI>();

            if (interaction_text_component != null)
            {
                interaction_Info_UI.SetActive(false); // Garante que a UI esteja desativada no início
                // O texto também pode ser limpo aqui, ou no Update se !IsAnyUIOpen
            }
            else
            {
                Debug.LogError("SelectionManager: Nenhum componente TextMeshProUGUI encontrado no 'interaction_Info_UI' ou em seus filhos.");
                interaction_Info_UI.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("SelectionManager: 'interaction_Info_UI' não está atribuído no Inspector. A UI de interação não funcionará.");
        }
    }

    void Update()
    {
        // --- NOVA LÓGICA DE BLOQUEIO DE UI ---
        if (IsAnyUIOpen)
        {
            // Se qualquer UI estiver aberta, limpa o estado de seleção e a UI de interação
            selectedInteractable = null;
            onTarget = false;
            if (interaction_Info_UI != null && interaction_Info_UI.activeSelf)
            {
                interaction_Info_UI.SetActive(false);
            }
            if (interaction_text_component != null)
            {
                interaction_text_component.text = ""; // Garante que o texto está limpo
            }
            return; // Sai do Update para não processar o raycast 3D
        }
        // --- FIM DA NOVA LÓGICA ---


        selectedInteractable = null; // Reinicia a cada frame
        onTarget = false; // Reinicia a cada frame


        // Verifica se a câmera principal está disponível antes de fazer o raycast
        if (mainCamera == null)
        {
            Debug.LogError("SelectionManager: mainCamera é nula. Certifique-se de que sua câmera principal tem a tag 'MainCamera'.");
            return;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayerMask))
        {
            var interactableComponent = hit.transform.GetComponent<InteractableObject>();

            if (interactableComponent != null && interactableComponent.playerInRange)
            {
                selectedInteractable = interactableComponent;
                onTarget = true;

                if (interaction_text_component != null)
                {
                    // Usa o GetInteractionPrompt para o texto da UI
                    interaction_text_component.text = selectedInteractable.GetInteractionPrompt() + " " + selectedInteractable.GetItemName();
                    interaction_Info_UI.SetActive(true);
                }
            }
        }

        // Se nenhum item interativo for selecionado (ou se saiu do alcance), desativa a UI de interação
        if (selectedInteractable == null && interaction_Info_UI != null && interaction_Info_UI.activeSelf)
        {
            interaction_Info_UI.SetActive(false);
        }
    }
}