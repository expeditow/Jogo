using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    // --- A VARIÁVEL QUE ESTAVA FALTANDO ---
    // Esta variável pública guardará a referência do slot de onde o item saiu.
    public Transform parentAfterDrag;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        // Pega a referência do canvas principal de forma mais robusta
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        // Guarda o slot de onde saiu
        parentAfterDrag = transform.parent;
        // Move o item para o topo da hierarquia do canvas para que fique na frente de tudo
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();

        // Deixa o ícone semitransparente e permite que o raycast passe por ele
        canvasGroup.alpha = .6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;
        // Move o ícone seguindo o mouse
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // O OnDrop do ItemSlot vai cuidar de definir o novo pai.
        // Se, ao final do arrasto, o pai ainda for o canvas, significa que foi solto em um local inválido.
        if (transform.parent == canvas.transform)
        {
            // Devolve o item para o slot original
            transform.SetParent(parentAfterDrag);
            rectTransform.localPosition = Vector3.zero;
        }

        // Restaura a aparência e o comportamento do ícone
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}