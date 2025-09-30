using UnityEngine;

public class Player : Entity
{
    #region States
    public PlayerIdleState IdleState { get; private set; }
    #endregion

    #region Input System
    public PlayerInput Input { get; private set; }
    #endregion
    private GameObject mainCamera;

    public float MoveSpeed = 2.0f;
    public float SprintSpeed = 5.335f;
    public float SpeedChangeRate = 10.0f;
    public float RotationSmoothTime = 0.12f;

    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.50f;
    public float FallTimeout = 0.15f;

    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        Input = GetComponent<PlayerInput>();
        IdleState = new PlayerIdleState(this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(IdleState);
    }

    protected override void Update()
    {
        base.Update();
        JumpAndGravity();
        Move();
    }

    private void Move()
    {
        float _speed;

        float targetSpeed = Input.sprint ? SprintSpeed : MoveSpeed;
        if (Input.move == Vector2.zero) targetSpeed = 0.0f;

        float speedOffset = 0.1f;
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        float inputMagnitude = Input.analogMovement ? Input.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        Vector3 inputDirection = new Vector3(Input.move.x, 0.0f, Input.move.y).normalized;

        if (Input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        controller.Move(
            targetDirection.normalized * (_speed * Time.deltaTime) +
            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
        );

        anim.SetFloat("Speed", _animationBlend);
        anim.SetFloat("MotionSpeed", inputMagnitude);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            anim.SetBool("Jump", false);
            anim.SetBool("FreeFall", false);

            _fallTimeoutDelta = FallTimeout;

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            if (Input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                anim.SetBool("Jump", true);
            }
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                anim.SetBool("FreeFall", true);
            }

            Input.jump = false;
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }
}
