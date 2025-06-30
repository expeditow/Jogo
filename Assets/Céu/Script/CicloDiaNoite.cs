using UnityEngine;
using System;
using System.Collections.Generic;

public class CicloDiaNoite : MonoBehaviour
{
    public Light directionalLight;
    public float diaDuracaoSegundos = 24.0f;
    public int atualHora;
    public float atualHoraDoDia = 8f / 24f; // Inicia �s 8h da manh�

� � public List<SkyBoxTimeMapping> timeMappings; // Mapeamento de horas para skyboxes

    private float blendedValue = 0.0f;
    private int numeroDoDia = 1;
    private float ultimaHoraDoDia = -1f;

    public static event Action<int> OnNovoDia;
    private ClimaSystem climaSystem;
    private bool isReady = false;

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

        atualHora = Mathf.FloorToInt(atualHoraDoDia * 24);
        ultimaHoraDoDia = atualHoraDoDia;

        Debug.Log($"[CicloDiaNoite] Iniciando no Dia {numeroDoDia} �s {atualHora:00}h ({atualHoraDoDia * 24:0.0}h)");
        OnNovoDia?.Invoke(numeroDoDia);
        isReady = true;

        AtualizarSkybox(); // Define o skybox inicial
� � }

    void Update()
    {
        float horaAnteriorFloat = atualHoraDoDia;
        atualHoraDoDia += Time.deltaTime / diaDuracaoSegundos;

        if (atualHoraDoDia >= 1.0f)
        {
            atualHoraDoDia %= 1.0f;
            ultimaHoraDoDia = atualHoraDoDia - (1.0f / diaDuracaoSegundos);
            numeroDoDia++;
            Debug.Log($"[CicloDiaNoite] Novo dia: {numeroDoDia}");
            OnNovoDia?.Invoke(numeroDoDia);
        }

        int horaCalculadaAnterior = atualHora;
        atualHora = Mathf.FloorToInt(atualHoraDoDia * 24);

� � � � // Atualiza rota��o da luz direcional
� � � � if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(new Vector3((atualHoraDoDia * 360f) - 90f, 170f, 0f));
        }
        AtualizarSkybox();
    }

    private void AtualizarSkybox()
    {
        if (climaSystem == null) { return; } 
� � � � Material skyboxFinalParaAplicar = RenderSettings.skybox; 
� � � � bool skyboxDeveMudar = false;

        if (climaSystem.IsRaining())
        {
� � � � � � bool ehChuvaNoturna = (atualHora >= 20 || atualHora < 5);
            if (ehChuvaNoturna)
            {
� � � � � � }

            else 
� � � � � � {
                if (climaSystem.rainSkyBox != null)
                {
                    if (RenderSettings.skybox != climaSystem.rainSkyBox)
                    {
                        skyboxFinalParaAplicar = climaSystem.rainSkyBox;
                        skyboxDeveMudar = true;
                    }

� � � � � � � � � � if (skyboxDeveMudar)
                    {
                        RenderSettings.skybox = skyboxFinalParaAplicar;
                        blendedValue = 0f; // Reseta transi��o ao mudar para o skybox de chuva
� � � � � � � � � � }
                    return; 
� � � � � � � � }
            }
        }
� � � � // Se n�o estiver chovendo de dia (pode estar ensolarado, ou pode ser chuva noturna):
� � � � Material skyboxMapeadoParaHora = null;

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

        else if (!climaSystem.IsRaining()) 
� � � � {
� � � � � � // Fallback para o sunnySkyBox (c�u limpo padr�o)
� � � � � � if (climaSystem.sunnySkyBox != null && RenderSettings.skybox != climaSystem.sunnySkyBox)
            {
                skyboxFinalParaAplicar = climaSystem.sunnySkyBox;
                skyboxDeveMudar = true;
            }
        }
� � � � // Se for chuva noturna e n�o houver mapeamento para a hora atual, o skybox n�o ser� alterado
� � � � if (skyboxDeveMudar && skyboxFinalParaAplicar != null)
        {
            RenderSettings.skybox = skyboxFinalParaAplicar;
            blendedValue = 0f; // Reseta o fator de transi��o quando o material base do skybox muda.
� � � � }

� � � � if (RenderSettings.skybox != null && RenderSettings.skybox.shader != null && RenderSettings.skybox.shader.name == "Custom/SkyboxTransition")
        {
� � � � � � blendedValue += Time.deltaTime;
� � � � � � blendedValue = Mathf.Clamp01(blendedValue);
            RenderSettings.skybox.SetFloat("_TransitionFactor", blendedValue);
        }
� � }
� � // M�todo para RainManager checar se pode pegar o dia (opcional)

� � public bool IsReady()
    {
        return isReady;
    }

    public int NumeroDoDia // Propriedade para RainManager acessar o dia atual
� � {
        get { return numeroDoDia; }
    }
}
[System.Serializable]

public class SkyBoxTimeMapping
{
    public string faseDoDia;
    public float hora; 
    public Material skyboxMaterial;
}