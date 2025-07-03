using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class CenaInfo
{
    public Sprite imagemDeCena;
    [TextArea(3, 5)]
    public string[] dialogos;
}

public class CutsceneMultiCenasFinal : MonoBehaviour
{
    [Header("Lista de Cenas")]
    public CenaInfo[] cenas;

    [Header("Componentes da UI")]
    public GameObject containerImagem; // NOVO: O objeto que segura a ImagemDaCena
    public GameObject containerDialogo; // NOVO: O objeto CaixaDialogo
    public Image imagemUI;
    public TextMeshProUGUI textoUI;
    public Button botaoAvancar;
    public Image telaFade;
    public GameObject painelGameOver;

    [Header("Configurações")]
    public float tempoDeFade = 3f;

    private int indiceCenaAtual = 0;
    private int indiceDialogoAtual = 0;

    // Start agora apenas inicia o processo de fade de entrada
    void Start()
    {
        StartCoroutine(FadeDeEntrada());
    }

    // --- NOVO: COROUTINE PARA O FADE DE ENTRADA ---
    IEnumerator FadeDeEntrada()
    {
        // 1. Prepara a cena para o fade de entrada
        telaFade.gameObject.SetActive(true);
        telaFade.color = new Color(0, 0, 0, 1); // Começa totalmente preto

        // Esconde os outros elementos para não aparecerem de repente
        containerImagem.SetActive(true);
        containerDialogo.SetActive(true);
        painelGameOver.SetActive(false);
        MostrarConteudoAtual();
        

        // 2. Executa o fade (clareando a tela)
        float timer = 0f;
        while (timer < tempoDeFade)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / tempoDeFade);
            telaFade.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        // 3. Garante que a tela fique totalmente visível e desativa a tela de fade
        telaFade.gameObject.SetActive(false);

        // 4. AGORA SIM, inicia a lógica da cutscene
    
        botaoAvancar.onClick.AddListener(Avancar);
        indiceCenaAtual = 0;
        indiceDialogoAtual = 0;
        
    }

    public void Avancar()
    {
        if (indiceCenaAtual >= cenas.Length) return; // Segurança para não avançar depois do fim

        CenaInfo cenaAtual = cenas[indiceCenaAtual];
        indiceDialogoAtual++;
        if (indiceDialogoAtual >= cenaAtual.dialogos.Length)
        {
            indiceCenaAtual++;
            indiceDialogoAtual = 0;
        }
        MostrarConteudoAtual();
    }

    void MostrarConteudoAtual()
    {
        if (indiceCenaAtual >= cenas.Length)
        {
            StartCoroutine(FadeParaGameOver());
            return;
        }
        CenaInfo cenaAtual = cenas[indiceCenaAtual];
        string dialogoAtual = cenaAtual.dialogos[indiceDialogoAtual];
        imagemUI.sprite = cenaAtual.imagemDeCena;
        textoUI.text = dialogoAtual;
    }

    IEnumerator FadeParaGameOver()
    {
        containerDialogo.SetActive(false);
        
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
        
        if (painelGameOver != null)
        {
            painelGameOver.SetActive(true);
        }
    }

    public void EncerrarJogo()
    {
        Debug.Log("O botão de sair foi clicado!");
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}