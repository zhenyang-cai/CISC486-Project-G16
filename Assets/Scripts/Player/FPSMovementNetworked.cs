using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSMovementNetworked : NetworkBehaviour
{
    public Vector3 currentVelocity { get; private set; }
    [Header("Source movement variables")]
    // Source movement
    public bool autoBhop = false;
    public float groundAccel = 20.0f;
    public float airAccel = 20.0f;
    public float groundSpeedMax = 10.0f; // Max player velocity on the ground.
    public float airSpeedMax = 10.0f; // Max player velocity in the air.
    public float groundFriction = 6.0f; // Ground friction
    public float wishSpeedCap = 1.0f;

    [Header("General movement variables")]
    public float jumpForce = 7.0f;
    public float gravityMultiplier = 2.0f;
    public float walkHeight = 2.0f;
    public float crouchHeight = 1.0f;
    public float crouchAccel = 5.0f; // Accel while crouching
    public float crouchSpeedMax = 5.0f; // Max crouching speed
    public float crouchSmoothingSpeed = 3.0f; // How quick the state transition is
    public float uncrouchObstacleTolerance = 0.1f; // How much space above head should there be

    [Header("Look parameters")]
    public float mouseSensitivity = 0.1f;
    public float gamepadSensitivity = 1f;
    public float verticalLookRange = 85.0f;
    public float cameraHeight = 1.6f;
    public float crouchCameraHeight = 0.8f;

    [Header("Audio")]
    public AudioClip landingAudioClip;
    public AudioClip[] footstepAudioClips;
    [Range(0, 1)] public float footstepAudioVolume = 0.5f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInput playerInputComponent;
    [SerializeField] private Animator animComponent;
    [SerializeField] private GameObject playerMesh;
    [SerializeField] private PauseMenu pauseController;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction crouchAction;
    // [HideInInspector] public InputAction sprintAction;
    // [HideInInspector] public InputAction attackAction;
    // [HideInInspector] public InputAction interactAction;

    // Private variables
    private float _verticalRotation;
    private float _currentGroundSpeedMax;
    private float _currentGroundAccel;
    private bool _gamepad = false;
    private float _animationBlend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
    //     Cursor.lockState = CursorLockMode.Locked;
    //     Cursor.visible = false;

    //     _currentGroundSpeedMax = groundSpeedMax;
    //     _currentGroundAccel = groundAccel;

    //     GetInputRefs();
    // }

    public override void OnStartClient()
    {
        if (IsOwner)
        {
            playerMesh.SetActive(false);
            playerInputComponent.enabled = true;
            playerCamera.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            _currentGroundSpeedMax = groundSpeedMax;
            _currentGroundAccel = groundAccel;

            GetInputRefs();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
            return;
        
        animComponent.SetBool("Grounded", characterController.isGrounded);

        HandleRotation();
        HandleCrouching();
        HandleMovement();

        if (characterController.enabled)
            characterController.Move(currentVelocity * Time.deltaTime);
    }

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
        jumpAction = playerInputComponent.actions.FindAction("Jump");
        crouchAction = playerInputComponent.actions.FindAction("Crouch");
        // sprintAction = playerInputComponent.actions.FindAction("Sprint");
        // attackAction = playerInputComponent.actions.FindAction("Attack");
        // interactAction = playerInputComponent.actions.FindAction("Interact");
    }

    private void HandleRotation()
    {
        if (pauseController.paused)
            return;

        if (!IsOwner) return; 

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

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            animComponent.SetBool("Jump", false);
            animComponent.SetBool("FreeFall", false);
            if (autoBhop && jumpAction.IsPressed()) {
                {
                    currentVelocity = new Vector3(currentVelocity.x, jumpForce, currentVelocity.z);
                    animComponent.SetBool("Jump", true);
                    // currentVelocity.y = jumpForce;
                }
            }
            else if (jumpAction.WasPressedThisFrame())
            {
                currentVelocity = new Vector3(currentVelocity.x, jumpForce, currentVelocity.z);
                // currentVelocity.y = jumpForce;
                animComponent.SetBool("Jump", true);
            }
        }
        else
        {
            animComponent.SetBool("FreeFall", true);
            currentVelocity = new Vector3(currentVelocity.x, currentVelocity.y + Physics.gravity.y * gravityMultiplier * Time.deltaTime, currentVelocity.z);
            // currentVelocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }
    
    // Helper function for calculating desired movement direction
    private Vector3 GetMovementDirection()
    {
        if (pauseController.paused)
            return Vector2.zero;
        
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        // return worldDirection.normalized;
        return worldDirection;
    }

    private void HandleMovement()
    {
        HandleJumping();

        Vector3 prevVelocity = currentVelocity;

        Vector3 worldDirection = GetMovementDirection();
        if (characterController.isGrounded)
            currentVelocity = GroundMovement(worldDirection, currentVelocity);
        else
            currentVelocity = AirMovement(worldDirection, currentVelocity);

        _animationBlend = Mathf.Lerp(_animationBlend, prevVelocity.magnitude, currentVelocity.magnitude);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        float inputMagnitude = _gamepad ? moveAction.ReadValue<Vector2>().magnitude : 1f;

        animComponent.SetFloat("Speed", _animationBlend);
        animComponent.SetFloat("MotionSpeed", inputMagnitude);
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
        float accelSpeed = _currentGroundAccel * Time.deltaTime;

        if (projectedSpeed + accelSpeed > _currentGroundSpeedMax)
            accelSpeed = _currentGroundSpeedMax - projectedSpeed;

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
            _currentGroundSpeedMax = crouchSpeedMax;
            _currentGroundAccel = crouchAccel;

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
                _currentGroundSpeedMax = groundSpeedMax;
                _currentGroundAccel = groundAccel;

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

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(characterController.center), footstepAudioVolume);
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (footstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, footstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(characterController.center), footstepAudioVolume);
            }
        }
    }

}