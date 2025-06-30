using UnityEngine;

public class JatoDeSangue : MonoBehaviour
{
    public GameObject prefabGota;
    public int quantidadePorExplosao = 50;
    public float intervalo = 0.8f;
    private float tempo;

    void Start()
    {
        ExplodirSangue();
    }

    void Update()
    {
        tempo += Time.deltaTime;
        if (tempo >= intervalo)
        {
            tempo = 0f;
            ExplodirSangue();
        }
    }

    void ExplodirSangue()
    {
        for (int i = 0; i < quantidadePorExplosao; i++)
        {
            GameObject gota = Instantiate(prefabGota, transform.position, Quaternion.identity);
            gota.AddComponent<GotaSangue>();
        }
    }
}
