using UnityEngine;
using System;
using System.Collections.Generic;

public class CicloDiaNoite : MonoBehaviour
{
    public Light directionalLight;
    public float diaDuracaoSegundos = 24.0f;
    public int atualHora;
    public float atualHoraDoDia = 8f / 24f; // Inicia às 8h da manhã 

    public List<SkyBoxTimeMapping> timeMappings; // Mapeamento de horas para skyboxes

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
                Debug.LogError("[CicloDiaNoite] Luz direcional não encontrada ou não configurada!");
            }
        }

        climaSystem = UnityEngine.Object.FindFirstObjectByType<ClimaSystem>();

        if (climaSystem == null)
        {
            Debug.LogError("[CicloDiaNoite] ClimaSystem não encontrado! A lógica de skybox de chuva pode não funcionar.");
        }

        atualHora = Mathf.FloorToInt(atualHoraDoDia * 24);
        ultimaHoraDoDia = atualHoraDoDia;

        Debug.Log($"[CicloDiaNoite] Iniciando no Dia {numeroDoDia} às {atualHora:00}h ({atualHoraDoDia * 24:0.0}h)");
        OnNovoDia?.Invoke(numeroDoDia);
        isReady = true;

        AtualizarSkybox(); // Define o skybox inicial
    }

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

        // Atualiza rotação da luz direcional
        if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(new Vector3((atualHoraDoDia * 360f) - 90f, 170f, 0f));
        }
        AtualizarSkybox();
    }

    private void AtualizarSkybox()
    {
        if (climaSystem == null) { return; } 
        Material skyboxFinalParaAplicar = RenderSettings.skybox; 
        bool skyboxDeveMudar = false;

        if (climaSystem.IsRaining())
        {
            bool ehChuvaNoturna = (atualHora >= 20 || atualHora < 5);
            if (ehChuvaNoturna)
            {
            }

            else 
            {
                if (climaSystem.rainSkyBox != null)
                {
                    if (RenderSettings.skybox != climaSystem.rainSkyBox)
                    {
                        skyboxFinalParaAplicar = climaSystem.rainSkyBox;
                        skyboxDeveMudar = true;
                    }

                    if (skyboxDeveMudar)
                    {
                        RenderSettings.skybox = skyboxFinalParaAplicar;
                        blendedValue = 0f; // Reseta transição ao mudar para o skybox de chuva
                    }
                    return; 
                }
            }
        }
        // Se não estiver chovendo de dia (pode estar ensolarado, ou pode ser chuva noturna):
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

        else if (!climaSystem.IsRaining()) 
        {
            // Fallback para o sunnySkyBox (céu limpo padrão)
            if (climaSystem.sunnySkyBox != null && RenderSettings.skybox != climaSystem.sunnySkyBox)
            {
                skyboxFinalParaAplicar = climaSystem.sunnySkyBox;
                skyboxDeveMudar = true;
            }
        }
        // Se for chuva noturna e não houver mapeamento para a hora atual, o skybox não será alterado
        if (skyboxDeveMudar && skyboxFinalParaAplicar != null)
        {
            RenderSettings.skybox = skyboxFinalParaAplicar;
            blendedValue = 0f; // Reseta o fator de transição quando o material base do skybox muda.
        }

        if (RenderSettings.skybox != null && RenderSettings.skybox.shader != null && RenderSettings.skybox.shader.name == "Custom/SkyboxTransition")
        {
            blendedValue += Time.deltaTime;
            blendedValue = Mathf.Clamp01(blendedValue);
            RenderSettings.skybox.SetFloat("_TransitionFactor", blendedValue);
        }
    }
    // Método para RainManager checar se pode pegar o dia (opcional)

    public bool IsReady()
    {
        return isReady;
    }

    public int NumeroDoDia // Propriedade para RainManager acessar o dia atual
    {
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