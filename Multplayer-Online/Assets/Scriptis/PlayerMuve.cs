using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using Unity.VisualScripting;
using System.Threading;
[RequireComponent(typeof(CharacterController))]
public class PlayerMuve : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 1.5f;
    public float gravity = -9.81f;

    [Header("Câmera")]
    public Transform cameraHolder;
    public float mouseSensitivity = 2f;
    public float clampAngle = 80f;
    public float smoothTime = 0.05f;

    private CharacterController controller;
    private PlayerInputActions inputActions;

    private Vector2 inputMove;
    private Vector2 inputLook;
    private Vector2 currentLook;
    private Vector2 currentLookVelocity;

    private Vector3 velocity;
    private float xRotation = 0f;
    private bool jumpQueued = false;
    //--Ajustes-pra-Sync-Online
    private Vector3 serverMoveInput;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => inputMove = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => inputMove = Vector2.zero;

        inputActions.Player.Look.performed += ctx => inputLook = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => inputLook = Vector2.zero;

        inputActions.Player.Jump.performed += _ => jumpQueued = true;
    }

    void OnEnable()
    {
        if(authority)
        {
            inputActions.Player.Enable();
            LockCursor(); // trava o cursor ao iniciar
        }
        
    }

    void OnDisable()
    {
        if (!authority)
        {
            inputActions.Player.Disable();
            UnlockCursor();// desbloqueia ao desabilitar
        }
        
    }
    void Start()
    {
        if (!authority)
        {
            cameraHolder.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!authority) return;//impede a entrada remota 
        // Movimento do mouse (rotação do jogador + câmera)
        HandleCursor();
        RotateView();

        // Movimento no plano XZ
        // Envia entrada para o servidor
        Vector3 moveDir = transform.right * inputMove.x + transform.forward * inputMove.y;
        CmdMove(moveDir, jumpQueued);
        jumpQueued = false;
    }
    [Command]
    void CmdMove(Vector3 moveInput, bool jump)
    {
        if (!controller) return;

        // Aplica movimento recebido
        Vector3 move = moveInput * moveSpeed * Time.deltaTime;
        controller.Move(move);

        if (controller.isGrounded && velocity.y < 0)
        velocity.y = -2f;

        if (jump && controller.isGrounded)
        velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void RotateView()
    {
        currentLook = Vector2.SmoothDamp(currentLook, inputLook, ref currentLookVelocity, smoothTime);

        float mouseX = inputLook.x * mouseSensitivity;
        float mouseY = inputLook.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    void HandleCursor()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            UnlockCursor();

        if (Mouse.current.rightButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            LockCursor();
    }
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
