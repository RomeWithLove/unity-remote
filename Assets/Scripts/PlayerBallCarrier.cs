using UnityEngine;

public class PlayerBallCarrier : MonoBehaviour
{
    [Header("Ball Visual Attachment")]
    [SerializeField] private GameObject carriedBallMesh;

    public bool HasBall { get; private set; } = false;

    private void Awake()
    {
        if (carriedBallMesh == null && transform.Find("CarriedBallMesh") != null)
        {
            carriedBallMesh = transform.Find("CarriedBallMesh").gameObject;
        }
        UpdateVisual();
    }

    public void AssignBall(bool state)
    {
        HasBall = state;
        UpdateVisual();
    }

    public void Fumble()
    {
        if (!HasBall) return;

        HasBall = false;
        UpdateVisual();

        if (SingleBall.Instance != null)
        {
            SingleBall.Instance.LaunchFromPlayer(transform.position);
        }
    }

    private void UpdateVisual()
    {
        if (carriedBallMesh != null)
        {
            carriedBallMesh.SetActive(HasBall);
        }
    }
}