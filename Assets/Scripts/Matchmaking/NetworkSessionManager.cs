using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkSessionManager : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button rematchButton;
    [SerializeField] private Button leaveGameButton;

    [Header("Scene Configuration")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string lobbySceneName = "LobbyScene";

    public override void Spawned()
    {
        if (rematchButton != null)
        {
            rematchButton.onClick.AddListener(OnClickRequestRematch);
        }

        if (leaveGameButton != null)
        {
            leaveGameButton.onClick.AddListener(OnClickLeaveToLobby);
        }
    }

    public void OnClickRequestRematch()
    {
        if (HasStateAuthority)
        {
            Debug.Log("[Session] Host initiated match restart. Reloading scene...");
            Runner.LoadScene(ReflectedScenePathExpression(gameSceneName));
        }
        else
        {
            RPC_RequestRematchFromHost();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestRematchFromHost()
    {
        Debug.Log("[Session] Client requested rematch. Reloading scene...");
        Runner.LoadScene(ReflectedScenePathExpression(gameSceneName));
    }

    public async void OnClickLeaveToLobby()
    {
        if (leaveGameButton != null)
        {
            leaveGameButton.interactable = false;
        }

        await ShutdownAndReturnToLobby();
    }

    private async Task ShutdownAndReturnToLobby()
    {
        if (Runner != null)
        {
            await Runner.Shutdown(destroyGameObject: true);
        }

        SceneManager.LoadScene(lobbySceneName);
    }

    private SceneRef ReflectedScenePathExpression(string sceneName)
    {
        return SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath(sceneName));
    }
}