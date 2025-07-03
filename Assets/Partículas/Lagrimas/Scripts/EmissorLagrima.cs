using UnityEngine;

public class EmissorLagrima : MonoBehaviour
{
    public GameObject prefabLagrima;
    public float intervalo = 1.5f;
    private float tempo;

    // NOVA VARIÁVEL: Controla se a lágrima deve ser emitida continuamente
    private bool isCrying = false;

    void Update()
    {
        // Só emite se a flag isCrying for verdadeira
        if (!isCrying) return;

        tempo += Time.deltaTime;
        if (tempo >= intervalo)
        {
            tempo = 0f;
            GameObject gota = Instantiate(prefabLagrima, transform.position, Quaternion.identity);

            // Verifica se o script GotaLagrima existe ou remova esta linha se não for necessário
            if (gota.GetComponent<GotaLagrima>() == null)
            {
                gota.AddComponent<GotaLagrima>();
            }
            Debug.Log("Lágrima emitida (contínua)!");
        }
    }

    // NOVO MÉTODO: Chamado externamente para INICIAR a emissão contínua
    public void StartCrying()
    {
        if (!isCrying) // Evita resetar o timer se já estiver chorando
        {
            isCrying = true;
            tempo = 0f; // Reseta o timer para que a primeira lágrima caia logo
            Debug.Log($"{gameObject.name} começou a chorar!");
        }
    }

    // NOVO MÉTODO: Chamado externamente para PARAR a emissão contínua (opcional, mas bom ter)
    public void StopCrying()
    {
        if (isCrying)
        {
            isCrying = false;
            Debug.Log($"{gameObject.name} parou de chorar.");
        }
    }
}