using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SinglePlayerController : MonoBehaviour
{
    [Header("Forward Locomotion")]
    [SerializeField] private float forwardSpeed = 8.0f;

    [Header("Tilt / Accelerometer Steering")]
    [SerializeField] private float tiltSensitivity = 12.0f;
    [SerializeField] private float tiltDeadzone = 0.05f;

    [Header("Horizontal Swipe Boost")]
    [SerializeField] private float swipeBoostDistance = 3.0f;
    [SerializeField] private float swipeBoostSpeed = 16.0f;

    [Header("Track Limits")]
    [SerializeField] private float leftBoundary = -4.5f;
    [SerializeField] private float rightBoundary = 4.5f;

    private CharacterController characterController;
    private float currentLateralSlideVelocity = 0f;
    private float targetLateralX;
    private bool isSliding = false;
    private float speedModifier = 1.0f;
    private float slowdownTimer = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        targetLateralX = transform.position.x;
    }

    private void OnEnable()
    {
        SwipeDetection.OnSwipeLeft += HandleSwipeLeft;
        SwipeDetection.OnSwipeRight += HandleSwipeRight;
    }

    private void OnDisable()
    {
        SwipeDetection.OnSwipeLeft -= HandleSwipeLeft;
        SwipeDetection.OnSwipeRight -= HandleSwipeRight;
    }

    private void Update()
    {
        if (slowdownTimer > 0f)
        {
            slowdownTimer -= Time.deltaTime;
            if (slowdownTimer <= 0f) speedModifier = 1.0f;
        }

        Vector3 moveVector = Vector3.forward * (forwardSpeed * speedModifier);

        // Read accelerometer tilt
        float tiltX = Input.acceleration.x;
        if (Mathf.Abs(tiltX) < tiltDeadzone) tiltX = 0f;

        // Keyboard fallback
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) tiltX = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) tiltX = 1f;
        }

        if (isSliding)
        {
            float newX = Mathf.MoveTowards(transform.position.x, targetLateralX, swipeBoostSpeed * Time.deltaTime);
            moveVector.x = (newX - transform.position.x) / Time.deltaTime;

            if (Mathf.Abs(transform.position.x - targetLateralX) < 0.05f)
            {
                isSliding = false;
            }
        }
        else
        {
            moveVector.x = tiltX * tiltSensitivity;
        }

        characterController.Move(moveVector * Time.deltaTime);

        // Enforce boundary clamping
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, leftBoundary, rightBoundary);
        transform.position = clampedPos;
    }

    private void HandleSwipeLeft()
    {
        targetLateralX = Mathf.Clamp(transform.position.x - swipeBoostDistance, leftBoundary, rightBoundary);
        isSliding = true;
    }

    private void HandleSwipeRight()
    {
        targetLateralX = Mathf.Clamp(transform.position.x + swipeBoostDistance, leftBoundary, rightBoundary);
        isSliding = true;
    }

    public void ApplySlowdown(float duration, float factor = 0.4f)
    {
        speedModifier = factor;
        slowdownTimer = duration;
    }

    public void CancelLateralSlide()
    {
        isSliding = false;
        targetLateralX = transform.position.x;
    }
}