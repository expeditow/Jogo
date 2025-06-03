using UnityEngine;
using System;
using System.Collections.Generic;

public class CicloDiaNoite : MonoBehaviour
{
    public Light directionalLight;

    public float diaDuracaoSegundos = 24.0f;
    public int atualHora; // Hora atual como inteiro (0-23)
    public float atualHoraDoDia = 8f / 24f; // Inicia �s 8h da manh� (0.0 a 1.0)

    public List<SkyBoxTimeMapping> timeMappings; // Mapeamento de horas para skyboxes

    private float blendedValue = 0.0f; // Para shaders de transi��o de skybox

    private int numeroDoDia = 1;
    private float ultimaHoraDoDia = -1f; // Para detectar a passagem do dia

    public static event Action<int> OnNovoDia;

    private ClimaSystem climaSystem; // Refer�ncia cacheada para o ClimaSystem
    private bool isReady = false; // Para RainManager saber se pode pegar o dia

    void Start()
    {
        if (directionalLight == null)
        {
            Light foundLight = UnityEngine.Object.FindFirstObjectByType<Light>();
            if (foundLight != null && foundLight.type == LightType.Directional)
            {
                directionalLight = foundLight;
            }
            else
            {
                Debug.LogError("[CicloDiaNoite] Luz direcional n�o encontrada ou n�o configurada!");
            }
        }

        climaSystem = UnityEngine.Object.FindFirstObjectByType<ClimaSystem>();
        if (climaSystem == null)
        {
            Debug.LogError("[CicloDiaNoite] ClimaSystem n�o encontrado! A l�gica de skybox de chuva pode n�o funcionar.");
        }

        // Calcula a hora inicial corretamente
        atualHora = Mathf.FloorToInt(atualHoraDoDia * 24);
        ultimaHoraDoDia = atualHoraDoDia; // Inicializa para evitar disparo de novo dia no primeiro frame

        Debug.Log($"[CicloDiaNoite] Iniciando no Dia {numeroDoDia} �s {atualHora:00}h ({atualHoraDoDia * 24:0.0}h)");
        OnNovoDia?.Invoke(numeroDoDia); // Notifica sobre o novo dia (ou dia inicial)
        isReady = true; // Sinaliza que o ciclo est� pronto

        AtualizarSkybox(); // Define o skybox inicial corretamente
    }

    void Update()
    {
        float horaAnteriorFloat = atualHoraDoDia;
        atualHoraDoDia += Time.deltaTime / diaDuracaoSegundos;

        // L�gica de passagem do dia
        if (atualHoraDoDia >= 1.0f) // Se passou de 24h
        {
            atualHoraDoDia %= 1.0f; // Volta para 0 e fra��o
            ultimaHoraDoDia = atualHoraDoDia - (1.0f / diaDuracaoSegundos); // Simula que estava um pouco antes da meia-noite
                                                                            // para garantir que a condi��o de novo dia seja verdadeira
            numeroDoDia++;
            Debug.Log($"[CicloDiaNoite] Novo dia: {numeroDoDia}");
            OnNovoDia?.Invoke(numeroDoDia);
        }
        else if (horaAnteriorFloat > atualHoraDoDia && Mathf.Approximately(horaAnteriorFloat, 1.0f))
        {
            // Caso raro de deltaTime grande que pulou exatamente para 0.0
            // Isso � coberto pelo atualHoraDoDia %= 1.0f, mas a detec��o de novo dia precisa ser robusta.
            // A l�gica de `ultimaHoraDoDia > atualHoraDoDia` original era boa, mas pode falhar
            // se atualHoraDoDia for exatamente 0.0 no frame da virada.

            // Vamos usar a mudan�a de hora inteira para detectar a passagem da meia-noite de forma mais simples
            // dentro da atualiza��o da hora.
        }


        int horaCalculadaAnterior = atualHora;
        atualHora = Mathf.FloorToInt(atualHoraDoDia * 24);

        // Atualiza rota��o da luz direcional
        if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(new Vector3((atualHoraDoDia * 360f) - 90f, 170f, 0f));
        }

        AtualizarSkybox();
    }

    private void AtualizarSkybox()
    {
        if (climaSystem == null) { return; } // N�o pode prosseguir sem ClimaSystem

        Material skyboxFinalParaAplicar = RenderSettings.skybox; // Come�a com o atual, pode ser alterado
        bool skyboxDeveMudar = false;

        if (climaSystem.IsRaining())
        {
            // `atualHora` � a hora inteira do dia (0-23)
            bool ehChuvaNoturna = (atualHora >= 20 || atualHora < 5);

            if (ehChuvaNoturna)
            {
                // CHUVA NOTURNA: Mant�m o skybox normal da hora (definido pelos timeMappings).
                // A l�gica para selecionar o skybox da hora (abaixo) ser� executada.
            }
            else // CHUVA DIURNA (entre 5h e 19h59)
            {
                if (climaSystem.rainSkyBox != null)
                {
                    if (RenderSettings.skybox != climaSystem.rainSkyBox)
                    {
                        skyboxFinalParaAplicar = climaSystem.rainSkyBox;
                        skyboxDeveMudar = true;
                    }
                    // Se o skybox de chuva diurna foi definido (ou j� era o correto),
                    // n�o h� mais nada a fazer nesta fun��o para este frame.
                    if (skyboxDeveMudar)
                    {
                        RenderSettings.skybox = skyboxFinalParaAplicar;
                        blendedValue = 0f; // Reseta transi��o ao mudar para o skybox de chuva
                    }
                    return; // Importante: Retorna para n�o aplicar a l�gica de skybox por hora.
                }
            }
        }

        // Se N�O ESTIVER CHOVENDO DE DIA (pode estar ensolarado, ou pode ser chuva noturna):
        // Procurar o skybox correspondente � hora atual nos mapeamentos.
        Material skyboxMapeadoParaHora = null;
        if (timeMappings != null)
        {
            foreach (SkyBoxTimeMapping mapping in timeMappings)
            {
                if (atualHora == mapping.hora)
                {
                    skyboxMapeadoParaHora = mapping.skyboxMaterial;
                    break;
                }
            }
        }

        if (skyboxMapeadoParaHora != null)
        {
            if (RenderSettings.skybox != skyboxMapeadoParaHora)
            {
                skyboxFinalParaAplicar = skyboxMapeadoParaHora;
                skyboxDeveMudar = true;
            }
        }
        else if (!climaSystem.IsRaining()) // Se NENHUM skybox mapeado para a hora E N�O est� chovendo
        {
            // Fallback para o sunnySkyBox (c�u limpo padr�o)
            if (climaSystem.sunnySkyBox != null && RenderSettings.skybox != climaSystem.sunnySkyBox)
            {
                skyboxFinalParaAplicar = climaSystem.sunnySkyBox;
                skyboxDeveMudar = true;
            }
        }
        // Se for chuva noturna e n�o houver mapeamento para a hora atual, o skybox n�o ser� alterado
        // pela l�gica de mapeamento, mantendo o �ltimo skybox noturno v�lido ou o que j� estava.

        // Aplicar a mudan�a de skybox se necess�rio
        if (skyboxDeveMudar && skyboxFinalParaAplicar != null)
        {
            RenderSettings.skybox = skyboxFinalParaAplicar;
            blendedValue = 0f; // Reseta o fator de transi��o quando o material base do skybox muda.
        }

        // Lidar com a transi��o do shader "Custom/SkyboxTransition"
        // Esta l�gica se aplica ao skybox que est� ATUALMENTE em RenderSettings.skybox.
        if (RenderSettings.skybox != null && RenderSettings.skybox.shader != null &&
            RenderSettings.skybox.shader.name == "Custom/SkyboxTransition")
        {
            // Se blendedValue foi resetado para 0 neste frame (porque o skybox mudou),
            // a transi��o come�ar� do in�cio.
            // Se o skybox n�o mudou e j� era este de transi��o, blendedValue continua de onde parou.
            blendedValue += Time.deltaTime; // Para uma transi��o mais controlada: Time.deltaTime / duracaoDaTransicaoEmSegundos
            blendedValue = Mathf.Clamp01(blendedValue);
            RenderSettings.skybox.SetFloat("_TransitionFactor", blendedValue);
        }
        // Se o skybox atual n�o usa o shader de transi��o, blendedValue (se resetado para 0) n�o ter� efeito
        // ou n�o ser� usado pelo shader.
    }

    // M�todo para RainManager checar se pode pegar o dia (opcional)
    public bool IsReady()
    {
        return isReady;
    }

    public int NumeroDoDia // Propriedade para RainManager acessar o dia atual
    {
        get { return numeroDoDia; }
    }
}

 //A classe SkyBoxTimeMapping permanece a mesma:
[System.Serializable]
public class SkyBoxTimeMapping
{
public string faseDoDia;
public float hora; // Deve ser inteiro (0-23) para corresponder a `atualHora`
public Material skyboxMaterial;
}