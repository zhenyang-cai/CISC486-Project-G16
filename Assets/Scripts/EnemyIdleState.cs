
using UnityEngine;

public class EnemyIdleState : EnemyPatrolState
{
    public EnemyIdleState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 2f;
    }

    public override void Update()
    {
        base.Update();

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.MoveState);
            return;
        }
    }

}
