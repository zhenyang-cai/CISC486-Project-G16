using UnityEngine;
using UnityEngine.InputSystem;

public class DroneMovement : MonoBehaviour
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
    public CharacterController characterController;
    public Camera mainCamera;
    // [SerializeField] private PlayerInput playerInputComponent;
    public PlayerInputHandler input;

    private Vector3 currentVelocity;
    // private bool gamepad = false;
    private float verticalRotation;

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
    }

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
        HandleMovement();
    }

    // Helper function for calculating desired movement direction
    private Vector3 GetMovementDirection()
    {
        Vector2 moveInput = input.moveAction.ReadValue<Vector2>();
        float upInput = input.jumpAction.ReadValue<float>();
        float downInput = input.crouchAction.ReadValue<float>();

        Vector3 inputDirection = new Vector3(moveInput.x, upInput - downInput, moveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        return worldDirection;
    }

    private void HandleRotation()
    {
        Vector2 lookInput = input.lookAction.ReadValue<Vector2>();

        float usedSensitivity = input.gamepad ? gamepadSensitivity : mouseSensitivity;

        float mouseXRotation = lookInput.x * usedSensitivity;
        float mouseYRotation = lookInput.y * usedSensitivity;

        // X rotation
        transform.Rotate(0, mouseXRotation, 0);

        // Y rotation
        verticalRotation = Mathf.Clamp(verticalRotation - mouseYRotation, -verticalLookRange, verticalLookRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleMovement()
    {
        Vector3 desiredMovement = GetMovementDirection();

        // Vertical movement
        if (desiredMovement.y == 0)
        {
            // slow down if not actively moving
            currentVelocity.y = approachZero(currentVelocity.y, airVerticalDecel * Time.deltaTime);
        }
        else
        {
            currentVelocity.y += desiredMovement.y * airVerticalAccel * Time.deltaTime;
            // clamp vertical speed
            currentVelocity.y = Mathf.Clamp(currentVelocity.y, -airVerticalMaxSpeed, airVerticalMaxSpeed);
        }

        // Horizontal movement
        if (desiredMovement.x == 0 && desiredMovement.z == 0)
        {
            // slow down if not actively moving
            Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            float newSpeed = approachZero(currentHorizontal.magnitude, airDecel * Time.deltaTime);

            Vector3 newHorizontal = currentHorizontal.normalized * newSpeed;
            currentVelocity.x = newHorizontal.x;
            currentVelocity.z = newHorizontal.z;
        }
        else
        {
            Vector3 desiredHorizonal = new Vector3(desiredMovement.x, 0, desiredMovement.z);
            Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0, currentVelocity.z);

            currentHorizontal += desiredHorizonal * airAccel * Time.deltaTime;
            currentHorizontal = currentHorizontal.normalized * Mathf.Clamp(currentHorizontal.magnitude, -airMaxSpeed, airMaxSpeed);

            currentVelocity.x = currentHorizontal.x;
            currentVelocity.z = currentHorizontal.z;
        }

        characterController.Move(currentVelocity * Time.deltaTime);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // When colliding with a wall, slide velocity along it
        if ((characterController.collisionFlags & CollisionFlags.Sides) != 0)
        {
            // Getting projected velocity
            Vector3 projectedVelocity = Vector3.ProjectOnPlane(currentVelocity, hit.normal);
            currentVelocity = projectedVelocity;
        }
    }
}
