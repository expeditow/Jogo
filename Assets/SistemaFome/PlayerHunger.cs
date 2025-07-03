using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHunger : MonoBehaviour
{
    public static PlayerHunger Instance { get; private set; }

    [Header("Configura��es da Fome")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float currentHunger;
    [SerializeField] private float hungerIncreaseRate = 1f;

    [Header("UI da Fome")]
    public Slider hungerSlider;
    public TextMeshProUGUI hungerText;

    // --- NOVAS VARI�VEIS PARA DANO POR FOME ---
    [Header("Dano por Fome")]
    public PlayerHealth playerHealth; // Refer�ncia ao script de sa�de do jogador
    [SerializeField] private float hungerDamageAmount = 5f; // Quanto de vida perde a cada vez
    [SerializeField] private float hungerDamageInterval = 2f; // Intervalo de tempo entre as perdas de vida
    private float lastHungerDamageTime; // Tempo do �ltimo dano por fome

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destr�i se j� existir outra inst�ncia
        }
        else
        {
            Instance = this; // Define esta inst�ncia como a �nica
            // Opcional: DontDestroyOnLoad(gameObject); se o Player n�o for destru�do entre cenas
        }
        currentHunger = maxHunger;
        lastHungerDamageTime = Time.time; // Inicializa o tempo do �ltimo dano

        // Tenta pegar o PlayerHealth no mesmo GameObject, se n�o for atribu�do no Inspector
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
        if (playerHealth == null)
        {
            Debug.LogError("PlayerHunger: PlayerHealth n�o encontrado no GameObject ou n�o atribu�do!");
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

        // --- NOVA L�GICA DE DANO POR FOME ---
        if (currentHunger <= 0f)
        {
            if (playerHealth != null)
            {
                // Verifica se j� passou o tempo suficiente para causar dano novamente
                if (Time.time >= lastHungerDamageTime + hungerDamageInterval)
                {
                    playerHealth.TakeDamage(hungerDamageAmount);
                    lastHungerDamageTime = Time.time; // Reseta o timer
                }
            }
            else
            {
                Debug.LogWarning("PlayerHealth � nulo no PlayerHunger. N�o � poss�vel causar dano por fome!");
            }
        }
        // --- FIM DA NOVA L�GICA ---
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
        Debug.Log($"Fome diminu�da em {amount}. Fome atual: {currentHunger}");
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