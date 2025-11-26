// https://fish-networking.gitbook.io/docs/tutorials/simple/starting-fishnets-connections

using FishNet.Managing;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (networkManager is null) networkManager = FindFirstObjectByType<NetworkManager>();
    }

    // A host is simply a server and a client, so start them both.
    public void StartHost()
    {
        StartServer();
        StartClient();
    }

    // The server can be started directly from the ServerManager or Transport
    public void StartServer()
    {
        networkManager.ServerManager.StartConnection();
    }

    // The client can be started directly from the ClientManager or Transport
    public void StartClient()
    {
        networkManager.ClientManager.StartConnection();
    }

    // This is set on the Transport to indicate where the client should connect.
    public void SetIPAddress(string text)
    {
        networkManager.TransportManager.Transport.SetClientAddress(text);
    }

    // public void LoadScene()
    // {
    //     // https://fish-networking.gitbook.io/docs/guides/features/scene-management/loading-scenes
    //     SceneLoadData sld = new SceneLoadData(gameScene);
    // }

    // This is the easiest place to put this
    public void QuitGame()
    {
        Application.Quit();
    }
}
