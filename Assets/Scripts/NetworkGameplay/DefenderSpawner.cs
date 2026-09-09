using Fusion;
using UnityEngine;

public class DefenderSpawner : NetworkBehaviour
{
    [Header("Prefab Reference")]
    [SerializeField] private NetworkPrefabRef defenderPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3.5f;
    [SerializeField] private float minSpawnDistanceZ = 25f;
    [SerializeField] private float maxSpawnDistanceZ = 40f;
    [SerializeField] private float[] lanePositionsX = new float[] { -2.5f, 0f, 2.5f };

    [Networked] private TickTimer SpawnTimer { get; set; }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            SpawnTimer = TickTimer.CreateFromSeconds(Runner, spawnInterval);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || !MatchmakingLauncher.IsGameActive) return;

        if (SpawnTimer.Expired(Runner))
        {
            SpawnDefenderAheadOfLeadPlayer();
            SpawnTimer = TickTimer.CreateFromSeconds(Runner, spawnInterval);
        }
    }

    private void SpawnDefenderAheadOfLeadPlayer()
    {
        Transform leadPlayer = GetLeadPlayerTransform();
        if (leadPlayer == null) return;

        float randomX = lanePositionsX[UnityEngine.Random.Range(0, lanePositionsX.Length)];
        float randomZOffset = UnityEngine.Random.Range(minSpawnDistanceZ, maxSpawnDistanceZ);

        Vector3 spawnPosition = new Vector3(
            randomX,
            leadPlayer.position.y,
            leadPlayer.position.z + randomZOffset
        );

        Quaternion spawnRotation = Quaternion.Euler(0f, 180f, 0f);

        if (defenderPrefab.IsValid)
        {
            Runner.Spawn(defenderPrefab, spawnPosition, spawnRotation, PlayerRef.None);
        }
    }

    private Transform GetLeadPlayerTransform()
    {
        Transform lead = null;
        float maxZ = float.MinValue;

        foreach (var player in Runner.ActivePlayers)
        {
            NetworkObject playerObj = Runner.GetPlayerObject(player);
            if (playerObj != null && playerObj.transform.position.z > maxZ)
            {
                maxZ = playerObj.transform.position.z;
                lead = playerObj.transform;
            }
        }

        return lead;
    }
}
