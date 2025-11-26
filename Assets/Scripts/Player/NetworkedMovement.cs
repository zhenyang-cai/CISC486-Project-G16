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
    public CharacterController characterController;
    public Camera playerCamera;
    public PlayerInput playerInputComponent;
    public PlayerInputHandler input;
    public GameObject playerMesh;

    protected Vector3 _currentVelocity;
    protected float _verticalRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public override void OnStartClient()
    {
        if (IsOwner)
        {
            playerMesh.SetActive(false);
            // playerInputComponent.enabled = true;
            // input.enabled = true;
            playerCamera.enabled = true;
            playerCamera.gameObject.GetComponent<AudioListener>().enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

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

    // Input action refs to poll for player input
    // protected void GetInputRefs()
    // {
    //     moveAction = playerInputComponent.actions.FindAction("Move");
    //     lookAction = playerInputComponent.actions.FindAction("Look");
    //     jumpAction = playerInputComponent.actions.FindAction("Jump");
    //     crouchAction = playerInputComponent.actions.FindAction("Crouch");
    // }

    // Helper function for calculating desired movement direction
    protected Vector3 GetMovementDirection()
    {      
        Vector2 moveInput = input.moveAction.ReadValue<Vector2>();
        float upInput = input.jumpAction.ReadValue<float>();
        float downInput = input.crouchAction.ReadValue<float>();

        Vector3 inputDirection = new Vector3(moveInput.x, upInput - downInput, moveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection;
    }

    protected void HandleRotation()
    {
        Vector2 lookInput = input.lookAction.ReadValue<Vector2>();

        float usedSensitivity = input.gamepad ? gamepadSensitivity : mouseSensitivity;

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
