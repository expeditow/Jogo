using UnityEngine;
using UnityEngine.UI; // Certifique-se de que este using está presente

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    [Header("Movement Settings")]
    public float speed = 12f;
    public float sprintSpeed = 20f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;

    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float staminaDepletionRate = 15f;
    public float staminaRegenerationRate = 10f;
    private float currentStamina;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("UI References")]
    public Slider staminaBar;

    Vector3 velocity;
    bool isGrounded;
    bool isSprinting = false;

    void Start()
    {
        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = currentStamina;
        }
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // --- LÓGICA PRINCIPAL DA ESTAMINA E CORRIDA ---
        isSprinting = Input.GetKey(KeyCode.LeftShift) && isGrounded && currentStamina > 0 && z > 0;

        float currentSpeed;
        if (isSprinting)
        {
            currentSpeed = sprintSpeed;
            currentStamina -= staminaDepletionRate * Time.deltaTime;
        }
        else
        {
            currentSpeed = speed;
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenerationRate * Time.deltaTime;
            }
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move.normalized * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (staminaBar != null)
        {
            staminaBar.value = currentStamina;
        }
    }

    // --- MÉTODOS PÚBLICOS PARA O SISTEMA DE COMBATE ACESSAR A ESTAMINA ---
    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    public void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina); // Garante que não vai abaixo de 0
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina; // Atualiza a UI
        }
        Debug.Log($"ESTAMINA: Consumiu {amount} de estamina. Estamina atual: {currentStamina}");
    }

    public void RegenerateStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina); // Garante que não vai acima de maxStamina
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina; // Atualiza a UI
        }
        Debug.Log($"ESTAMINA: Regenerou {amount} de estamina. Estamina atual: {currentStamina}");
    }
}