using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHunger : MonoBehaviour
{
    public static PlayerHunger Instance { get; private set; }

    [Header("Configurações da Fome")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger;
    [SerializeField] private float hungerIncreaseRate = 1f;

    [Header("UI da Fome")]
    public Slider hungerSlider;
    public TextMeshProUGUI hungerText;

    // --- NOVAS VARIÁVEIS PARA DANO POR FOME ---
    [Header("Dano por Fome")]
    public PlayerHealth playerHealth; // Referência ao script de saúde do jogador
    [SerializeField] private float hungerDamageAmount = 5f; // Quanto de vida perde a cada vez
    [SerializeField] private float hungerDamageInterval = 2f; // Intervalo de tempo entre as perdas de vida
    private float lastHungerDamageTime; // Tempo do último dano por fome

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destrói se já existir outra instância
        }
        else
        {
            Instance = this; // Define esta instância como a única
            // Opcional: DontDestroyOnLoad(gameObject); se o Player não for destruído entre cenas
        }
        currentHunger = maxHunger;
        lastHungerDamageTime = Time.time; // Inicializa o tempo do último dano

        // Tenta pegar o PlayerHealth no mesmo GameObject, se não for atribuído no Inspector
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHunger: PlayerHealth não encontrado no GameObject ou não atribuído!");
        }
    }

    void Start()
    {
        UpdateHungerUI();
    }

    void Update()
    {
        currentHunger -= hungerIncreaseRate * Time.deltaTime;
        currentHunger = Mathf.Max(0f, currentHunger);
        UpdateHungerUI();

        // --- NOVA LÓGICA DE DANO POR FOME ---
        if (currentHunger <= 0f)
        {
            if (playerHealth != null)
            {
                // Verifica se já passou o tempo suficiente para causar dano novamente
                if (Time.time >= lastHungerDamageTime + hungerDamageInterval)
                {
                    playerHealth.TakeDamage(hungerDamageAmount);
                    lastHungerDamageTime = Time.time; // Reseta o timer
                }
            }
            else
            {
                Debug.LogWarning("PlayerHealth é nulo no PlayerHunger. Não é possível causar dano por fome!");
            }
        }
        // --- FIM DA NOVA LÓGICA ---
    }

    void UpdateHungerUI()
    {
        if (hungerSlider != null)
        {
            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = currentHunger;
        }

        if (hungerText != null)
        {
            hungerText.text = "Fome: " + Mathf.RoundToInt(currentHunger).ToString();
        }
    }

    public void AddHunger(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Min(maxHunger, currentHunger);
        UpdateHungerUI();
        Debug.Log($"Fome aumentada em {amount}. Fome atual: {currentHunger}");
    }

    public void RemoveHunger(float amount)
    {
        currentHunger -= amount;
        currentHunger = Mathf.Max(0f, currentHunger);
        UpdateHungerUI();
        Debug.Log($"Fome diminuída em {amount}. Fome atual: {currentHunger}");
    }

    public float GetCurrentHunger()
    {
        return currentHunger;
    }

    public float GetMaxHunger()
    {
        return maxHunger;
    }
}