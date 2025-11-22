using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class DroneMovementNetworked : NetworkBehaviour
{
    [Header("General movement variables")]
    public float airAccel = 5f;
    public float airDecel = 2.5f;
    public float airMaxSpeed = 10f;
    public float airVerticalAccel = 5f;
    public float airVerticalDecel = 10f;
    public float airVerticalMaxSpeed = 5f;

    [Header("Look parameters")]
    public float mouseSensitivity = 0.1f;
    public float gamepadSensitivity = 1f;
    public float verticalLookRange = 85.0f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInput playerInputComponent;
    [SerializeField] private GameObject playerMesh;
    [SerializeField] private PauseMenu pauseController;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction ascendAction;
    private InputAction descendAction;
    // [HideInInspector] public InputAction attackAction;
    // [HideInInspector] public InputAction interactAction;

    private Vector3 _currentVelocity;
    private bool _gamepad = false;
    private float _verticalRotation;

    // Given a value, attempt to change it by a given amount towards zero.
    private float approachZero(float value, float change)
    {
        return value > 0 ? Mathf.Max(0, value - change) : Mathf.Min(0, value + change);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        
        HandleRotation();
        HandleMovement();

        if (characterController.enabled)
            characterController.Move(_currentVelocity * Time.deltaTime);
    }

    // Check for gamepad usage
    private void OnControlsChanged()
    {
        Debug.Log("Controls changed for player ID" + playerInputComponent.user.id);
        var device = playerInputComponent.GetDevice<Gamepad>();
        _gamepad = device != null;
    }

    // Input action refs to poll for player input
    private void GetInputRefs()
    {
        moveAction = playerInputComponent.actions.FindAction("Move");
        lookAction = playerInputComponent.actions.FindAction("Look");
        ascendAction = playerInputComponent.actions.FindAction("Jump");
        descendAction = playerInputComponent.actions.FindAction("Crouch");
        // attackAction = playerInputComponent.actions.FindAction("Attack");
        // interactAction = playerInputComponent.actions.FindAction("Interact");
    }

    // Helper function for calculating desired movement direction
    private Vector3 GetMovementDirection()
    {
        if (pauseController.paused)
            return Vector3.zero;
        
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float upInput = ascendAction.ReadValue<float>();
        float downInput = descendAction.ReadValue<float>();

        Vector3 inputDirection = new Vector3(moveInput.x, upInput - downInput, moveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection;
    }

    private void HandleRotation()
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

    private void HandleMovement()
    {
        Vector3 desiredMovement = GetMovementDirection();

        // Vertical movement
        if (desiredMovement.y == 0)
        {
            // slow down if not actively moving
            _currentVelocity.y = approachZero(_currentVelocity.y, airVerticalDecel * Time.deltaTime);
        }
        else
        {
            _currentVelocity.y += desiredMovement.y * airVerticalAccel * Time.deltaTime;
            // clamp vertical speed
            _currentVelocity.y = Mathf.Clamp(_currentVelocity.y, -airVerticalMaxSpeed, airVerticalMaxSpeed);
        }

        // Horizontal movement
        if (desiredMovement.x == 0 && desiredMovement.z == 0)
        {
            // slow down if not actively moving
            Vector3 currentHorizontal = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);
            float newSpeed = approachZero(currentHorizontal.magnitude, airDecel * Time.deltaTime);

            Vector3 newHorizontal = currentHorizontal.normalized * newSpeed;
            _currentVelocity.x = newHorizontal.x;
            _currentVelocity.z = newHorizontal.z;
        }
        else
        {
            Vector3 desiredHorizonal = new Vector3(desiredMovement.x, 0, desiredMovement.z);
            Vector3 currentHorizontal = new Vector3(_currentVelocity.x, 0, _currentVelocity.z);

            currentHorizontal += desiredHorizonal * airAccel * Time.deltaTime;
            currentHorizontal = currentHorizontal.normalized * Mathf.Clamp(currentHorizontal.magnitude, -airMaxSpeed, airMaxSpeed);

            _currentVelocity.x = currentHorizontal.x;
            _currentVelocity.z = currentHorizontal.z;
        }

        characterController.Move(_currentVelocity * Time.deltaTime);
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
