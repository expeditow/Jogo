using UnityEngine;

public class ClimaSystem : MonoBehaviour
{
    [Range(0f, 1f)]
    public float chanceRain = 0.5f; // Usado para l�gica inicial ou climas n�o gerenciados pelo RainManager

    public GameObject rainEffect;
    public Material rainSkyBox; // Usado por CicloDiaNoite
    public Material sunnySkyBox; // Usado por CicloDiaNoite

    public bool isSpecialWeather; // Para for�ar um clima espec�fico no in�cio

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

        // No in�cio, ClimaSystem define um estado, mas CicloDiaNoite controlar� o skybox.
        if (isSpecialWeather)
        {
            UpdateWeatherState(WeatherCondition.Rainy); // Apenas define o estado e ativa/desativa efeito
        }
        else
        {
            // Para um in�cio aleat�rio n�o gerenciado pelo RainManager:
            // WeatherCondition randomWeather = Random.value < chanceRain ? WeatherCondition.Rainy : WeatherCondition.Sunny;
            // UpdateWeatherState(randomWeather);
            // � mais seguro come�ar ensolarado e deixar RainManager controlar a chuva agendada.
            UpdateWeatherState(WeatherCondition.Sunny);
        }
    }

    // M�todo chamado pelo RainManager para atualizar o estado do clima
    public void UpdateWeatherState(WeatherCondition newWeather)
    {
        currentWeather = newWeather;

        if (rainEffect != null)
        {
            rainEffect.SetActive(currentWeather == WeatherCondition.Rainy);
        }
        // NENHUMA mudan�a de skybox aqui. CicloDiaNoite.cs cuidar� disso.
    }

    // Chamado por CicloDiaNoite para saber se est� chovendo
    public bool IsRaining()
    {
        return currentWeather == WeatherCondition.Rainy;
    }

    // O m�todo SetWeather original pode ser mantido para mudan�as de clima "for�adas"
    // que n�o v�m do RainManager e que voc� queira que mudem o skybox imediatamente.
    // No entanto, para a chuva agendada, UpdateWeatherState � prefer�vel.
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