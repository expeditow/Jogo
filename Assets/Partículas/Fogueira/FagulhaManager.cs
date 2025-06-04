using UnityEngine;

public class FagulhaManager : MonoBehaviour
{
    public GameObject fagulhaPrefab;
    public float intervalo = 0.3f;
    public float forcaVertical = 2f;

    float tempo;

    void Update()
    {
        tempo += Time.deltaTime;
        if (tempo >= intervalo)
        {
            tempo = 0f;
            CriarFagulha();
        }
    }

    void CriarFagulha()
    {
        Vector3 origem = transform.position;
        GameObject novaFagulha = Instantiate(fagulhaPrefab, origem, Random.rotation);
        novaFagulha.AddComponent<Fagulha>();
    }
}
