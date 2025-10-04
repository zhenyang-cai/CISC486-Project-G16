
using UnityEngine;

public class EnemyAlertState : EnemyState
{

    Vector3 lastSeenPos;

    public EnemyAlertState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.agent.speed = 3.2f;
        
        lastSeenPos = enemy.visionSensor.LastSeenPos;
        enemy.MoveTo(lastSeenPos);
    }

    public override void Exit()
    {
        base.Exit();
        enemy.agent.speed = 2f;
    }

    public override void Update()
    {
        base.Update();

        if (lastSeenPos != enemy.visionSensor.LastSeenPos)
        {
            lastSeenPos = enemy.visionSensor.LastSeenPos;
            enemy.MoveTo(lastSeenPos);
        }

        if (enemy.agent.remainingDistance <= enemy.agent.stoppingDistance && !enemy.agent.pathPending)
        {
            if (enemy.visionSensor.State == VisionSensor.SuspicionState.None)
                stateMachine.ChangeState(enemy.IdleState);
        }

        if (enemy.visionSensor.State == VisionSensor.SuspicionState.Confirmed)
            stateMachine.ChangeState(enemy.ChaseState);
    }
}
