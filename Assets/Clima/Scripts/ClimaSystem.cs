using UnityEngine;

public class ClimaSystem : MonoBehaviour
{
    [Range(0f, 1f)]
    public float chanceRain = 0.5f; // Usado para lógica inicial ou climas não gerenciados pelo RainManager

    public GameObject rainEffect;
    public Material rainSkyBox; // Usado por CicloDiaNoite
    public Material sunnySkyBox; // Usado por CicloDiaNoite

    public bool isSpecialWeather; // Para forçar um clima específico no início

    public enum WeatherCondition
    {
        Sunny,
        Rainy,
    }

    private WeatherCondition currentWeather = WeatherCondition.Sunny;
    // private RainManager rainManager; // Opcional, se precisar interagir de volta

    private void Start()
    {
        // rainManager = Object.FindFirstObjectByType<RainManager>(); // Opcional

        // No início, ClimaSystem define um estado, mas CicloDiaNoite controlará o skybox.
        if (isSpecialWeather)
        {
            UpdateWeatherState(WeatherCondition.Rainy); // Apenas define o estado e ativa/desativa efeito
        }
        else
        {
            // Para um início aleatório não gerenciado pelo RainManager:
            // WeatherCondition randomWeather = Random.value < chanceRain ? WeatherCondition.Rainy : WeatherCondition.Sunny;
            // UpdateWeatherState(randomWeather);
            // É mais seguro começar ensolarado e deixar RainManager controlar a chuva agendada.
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
        // NENHUMA mudança de skybox aqui. CicloDiaNoite.cs cuidará disso.
    }

    // Chamado por CicloDiaNoite para saber se está chovendo
    public bool IsRaining()
    {
        return currentWeather == WeatherCondition.Rainy;
    }

    // O método SetWeather original pode ser mantido para mudanças de clima "forçadas"
    // que não vêm do RainManager e que você queira que mudem o skybox imediatamente.
    // No entanto, para a chuva agendada, UpdateWeatherState é preferível.
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