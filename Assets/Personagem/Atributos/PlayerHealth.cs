using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // ESSENCIAL: Adicione esta linha para poder usar componentes de UI como o Slider

public class PlayerHealth : MonoBehaviour
{
    [Header("Atributos de Vida")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Refer�ncias da UI")]
    public Slider healthBar; // Crie uma refer�ncia p�blica para o Slider

    void Awake()
    {
        currentHealth = maxHealth;
        // Configure o valor m�ximo da barra de vida no in�cio
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(int damageAmount)
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
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(int healAmount)
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
    }

    // --- Voc� pode remover ou manter a fun��o de teste ---
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(10);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(10);
        }
    }
}