// https://fish-networking.gitbook.io/docs/tutorials/simple/making-a-custom-player-spawner/set-player-number

using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CoopPlayerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkObject agentPrefab;
    [SerializeField] private NetworkObject dronePrefab;
    // [SerializeField] private int requiredPlayerCount;
    
    private NetworkManager networkManager;

    private void Awake()
    {
        networkManager = GetComponentInParent<NetworkManager>();
        if (networkManager == null)
            networkManager = InstanceFinder.NetworkManager;

        if (networkManager == null)
        {
            Debug.LogWarning($"CountBasedPlayerSpawner cannot work as a NetworkManager couldn't be found.");
            return;
        }

        networkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    private void OnDestroy()
    {
        if (networkManager != null)
            networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
    }

    private void OnClientLoadedStartScenes(NetworkConnection _, bool asServer)
    {
        if (!asServer)
            return;

        List<NetworkConnection> authenticatedClients = networkManager.ServerManager.Clients.Values
            .Where(conn => conn.IsAuthenticated).ToList();

        if (authenticatedClients.Count < 2) return;

        NetworkObject agent = Instantiate(agentPrefab);
        networkManager.ServerManager.Spawn(agent, authenticatedClients[0]);
        // If the client isn't observing this scene, make them an observer of it.
        if (!authenticatedClients[0].Scenes.Contains(gameObject.scene))
            networkManager.SceneManager.AddOwnerToDefaultScene(agent);

        NetworkObject drone = Instantiate(dronePrefab);
        networkManager.ServerManager.Spawn(drone, authenticatedClients[1]);
        if (!authenticatedClients[1].Scenes.Contains(gameObject.scene))
            networkManager.SceneManager.AddOwnerToDefaultScene(drone);
    }
}