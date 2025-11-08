using QFramework;
using UnityEngine;

public class Game : Architecture<Game>
{
    protected override void Init()
    {
        RegisterSystem<IEnemySystem>(new EnemySystem());
    }

    protected override void ExecuteCommand(ICommand command)
    {
        base.ExecuteCommand(command);
    }

    protected override TResult ExecuteCommand<TResult>(ICommand<TResult> command)
    {
        return base.ExecuteCommand(command);
    }
}


