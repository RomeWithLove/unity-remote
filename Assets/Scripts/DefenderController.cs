using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class DefenderController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float diveSpeed = 8.0f;
    [SerializeField] private float despawnDistanceBehind = 15.0f;

    public bool IsDiving { get; private set; } = false;

    private Vector3 diveTargetPosition;
    private Animator animator;
    private Transform playerTransform;
    private static readonly int DiveHash = Animator.StringToHash("Dive");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        IsDiving = false;
        FindPlayer();
    }

    private void Update()
    {
        if (!IsDiving)
        {
            transform.position += transform.forward * runSpeed * Time.deltaTime;
        }
        else
        {
            Vector3 dir = (diveTargetPosition - transform.position).normalized;
            transform.position += dir * diveSpeed * Time.deltaTime;
        }

        CheckDespawn();
    }

    public void OnPlayerDetected(Transform target)
    {
        if (IsDiving) return;

        IsDiving = true;
        diveTargetPosition = target.position;

        if (animator != null)
        {
            animator.SetTrigger(DiveHash);
        }
    }

    private void CheckDespawn()
    {
        if (playerTransform != null && transform.position.z < (playerTransform.position.z - despawnDistanceBehind))
        {
            Destroy(gameObject);
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }
}