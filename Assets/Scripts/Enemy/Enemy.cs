using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Entity
{
    #region States
    public EnemyIdleState IdleState { get; private set; }
    public EnemyMoveState MoveState { get; private set; }
    public EnemyAlertState AlertState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    #endregion

    public List<Transform> PatrolPoints;

    public NavMeshAgent agent { get; private set; }
    public VisionSensor visionSensor { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        visionSensor = GetComponent<VisionSensor>();

        IdleState = new EnemyIdleState(this);
        MoveState = new EnemyMoveState(this);
        AlertState = new EnemyAlertState(this);
        ChaseState = new EnemyChaseState(this);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(IdleState);
    }

    protected override void Update()
    {
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

}
