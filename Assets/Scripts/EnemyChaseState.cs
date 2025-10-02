
using Unity.VisualScripting;
using UnityEngine;

public class EnemyChaseState : EnemyState
{

    public EnemyChaseState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.agent.speed = 5.2f;
    }

    public override void Exit()
    {
        base.Exit();
        enemy.agent.speed = 3.2f;
    }

    public override void Update()
    {
        base.Update();

        if (enemy.visionSensor.ConfirmedTarget)
        {
            enemy.MoveTo(enemy.visionSensor.ConfirmedTarget.position);
        }
        else if (enemy.visionSensor.State == VisionSensor.SuspicionState.Investigate)
        {
            stateMachine.ChangeState(enemy.AlertState);
        }
        else if (enemy.visionSensor.State == VisionSensor.SuspicionState.None)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }
        
    }
}
