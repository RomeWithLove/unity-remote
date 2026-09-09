using System.Collections;
using UnityEngine;
using Fusion;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Top-Down Perspective Settings")]
    [SerializeField] private float cameraHeight = 18f;
    [SerializeField, Range(60f, 90f)] private float topDownPitchAngle = 80f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Viewport Positioning")]
    [SerializeField, Range(0.1f, 0.5f)] private float screenHeightRatio = 0.25f;
    [SerializeField] private bool lockLateralCenter = true;

    private Vector3 shakeOffset = Vector3.zero;
    private Coroutine shakeCoroutine;
    private Camera cam;
    private Transform target;

    private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(topDownPitchAngle, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindLocalPlayer();
            return;
        }

        float forwardLeadOffset = CalculateForwardLeadOffset();
        float targetX = lockLateralCenter ? 0f : target.position.x;
        float targetY = cameraHeight;
        float targetZ = target.position.z + forwardLeadOffset;

        Vector3 desiredPosition = new Vector3(targetX, targetY, targetZ);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime) + shakeOffset;
    }

    public void TriggerShake(float duration = 0.25f, float magnitude = 0.4f)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ExecuteShake(duration, magnitude));
    }

    private IEnumerator ExecuteShake(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float currentMagnitude = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            shakeOffset = new Vector3(
                UnityEngine.Random.Range(-1f, 1f) * currentMagnitude,
                0f,
                UnityEngine.Random.Range(-1f, 1f) * currentMagnitude
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeCoroutine = null;
    }

    private float CalculateForwardLeadOffset()
    {
        float totalFrustumHeight;
        if (cam.orthographic)
        {
            totalFrustumHeight = 2f * cam.orthographicSize;
        }
        else
        {
            float distanceToGround = cameraHeight;
            totalFrustumHeight = 2f * distanceToGround * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }

        float viewportOffsetFromCenter = 0.5f - screenHeightRatio;
        return totalFrustumHeight * viewportOffsetFromCenter;
    }

    private void FindLocalPlayer()
    {
        var players = FindObjectsOfType<RunnerPlayer>();
        foreach (var player in players)
        {
            if (player.Object != null && player.Object.HasInputAuthority)
            {
                target = player.transform;
                break;
            }
        }
    }
}