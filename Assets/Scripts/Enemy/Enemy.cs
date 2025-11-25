using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using QFramework;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Entity, IController
{
    public IArchitecture GetArchitecture() => Game.Interface;
    
    #region States
    public EnemyIdleState IdleState { get; private set; }
    public EnemyMoveState MoveState { get; private set; }
    public EnemyAlertState AlertState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyStunnedState StunnedState { get; private set; }
    #endregion

    public List<Transform> PatrolPoints;

    public NavMeshAgent agent { get; private set; }
    public VisionSensor visionSensor { get; private set; }

    // Cache renderer/color data for networked stun visuals.
    SkinnedMeshRenderer _meshRenderer;
    Color _stunOriginalColor;
    bool _hasStunOriginalColor;
    public readonly SyncVar<bool> stunnedVisual = new SyncVar<bool>();

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!NetworkManager.IsServerStarted)
            agent.enabled = false;
    }

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        visionSensor = GetComponent<VisionSensor>();

        _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        stunnedVisual.OnChange += UpdateStunColor;

        IdleState = new EnemyIdleState(this);
        MoveState = new EnemyMoveState(this);
        AlertState = new EnemyAlertState(this);
        ChaseState = new EnemyChaseState(this);
        AttackState = new EnemyAttackState(this);
        StunnedState = new EnemyStunnedState(this);
    }

    void OnDestroy()
    {
        stunnedVisual.OnChange -= UpdateStunColor;
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(IdleState);
        this.RegisterEvent<PlayerSpottedEvent>(e =>
        {
            visionSensor.TriggerAlert(e.Target);
        });
    }

    protected override void Update()
    {
        if (!IsServerInitialized) return;

        base.Update();
        anim.SetBool("Grounded", isGrounded);
        anim.SetFloat("Speed", agent.velocity.magnitude);
        anim.SetFloat("MotionSpeed", agent.desiredVelocity.magnitude / agent.speed);
    }

    public void MoveTo(Vector3 pos)
    {
        if (NavMesh.SamplePosition(pos, out var hit, 1f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    public void FaceTarget(Vector3 lookAt)
    {
        Vector3 dir = lookAt - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            var rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8f);
        }
    }

    public void ApplyStun()
    {
        if (!IsServerInitialized) return;
        if (stateMachine?.CurrentState == StunnedState) return;

        stateMachine?.ChangeState(StunnedState);
    }

    public void SetStunVisual(bool isActive)
    {
        if (!IsServerInitialized)
            return;

        stunnedVisual.Value = isActive;
    }

    void UpdateStunColor(bool previous, bool next, bool asServer)
    {
        if (_meshRenderer == null) _meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (_meshRenderer == null) return;

        if (!_hasStunOriginalColor)
        {
            _stunOriginalColor = _meshRenderer.material.color;
            _hasStunOriginalColor = true;
        }

        Color target = next ? Color.blue : _stunOriginalColor;
        _meshRenderer.material.SetColor("_BaseColor", target);
    }
}
