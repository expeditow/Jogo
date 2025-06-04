using UnityEngine;

public class Fagulha : MonoBehaviour
{
    float velocidadeVertical;
    float tempoVida;
    float tempoMax = 2f;

    void Start()
    {
        velocidadeVertical = Random.Range(0.5f, 1.5f);
        tempoVida = 0f;
    }

    void Update()
    {
        transform.position += new Vector3(0, velocidadeVertical, 0) * Time.deltaTime;
        transform.Rotate(Vector3.up * 100f * Time.deltaTime);
        tempoVida += Time.deltaTime;

        if (tempoVida > tempoMax)
        {
            Destroy(gameObject);
        }
    }
}