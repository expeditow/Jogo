using UnityEngine;

public class EmissorLagrima : MonoBehaviour
{
    public GameObject prefabLagrima;
    public float intervalo = 1.5f;
    private float tempo;

    // NOVA VARI�VEL: Controla se a l�grima deve ser emitida continuamente
    private bool isCrying = false;

    void Update()
    {
        // S� emite se a flag isCrying for verdadeira
        if (!isCrying) return;

        tempo += Time.deltaTime;
        if (tempo >= intervalo)
        {
            tempo = 0f;
            GameObject gota = Instantiate(prefabLagrima, transform.position, Quaternion.identity);

            // Verifica se o script GotaLagrima existe ou remova esta linha se n�o for necess�rio
            if (gota.GetComponent<GotaLagrima>() == null)
            {
                gota.AddComponent<GotaLagrima>();
            }
            Debug.Log("L�grima emitida (cont�nua)!");
        }
    }

    // NOVO M�TODO: Chamado externamente para INICIAR a emiss�o cont�nua
    public void StartCrying()
    {
        if (!isCrying) // Evita resetar o timer se j� estiver chorando
        {
            isCrying = true;
            tempo = 0f; // Reseta o timer para que a primeira l�grima caia logo
            Debug.Log($"{gameObject.name} come�ou a chorar!");
        }
    }

    // NOVO M�TODO: Chamado externamente para PARAR a emiss�o cont�nua (opcional, mas bom ter)
    public void StopCrying()
    {
        if (isCrying)
        {
            isCrying = false;
            Debug.Log($"{gameObject.name} parou de chorar.");
        }
    }
}