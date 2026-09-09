using System.Collections;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCollisionHandler : NetworkBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 6.0f;
    [SerializeField] private float stumbleDuration = 0.8f;

    [Networked, OnChangedRender(nameof(OnStumbleStateChanged))]
    public NetworkBool IsStumbling { get; set; }

    private Animator animator;
    private RunnerPlayer runnerPlayer;
    private static readonly int StumbleHash = Animator.StringToHash("Stumble");

    public override void Spawned()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        runnerPlayer = GetComponent<RunnerPlayer>();

        if (HasStateAuthority)
        {
            IsStumbling = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!HasStateAuthority) return;

        if (collision.gameObject.CompareTag("Defender") && !IsStumbling)
        {
            Vector3 pushDirection = -transform.forward;
            StartCoroutine(ExecuteStumbleRoutine(pushDirection));

            if (Object.HasInputAuthority && CameraFollow.Instance != null)
            {
                CameraFollow.Instance.TriggerShake(duration: 0.3f, magnitude: 0.5f);
            }
        }
    }

    private IEnumerator ExecuteStumbleRoutine(Vector3 pushDirection)
    {
        IsStumbling = true;

        if (runnerPlayer != null)
        {
            runnerPlayer.ApplySlowdown(stumbleDuration);
        }

        float elapsed = 0f;
        while (elapsed < stumbleDuration)
        {
            transform.position += pushDirection * knockbackForce * Runner.DeltaTime;
            elapsed += Runner.DeltaTime;
            yield return null;
        }

        IsStumbling = false;
    }

    private void OnStumbleStateChanged()
    {
        if (IsStumbling && animator != null)
        {
            animator.SetTrigger(StumbleHash);
        }
    }
}
