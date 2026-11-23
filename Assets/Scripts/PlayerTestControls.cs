using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public class PlayerTestControls : MonoBehaviour
{
    private const float MouseLookBoost = 1.6f;

    [SerializeField] private Camera viewCamera;
    [SerializeField, Min(0f)] private float movementSpeed = 3f;
    [SerializeField, Min(0f)] private float mouseSensitivity = 0.25f;
    [SerializeField, Range(1f, 89f)] private float pitchLimit = 80f;
    [SerializeField, Min(0f)] private float gravity = 20f;
    [SerializeField, Min(0f)] private float flightSpeed = 6f;
    [SerializeField, Min(0f)] private float keyboardLookSpeed = 100f;

    private CharacterController controller;
    private float pitch;
    private float verticalSpeed;
    private bool cursorCaptured;
    private bool isFlying;

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

        Keyboard keyboard = canControlPlayer ? Keyboard.current : null;
        if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
        {
            isFlying = !isFlying;
            verticalSpeed = 0f;
        }

        if (canControlPlayer)
        {
            RotateCamera(keyboard);
        }

        MovePlayer(keyboard);
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

    private void RotateCamera(Keyboard keyboard)
    {
        Vector2 look = Mouse.current != null
            ? Mouse.current.delta.ReadValue() * (mouseSensitivity * MouseLookBoost)
            : Vector2.zero;
        if (keyboard != null)
        {
            Vector2 keyboardLook = new Vector2(
                Axis(keyboard.lKey.isPressed, keyboard.jKey.isPressed),
                Axis(keyboard.iKey.isPressed, keyboard.kKey.isPressed));
            look += keyboardLook * (keyboardLookSpeed * Time.deltaTime);
        }

        transform.Rotate(0f, look.x, 0f);
        pitch = Mathf.Clamp(pitch - look.y, -pitchLimit, pitchLimit);
        viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private static float Axis(bool positive, bool negative)
    {
        return (positive ? 1f : 0f) - (negative ? 1f : 0f);
    }

    private void MovePlayer(Keyboard keyboard)
    {
        Vector3 input = Vector3.zero;

        if (keyboard != null)
        {
            input.x = Axis(keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed,
                keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed);
            input.z = Axis(keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed,
                keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed);
            if (isFlying)
            {
                input.y = Axis(keyboard.spaceKey.isPressed,
                    keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed);
            }
        }

        Vector3 direction = transform.TransformDirection(Vector3.ClampMagnitude(input, 1f));
        if (isFlying)
        {
            controller.Move(direction * (flightSpeed * Time.deltaTime));
            return;
        }

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
