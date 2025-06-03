using UnityEngine;

public class RainManager : MonoBehaviour
{
    public ParticleSystem rainParticleSystem;

    public int intervaloDias = 3; // A cada 3 dias
    public float minRainDuration = 10f;
    public float maxRainDuration = 25f;

    private bool isRaining = false; // Estado interno do RainManager
    private int ultimoDiaChuva = -999;
    private float horaChuvaAgendada = -1;

    private CicloDiaNoite ciclo;
    private ClimaSystem climaSystem;

    void Start()
    {
        ciclo = Object.FindFirstObjectByType<CicloDiaNoite>();
        climaSystem = Object.FindFirstObjectByType<ClimaSystem>();

        CicloDiaNoite.OnNovoDia += VerificarChuva;

        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop(); // Garante que não comece chovendo por partículas ativas
        }
        // Se houver uma chuva agendada para o dia 1 na hora inicial,
        // é preciso garantir que VerificarChuva seja chamado após CicloDiaNoite.Start() ter invocado OnNovoDia.
        // Alternativamente, RainManager pode pedir o dia atual a CicloDiaNoite aqui.
        if (ciclo != null && ciclo.IsReady()) // Adicionar um método IsReady() em CicloDiaNoite se necessário
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
        // A lógica de mudar RenderSettings.skybox foi removida daqui.

        if (!isRaining && horaChuvaAgendada >= 0 && ciclo != null)
        {
            int horaAtual = Mathf.FloorToInt(ciclo.atualHoraDoDia * 24);
            if (horaAtual == Mathf.FloorToInt(horaChuvaAgendada))
            {
                // Checa se o ClimaSystem já não reporta chuva (para evitar duplicação se outra fonte iniciou)
                if (climaSystem != null && !climaSystem.IsRaining())
                {
                    StartRain();
                    horaChuvaAgendada = -1; // Reseta para não disparar continuamente na mesma hora
                }
                else if (climaSystem == null) // Se não houver ClimaSystem, apenas inicia a chuva
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
            bool vaiChoverHoje = Random.value < 0.8f; // 80% de chance de chover se o intervalo passou
            if (vaiChoverHoje)
            {
                horaChuvaAgendada = Random.Range(0f, 24f); // Agenda para uma hora aleatória (float)
                ultimoDiaChuva = diaAtual;
                Debug.Log($"[RainManager] Chuva agendada para o dia {diaAtual} às {horaChuvaAgendada:0.0}h");
            }
        }
    }

    void StartRain()
    {
        isRaining = true; // Atualiza estado interno
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Play();
        }

        if (climaSystem != null)
        {
            climaSystem.UpdateWeatherState(ClimaSystem.WeatherCondition.Rainy);
        }

        float duracao = Random.Range(minRainDuration, maxRainDuration);
        Invoke("StopRain", duracao);
        Debug.Log($"[RainManager] Chuva começou! Duração: {duracao:0.0} segundos");
    }

    void StopRain()
    {
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop();
        }
        isRaining = false; // Atualiza estado interno

        if (climaSystem != null)
        {
            climaSystem.UpdateWeatherState(ClimaSystem.WeatherCondition.Sunny);
        }
        Debug.Log("[RainManager] Chuva parou.");
        // CicloDiaNoite.AtualizarSkybox() irá detectar a mudança no próximo Update
        // e restaurar o skybox apropriado para a hora.
    }
}