using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    [Header("Top-Down Perspective Settings")]
    [SerializeField] private float cameraHeight = 18f;
    [SerializeField, Range(45f, 90f)] private float topDownPitchAngle = 80f;
    [SerializeField] private float smoothSpeed = 15f;

    [Header("Viewport Positioning")]
    [Tooltip("Target normalized vertical screen position for the runner (0.25 = quarter screen from bottom)")]
    [SerializeField, Range(0.1f, 0.5f)] private float screenHeightRatio = 0.25f;

    [Tooltip("Lock camera strictly down the center line of the track (X = 0)")]
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
            var player = FindObjectOfType<SinglePlayerController>();
            if (player != null) target = player.transform;
            return;
        }

        float cameraToPlayerGroundZ = CalculateCameraToPlayerGroundDistance();

        float targetX = lockLateralCenter ? 0f : target.position.x;
        float targetY = cameraHeight;
        float targetZ = target.position.z - cameraToPlayerGroundZ;

        Vector3 desiredPos = new Vector3(targetX, targetY, targetZ);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime) + shakeOffset;
        transform.rotation = Quaternion.Euler(topDownPitchAngle, 0f, 0f);
    }

    public void TriggerShake(float duration = 0.25f, float magnitude = 0.4f)
    {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ExecuteShake(duration, magnitude));
    }

    private IEnumerator ExecuteShake(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float currentMag = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * currentMag,
                0f,
                Random.Range(-1f, 1f) * currentMag
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeCoroutine = null;
    }

    private float CalculateCameraToPlayerGroundDistance()
    {
        if (cam == null) cam = GetComponent<Camera>();

        if (cam.orthographic)
        {
            float frustumHeight = 2f * cam.orthographicSize;
            return frustumHeight * (screenHeightRatio - 0.5f);
        }

        float verticalAngleFromCenter = (screenHeightRatio - 0.5f) * cam.fieldOfView;
        float rayPitchFromGround = topDownPitchAngle - verticalAngleFromCenter;

        float clampedPitch = Mathf.Clamp(rayPitchFromGround, 10f, 89f);
        float pitchRad = clampedPitch * Mathf.Deg2Rad;

        return cameraHeight / Mathf.Tan(pitchRad);
    }
}