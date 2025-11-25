// https://fish-networking.gitbook.io/docs/tutorials/simple/making-a-custom-player-spawner/set-player-number

using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Transporting;
using NUnit.Framework;
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
        _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionStateChanged;
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
        agent.transform.position = agentSpawn.position;
        _networkManager.ServerManager.Spawn(agent, authenticatedClients[0]);
        // If the client isn't observing this scene, make them an observer of it.
        if (!authenticatedClients[0].Scenes.Contains(gameObject.scene))
            _networkManager.SceneManager.AddOwnerToDefaultScene(agent);

        NetworkObject drone = Instantiate(dronePrefab);
        drone.transform.position = droneSpawn.position;
        _networkManager.ServerManager.Spawn(drone, authenticatedClients[1]);
        if (!authenticatedClients[1].Scenes.Contains(gameObject.scene))
            _networkManager.SceneManager.AddOwnerToDefaultScene(drone);
    }
    
    private void OnRemoteConnectionStateChanged(NetworkConnection nc, RemoteConnectionStateArgs args)
    {
        if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            Debug.Log($"[CoopPlayerSpawner] Client disconnected. Closing server...");
            if (_networkManager.IsServerStarted)
                _networkManager.ServerManager.StopConnection(true); // disconnect everyone
        }
    }
}