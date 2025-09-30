
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
        enemy.agent.speed = 5.2f;
        lastSeenPos = enemy.visionSensor.lastSeenPos;
        enemy.MoveTo(lastSeenPos);
        stateTimer = -5f;
    }

    public override void Exit()
    {
        base.Exit();
        enemy.agent.speed = 2.0f;
        enemy.visionSensor.currentTarget = null;
    }

    public override void Update()
    {
        base.Update();

        if (lastSeenPos != enemy.visionSensor.lastSeenPos)
        {
            lastSeenPos = enemy.visionSensor.lastSeenPos;
            enemy.MoveTo(lastSeenPos);
            stateTimer = -5f;
        }

        if (enemy.agent.remainingDistance <= enemy.agent.stoppingDistance && !enemy.agent.pathPending)
        {
            if (stateTimer < -3)
                stateTimer = 3f;
            else if (stateTimer < 0)
                stateMachine.ChangeState(enemy.IdleState);

            LookAround();
        }
    }

    public void LookAround(float angle = 100f, float speed = 2f)
    {
        float yaw = Mathf.Sin(Time.time * speed) * angle;
        Quaternion targetRot = Quaternion.Euler(0, enemy.transform.eulerAngles.y + yaw, 0);
        enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRot, Time.deltaTime);
    }
}
