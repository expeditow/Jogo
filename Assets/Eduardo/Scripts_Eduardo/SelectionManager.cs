using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Necessário para usar TextMeshProUGUI

public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI;

    public static SelectionManager Instance { get; set; }
    public bool onTarget;
    private TextMeshProUGUI interaction_text;

    private void Start()
    {
        onTarget = false;
        if (interaction_Info_UI != null)
        {
            // Ativa a UI uma vez para forçar o layout a inicializar corretamente
            interaction_Info_UI.SetActive(true);

            // Pega o componente de texto da UI
            interaction_text = interaction_Info_UI.GetComponent<TextMeshProUGUI>();

            // Esconde novamente
            interaction_Info_UI.SetActive(false);
        }
    }

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

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var selectionTransform = hit.transform;
            var interactable = selectionTransform.GetComponent<InteractableObject>();

            if (interactable != null && interaction_text != null && interactable.playerInRange)
            {
                onTarget = true;
                interaction_text.text = interactable.GetItemName();
                interaction_Info_UI.SetActive(true);
            }
            else
            {
                onTarget = false;
                interaction_Info_UI.SetActive(false);
            }
        }
        else
        {
            onTarget = false;
            interaction_Info_UI.SetActive(false);
        }
    }
}