
using UnityEngine;

public class EnemyMoveState : EnemyPatrolState
{

    public EnemyMoveState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.MoveTo(GetCurrentPoint());
    }

    public override void Update()
    {
        base.Update();

        if (enemy.agent.remainingDistance <= enemy.agent.stoppingDistance && !enemy.agent.pathPending)
        {
            SetNextPoint();
            stateMachine.ChangeState(enemy.IdleState);
        }
    }
}
