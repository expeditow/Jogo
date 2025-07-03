using UnityEngine;
using UnityEngine.SceneManagement; // Importar para gerenciar cenas

public class MenuManager : MonoBehaviour
{
    // Método para o botão "Começar"
    public void IniciarJogo()
    {
        // O nome da cena do seu jogo principal (ex: "GameScene", "Fase1")
        // Certifique-se de que esta cena está adicionada em File > Build Settings
        SceneManager.LoadScene("CutscenesInicioCena");
        Debug.Log("Iniciando o jogo...");
    }

    // Método para o botão "Sair"
    public void SairDoJogo()
    {
        // Este comando só funciona quando o jogo é compilado (build).
        // No editor da Unity, ele apenas exibirá a mensagem de log.
        Application.Quit();
        Debug.Log("Saindo do jogo!");
    }
}