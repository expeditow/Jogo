using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ControladorDeCutsceneMeio : MonoBehaviour
{
    [Header("Conexões da UI")]
    public GameObject caixaDialogo;
    public TextMeshProUGUI textoDialogo;
    public Button botaoAvancar;
    public Image telaFade;

    [Header("Conteúdo da Cutscene")]
    [TextArea(3, 10)]
    public string[] dialogos;
    public float tempoDeFade = 2.0f;

    private int indiceDialogo = 0;

    // O método Start apenas chama a rotina principal
    void Start()
    {
        StartCoroutine(ExecutarCutscene());
    }

    // Rotina principal que controla todo o fluxo
    IEnumerator ExecutarCutscene()
    {
        // --- FASE 1: FADE DE ENTRADA ---
        // Prepara a tela para o fade, já deixando-a preta e opaca
        telaFade.gameObject.SetActive(true);
        telaFade.color = new Color(0, 0, 0, 1);
        caixaDialogo.SetActive(false); // Garante que o diálogo comece escondido

        // Executa o fade de entrada (clareia a tela)
        float timer = 0f;
        while (timer < tempoDeFade)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / tempoDeFade);
            telaFade.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        telaFade.gameObject.SetActive(false);


        // --- FASE 2: DIÁLOGO ---
        // Mostra a caixa de diálogo e prepara o primeiro texto
        caixaDialogo.SetActive(true);
        indiceDialogo = 0;
        textoDialogo.text = dialogos[indiceDialogo];
        botaoAvancar.onClick.AddListener(ProximoDialogo); // Ativa o botão
    }
    
    // Função chamada pelo clique do botão
    public void ProximoDialogo()
    {
        indiceDialogo++; // Avança para a próxima frase

        // Verifica se ainda há diálogos na lista
        if (indiceDialogo < dialogos.Length)
        {
            textoDialogo.text = dialogos[indiceDialogo];
        }
        else
        {
            // Se os diálogos acabaram, inicia o fade de saída
            StartCoroutine(FadeDeSaida());
        }
    }

    // Rotina para o fade de saída
    IEnumerator FadeDeSaida()
    {
        // Desativa o botão e esconde a caixa de diálogo
        botaoAvancar.interactable = false;
        caixaDialogo.SetActive(false);

        // Ativa e executa o fade para preto
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
        Debug.Log("Cutscene finalizada.");
        // Aqui você poderia adicionar uma tela de Game Over ou carregar outra cena
    }
}