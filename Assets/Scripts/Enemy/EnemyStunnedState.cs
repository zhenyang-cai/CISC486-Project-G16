
using UnityEngine;
using UnityEngine.AI;

public class EnemyStunnedState : EnemyState
{
    public EnemyStunnedState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 5f;

        enemy.SetStunVisual(true);
        
        NavMeshAgent navMeshAgent = entity.gameObject.GetComponentInChildren<NavMeshAgent>();
        if (navMeshAgent != null)
            navMeshAgent.enabled = false;
    }

    public override void Exit()
    {
        base.Exit();
        enemy.SetStunVisual(false);

        NavMeshAgent navMeshAgent = entity.gameObject.GetComponentInChildren<NavMeshAgent>();
        if (navMeshAgent != null)
            navMeshAgent.enabled = true;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
            stateMachine.ChangeState(enemy.MoveState);
    }
}
