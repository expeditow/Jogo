using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement; // --- NOVO: Namespace necessário para carregar cenas

public class DialogoCutsceneInicio : MonoBehaviour
{
    [Header("Componentes da UI")]
    public TextMeshProUGUI textoUI;
    public Button botaoAvancar;
    public Image telaFade;

    [Header("Configurações")]
    public float tempoDeFade = 2.5f;
    // --- NOVO: Campo para definir o nome da cena a ser carregada ---
    [Tooltip("O nome exato da cena a ser carregada após o diálogo.")]
    public string nomeDaCenaParaCarregar;

    [Header("Frases do Diálogo")]
    [TextArea(3, 10)]
    public string[] frases;

    private int indiceAtual = 0;

    void Start()
    {
        StartCoroutine(FadeDeEntrada());
    }

    IEnumerator FadeDeEntrada()
    {
        telaFade.gameObject.SetActive(true);
        telaFade.color = new Color(0, 0, 0, 1);

        indiceAtual = 0;
        MostrarFraseAtual();

        float timer = 0f;
        while (timer < tempoDeFade)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / tempoDeFade);
            telaFade.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        telaFade.gameObject.SetActive(false);
        botaoAvancar.onClick.AddListener(AvancarDialogo);
    }

    void MostrarFraseAtual()
    {
        if (indiceAtual < frases.Length)
        {
            textoUI.text = frases[indiceAtual];
        }
        else
        {
            // --- MODIFICADO: Agora vamos definitivamente iniciar o fade de saída ---
            Debug.Log("Fim do diálogo. Iniciando fade para a próxima cena.");
            botaoAvancar.interactable = false;
            StartCoroutine(FadeParaPreto());
        }
    }

    public void AvancarDialogo()
    {
        indiceAtual++;
        MostrarFraseAtual();
    }

    IEnumerator FadeParaPreto()
    {
        // --- MODIFICADO: Verificação para garantir que um nome de cena foi fornecido ---
        if (string.IsNullOrEmpty(nomeDaCenaParaCarregar))
        {
            Debug.LogError("O nome da cena para carregar não foi definido no Inspector!");
            yield break; // Interrompe a rotina se o nome da cena estiver vazio
        }

        telaFade.gameObject.SetActive(true);
        float timer = 0f;
        while (timer < tempoDeFade)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / tempoDeFade);
            telaFade.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        telaFade.color = new Color(0, 0, 0, 1);

        // --- NOVO: A linha que carrega a sua cena principal! ---
        SceneManager.LoadScene(nomeDaCenaParaCarregar);
    }
}