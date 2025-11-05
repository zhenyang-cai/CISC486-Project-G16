
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyPatrolState : EnemyState
{

    public List<Vector3> PatrolPoints { get; private set; }
    public int CurrentIndex { get; private set; }

    public EnemyPatrolState(Enemy enemy) : base(enemy)
    {
        CurrentIndex = 0;
        PatrolPoints = enemy.PatrolPoints.Select(t => t.position).ToList();
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

    public void SetPatrolPoints(List<Vector3> points)
    {
        PatrolPoints = points;
    }

    protected Vector3 GetCurrentPoint()
    {
        if (PatrolPoints.Count > 0)
        {
            return PatrolPoints[CurrentIndex];
        }
        return enemy.transform.position;
    }

    protected void SetNextPoint()
    {
        if (PatrolPoints.Count > 0)
            CurrentIndex = (CurrentIndex + 1) % PatrolPoints.Count;
    }

}
