using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class NetworkedMovement : NetworkBehaviour
{
    [Header("Look parameters")]
    public float mouseSensitivity = 0.1f;
    public float gamepadSensitivity = 1f;
    public float verticalLookRange = 85.0f;

    [Header("References")]
    [SerializeField] protected CharacterController characterController;
    [SerializeField] protected Camera playerCamera;
    [SerializeField] protected PlayerInput playerInputComponent;
    [SerializeField] protected GameObject playerMesh;
    [SerializeField] protected PauseMenu pauseController;

    protected InputAction moveAction;
    protected InputAction lookAction;
    protected InputAction jumpAction;
    protected InputAction crouchAction;

    protected Vector3 _currentVelocity;
    protected bool _gamepad = false;
    protected float _verticalRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GetInputRefs();
    }

    public override void OnStartClient()
    {
        if (IsOwner)
        {
            playerMesh.SetActive(false);
            playerInputComponent.enabled = true;
            playerCamera.enabled = true;
            pauseController.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            GetInputRefs();

            OnStartExtra();
        }
    }

    protected abstract void OnStartExtra();
    
    // Update is called once per frame
    protected void Update()
    {
        if (!IsOwner) return;
        
        HandleRotation();
        HandleMovement();

        if (characterController.enabled)
            characterController.Move(_currentVelocity * Time.deltaTime);
    }

    protected abstract void HandleMovement();

    // Check for gamepad usage
    void OnControlsChanged()
    {
        Debug.Log("Controls changed for player ID" + playerInputComponent.user.id);
        var device = playerInputComponent.GetDevice<Gamepad>();
        _gamepad = device != null;
    }

    // Input action refs to poll for player input
    protected void GetInputRefs()
    {
        moveAction = playerInputComponent.actions.FindAction("Move");
        lookAction = playerInputComponent.actions.FindAction("Look");
        jumpAction = playerInputComponent.actions.FindAction("Jump");
        crouchAction = playerInputComponent.actions.FindAction("Crouch");
        // attackAction = playerInputComponent.actions.FindAction("Attack");
        // interactAction = playerInputComponent.actions.FindAction("Interact");
    }

    // Helper function for calculating desired movement direction
    protected Vector3 GetMovementDirection()
    {
        if (pauseController.paused)
            return Vector3.zero;
        
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float upInput = jumpAction.ReadValue<float>();
        float downInput = crouchAction.ReadValue<float>();

        Vector3 inputDirection = new Vector3(moveInput.x, upInput - downInput, moveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection;
    }

    protected void HandleRotation()
    {
        if (pauseController.paused)
            return;
        
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float usedSensitivity = _gamepad ? gamepadSensitivity : mouseSensitivity;

        float mouseXRotation = lookInput.x * usedSensitivity;
        float mouseYRotation = lookInput.y * usedSensitivity;

        // X rotation
        transform.Rotate(0, mouseXRotation, 0);

        // Y rotation
        _verticalRotation = Mathf.Clamp(_verticalRotation - mouseYRotation, -verticalLookRange, verticalLookRange);
        playerCamera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // When colliding with a wall, slide velocity along it
        if ((characterController.collisionFlags & CollisionFlags.Sides) != 0)
        {
            // Getting projected velocity
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(_currentVelocity, hit.normal);
            _currentVelocity = projectedVelocity;
            // Debug.DrawRay(hit.point, projectedVelocity, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
        }
    }
}
