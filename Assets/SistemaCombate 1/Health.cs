// Health.cs - VERS�O FINAL E FLEX�VEL
using UnityEngine;
using UnityEngine.SceneManagement; // --- NOVO: Namespace necess�rio para carregar cenas

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Loot Settings")]
    public GameObject lootDropPrefab;

    [Header("Eventos de Morte")]
    [Tooltip("Arraste para c� um GameObject para ativar nesta cena quando o personagem morrer.")]
    public GameObject objetoParaAtivarNaMorte;

    // --- NOVO ---
    [Tooltip("Digite o nome exato da cena a ser carregada quando este personagem morrer.")]
    public string cenaParaCarregarNaMorte;
    // --- FIM DO NOVO ---

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} tomou {amount} de dano. Vida atual: {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} foi derrotado!");

        // L�gica de Loot
        if (lootDropPrefab != null)
        {
            Instantiate(lootDropPrefab, transform.position, Quaternion.identity);
        }

        // L�gica para ativar um objeto na mesma cena
        if (objetoParaAtivarNaMorte != null)
        {
            objetoParaAtivarNaMorte.SetActive(true);
        }

        // --- NOVO: L�gica para carregar uma nova cena ---
        // Verifica se um nome de cena foi fornecido
        if (!string.IsNullOrEmpty(cenaParaCarregarNaMorte))
        {
            Debug.Log($"Carregando a cena: {cenaParaCarregarNaMorte}");
            SceneManager.LoadScene(cenaParaCarregarNaMorte);
        }
        // --- FIM DO NOVO ---

        // Destr�i o objeto do personagem que morreu.
        // Se uma nova cena for carregada, a destrui��o do objeto antigo � impl�cita.
        Destroy(gameObject);
    }
}