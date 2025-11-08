
using QFramework;
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
        var target = enemy.visionSensor.ConfirmedTarget;
        if (target)
            enemy.SendCommand(new PlayerSpottedCommand(target));
    }

    public override void Exit()
    {
        base.Exit();
        enemy.agent.speed = 3.2f;
    }

    public override void Update()
    {
        base.Update();
        var target = enemy.visionSensor.ConfirmedTarget;
        if (target)
        {
            Vector3 dir = (target.position - enemy.transform.position).normalized;
            Vector3 stopPos = target.position - dir * 2f;

            enemy.FaceTarget(target.position);
            enemy.MoveTo(stopPos);

            float dist = Vector3.Distance(enemy.transform.position, target.position);
            if (dist <= 6f)
            {
                stateMachine.ChangeState(enemy.AttackState);
                return;
            }
        }
        else
        {
            Vector3 dir = (enemy.visionSensor.LastSeenPos - enemy.transform.position).normalized;
            Vector3 stopPos = enemy.visionSensor.LastSeenPos - dir * 2f;
            enemy.MoveTo(stopPos);
        }

        if (enemy.visionSensor.State == VisionSensor.SuspicionState.Investigate)
        {
            stateMachine.ChangeState(enemy.AlertState);
        }
        else if (enemy.visionSensor.State == VisionSensor.SuspicionState.None)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }

    }
    
    
}
