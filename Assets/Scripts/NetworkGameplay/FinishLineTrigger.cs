using Fusion;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(NetworkObject))]
public class FinishLineTrigger : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI winnerText;

    [Networked, OnChangedRender(nameof(OnMatchFinishedChanged))]
    public NetworkBool IsMatchFinished { get; set; }

    [Networked] public PlayerRef WinningPlayer { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            IsMatchFinished = false;
            WinningPlayer = PlayerRef.None;
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority || IsMatchFinished) return;

        if (other.TryGetComponent<RunnerPlayer>(out var player))
        {
            IsMatchFinished = true;
            WinningPlayer = player.Object.InputAuthority;
            Debug.Log($"[FinishLine] Player {WinningPlayer} crossed first! Match Finished.");
        }
    }

    private void OnMatchFinishedChanged()
    {
        if (IsMatchFinished)
        {
            DisplayGameOverUI();
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVictoryCheer();
            }
        }
    }

    private void DisplayGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (winnerText != null)
        {
            if (Runner.LocalPlayer == WinningPlayer)
            {
                winnerText.text = "Victory! You Won!";
            }
            else
            {
                winnerText.text = $"Game Over! Player {WinningPlayer.RawEncoded + 1} Won!";
            }
        }
    }
}
