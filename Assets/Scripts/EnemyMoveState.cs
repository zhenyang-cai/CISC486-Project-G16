
using UnityEngine;

public class EnemyMoveState : EnemyState
{
    private Vector3 target;

    public EnemyMoveState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.MoveTo(target);
    }

    public override void Update()
    {
        base.Update();

        if (enemy.visionSensor.currentTarget != null)
        {
            stateMachine.ChangeState(enemy.AlertState);
            return;
        }

        if (enemy.agent.remainingDistance <= enemy.agent.stoppingDistance && !enemy.agent.pathPending)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }
    }

    public void SetTarget(Vector3 pos)
    {
        target = pos;
    }
}
