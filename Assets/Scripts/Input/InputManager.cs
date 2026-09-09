using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    public delegate void StartTouch(Vector2 position, float time);
    public event StartTouch OnStartTouch;

    public delegate void EndTouch(Vector2 position, float time);
    public event EndTouch OnEndTouch;

    private PlayerControls playerControls;

    protected override void Awake()
    {
        base.Awake();
        TouchSimulation.Enable();

        if (playerControls == null)
        {
            playerControls = new PlayerControls();
        }
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
        }

        playerControls.Enable();
        playerControls.Touch.PrimaryContact.started += StartTouchPrimary;
        playerControls.Touch.PrimaryContact.canceled += EndTouchPrimary;
    }

    private void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.Touch.PrimaryContact.started -= StartTouchPrimary;
            playerControls.Touch.PrimaryContact.canceled -= EndTouchPrimary;
            playerControls.Disable();
        }
    }

    private void StartTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnStartTouch != null)
        {
            Vector2 screenPos = playerControls.Touch.PrimaryPosition.ReadValue<Vector2>();
            OnStartTouch(screenPos, (float)context.startTime);
        }
    }

    private void EndTouchPrimary(InputAction.CallbackContext context)
    {
        if (OnEndTouch != null)
        {
            Vector2 screenPos = playerControls.Touch.PrimaryPosition.ReadValue<Vector2>();
            OnEndTouch(screenPos, (float)context.time);
        }
    }

    public Vector2 PrimaryPosition()
    {
        if (playerControls == null) return Vector2.zero;
        return playerControls.Touch.PrimaryPosition.ReadValue<Vector2>();
    }
}
