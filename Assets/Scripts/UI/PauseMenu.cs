using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public Canvas menu;
    public bool paused = false;

    public void TogglePause()
    {
        if (!paused)
            Pause();
        else
            Resume();
    }

    public void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        menu.enabled = true;
        paused = true;

        PlayerInputHandler playerInputHandler = FindFirstObjectByType<PlayerInputHandler>();
        playerInputHandler?.ActionMapPause();
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        menu.enabled = false;
        paused = false;

        PlayerInputHandler playerInputHandler = FindFirstObjectByType<PlayerInputHandler>();
        playerInputHandler?.ActionMapResume();
    }

    public void Disconnect()
    {
        FishNet.InstanceFinder.NetworkManager.ClientManager.StopConnection();
        // _networkManager = FindFirstObjectByType<NetworkManager>();
        // _networkManager?.ClientManager.StopConnection();
    }
}