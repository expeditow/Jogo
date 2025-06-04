using UnityEngine;
using System; // Não necessário System aqui, mas ok

public class RainManager : MonoBehaviour
{
    [Header("Rain Effects")]
    public ParticleSystem rainParticleSystem;
    public GameObject rainEffectParent; // Novo: O GameObject pai que contém o sistema de partículas da chuva
                                        // Este será o objeto que se moverá com o player.
                                        // Se rainParticleSystem já é o pai, então basta atribuir ele mesmo.

    [Header("Player Tracking")]
    public Transform playerTransform; // Referência ao transform do player

    [Header("Rain Schedule")]
    public int intervaloDias = 3; // A cada 3 dias
    public float minRainDuration = 10f;
    public float maxRainDuration = 25f;

    private bool isRaining = false; // Estado interno do RainManager
    private int ultimoDiaChuva = -999;
    private float horaChuvaAgendada = -1;

    private CicloDiaNoite ciclo;
    private ClimaSystem climaSystem;

    // Distância vertical da chuva em relação ao player
    public float rainVerticalOffset = 20f; // Ex: A chuva fica 20 unidades acima do player

    void Start()
    {
        // CORREÇÃO AQUI: Usando FindObjectOfType
        ciclo = FindObjectOfType<CicloDiaNoite>();
        climaSystem = FindObjectOfType<ClimaSystem>();

        // Tenta encontrar o player automaticamente se não for atribuído
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
                Debug.LogWarning("[RainManager] Player não encontrado na cena. Certifique-se de que o player tem a tag 'Player' ou atribua-o manualmente no Inspector.");
            }
        }

        // Tenta encontrar o rainEffectParent automaticamente se não for atribuído
        if (rainEffectParent == null)
        {
            if (rainParticleSystem != null)
            {
                rainEffectParent = rainParticleSystem.gameObject; // Assume que o próprio sistema de partículas é o que deve ser movido
            }
            else
            {
                Debug.LogError("[RainManager] 'rainEffectParent' ou 'rainParticleSystem' não estão atribuídos no Inspector. A chuva não poderá seguir o player.");
                enabled = false; // Desativa o script se não houver referência essencial
                return;
            }
        }


        CicloDiaNoite.OnNovoDia += VerificarChuva;

        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop(); // Garante que não comece chovendo por partículas ativas
            // Garante que o objeto da chuva não esteja ativo visualmente se não estiver chovendo
            if (rainEffectParent != null)
            {
                rainEffectParent.SetActive(false);
            }
        }

        if (ciclo != null && ciclo.IsReady())
        {
            VerificarChuva(ciclo.NumeroDoDia); // Para checar no início do jogo
        }
    }

    void OnDestroy()
    {
        CicloDiaNoite.OnNovoDia -= VerificarChuva;
    }

    void Update()
    {
        // Se estiver chovendo e tivermos a referência do player e do objeto da chuva
        if (isRaining && playerTransform != null && rainEffectParent != null)
        {
            // Move o objeto pai da chuva para a posição do player, mantendo um offset vertical
            rainEffectParent.transform.position = new Vector3(
                playerTransform.position.x,
                playerTransform.position.y + rainVerticalOffset, // Adiciona o offset vertical
                playerTransform.position.z
            );
        }

        // Lógica de agendamento de chuva (mantida do seu código)
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
                Debug.Log($"[RainManager] Chuva agendada para o dia {diaAtual} às {horaChuvaAgendada:0.0}h");
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
        Debug.Log($"[RainManager] Chuva começou! Duração: {duracao:0.0} segundos");
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