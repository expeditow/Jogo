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

    public bool onTarget { get; private set; }
    public InteractableObject selectedInteractable { get; private set; }

    [Header("Raycast Settings")]
    public float interactionDistance = 10f; 
    public LayerMask interactableLayerMask;  

    private TextMeshProUGUI interaction_text_component; 

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

        if (interaction_Info_UI != null)
        {
            
            interaction_text_component = interaction_Info_UI.GetComponentInChildren<TextMeshProUGUI>();

            if (interaction_text_component != null)
            {
                interaction_Info_UI.SetActive(false);
            }
            else
            {
                Debug.LogError("SelectionManager: Nenhum componente TextMeshProUGUI encontrado no 'interaction_Info_UI' ou em seus filhos.");
                interaction_Info_UI.SetActive(false); 
            }
        }
        else
        {
            Debug.LogWarning("SelectionManager: 'interaction_Info_UI' não está atribuído no Inspector.");
        }
    }

    void Update()
    {

        selectedInteractable = null;
        onTarget = false; 

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); 
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayerMask))
        {
            var selectionTransform = hit.transform;
            var interactableComponent = selectionTransform.GetComponent<InteractableObject>();


            if (interactableComponent != null && interactableComponent.playerInRange)
            {
                selectedInteractable = interactableComponent; 
                onTarget = true;

                if (interaction_text_component != null)
                {
                    interaction_text_component.text = selectedInteractable.GetItemName(); 
                    interaction_Info_UI.SetActive(true);
                }
            }
        }

        if (selectedInteractable == null && interaction_Info_UI != null && interaction_Info_UI.activeSelf)
        {
            interaction_Info_UI.SetActive(false);
        }
    }
}