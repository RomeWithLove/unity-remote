using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 6.0f;
    [SerializeField] private float stumbleDuration = 0.8f;
    [SerializeField] private float immunityDuration = 1.2f;

    public bool IsStumbling { get; private set; }
    private bool isImmune;

    private Animator animator;
    private SinglePlayerController playerController;
    private static readonly int StumbleHash = Animator.StringToHash("Stumble");

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<SinglePlayerController>();

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isImmune || IsStumbling) return;

        if (collision.gameObject.CompareTag("Defender"))
        {
            StartCoroutine(ExecuteStumbleRoutine(-transform.forward));

            if (CameraFollow.Instance != null)
            {
                CameraFollow.Instance.TriggerShake(0.3f, 0.5f);
            }
        }
    }

    private IEnumerator ExecuteStumbleRoutine(Vector3 pushDirection)
    {
        IsStumbling = true;
        isImmune = true;

        if (playerController != null)
        {
            playerController.CancelLateralSlide();
            playerController.ApplySlowdown(stumbleDuration);
        }

        if (animator != null) animator.SetTrigger(StumbleHash);

        float elapsed = 0f;
        while (elapsed < stumbleDuration)
        {
            transform.position += pushDirection * knockbackForce * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        IsStumbling = false;

        yield return new WaitForSeconds(immunityDuration);
        isImmune = false;
    }
}