// https://fish-networking.gitbook.io/docs/tutorials/simple/making-a-custom-player-spawner/set-player-number

using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CoopPlayerSpawner : MonoBehaviour
{
    public NetworkObject agentPrefab;
    public NetworkObject dronePrefab;
    public Transform agentSpawn;
    public Transform droneSpawn;
    
    [Header("UI")]
    // public Canvas waitingText;

    private NetworkManager _networkManager;

    private void Awake()
    {
        _networkManager = GetComponentInParent<NetworkManager>();
        if (_networkManager == null)
            _networkManager = InstanceFinder.NetworkManager;

        if (_networkManager == null)
        {
            Debug.LogWarning($"CountBasedPlayerSpawner cannot work as a NetworkManager couldn't be found.");
            return;
        }

        _networkManager.SceneManager.OnClientLoadedStartScenes += OnClientLoadedStartScenes;
    }

    private void OnDestroy()
    {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= OnClientLoadedStartScenes;
    }

    private void OnClientLoadedStartScenes(NetworkConnection _, bool asServer)
    {
        if (!asServer)
            return;

        List<NetworkConnection> authenticatedClients = _networkManager.ServerManager.Clients.Values
            .Where(conn => conn.IsAuthenticated).ToList();

        // Spawn players once there's at least two
        if (authenticatedClients.Count < 2) return;

        NetworkObject agent = Instantiate(agentPrefab);
        _networkManager.ServerManager.Spawn(agent, authenticatedClients[0]);
        agent.transform.position = agentSpawn.position;
        // If the client isn't observing this scene, make them an observer of it.
        if (!authenticatedClients[0].Scenes.Contains(gameObject.scene))
            _networkManager.SceneManager.AddOwnerToDefaultScene(agent);

        NetworkObject drone = Instantiate(dronePrefab);
        _networkManager.ServerManager.Spawn(drone, authenticatedClients[1]);
        drone.transform.position = droneSpawn.position;
        if (!authenticatedClients[1].Scenes.Contains(gameObject.scene))
            _networkManager.SceneManager.AddOwnerToDefaultScene(drone);
    }
}