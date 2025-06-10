using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public class PlayerMuve : NetworkBehaviour
{   //Movimentação basica dos jogadores
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
    //private bool _canMove = false;

    public override void OnStartAuthority()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx => inputMove = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => inputMove = Vector2.zero;

        inputActions.Player.Look.performed += ctx => inputLook = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => inputLook = Vector2.zero;

        inputActions.Player.Jump.performed += _ => jumpQueued = true;

        inputActions.Player.Enable();
        LockCursor();
        //_canMove = true;
        if (cameraHolder != null)
            cameraHolder.gameObject.SetActive(true);
    }

    public override void OnStopAuthority()
    {
        //_canMove = false;
        if (inputActions != null)
            inputActions.Player.Disable();
        UnlockCursor();
        if (cameraHolder != null)
            cameraHolder.gameObject.SetActive(false);
    }

    void Update()
    {
        if (/*_canMove != true ||*/  !authority || !NetworkClient.ready) return;//

        HandleCursor();
        RotateView();

        Vector3 moveDir = transform.right * inputMove.x + transform.forward * inputMove.y;
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (jumpQueued && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        jumpQueued = false;
        velocity.y += gravity * Time.deltaTime;

        controller.Move((moveDir * moveSpeed + velocity) * Time.deltaTime);
    }

    void RotateView()
    {
        currentLook = Vector2.SmoothDamp(currentLook, inputLook, ref currentLookVelocity, smoothTime);

        float mouseX = currentLook.x * mouseSensitivity;
        float mouseY = currentLook.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);

        if (cameraHolder != null)
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
