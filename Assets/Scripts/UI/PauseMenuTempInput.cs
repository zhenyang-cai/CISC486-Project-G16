// for when no one is connected but there's no players so you can't open the menu

using FishNet.Object;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PauseMenuTempInput : MonoBehaviour
{
    public PlayerInput input;
    public PauseMenu pauseMenu;
    public InputAction menuAction { get; private set; }
    public InputAction menuUIAction { get; private set; }

    void Start()
    {
        // Pause menu stuff
        menuAction = input.actions.FindActionMap("Player").FindAction("Menu");
        menuUIAction = input.actions.FindActionMap("UI").FindAction("Menu");
        pauseMenu.enabled = true;
        pauseMenu.gameObject.GetComponent<EventSystem>().enabled = true;
        input.uiInputModule = pauseMenu.gameObject.GetComponent<InputSystemUIInputModule>();
        menuAction.performed += OnPause;
        menuUIAction.performed += OnPause;
    }

    void OnDestroy()
    {
        Debug.Log("[PauseMenuTempInput] Removing callbacks on destroy...");
        menuAction.performed -= OnPause;
        menuUIAction.performed -= OnPause;
    }

    void OnPause(InputAction.CallbackContext ctx) {
        pauseMenu.TogglePause();

        if (pauseMenu.paused)
            ActionMapPause();
        else
            ActionMapResume();
    }

    public void ActionMapPause()
    {
        input.SwitchCurrentActionMap("UI");
    }
    public void ActionMapResume()
    {
        input.SwitchCurrentActionMap("Player");
    }
}