using UnityEngine;

public class GotaSangue : MonoBehaviour
{
    private Vector3 direcao;
    private float velocidade;

    void Start()
    {
        direcao = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1.5f),
            Random.Range(0.5f, 1.5f)
        ).normalized;

        velocidade = Random.Range(5f, 10f);
        Destroy(gameObject, 0.2f);
    }

    void Update()
    {
        transform.position += direcao * velocidade * Time.deltaTime;
    }
}