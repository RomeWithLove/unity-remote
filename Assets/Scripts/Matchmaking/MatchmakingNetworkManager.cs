using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

[RequireComponent(typeof(NetworkRunner))]
public class MatchmakingNetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private MatchmakingUIController uiController;
    [SerializeField] private int maxPlayers = 4;

    private NetworkRunner _runner;
    private bool _isReady = false;

    private void Awake()
    {
        _runner = GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;
    }

    public async void StartQuickJoin()
    {
        uiController.ShowSearching("Searching for quick match...");
        await Connect(string.Empty, MatchmakingMode.FillRoom);
    }

    public async void StartFindMatch(string sessionName)
    {
        string targetSession = string.IsNullOrWhiteSpace(sessionName) ? "DefaultLobby" : sessionName.Trim();
        uiController.ShowSearching($"Finding room: {targetSession}...");
        await Connect(targetSession, MatchmakingMode.FillRoom);
    }

    public async void CancelMatchmaking()
    {
        if (_runner != null && _runner.IsRunning)
        {
            await _runner.Shutdown();
        }
        uiController.ShowMainMenu();
    }

    public async void LeaveLobby()
    {
        if (_runner != null && _runner.IsRunning)
        {
            await _runner.Shutdown();
        }
        uiController.ShowMainMenu();
    }

    public void ToggleReady()
    {
        _isReady = !_isReady;
        if (_runner.IsServer && _isReady)
        {
            RPC_StartCountdown();
        }
    }

    private async Task Connect(string sessionName, MatchmakingMode mode)
    {
        var args = new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = sessionName,
            PlayerCount = maxPlayers,
            MatchmakingMode = mode,
            SceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>() ?? gameObject.AddComponent<NetworkSceneManagerDefault>()
        };

        var result = await _runner.StartGame(args);
        if (!result.Ok)
        {
            Debug.LogError($"Failed to join match: {result.ShutdownReason}");
            uiController.ShowMainMenu();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_StartCountdown()
    {
        StartCoroutine(RunCountdownRoutine());
    }

    private System.Collections.IEnumerator RunCountdownRoutine()
    {
        uiController.ShowCountdown("Ready");
        yield return new WaitForSeconds(1.0f);
        uiController.ShowCountdown("Set");
        yield return new WaitForSeconds(1.0f);
        uiController.ShowCountdown("Go!");
        yield return new WaitForSeconds(0.5f);
        uiController.HideAllForGameplay();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        int count = 0;
        foreach (var _ in runner.ActivePlayers) count++;
        uiController.ShowLobby(runner.SessionInfo.Name, count, maxPlayers);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        int count = 0;
        foreach (var _ in runner.ActivePlayers) count++;
        uiController.UpdatePlayerCount(count, maxPlayers);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) => uiController.ShowMainMenu();
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
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