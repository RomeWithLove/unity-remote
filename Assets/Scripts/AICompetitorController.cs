using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AICompetitorController : MonoBehaviour
{
    [Header("Speed & Rubber-banding")]
    [SerializeField] private float baseSpeed = 7.5f;
    [SerializeField] private float rubberBandCatchupFactor = 1.5f;
    [SerializeField] private float rubberBandSlowdownFactor = 0.7f;

    [Header("Targeting & Steering")]
    [SerializeField] private float steerSpeed = 6.0f;
    [SerializeField] private float leftBoundary = -4.5f;
    [SerializeField] private float rightBoundary = 4.5f;

    private CharacterController characterController;
    private Transform playerTransform;
    private PlayerBallCarrier botBallCarrier;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        botBallCarrier = GetComponent<PlayerBallCarrier>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            return;
        }

        // Rubber-banding speed logic relative to player Z
        float currentSpeed = baseSpeed;
        float zDelta = playerTransform.position.z - transform.position.z;

        if (zDelta > 4.0f) currentSpeed *= rubberBandCatchupFactor;
        else if (zDelta < -6.0f) currentSpeed *= rubberBandSlowdownFactor;

        // Determine horizontal steering target
        float targetX = transform.position.x;

        if (SingleBall.Instance != null && SingleBall.Instance.IsActiveInWorld)
        {
            targetX = SingleBall.Instance.transform.position.x;
        }
        else
        {
            var playerCarrier = playerTransform.GetComponent<PlayerBallCarrier>();
            if (playerCarrier != null && playerCarrier.HasBall)
            {
                targetX = playerTransform.position.x;
            }
        }

        float newX = Mathf.MoveTowards(transform.position.x, targetX, steerSpeed * Time.deltaTime);
        newX = Mathf.Clamp(newX, leftBoundary, rightBoundary);

        Vector3 moveDelta = new Vector3(newX - transform.position.x, 0f, currentSpeed * Time.deltaTime);
        characterController.Move(moveDelta);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<PlayerBallCarrier>(out var carrier))
        {
            if (carrier.HasBall)
            {
                carrier.Fumble();
            }
        }
    }
}