
using QFramework;
using UnityEngine;

public struct PlayerSpottedEvent
{
    public Transform Target;
}

public class PlayerSpottedCommand : AbstractCommand
{
    public Transform playerTransform { get; private set; }

    public PlayerSpottedCommand(Transform target)
    {
        playerTransform = target;
    }

    protected override void OnExecute()
    {
        if (playerTransform == null) return;
        this.SendEvent(new PlayerSpottedEvent
        {
            Target = playerTransform
        });
    }
}

public interface IEnemySystem : ISystem
{

}

public class EnemySystem : AbstractSystem, IEnemySystem
{
    protected override void OnInit()
    {
        this.RegisterEvent<PlayerSpottedEvent>(e =>
        {
            Debug.Log(
                $"EnemySystem: PlayerSpottedEvent Called. {e.Target} {e.Target.position}"
            );
        });
    }
}
