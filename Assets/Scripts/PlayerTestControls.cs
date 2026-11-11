using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public sealed class PlayerTestControls : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField, Min(0f)] private float movementSpeed = 3f;
    [SerializeField, Min(0f)] private float mouseSensitivity = 0.1f;
    [SerializeField, Range(1f, 89f)] private float pitchLimit = 80f;
    [SerializeField, Min(0f)] private float gravity = 20f;

    private CharacterController controller;
    private float pitch;
    private float verticalSpeed;
    private bool cursorCaptured;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (viewCamera == null || !viewCamera.transform.IsChildOf(transform))
        {
            Debug.LogError("PlayerTestControls: Assign a camera parented under this player.", this);
            enabled = false;
            return;
        }

        pitch = Mathf.DeltaAngle(0f, viewCamera.transform.localEulerAngles.x);
    }

    private void Start()
    {
        SetCursorCaptured(true);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            SetCursorCaptured(!cursorCaptured);

        bool acceptsInput = Application.isFocused && cursorCaptured && Cursor.lockState == CursorLockMode.Locked;
        if (acceptsInput && Mouse.current != null)
        {
            // Mouse delta is already a per-frame displacement; do not multiply by deltaTime.
            Vector2 look = Mouse.current.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(0f, look.x, 0f, Space.Self);
            pitch = Mathf.Clamp(pitch - look.y, -pitchLimit, pitchLimit);
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        Vector2 input = Vector2.zero;
        if (acceptsInput && keyboard != null)
        {
            input.x = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
            input.y = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            input = Vector2.ClampMagnitude(input, 1f);
        }

        if (controller.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -2f;
        verticalSpeed -= gravity * Time.deltaTime;

        Vector3 velocity = (transform.right * input.x + transform.forward * input.y) * movementSpeed;
        velocity.y = verticalSpeed;
        CollisionFlags collisions = controller.Move(velocity * Time.deltaTime);
        if ((collisions & CollisionFlags.Above) != 0 && verticalSpeed > 0f)
            verticalSpeed = 0f;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
            SetCursorCaptured(false);
    }

    private void OnDisable()
    {
        SetCursorCaptured(false);
    }

    private void SetCursorCaptured(bool captured)
    {
        cursorCaptured = captured;
        Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !captured;
    }
}
