
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
        var target = enemy.visionSensor.ConfirmedTarget;
        if (target)
        {
            float dist = Vector3.Distance(enemy.transform.position, target.position);

            if (dist > enemy.agent.stoppingDistance + 2f)
            {
                enemy.agent.isStopped = false;
                enemy.agent.SetDestination(target.position);
            }
            else
            {
                enemy.agent.isStopped = true;
                FaceTarget(target.position);
            }
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
    
    void FaceTarget(Vector3 lookAt)
{
    Vector3 dir = lookAt - enemy.transform.position;
    dir.y = 0;
    if (dir.sqrMagnitude > 0.001f)
    {
        var rot = Quaternion.LookRotation(dir);
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, rot, Time.deltaTime * 8f);
    }
}
}
