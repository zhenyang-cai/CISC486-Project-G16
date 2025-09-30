
public class EnemyState : EntityState
{
    protected Enemy enemy { get; private set; }
    public EnemyState(Enemy enemy) : base(enemy)
    {
        this.enemy = enemy;
    }
}
