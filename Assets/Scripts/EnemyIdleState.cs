
using UnityEngine;

public class EnemyIdleState : EnemyState
{
    
    private Vector3 pointA = new (-6, 0, 18);
    private Vector3 pointB = new (-6, 0, 5);

    public EnemyIdleState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 2f;

        float distA = Vector3.Distance(enemy.transform.position, pointA);
        float distB = Vector3.Distance(enemy.transform.position, pointB);
        Vector3 target = distA < distB ? pointB : pointA;
        enemy.MoveState.SetTarget(target);
    }

    public override void Update()
    {
        base.Update();

        if (enemy.visionSensor.currentTarget != null)
        {
            stateMachine.ChangeState(enemy.AlertState);
            return;
        }

        if (stateTimer < 0)
        {
            stateMachine.ChangeState(enemy.MoveState);
            return;
        }
    }

}
