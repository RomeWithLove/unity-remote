using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeDetection : MonoBehaviour
{
    public static event Action OnSwipeLeft;
    public static event Action OnSwipeRight;

    [Header("Swipe Thresholds")]
    [SerializeField] private float minimumDistance = 0.15f;
    [SerializeField] private float maximumTime = 0.8f;
    [SerializeField, Range(0f, 1f)] private float directionThreshold = 0.6f;

    private InputManager inputManager;
    private Vector2 startPosition;
    private float startTime;
    private Vector2 endPosition;
    private float endTime;

    private void Start()
    {
        inputManager = InputManager.Instance;
        if (inputManager != null)
        {
            inputManager.OnStartTouch += SwipeStart;
            inputManager.OnEndTouch += SwipeEnd;
        }
    }

    private void OnDisable()
    {
        if (inputManager != null)
        {
            inputManager.OnStartTouch -= SwipeStart;
            inputManager.OnEndTouch -= SwipeEnd;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
        {
            OnSwipeLeft?.Invoke();
        }
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
        {
            OnSwipeRight?.Invoke();
        }
    }

    private void SwipeStart(Vector2 position, float time)
    {
        startPosition = position;
        startTime = time;
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        endPosition = position;
        endTime = time;
        DetectSwipe();
    }

    private void DetectSwipe()
    {
        Vector2 delta = endPosition - startPosition;
        float distance = delta.magnitude;
        float duration = endTime - startTime;

        if (distance >= minimumDistance && duration <= maximumTime)
        {
            Vector2 direction = delta.normalized;
            if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
            {
                OnSwipeLeft?.Invoke();
            }
            else if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
            {
                OnSwipeRight?.Invoke();
            }
        }
    }
}