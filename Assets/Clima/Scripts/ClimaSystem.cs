using UnityEngine;

public class ClimaSystem : MonoBehaviour
{
    [Range(0f, 1f)]
    public float chanceRain = 0.5f;

    public GameObject rainEffect;
    public Material rainSkyBox; 
    public Material sunnySkyBox; 

    public bool isSpecialWeather; // Para forçar um clima específico no início

    public enum WeatherCondition
    {
        Sunny,
        Rainy,
    }

    private WeatherCondition currentWeather = WeatherCondition.Sunny;

    private void Start()
    {
        if (isSpecialWeather)
        {
            UpdateWeatherState(WeatherCondition.Rainy); // Apenas define o estado e ativa/desativa efeito
        }
        else
        {
            UpdateWeatherState(WeatherCondition.Sunny);
        }
    }

    // Método chamado pelo RainManager para atualizar o estado do clima
    public void UpdateWeatherState(WeatherCondition newWeather)
    {
        currentWeather = newWeather;

        if (rainEffect != null)
        {
            rainEffect.SetActive(currentWeather == WeatherCondition.Rainy);
        }
    }

    // Chamado por CicloDiaNoite para saber se está chovendo
    public bool IsRaining()
    {
        return currentWeather == WeatherCondition.Rainy;
    }

    public void SetWeather(WeatherCondition weather, bool changeSkyboxImmediately)
    {
        currentWeather = weather;

        if (rainEffect != null)
        {
            rainEffect.SetActive(currentWeather == WeatherCondition.Rainy);
        }

        if (changeSkyboxImmediately)
        {
            if (currentWeather == WeatherCondition.Rainy && rainSkyBox != null)
            {
                RenderSettings.skybox = rainSkyBox;
            }
            else if (currentWeather == WeatherCondition.Sunny && sunnySkyBox != null)
            {
                RenderSettings.skybox = sunnySkyBox;
            }
        }
    }
}