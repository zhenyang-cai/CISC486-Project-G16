using UnityEngine;

public class FPSMovementNetworked : NetworkedMovement
{
    [Header("Source movement variables")]
    // Source movement
    public bool autoBhop = false;
    public float groundAccel = 40.0f;
    public float airAccel = 10.0f;
    public float groundSpeedMax = 20.0f; // Max player velocity on the ground.
    public float airSpeedMax = 5.0f; // Max player velocity in the air.
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
    public float cameraHeight = 1.6f;
    public float crouchCameraHeight = 0.8f;

    [Header("Audio")]
    public AudioClip landingAudioClip;
    public AudioClip[] footstepAudioClips;
    [Range(0, 1)] public float footstepAudioVolume = 0.5f;

    [Header("References")]
    [SerializeField] private Animator animComponent;

    // Private variables
    private float _animationBlend;
    private float _currentGroundSpeedMax;
    private float _currentGroundAccel;

    protected override void OnStartExtra()
    {
        _currentGroundSpeedMax = groundSpeedMax;
        _currentGroundAccel = groundAccel;
    }

    new void Update()
    {
        if (!IsOwner)
            return;
        
        animComponent.SetBool("Grounded", characterController.isGrounded);

        HandleRotation();
        HandleCrouching();
        HandleMovement();

        if (characterController.enabled)
            characterController.Move(_currentVelocity * Time.deltaTime);
    }
    
    protected override void HandleMovement()
    {
        HandleJumping();

        Vector3 prevVelocity = _currentVelocity;

        Vector3 worldDirection = GetMovementDirection();
        if (characterController.isGrounded)
            _currentVelocity = GroundMovement(worldDirection, _currentVelocity);
        else
            _currentVelocity = AirMovement(worldDirection, _currentVelocity);

        _animationBlend = Mathf.Lerp(_animationBlend, prevVelocity.magnitude, _currentVelocity.magnitude);
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

    private void HandleCrouching()
    {
        float crouchState = crouchAction.ReadValue<float>();

        if (crouchState == 1f)
        {
            _currentGroundSpeedMax = crouchSpeedMax;
            _currentGroundAccel = crouchAccel;

            // change height and adjust position
            characterController.height = Mathf.LerpUnclamped(characterController.height, crouchHeight, crouchSmoothingSpeed * Time.deltaTime);
        }
        else
        {
            if (!Physics.Raycast(new Vector3(transform.position.x, transform.position.y + characterController.height / 2, transform.position.z), Vector3.up, walkHeight - characterController.center.y + uncrouchObstacleTolerance))
            {
                _currentGroundSpeedMax = groundSpeedMax;
                _currentGroundAccel = groundAccel;

                // change height and adjust position
                characterController.height = Mathf.LerpUnclamped(characterController.height, walkHeight, crouchSmoothingSpeed * Time.deltaTime);
            }
        }
        characterController.center = new Vector3(characterController.center.x, characterController.height / 2, characterController.center.z);
        playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, cameraHeight * (characterController.height / walkHeight), playerCamera.transform.localPosition.z);
    }

    private void HandleJumping()
    {
        if (characterController.isGrounded)
        {
            animComponent.SetBool("Jump", false);
            animComponent.SetBool("FreeFall", false);
            if (autoBhop && jumpAction.IsPressed()) {
                {
                    _currentVelocity = new Vector3(_currentVelocity.x, jumpForce, _currentVelocity.z);
                    animComponent.SetBool("Jump", true);
                }
            }
            else if (jumpAction.WasPressedThisFrame())
            {
                _currentVelocity = new Vector3(_currentVelocity.x, jumpForce, _currentVelocity.z);
                animComponent.SetBool("Jump", true);
            }
        }
        else
        {
            animComponent.SetBool("FreeFall", true);
            _currentVelocity = new Vector3(_currentVelocity.x, _currentVelocity.y + Physics.gravity.y * gravityMultiplier * Time.deltaTime, _currentVelocity.z);
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