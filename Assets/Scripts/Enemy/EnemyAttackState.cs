
using UnityEngine;

public class EnemyAttackState : EnemyState
{
    readonly float MaxAttackRange = 12f;

    float enemySpeedRecord;

    public EnemyAttackState(Enemy enemy) : base(enemy)
    {
    }

    public override void Enter()
    {
        base.Enter();
        anim.SetBool("IsAiming", true);
        enemySpeedRecord = enemy.agent.speed;
        enemy.agent.speed = 2f;
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("IsAiming", false);
        enemy.agent.speed = enemySpeedRecord;
    }

    public override void Update()
    {
        base.Update();
        var target = enemy.visionSensor.ConfirmedTarget;
        if (target == null)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        enemy.FaceTarget(target.position);

        float dist = Vector3.Distance(enemy.transform.position, target.position);
        if (dist >= MaxAttackRange)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // TODO: Enemy Attack
        // if (Time.time - lastAttackTime >= attackCooldown)
        // {
        //     lastAttackTime = Time.time;
        //     PerformAttack();
        // }
    }
    
}
