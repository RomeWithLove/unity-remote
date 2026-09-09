using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchmakingUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup searchingPanel;
    [SerializeField] private CanvasGroup lobbyWaitingPanel;

    [Header("Main Menu Controls")]
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button findMatchButton;
    [SerializeField] private TMP_InputField customRoomInput;

    [Header("Searching Controls")]
    [SerializeField] private TextMeshProUGUI searchingStatusText;
    [SerializeField] private Button cancelSearchButton;

    [Header("Lobby Waiting Controls")]
    [SerializeField] private TextMeshProUGUI lobbyTitleText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveLobbyButton;

    [Header("Game HUD")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Logic Binding")]
    [SerializeField] private MatchmakingNetworkManager networkManager;

    private void Awake()
    {
        quickJoinButton.onClick.AddListener(OnQuickJoinClicked);
        findMatchButton.onClick.AddListener(OnFindMatchClicked);
        cancelSearchButton.onClick.AddListener(OnCancelClicked);
        leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClicked);
        readyButton.onClick.AddListener(OnReadyClicked);

        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetPanelState(mainMenuPanel, true);
        SetPanelState(searchingPanel, false);
        SetPanelState(lobbyWaitingPanel, false);
        countdownText.gameObject.SetActive(false);
    }

    public void ShowSearching(string message)
    {
        SetPanelState(mainMenuPanel, false);
        SetPanelState(searchingPanel, true);
        SetPanelState(lobbyWaitingPanel, false);
        searchingStatusText.text = message;
    }

    public void ShowLobby(string roomName, int currentPlayers, int maxPlayers)
    {
        SetPanelState(mainMenuPanel, false);
        SetPanelState(searchingPanel, false);
        SetPanelState(lobbyWaitingPanel, true);
        lobbyTitleText.text = $"Lobby: {roomName}";
        UpdatePlayerCount(currentPlayers, maxPlayers);
    }

    public void UpdatePlayerCount(int currentPlayers, int maxPlayers)
    {
        playerCountText.text = $"Players: {currentPlayers} / {maxPlayers}";
    }

    public void ShowCountdown(string text)
    {
        SetPanelState(lobbyWaitingPanel, false);
        countdownText.gameObject.SetActive(true);
        countdownText.text = text;
    }

    public void HideAllForGameplay()
    {
        SetPanelState(mainMenuPanel, false);
        SetPanelState(searchingPanel, false);
        SetPanelState(lobbyWaitingPanel, false);
        countdownText.gameObject.SetActive(false);
    }

    private void SetPanelState(CanvasGroup group, bool active)
    {
        group.alpha = active ? 1f : 0f;
        group.interactable = active;
        group.blocksRaycasts = active;
    }

    private void OnQuickJoinClicked() => networkManager.StartQuickJoin();
    private void OnFindMatchClicked() => networkManager.StartFindMatch(customRoomInput.text);
    private void OnCancelClicked() => networkManager.CancelMatchmaking();
    private void OnLeaveLobbyClicked() => networkManager.LeaveLobby();
    private void OnReadyClicked() => networkManager.ToggleReady();
}
