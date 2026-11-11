using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class PlayerTestControls : MonoBehaviour
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

    // Update runs once each frame. Each task has its own method below.
    private void Update()
    {
        CheckEscapeKey();

        bool canControlPlayer = Application.isFocused && cursorCaptured &&
            Cursor.lockState == CursorLockMode.Locked;

        if (canControlPlayer)
        {
            RotateCamera();
        }

        // Continue applying gravity even when the mouse is released.
        MovePlayer(canControlPlayer);
    }

    private void CheckEscapeKey()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorCaptured(!cursorCaptured);
        }
    }

    private void RotateCamera()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Vector2 mouseMovement = Mouse.current.delta.ReadValue();
        float horizontalLook = mouseMovement.x * mouseSensitivity;
        float verticalLook = mouseMovement.y * mouseSensitivity;

        // Turn the whole player left/right; tilt only the camera up/down.
        transform.Rotate(0f, horizontalLook, 0f);
        pitch = pitch - verticalLook;
        pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);
        viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // Mouse delta already measures movement this frame. It needs no deltaTime.
    }

    private void MovePlayer(bool canControlPlayer)
    {
        Vector2 input = Vector2.zero;
        Keyboard keyboard = Keyboard.current;

        if (canControlPlayer && keyboard != null)
        {
            if (keyboard.wKey.isPressed)
            {
                input.y = input.y + 1f;
            }
            if (keyboard.sKey.isPressed)
            {
                input.y = input.y - 1f;
            }
            if (keyboard.dKey.isPressed)
            {
                input.x = input.x + 1f;
            }
            if (keyboard.aKey.isPressed)
            {
                input.x = input.x - 1f;
            }
        }

        // Prevent diagonal movement from being faster than straight movement.
        input = Vector2.ClampMagnitude(input, 1f);
        Vector3 direction = transform.right * input.x + transform.forward * input.y;
        Vector3 velocity = direction * movementSpeed;

        if (controller.isGrounded && verticalSpeed < 0f)
        {
            // A small downward speed keeps the controller touching the floor.
            verticalSpeed = -2f;
        }

        verticalSpeed = verticalSpeed - gravity * Time.deltaTime;
        velocity.y = verticalSpeed;

        // Speed is in metres per second. deltaTime converts it to this frame's distance.
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused)
        {
            SetCursorCaptured(false);
        }
    }

    private void OnDisable()
    {
        SetCursorCaptured(false);
    }

    private void SetCursorCaptured(bool captured)
    {
        cursorCaptured = captured;
        if (captured)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
        Cursor.visible = !captured;
    }
}
