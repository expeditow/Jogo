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
            rainParticleSystem.Stop(); // Garante que n�o comece chovendo por part�culas ativas
        }
        // Se houver uma chuva agendada para o dia 1 na hora inicial,
        // � preciso garantir que VerificarChuva seja chamado ap�s CicloDiaNoite.Start() ter invocado OnNovoDia.
        // Alternativamente, RainManager pode pedir o dia atual a CicloDiaNoite aqui.
        if (ciclo != null && ciclo.IsReady()) // Adicionar um m�todo IsReady() em CicloDiaNoite se necess�rio
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
        // A l�gica de mudar RenderSettings.skybox foi removida daqui.

        if (!isRaining && horaChuvaAgendada >= 0 && ciclo != null)
        {
            int horaAtual = Mathf.FloorToInt(ciclo.atualHoraDoDia * 24);
            if (horaAtual == Mathf.FloorToInt(horaChuvaAgendada))
            {
                // Checa se o ClimaSystem j� n�o reporta chuva (para evitar duplica��o se outra fonte iniciou)
                if (climaSystem != null && !climaSystem.IsRaining())
                {
                    StartRain();
                    horaChuvaAgendada = -1; // Reseta para n�o disparar continuamente na mesma hora
                }
                else if (climaSystem == null) // Se n�o houver ClimaSystem, apenas inicia a chuva
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
                horaChuvaAgendada = Random.Range(0f, 24f); // Agenda para uma hora aleat�ria (float)
                ultimoDiaChuva = diaAtual;
                Debug.Log($"[RainManager] Chuva agendada para o dia {diaAtual} �s {horaChuvaAgendada:0.0}h");
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
        Debug.Log($"[RainManager] Chuva come�ou! Dura��o: {duracao:0.0} segundos");
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
        // CicloDiaNoite.AtualizarSkybox() ir� detectar a mudan�a no pr�ximo Update
        // e restaurar o skybox apropriado para a hora.
    }
}