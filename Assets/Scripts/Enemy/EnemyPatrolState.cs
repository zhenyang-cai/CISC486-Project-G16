
using System.Collections.Generic;
using UnityEngine;

public class EnemyPatrolState : EnemyState
{

    public List<Vector3> PatrolPoints { get; private set; }
    public int CurrentIndex { get; private set; }

    public EnemyPatrolState(Enemy enemy) : base(enemy)
    {
        CurrentIndex = 0;
        PatrolPoints = new List<Vector3>
        {
            new(-6, 0, 18),
            new(-6, 0, 5)
        };
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
    }

    public void SetPatrolPoints(List<Vector3> points)
    {
        PatrolPoints = points;
    }

    protected Vector3 GetCurrentPoint()
    {
        return PatrolPoints[CurrentIndex];
    }

    protected void SetNextPoint()
    {
        CurrentIndex = (CurrentIndex + 1) % PatrolPoints.Count;
    }

}
