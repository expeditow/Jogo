using UnityEngine;
using System; // N�o necess�rio System aqui, mas ok

public class RainManager : MonoBehaviour
{
    [Header("Rain Effects")]
    public ParticleSystem rainParticleSystem;
    public GameObject rainEffectParent; // Novo: O GameObject pai que cont�m o sistema de part�culas da chuva
                                        // Este ser� o objeto que se mover� com o player.
                                        // Se rainParticleSystem j� � o pai, ent�o basta atribuir ele mesmo.

    [Header("Player Tracking")]
    public Transform playerTransform; // Refer�ncia ao transform do player

    [Header("Rain Schedule")]
    public int intervaloDias = 3; // A cada 3 dias
    public float minRainDuration = 10f;
    public float maxRainDuration = 25f;

    private bool isRaining = false; // Estado interno do RainManager
    private int ultimoDiaChuva = -999;
    private float horaChuvaAgendada = -1;

    private CicloDiaNoite ciclo;
    private ClimaSystem climaSystem;

    // Dist�ncia vertical da chuva em rela��o ao player
    public float rainVerticalOffset = 20f; // Ex: A chuva fica 20 unidades acima do player

    void Start()
    {
        // CORRE��O AQUI: Usando FindObjectOfType
        ciclo = FindObjectOfType<CicloDiaNoite>();
        climaSystem = FindObjectOfType<ClimaSystem>();

        // Tenta encontrar o player automaticamente se n�o for atribu�do
        if (playerTransform == null)
        {
            // Assumindo que seu player tem a tag "Player"
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                playerTransform = playerGO.transform;
            }
            else
            {
                Debug.LogWarning("[RainManager] Player n�o encontrado na cena. Certifique-se de que o player tem a tag 'Player' ou atribua-o manualmente no Inspector.");
            }
        }

        // Tenta encontrar o rainEffectParent automaticamente se n�o for atribu�do
        if (rainEffectParent == null)
        {
            if (rainParticleSystem != null)
            {
                rainEffectParent = rainParticleSystem.gameObject; // Assume que o pr�prio sistema de part�culas � o que deve ser movido
            }
            else
            {
                Debug.LogError("[RainManager] 'rainEffectParent' ou 'rainParticleSystem' n�o est�o atribu�dos no Inspector. A chuva n�o poder� seguir o player.");
                enabled = false; // Desativa o script se n�o houver refer�ncia essencial
                return;
            }
        }


        CicloDiaNoite.OnNovoDia += VerificarChuva;

        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop(); // Garante que n�o comece chovendo por part�culas ativas
            // Garante que o objeto da chuva n�o esteja ativo visualmente se n�o estiver chovendo
            if (rainEffectParent != null)
            {
                rainEffectParent.SetActive(false);
            }
        }

        if (ciclo != null && ciclo.IsReady())
        {
            VerificarChuva(ciclo.NumeroDoDia); // Para checar no in�cio do jogo
        }
    }

    void OnDestroy()
    {
        CicloDiaNoite.OnNovoDia -= VerificarChuva;
    }

    void Update()
    {
        // Se estiver chovendo e tivermos a refer�ncia do player e do objeto da chuva
        if (isRaining && playerTransform != null && rainEffectParent != null)
        {
            // Move o objeto pai da chuva para a posi��o do player, mantendo um offset vertical
            rainEffectParent.transform.position = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y + rainVerticalOffset, // Adiciona o offset vertical
                playerTransform.position.z
            );
        }

        // L�gica de agendamento de chuva (mantida do seu c�digo)
        if (!isRaining && horaChuvaAgendada >= 0 && ciclo != null)
        {
            int horaAtual = Mathf.FloorToInt(ciclo.atualHoraDoDia * 24);
            if (horaAtual == Mathf.FloorToInt(horaChuvaAgendada))
            {
                if (climaSystem != null && !climaSystem.IsRaining())
                {
                    StartRain();
                    horaChuvaAgendada = -1;
                }
                else if (climaSystem == null)
                {
                    StartRain();
                    horaChuvaAgendada = -1;
                }
            }
        }
    }

    void VerificarChuva(int diaAtual)
    {
        if ((diaAtual - ultimoDiaChuva) >= intervaloDias)
        {
            bool vaiChoverHoje = UnityEngine.Random.value < 0.8f; // Usar UnityEngine.Random para evitar ambiguidade
            if (vaiChoverHoje)
            {
                horaChuvaAgendada = UnityEngine.Random.Range(0f, 24f); // Usar UnityEngine.Random
                ultimoDiaChuva = diaAtual;
                Debug.Log($"[RainManager] Chuva agendada para o dia {diaAtual} �s {horaChuvaAgendada:0.0}h");
            }
        }
    }

    void StartRain()
    {
        isRaining = true; // Atualiza estado interno
        if (rainEffectParent != null) // Ativa o objeto pai da chuva
        {
            rainEffectParent.SetActive(true);
        }
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Play();
        }

        if (climaSystem != null)
        {
            climaSystem.UpdateWeatherState(ClimaSystem.WeatherCondition.Rainy);
        }

        float duracao = UnityEngine.Random.Range(minRainDuration, maxRainDuration); // Usar UnityEngine.Random
        Invoke("StopRain", duracao);
        Debug.Log($"[RainManager] Chuva come�ou! Dura��o: {duracao:0.0} segundos");
    }

    void StopRain()
    {
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop();
        }
        if (rainEffectParent != null) // Desativa o objeto pai da chuva
        {
            rainEffectParent.SetActive(false);
        }
        isRaining = false; // Atualiza estado interno

        if (climaSystem != null)
        {
            climaSystem.UpdateWeatherState(ClimaSystem.WeatherCondition.Sunny);
        }
        Debug.Log("[RainManager] Chuva parou.");
    }
}