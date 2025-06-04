using UnityEngine;

public class EmissorLagrima : MonoBehaviour
{
    public GameObject prefabLagrima;
    public float intervalo = 1.5f;
    private float tempo;

    void Update()
    {
        tempo += Time.deltaTime;
        if (tempo >= intervalo)
        {
            tempo = 0f;
            GameObject gota = Instantiate(prefabLagrima, transform.position, Quaternion.identity);
            gota.AddComponent<GotaLagrima>();
        }
    }
}
