using Fusion;
using UnityEngine;

public class NetworkPlayerContact : NetworkBehaviour
{
    [Networked] public ContactType CurrentContactType { get; set; }

    [Networked, OnChangedRender(nameof(OnLastContactChanged))]
    public ContactType LastReceivedContactType { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            CurrentContactType = ContactType.Normal;
            LastReceivedContactType = ContactType.Normal;
        }

        if (HasInputAuthority)
        {
            SwipeDetection.OnSwipeLeft += HandleLocalSwipeLeft;
            SwipeDetection.OnSwipeRight += HandleLocalSwipeRight;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasInputAuthority)
        {
            SwipeDetection.OnSwipeLeft -= HandleLocalSwipeLeft;
            SwipeDetection.OnSwipeRight -= HandleLocalSwipeRight;
        }
    }

    private void HandleLocalSwipeLeft()
    {
        if (!HasInputAuthority) return;
        RPC_SetContactType(ContactType.ContactType1);
    }

    private void HandleLocalSwipeRight()
    {
        if (!HasInputAuthority) return;
        RPC_SetContactType(ContactType.ContactType2);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetContactType(ContactType newType)
    {
        CurrentContactType = newType;
        Debug.Log($"[Server] Player {Object.InputAuthority} contact type updated to: {CurrentContactType}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!HasStateAuthority) return;

        if (other.TryGetComponent<NetworkPlayerContact>(out var otherPlayer))
        {
            LastReceivedContactType = otherPlayer.CurrentContactType;
            Debug.Log($"[Server Simulation] Player {Object.InputAuthority} collided with Player {otherPlayer.Object.InputAuthority}! " +
                      $"Recorded incoming Contact Type: {LastReceivedContactType}");
        }
    }

    private void OnLastContactChanged()
    {
        Debug.Log($"[Client Display - Player {Object.InputAuthority}] Last Contact Updated to: {LastReceivedContactType}");
    }
}
