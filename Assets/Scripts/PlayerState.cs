
public class PlayerState : EntityState
{
    protected Player player { get; private set; }
    public PlayerState(Player player) : base(player)
    {
        this.player = player;
    }
}
