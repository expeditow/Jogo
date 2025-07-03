// JatoDeSangue.cs (Com Quantidade Aleat�ria)
using UnityEngine;

public class JatoDeSangue : MonoBehaviour
{
    [Header("Configura��o do Efeito")]
    [Tooltip("O Prefab da gota de sangue individual.")]
    public GameObject prefabGota;

    [Header("Quantidade Aleat�ria de Gotas")]
    [Tooltip("A quantidade M�NIMA de gotas que ser�o criadas na explos�o.")]
    public int quantidadeMinima = 25;
    [Tooltip("A quantidade M�XIMA de gotas que ser�o criadas na explos�o.")]
    public int quantidadeMaxima = 60;


    // A fun��o Start() � chamada automaticamente apenas uma vez quando o objeto � criado.
    void Start()
    {
        // 1. Sorteia um n�mero inteiro entre o m�nimo e o m�ximo definidos.
        //    O +1 � importante porque para n�meros inteiros, o 'max' do Random.Range � exclusivo.
        int quantidadeSorteada = Random.Range(quantidadeMinima, quantidadeMaxima + 1);

        // 2. Chama o m�todo para criar a explos�o, passando a quantidade que foi sorteada.
        ExplodirSangue(quantidadeSorteada);

        // 3. Destr�i este objeto gerenciador ap�s um tempo, para n�o poluir a cena.
        //    Usamos o tempo de 2 segundos que funcionou bem no nosso teste anterior.
        Destroy(gameObject, 2.0f);
    }

    /// <summary>
    /// Cria uma explos�o de sangue com uma quantidade espec�fica de gotas.
    /// </summary>
    /// <param name="quantidade">O n�mero de gotas a serem criadas.</param>
    void ExplodirSangue(int quantidade)
    {
        if (prefabGota == null)
        {
            Debug.LogError("O prefab da gota de sangue n�o foi configurado no JatoDeSangue!", this);
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