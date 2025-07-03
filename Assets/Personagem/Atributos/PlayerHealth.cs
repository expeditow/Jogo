using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ESSENCIAL: Adicione esta linha para poder usar componentes de UI como o Slider
// using TMPro; // Adicione se voc� for usar TextMeshPro para a UI da vida

public class PlayerHealth : MonoBehaviour
{
    [Header("Atributos de Vida")]
    public int maxHealth = 100;
    public float currentHealth; // MUDAN�A: currentHealth agora � float

    [Header("Refer�ncias da UI")]
    public Slider healthBar; // Crie uma refer�ncia p�blica para o Slider
    // public TextMeshProUGUI healthText; // Opcional: Para texto de vida
    public static PlayerHealth Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Garante que s� h� uma inst�ncia
        }
        else
        {
            Instance = this;
        }
        currentHealth = maxHealth;
        // Configure o valor m�ximo da barra de vida no in�cio
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    // MUDAN�A: damageAmount agora � float
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
            currentHealth = 0; // Garante que n�o vai para valores negativos na UI
            Die();
        }
    }

    // MUDAN�A: healAmount agora � float
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

    // --- Voc� pode remover ou manter a fun��o de teste ---
    void Update()
    {
        // Exemplo de teste (remover ou modificar para seu uso)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(10); // Passando um float, mesmo que seja um n�mero inteiro
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(10); // Passando um float
        }
    }
}