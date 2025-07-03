// JatoDeSangue.cs (Com Quantidade Aleatória)
using UnityEngine;

public class JatoDeSangue : MonoBehaviour
{
    [Header("Configuração do Efeito")]
    [Tooltip("O Prefab da gota de sangue individual.")]
    public GameObject prefabGota;

    [Header("Quantidade Aleatória de Gotas")]
    [Tooltip("A quantidade MÍNIMA de gotas que serão criadas na explosão.")]
    public int quantidadeMinima = 25;
    [Tooltip("A quantidade MÁXIMA de gotas que serão criadas na explosão.")]
    public int quantidadeMaxima = 60;


    // A função Start() é chamada automaticamente apenas uma vez quando o objeto é criado.
    void Start()
    {
        // 1. Sorteia um número inteiro entre o mínimo e o máximo definidos.
        //    O +1 é importante porque para números inteiros, o 'max' do Random.Range é exclusivo.
        int quantidadeSorteada = Random.Range(quantidadeMinima, quantidadeMaxima + 1);

        // 2. Chama o método para criar a explosão, passando a quantidade que foi sorteada.
        ExplodirSangue(quantidadeSorteada);

        // 3. Destrói este objeto gerenciador após um tempo, para não poluir a cena.
        //    Usamos o tempo de 2 segundos que funcionou bem no nosso teste anterior.
        Destroy(gameObject, 2.0f);
    }

    /// <summary>
    /// Cria uma explosão de sangue com uma quantidade específica de gotas.
    /// </summary>
    /// <param name="quantidade">O número de gotas a serem criadas.</param>
    void ExplodirSangue(int quantidade)
    {
        if (prefabGota == null)
        {
            Debug.LogError("O prefab da gota de sangue não foi configurado no JatoDeSangue!", this);
            return;
        }

        // Cria a quantidade de gotas que foi sorteada.
        for (int i = 0; i < quantidade; i++)
        {
            GameObject gota = Instantiate(prefabGota, transform.position, Quaternion.identity);

            if (gota.GetComponent<GotaSangue>() == null)
            {
                gota.AddComponent<GotaSangue>();
            }
        }

        Debug.Log($"Jato de Sangue: Criou {quantidade} gotas.");
    }
}