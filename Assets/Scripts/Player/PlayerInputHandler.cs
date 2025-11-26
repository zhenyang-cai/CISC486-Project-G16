using System.Linq;
using FishNet.Object;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : NetworkBehaviour {
    public PlayerInput input;
    public bool gamepad { get; private set; }
    public bool isPaused;
    public PauseMenu _pauseMenu;

    public InputAction moveAction { get; private set; }
    public InputAction lookAction { get; private set; }
    public InputAction jumpAction { get; private set; }
    public InputAction crouchAction { get; private set; }
    public InputAction attackAction { get; private set; }
    public InputAction interactAction { get; private set; }
    public InputAction reloadAction { get; private set; }
    public InputAction menuAction { get; private set; }
    public InputAction menuUIAction { get; private set; }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            // Getting inputs 
            input.enabled = true;
            input.onControlsChanged += OnControlsChanged;

            moveAction = input.actions.FindAction("Move");
            lookAction = input.actions.FindAction("Look");
            jumpAction = input.actions.FindAction("Jump");
            crouchAction = input.actions.FindAction("Crouch");
            attackAction = input.actions.FindAction("Attack");
            interactAction = input.actions.FindAction("Interact");
            reloadAction = input.actions.FindAction("Reload");

            // Pause menu stuff
            menuAction = input.actions.FindActionMap("Player").FindAction("Menu");
            menuUIAction = input.actions.FindActionMap("UI").FindAction("Menu");

            if (_pauseMenu is null) _pauseMenu = FindFirstObjectByType<PauseMenu>();
            _pauseMenu.enabled = true;
            _pauseMenu.gameObject.GetComponent<EventSystem>().enabled = true;
            input.uiInputModule = _pauseMenu.gameObject.GetComponent<InputSystemUIInputModule>();
            
            menuAction.performed += OnPause;
            menuUIAction.performed += OnPause;
        }
        else
        {
            // disable self if not owner
            Destroy(this);
        }
    }

    void OnDestroy()
    {
        if (menuAction is not null)
            menuAction.performed -= OnPause;
        if (menuUIAction is not null)
            menuUIAction.performed -= OnPause;
    }

    void OnControlsChanged(PlayerInput input)
    {
        Debug.Log("Controls changed for player ID" + input.user.id);
        var device = input.GetDevice<Gamepad>();
        gamepad = device != null;
    }

    void OnPause(InputAction.CallbackContext ctx) {
        _pauseMenu.TogglePause();
        isPaused = _pauseMenu.paused;

        if (_pauseMenu.paused)
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
