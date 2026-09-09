using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using Fusion.Sockets;

[RequireComponent(typeof(NetworkRunner))]
public class MatchmakingLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network Prefabs")]
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [SerializeField] private NetworkPrefabRef ballPrefab;
    [SerializeField] private Vector3 ballInitialSpawnPosition = new Vector3(0f, 0.5f, 15f);

    [Header("UI References")]
    [SerializeField] private Button joinGameButton;
    [SerializeField] private TextMeshProUGUI countdownText;

    private NetworkRunner _runner;

    [Header("Match Settings")]
    [SerializeField] private int requiredPlayerCount = 4;

    private bool _isCountdownRunning = false;
    private float _countdownRemaining = 3.0f;
    private bool _gameStarted = false;

    public static bool IsGameActive { get; private set; } = false;

    private void Awake()
    {
        _runner = GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;
    }

    private void Update()
    {
        if (_isCountdownRunning && !_gameStarted)
        {
            _countdownRemaining -= Time.deltaTime;
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                if (_countdownRemaining > 2.0f) { countdownText.text = "Ready"; }
                else if (_countdownRemaining > 1.0f) { countdownText.text = "Set"; }
                else if (_countdownRemaining > 0.0f) { countdownText.text = "Go!"; }
                else
                {
                    countdownText.gameObject.SetActive(false);
                    _gameStarted = true;
                    _isCountdownRunning = false;
                    IsGameActive = true;
                    if (AudioManager.Instance != null) { AudioManager.Instance.PlayMatchStartCheer(); }
                    if (_runner != null && _runner.IsServer) { SpawnMatchBall(); }
                }
            }
        }
    }

    public async void OnClickJoinGame()
    {
        if (joinGameButton != null) { joinGameButton.interactable = false; }
        await StartMatchmaking();
    }

    private async Task StartMatchmaking()
    {
        var startGameArgs = new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = string.Empty,
            PlayerCount = requiredPlayerCount,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            ObjectProvider = GetComponent<PooledNetworkObjectProvider>() ?? gameObject.AddComponent<PooledNetworkObjectProvider>()
        };

        Debug.Log("[Matchmaking] Searching for a game or hosting a new room...");
        var result = await _runner.StartGame(startGameArgs);
        if (!result.Ok)
        {
            Debug.LogError($"[Matchmaking] Failed to start game: {result.ShutdownReason}");
            if (joinGameButton != null) { joinGameButton.interactable = true; }
        }
    }

    private void SpawnMatchBall()
    {
        if (ballPrefab.IsValid)
        {
            Debug.Log("[Matchmaking] Spawning authoritative NetworkBall ahead of runners...");
            _runner.Spawn(ballPrefab, ballInitialSpawnPosition, Quaternion.identity, PlayerRef.None);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartMatchCountdown()
    {
        Debug.Log("[Matchmaking] All players connected. Starting 3-second match countdown...");
        _countdownRemaining = 3.0f;
        _isCountdownRunning = true;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Debug.Log($"[Matchmaking] Player {player} connected. Spawning runner5 prefab...");
            Vector3 spawnPosition = new Vector3((player.RawEncoded % 4) * 2f - 3f, 0f, 0f);
            runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);

            int currentConnectedPlayers = 0;
            foreach (var _ in runner.ActivePlayers) { currentConnectedPlayers++; }

            if (currentConnectedPlayers >= requiredPlayerCount && !_isCountdownRunning && !_gameStarted)
            {
                RPC_StartMatchCountdown();
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}