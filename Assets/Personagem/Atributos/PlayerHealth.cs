using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ESSENCIAL: Adicione esta linha para poder usar componentes de UI como o Slider
// using TMPro; // Adicione se você for usar TextMeshPro para a UI da vida

public class PlayerHealth : MonoBehaviour
{
    [Header("Atributos de Vida")]
    public int maxHealth = 100;
    public float currentHealth; // MUDANÇA: currentHealth agora é float

    [Header("Referências da UI")]
    public Slider healthBar; // Crie uma referência pública para o Slider
    // public TextMeshProUGUI healthText; // Opcional: Para texto de vida
    public static PlayerHealth Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Garante que só há uma instância
        }
        else
        {
            Instance = this;
        }
        currentHealth = maxHealth;
        // Configure o valor máximo da barra de vida no início
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    // MUDANÇA: damageAmount agora é float
    public void TakeDamage(float damageAmount)
    {
        if (damageAmount < 0) return;

        currentHealth -= damageAmount;
        Debug.Log("Player tomou " + damageAmount + " de dano. Vida atual: " + currentHealth);

        // Atualiza o valor visual da barra de vida
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0; // Garante que não vai para valores negativos na UI
            Die();
        }
    }

    // MUDANÇA: healAmount agora é float
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Player curou " + healAmount + ". Vida atual: " + currentHealth);

        // Atualiza o valor visual da barra de vida
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log("O Player morreu!");
        gameObject.SetActive(false);
        // Opcional: Application.Quit(); ou SceneManager.LoadScene("GameOverScene");
    }

    // --- Você pode remover ou manter a função de teste ---
    void Update()
    {
        // Exemplo de teste (remover ou modificar para seu uso)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(10); // Passando um float, mesmo que seja um número inteiro
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(10); // Passando um float
        }
    }
}