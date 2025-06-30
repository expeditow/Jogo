using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public Canvas mainCanvas;  // Referência ao canvas da UI

    private void Awake()
    {
        // Garante que só exista um ItemManager na cena
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}