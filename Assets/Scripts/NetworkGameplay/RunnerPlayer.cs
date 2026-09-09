using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkPlayerContact))]
public class RunnerPlayer : NetworkBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float forwardSpeed = 7.0f;
    [SerializeField] private float slowSpeed = 2.5f;
    [SerializeField] private float shiftSpeed = 10.0f;
    [SerializeField] private float laneDistance = 2.0f;

    [Header("Lane Boundaries")]
    [SerializeField] private float leftBoundary = -3.5f;
    [SerializeField] private float rightBoundary = 3.5f;

    [Header("Carried Visual Reference")]
    [SerializeField] private GameObject carriedBallVisual;

    [Networked, OnChangedRender(nameof(OnBallPossessionChanged))]
    public NetworkBool HasBall { get; set; }

    [Networked] private TickTimer SlowdownTimer { get; set; }
    [Networked] private float NetworkedTargetX { get; set; }

    private CharacterController controller;
    private NetworkPlayerContact contactScript;

    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
        contactScript = GetComponent<NetworkPlayerContact>();

        if (carriedBallVisual == null && transform.childCount > 0)
        {
            Transform visualTransform = transform.Find("CarriedBallVisual");
            if (visualTransform != null)
            {
                carriedBallVisual = visualTransform.gameObject;
            }
        }

        if (HasStateAuthority)
        {
            NetworkedTargetX = transform.position.x;
            HasBall = false;
        }

        if (HasInputAuthority)
        {
            SwipeDetection.OnSwipeLeft += HandleLocalSwipeLeft;
            SwipeDetection.OnSwipeRight += HandleLocalSwipeRight;
        }

        UpdateCarriedBallVisual();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority)
        {
            SwipeDetection.OnSwipeLeft -= HandleLocalSwipeLeft;
            SwipeDetection.OnSwipeRight -= HandleLocalSwipeRight;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!MatchmakingLauncher.IsGameActive) return;

        float currentSpeed = (SlowdownTimer.IsRunning && !SlowdownTimer.Expired(Runner))
            ? slowSpeed
            : forwardSpeed;

        Vector3 move = Vector3.forward * currentSpeed * Runner.DeltaTime;
        float newX = Mathf.MoveTowards(transform.position.x, NetworkedTargetX, shiftSpeed * Runner.DeltaTime);
        move.x = newX - transform.position.x;

        controller.Move(move);
    }

    public void AssignBall(bool state)
    {
        if (HasStateAuthority)
        {
            HasBall = state;
        }
    }

    public void ApplySlowdown(float duration)
    {
        if (HasStateAuthority)
        {
            SlowdownTimer = TickTimer.CreateFromSeconds(Runner, duration);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        if (other.TryGetComponent<RunnerPlayer>(out var attackingPlayer))
        {
            NetworkPlayerContact attackerContact = attackingPlayer.GetComponent<NetworkPlayerContact>();

            if (HasBall && attackerContact != null && attackerContact.CurrentContactType == ContactType.ContactType1)
            {
                HasBall = false;

                if (NetworkBall.Instance != null)
                {
                    NetworkBall.Instance.LaunchFromPlayer(transform.position);
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayTackleRoar();
                }

                ApplySlowdown(1.0f);
            }
        }
    }

    private void OnBallPossessionChanged()
    {
        UpdateCarriedBallVisual();
    }

    private void UpdateCarriedBallVisual()
    {
        if (carriedBallVisual != null)
        {
            carriedBallVisual.SetActive(HasBall);
        }
    }

    private void HandleLocalSwipeLeft()
    {
        if (!HasInputAuthority) return;
        RPC_RequestLaneChange(Mathf.Clamp(NetworkedTargetX - laneDistance, leftBoundary, rightBoundary));
    }

    private void HandleLocalSwipeRight()
    {
        if (!HasInputAuthority) return;
        RPC_RequestLaneChange(Mathf.Clamp(NetworkedTargetX + laneDistance, leftBoundary, rightBoundary));
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestLaneChange(float targetX)
    {
        NetworkedTargetX = targetX;
    }
}