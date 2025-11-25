using System;
using FishNet.Managing;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public Canvas menu;
    public bool paused = false;
    public NetworkManager networkManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                Resume();
            } 
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                menu.enabled = true;
                paused = true;                
            }
        }
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        menu.enabled = false;
        paused = false;
    }

    public void Disconnect()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        networkManager?.ClientManager.StopConnection();
    }
}