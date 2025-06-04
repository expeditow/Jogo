using UnityEngine;

public class SanguePingando : MonoBehaviour
{
    public GameObject prefabGota;
    public float intervalo = 0.5f; // tempo entre cada gota
    public float forcaQueda = 1.0f;

    void Start()
    {
        InvokeRepeating(nameof(PingarGota), 0f, intervalo);
    }

    void PingarGota()
    {
        GameObject gota = Instantiate(prefabGota, transform.position, Quaternion.identity);
        Rigidbody rb = gota.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        }
    }
}
