using System;
using UnityEngine;

public class SwipeDetection : MonoBehaviour
{
    public static event Action OnSwipeLeft;
    public static event Action OnSwipeRight;
    public static event Action OnSwipeUp;
    public static event Action OnSwipeDown;

    [Header("Swipe Thresholds")]
    [SerializeField] private float minimumDistance = 50f;
    [SerializeField] private float maximumTime = 1f;
    [SerializeField, Range(0f, 1f)] private float directionThreshold = 0.7f;

    private InputManager inputManager;
    private Vector2 startPosition;
    private float startTime;
    private Vector2 endPosition;
    private float endTime;

    private void Awake()
    {
        inputManager = InputManager.Instance;
    }

    private void OnEnable()
    {
        inputManager.OnStartTouch += SwipeStart;
        inputManager.OnEndTouch += SwipeEnd;
    }

    private void OnDisable()
    {
        inputManager.OnStartTouch -= SwipeStart;
        inputManager.OnEndTouch -= SwipeEnd;
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
            SwipeDirection(direction);
        }
    }

    private void SwipeDirection(Vector2 direction)
    {
        if (Vector2.Dot(Vector2.left, direction) > directionThreshold)
        {
            Debug.Log("[Swipe] Swipe Left Detected!");
            OnSwipeLeft?.Invoke();
        }
        else if (Vector2.Dot(Vector2.right, direction) > directionThreshold)
        {
            Debug.Log("[Swipe] Swipe Right Detected!");
            OnSwipeRight?.Invoke();
        }
        else if (Vector2.Dot(Vector2.up, direction) > directionThreshold)
        {
            Debug.Log("[Swipe] Swipe Up Detected!");
            OnSwipeUp?.Invoke();
        }
        else if (Vector2.Dot(Vector2.down, direction) > directionThreshold)
        {
            Debug.Log("[Swipe] Swipe Down Detected!");
            OnSwipeDown?.Invoke();
        }
    }
}