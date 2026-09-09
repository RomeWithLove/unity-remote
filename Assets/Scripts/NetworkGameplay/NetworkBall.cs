using Fusion;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(NetworkTransform))]
public class NetworkBall : NetworkBehaviour
{
    public static NetworkBall Instance { get; private set; }

    [Header("Speed Settings")]
    [SerializeField] private float baseMoveSpeed = 3.0f;
    [SerializeField] private float launchSpeed = 16.0f;
    [SerializeField] private float launchDuration = 1.2f;

    [Header("Visual & Physics References")]
    [SerializeField] private GameObject visualMesh;
    private SphereCollider triggerCollider;

    [Networked, OnChangedRender(nameof(OnBallVisibilityChanged))]
    public NetworkBool IsActiveInWorld { get; set; }

    [Networked] private NetworkBool IsLaunched { get; set; }
    [Networked] private TickTimer LaunchTimer { get; set; }

    public override void Spawned()
    {
        Instance = this;

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<SphereCollider>();
        }
        triggerCollider.isTrigger = true;

        if (visualMesh == null && transform.childCount > 0)
        {
            visualMesh = transform.GetChild(0).gameObject;
        }

        if (HasStateAuthority)
        {
            IsActiveInWorld = true;
            IsLaunched = false;
        }

        UpdateVisualState();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !IsActiveInWorld) return;

        float speed = baseMoveSpeed;

        if (IsLaunched)
        {
            speed = launchSpeed;
            if (LaunchTimer.Expired(Runner))
            {
                IsLaunched = false;
            }
        }

        transform.position += Vector3.forward * speed * Runner.DeltaTime;
    }

    public void CollectBall()
    {
        if (!HasStateAuthority) return;

        IsActiveInWorld = false;
        IsLaunched = false;
    }

    public void LaunchFromPlayer(Vector3 carrierPosition)
    {
        if (!HasStateAuthority) return;

        transform.position = carrierPosition + Vector3.forward * 2.0f;
        IsActiveInWorld = true;
        IsLaunched = true;
        LaunchTimer = TickTimer.CreateFromSeconds(Runner, launchDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority || !IsActiveInWorld || !MatchmakingLauncher.IsGameActive) return;

        if (other.TryGetComponent<RunnerPlayer>(out var player))
        {
            if (!player.HasBall)
            {
                player.AssignBall(true);
                CollectBall();
            }
        }
    }

    private void OnBallVisibilityChanged()
    {
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (visualMesh != null)
        {
            visualMesh.SetActive(IsActiveInWorld);
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = IsActiveInWorld;
        }
    }
}
