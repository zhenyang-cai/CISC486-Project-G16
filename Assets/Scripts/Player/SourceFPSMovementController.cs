// FPS Movement controller based on Source/Quake player movement
// with help from https://www.youtube.com/watch?v=vBWcb_0HF1c
using UnityEngine;
using UnityEngine.InputSystem;

public class SourceFPSMovementController : MonoBehaviour
{
    public Vector3 currentVelocity { get; private set; }
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
    [SerializeField] private float walkHeight = 2.0f;
    [SerializeField] private float crouchHeight = 1.0f;
    [SerializeField] private float crouchAccel = 5.0f; // Accel while crouching
    [SerializeField] private float crouchSpeedMax = 5.0f; // Max crouching speed
    [SerializeField] private float crouchSmoothingSpeed = 3.0f; // How quick the state transition is
    [SerializeField] private float uncrouchObstacleTolerance = 0.1f; // How much space above head should there be

    [Header("Look parameters")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float gamepadSensitivity = 1f;
    [SerializeField] private float verticalLookRange = 85.0f;
    [SerializeField] private float cameraHeight = 1.6f;
    [SerializeField] private float crouchCameraHeight = 0.8f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInput playerInputComponent;

    [HideInInspector] public InputAction moveAction;
    [HideInInspector] public InputAction lookAction;
    [HideInInspector] public InputAction jumpAction;
    [HideInInspector] public InputAction crouchAction;
    // [HideInInspector] public InputAction sprintAction;
    // [HideInInspector] public InputAction attackAction;
    // [HideInInspector] public InputAction interactAction;

    // Private variables

    private float verticalRotation;
    private float currentGroundSpeedMax;
    private float currentGroundAccel;
    private bool gamepad = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentGroundSpeedMax = groundSpeedMax;
        currentGroundAccel = groundAccel;

        GetInputRefs();
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleCrouching();
        HandleMovement();

        characterController.Move(currentVelocity * Time.deltaTime);
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
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            if (autoBhop)
            {
                if (jumpAction.IsPressed())
                {
                    currentVelocity = new Vector3(currentVelocity.x, jumpForce, currentVelocity.z);
                    // currentVelocity.y = jumpForce;
                }
            }
            else
            {
                if (jumpAction.WasPressedThisFrame())
                {
                    currentVelocity = new Vector3(currentVelocity.x, jumpForce, currentVelocity.z);
                    // currentVelocity.y = jumpForce;
                }
            }
        }
        else
        {
            currentVelocity = new Vector3(currentVelocity.x, currentVelocity.y + Physics.gravity.y * gravityMultiplier * Time.deltaTime, currentVelocity.z);
            // currentVelocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
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
        float accelSpeed = currentGroundAccel * Time.deltaTime;

        if (projectedSpeed + accelSpeed > currentGroundSpeedMax)
            accelSpeed = currentGroundSpeedMax - projectedSpeed;

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

    // Handle crouching
    private void HandleCrouching()
    {
        float crouchState = crouchAction.ReadValue<float>();

        // Vector3 currentPosition = characterController.center;

        if (crouchState == 1f)
        {
            currentGroundSpeedMax = crouchSpeedMax;
            currentGroundAccel = crouchAccel;

            // change height and adjust position
            // characterController.height = crouchHeight;
            characterController.height = Mathf.LerpUnclamped(characterController.height, crouchHeight, crouchSmoothingSpeed * Time.deltaTime);
            // characterController.center = new Vector3(characterController.center.x, characterController.height / 2, characterController.center.z);
            // playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, cameraHeight * (characterController.height / walkHeight), playerCamera.transform.localPosition.z);
        }
        else
        {
            if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + characterController.height / 2, transform.position.z), Vector3.up, walkHeight - characterController.center.y + uncrouchObstacleTolerance))
            {
                currentGroundSpeedMax = groundSpeedMax;
                currentGroundAccel = groundAccel;

                // change height and adjust position
                characterController.height = Mathf.LerpUnclamped(characterController.height, walkHeight, crouchSmoothingSpeed * Time.deltaTime);
                // characterController.center = new Vector3(characterController.center.x, characterController.height / 2, characterController.center.z);
                // playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, cameraHeight * (characterController.height / walkHeight), playerCamera.transform.localPosition.z);
            }
        }
        characterController.center = new Vector3(characterController.center.x, characterController.height / 2, characterController.center.z);
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, cameraHeight * (characterController.height / walkHeight), playerCamera.transform.localPosition.z);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 oldVelocity = currentVelocity;
        // When colliding with a roof, reflect the player's velocity away
        if ((characterController.collisionFlags & CollisionFlags.Above) != 0 && !characterController.isGrounded)
        {
            // For whatever reason it's double counting collisions w/ ceilings
            // quick hack: the reflection should go away from the collision, otherwise it's invalid
            Vector3 reflectedVelocity = Vector3.Reflect(currentVelocity, hit.normal);
            if (Vector3.Dot(reflectedVelocity, hit.normal) >= 0)
            {
                currentVelocity = reflectedVelocity * 0.5f;
            }
            // Debug.DrawRay(hit.point, currentVelocity, new Color(0f, 1f, 0f), 10f);
            // Debug.DrawRay(hit.point, hit.normal, new Color(1f, 1f, 1f), 10f);
            // Debug.DrawRay(hit.point, oldVelocity, new Color(1f, 0f, 0f), 10f);
        }
        // When colliding with a wall, slide velocity along it
        else if ((characterController.collisionFlags & CollisionFlags.Sides) != 0)
        {
            // Getting projected velocity
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(currentVelocity, hit.normal);
            currentVelocity = projectedVelocity;
        }

    }
}