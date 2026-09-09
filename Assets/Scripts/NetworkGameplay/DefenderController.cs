using Fusion;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NetworkTransform))]
public class DefenderController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float diveSpeed = 8.0f;
    [SerializeField] private float despawnDistanceBehind = 15.0f;

    [Networked, OnChangedRender(nameof(OnDiveStateChanged))]
    public NetworkBool IsDiving { get; set; }

    [Networked] private Vector3 DiveTargetPosition { get; set; }

    private Animator animator;
    private static readonly int DiveHash = Animator.StringToHash("Dive");

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (HasStateAuthority)
        {
            IsDiving = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !MatchmakingLauncher.IsGameActive) return;

        if (!IsDiving)
        {
            transform.position += transform.forward * runSpeed * Runner.DeltaTime;
        }
        else
        {
            Vector3 directionToTarget = (DiveTargetPosition - transform.position).normalized;
            transform.position += directionToTarget * diveSpeed * Runner.DeltaTime;
        }

        CheckDespawnCondition();
    }

    public void OnPlayerDetected(Transform playerTransform)
    {
        if (!HasStateAuthority || IsDiving) return;

        IsDiving = true;
        DiveTargetPosition = playerTransform.position;
        TriggerDiveAnimation();
    }

    private void CheckDespawnCondition()
    {
        float minPlayerZ = float.MaxValue;
        bool hasActivePlayers = false;

        foreach (var player in Runner.ActivePlayers)
        {
            NetworkObject playerObj = Runner.GetPlayerObject(player);
            if (playerObj != null)
            {
                hasActivePlayers = true;
                if (playerObj.transform.position.z < minPlayerZ)
                {
                    minPlayerZ = playerObj.transform.position.z;
                }
            }
        }

        if (hasActivePlayers && transform.position.z < (minPlayerZ - despawnDistanceBehind))
        {
            Runner.Despawn(Object);
        }
    }

    private void OnDiveStateChanged()
    {
        if (IsDiving)
        {
            TriggerDiveAnimation();
        }
    }

    private void TriggerDiveAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(DiveHash);
        }
    }
}