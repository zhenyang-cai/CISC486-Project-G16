
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPatrolState : EnemyState
{
    public int CurrentIndex { get; private set; }

    public EnemyPatrolState(Enemy enemy) : base(enemy)
    {
        CurrentIndex = 0;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        if (enemy.visionSensor.State == VisionSensor.SuspicionState.Investigate)
        {
            stateMachine.ChangeState(enemy.AlertState);
            return;
        }
        else if (enemy.visionSensor.State == VisionSensor.SuspicionState.Confirmed)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }
    }

    protected Vector3 GetCurrentPoint()
    {
        List<Vector3> PatrolPoints = enemy.PatrolPoints.Select(t => t.position).ToList();
        if (PatrolPoints.Count > 0)
        {
            return PatrolPoints[CurrentIndex];
        }
        return enemy.transform.position;
    }

    protected void SetNextPoint()
    {
        List<Vector3> PatrolPoints = enemy.PatrolPoints.Select(t => t.position).ToList();
        if (PatrolPoints.Count > 0)
            CurrentIndex = (CurrentIndex + 1) % PatrolPoints.Count;
    }

}
