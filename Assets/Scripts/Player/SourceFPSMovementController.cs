// FPS Movement controller based on Source/Quake player movement
// with help from https://www.youtube.com/watch?v=vBWcb_0HF1c
using UnityEngine;
using UnityEngine.InputSystem;

public class SourceFPSMovementController : MonoBehaviour
{
    [Header("Source movement variables")]
    // Source movement
    [SerializeField] private bool autoBhop = false;
    [SerializeField] private float groundAccel = 20.0f;
    [SerializeField] private float airAccel = 20.0f;
    [SerializeField] private float groundSpeedMax = 10.0f; // Max player velocity on the ground.
    [SerializeField] private float airSpeedMax = 10.0f; // Max player velocity in the air.
    // [SerializeField] private float maxSpeed = 10.0f; // Max velocity.
    [SerializeField] private float groundFriction = 6.0f; // Ground friction
    [SerializeField] private float wishSpeedCap = 1.0f;

    [Header("General movement variables")]
    [SerializeField] private float jumpForce = 7.0f;
    [SerializeField] private float gravityMultiplier = 2.0f;

    [Header("Look parameters")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gamepadSensitivity = 1f;
    [SerializeField] private float verticalLookRange = 85.0f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerInput playerInputComponent;

    [HideInInspector] public InputAction moveAction;
    [HideInInspector] public InputAction lookAction;
    [HideInInspector] public InputAction jumpAction;
    [HideInInspector] public InputAction crouchAction;
    // [HideInInspector] public InputAction sprintAction;
    // [HideInInspector] public InputAction attackAction;
    // [HideInInspector] public InputAction interactAction;

    private Vector3 currentVelocity;
    private bool gamepad = false;
    private float verticalRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GetInputRefs();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    private void OnControlsChanged()
    {
        Debug.Log("Controls changed for player ID" + playerInputComponent.user.id);
        var device = playerInputComponent.GetDevice<Gamepad>();
        gamepad = device != null;
    }

    // Input action refs to poll for player input
    private void GetInputRefs()
    {
        moveAction = playerInputComponent.actions.FindAction("Move");
        lookAction = playerInputComponent.actions.FindAction("Look");
        jumpAction = playerInputComponent.actions.FindAction("Jump");
        crouchAction = playerInputComponent.actions.FindAction("Crouch");
        // sprintAction = playerInputComponent.actions.FindAction("Sprint");
        // attackAction = playerInputComponent.actions.FindAction("Attack");
        // interactAction = playerInputComponent.actions.FindAction("Interact");
    }

    // Helper function for calculating desired movement direction
    private Vector3 GetMovementDirection()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        // return worldDirection.normalized;
        return worldDirection;
    }

    private void HandleRotation()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        float usedSensitivity = gamepad ? gamepadSensitivity : mouseSensitivity;

        float mouseXRotation = lookInput.x * usedSensitivity;
        float mouseYRotation = lookInput.y * usedSensitivity;

        // X rotation
        transform.Rotate(0, mouseXRotation, 0);

        // Y rotation
        verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -verticalLookRange, verticalLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            if (autoBhop)
            {
                if (jumpAction.IsPressed())
                {
                    currentVelocity.y = jumpForce;
                }
            }
            else
            {
                if (jumpAction.WasPressedThisFrame())
                {
                    currentVelocity.y = jumpForce;
                }
            }
        }
        else
        {
            currentVelocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    private void HandleMovement()
    {
        HandleJumping();

        Vector3 worldDirection = GetMovementDirection();
        if (characterController.isGrounded)
            currentVelocity = GroundMovement(worldDirection, currentVelocity);
        else
            currentVelocity = AirMovement(worldDirection, currentVelocity);

        characterController.Move(currentVelocity * Time.deltaTime);
    }

    private Vector3 GroundMovement(Vector3 accelDirection, Vector3 prevVelocity)
    {
        // friction calculations
        Vector3 newVelocity = prevVelocity;
        float speed = newVelocity.magnitude;
        if (speed != 0)
        {
            float drop = speed * groundFriction * Time.deltaTime;
            newVelocity *= Mathf.Max(speed - drop, 0) / speed;
        }

        // Apply ground movement acceleration
        float projectedSpeed = Vector3.Dot(newVelocity, accelDirection);
        float accelSpeed = groundAccel * Time.deltaTime;

        if (projectedSpeed + accelSpeed > groundSpeedMax)
            accelSpeed = groundSpeedMax - projectedSpeed;

        return newVelocity + accelDirection * accelSpeed;
    }

    // https://youtu.be/gRqoXy-0d84?t=116
    private Vector3 AirMovement(Vector3 accelDirection, Vector3 currentVelocity)
    {
        Vector3 newVelocity = currentVelocity;

        float wishSpeed = airSpeedMax;
        if (wishSpeed != 0 && wishSpeed > wishSpeedCap)
            wishSpeed = wishSpeedCap;

        // veer amount
        float currentSpeed = Vector3.Dot(currentVelocity, accelDirection);
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0)
            return newVelocity;

        float accelSpeed = airSpeedMax * airAccel * Time.deltaTime;
        if (accelSpeed > addSpeed)
            accelSpeed = addSpeed;

        return newVelocity + accelDirection * accelSpeed;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // When colliding with a wall, slide velocity along it
        if ((characterController.collisionFlags & CollisionFlags.Sides) != 0)
        {
            // Getting projected velocity
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(currentVelocity, hit.normal);
            currentVelocity = projectedVelocity;
            // Debug.DrawRay(hit.point, projectedVelocity, Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f), 10f);
        }
    }
}