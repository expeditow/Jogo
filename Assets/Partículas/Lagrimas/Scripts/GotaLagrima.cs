using UnityEngine;

public class GotaLagrima : MonoBehaviour
{
    private float velocidade = 1.0f;

    void Start()
    {
        Destroy(gameObject, 3f); // desaparece após um tempo
    }

    void Update()
    {
        transform.position += Vector3.down * velocidade * Time.deltaTime;
    }
}
