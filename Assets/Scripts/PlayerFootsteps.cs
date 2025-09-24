using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(CharacterController))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Footstep Clips")]
    public AudioClip[] walkClips;
    public AudioClip[] runClips;
    public AudioClip jumpClip;
    public AudioClip landClip;

    [Header("Settings")]
    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.3f;

    [Header("Input")]
    public InputActionReference runAction; // Asignar Shift en PC, joystick press en Quest

    private float stepTimer;
    private FirstPersonController fpsController;
    private CharacterController charController;

    private bool wasGrounded;

    void Start()
    {
        fpsController = GetComponent<FirstPersonController>();
        charController = GetComponent<CharacterController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        stepTimer = walkStepInterval;
        wasGrounded = charController.isGrounded;
    }

    void Update()
    {
        // Revisar si el jugador está en el suelo
        bool isGrounded = charController.isGrounded;

        // Detectar aterrizaje
        if (!wasGrounded && isGrounded)
            PlayLand();

        // Detectar salto
        if (wasGrounded && !isGrounded)
            PlayJump();

        wasGrounded = isGrounded;

        // Solo reproducir pasos si está en el suelo
        if (!isGrounded) return;

        // Detectar movimiento real usando la velocidad del CharacterController
        bool isMoving = charController.velocity.magnitude > 0.1f;
        bool isRunning = runAction != null && runAction.action.IsPressed();

        float interval = isRunning ? runStepInterval : walkStepInterval;
        stepTimer -= Time.deltaTime;

        if (isMoving && stepTimer <= 0f)
        {
            PlayFootstep(isRunning);
            stepTimer = interval;
        }
    }

    void PlayFootstep(bool running)
    {
        AudioClip[] clips = running ? runClips : walkClips;
        if (clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }

    public void PlayJump()
    {
        if (jumpClip != null)
            audioSource.PlayOneShot(jumpClip);
    }

    public void PlayLand()
    {
        if (landClip != null)
            audioSource.PlayOneShot(landClip);
    }
}
