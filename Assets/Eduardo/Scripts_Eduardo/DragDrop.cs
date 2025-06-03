using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    public static GameObject itemBeingDragged;
    Vector3 startPosition;
    Transform startParent;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        // Pega a referência do canvas via singleton
        canvas = ItemManager.Instance?.mainCanvas;

        if (canvas == null)
        {
            Debug.LogError("Canvas não foi encontrado! Verifique se o ItemManager está na cena e se o mainCanvas foi atribuído.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Debug.Log("OnBeginDrag");
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;

        startPosition = transform.position;
        startParent = transform.parent;

        transform.SetParent(canvas.transform); // Manter no canvas para não ser afetado por layout
        itemBeingDragged = gameObject;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        // Move o item considerando o scale do canvas
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        itemBeingDragged = null;

        if (transform.parent == startParent || transform.parent == canvas.transform)
        {
            transform.position = startPosition;
            transform.SetParent(startParent);
        }

        Debug.Log("OnEndDrag");
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}