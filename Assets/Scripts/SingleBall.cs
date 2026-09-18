using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SingleBall : MonoBehaviour
{
    public static SingleBall Instance { get; private set; }

    [Header("Speed Settings")]
    [SerializeField] private float baseMoveSpeed = 3.0f;
    [SerializeField] private float launchSpeed = 16.0f;
    [SerializeField] private float launchDuration = 1.2f;

    [Header("Visual & Physics Components")]
    [SerializeField] private GameObject visualMesh;
    private SphereCollider triggerCollider;

    public bool IsActiveInWorld { get; private set; } = true;
    private bool isLaunched = false;
    private float launchTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        triggerCollider = GetComponent<SphereCollider>();
        triggerCollider.isTrigger = true;

        if (visualMesh == null && transform.childCount > 0)
        {
            visualMesh = transform.GetChild(0).gameObject;
        }

        UpdateVisualState();
    }

    private void Update()
    {
        if (!IsActiveInWorld) return;

        float currentSpeed = baseMoveSpeed;

        if (isLaunched)
        {
            currentSpeed = launchSpeed;
            launchTimer -= Time.deltaTime;
            if (launchTimer <= 0f) isLaunched = false;
        }

        transform.position += Vector3.forward * currentSpeed * Time.deltaTime;
    }

    public void CollectBall()
    {
        IsActiveInWorld = false;
        isLaunched = false;
        UpdateVisualState();
    }

    public void LaunchFromPlayer(Vector3 carrierPosition)
    {
        transform.position = carrierPosition + Vector3.forward * 2.0f;
        IsActiveInWorld = true;
        isLaunched = true;
        launchTimer = launchDuration;
        UpdateVisualState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsActiveInWorld) return;

        if (other.TryGetComponent<PlayerBallCarrier>(out var carrier))
        {
            if (!carrier.HasBall)
            {
                carrier.AssignBall(true);
                CollectBall();
            }
        }
    }

    private void UpdateVisualState()
    {
        if (visualMesh != null) visualMesh.SetActive(IsActiveInWorld);
        if (triggerCollider != null) triggerCollider.enabled = IsActiveInWorld;
    }
}