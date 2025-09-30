using UnityEngine;

public abstract class EntityState
{
    protected Entity entity;
    protected Animator anim;
    protected StateMachine stateMachine;
    protected float stateTimer;

    public EntityState(Entity entity)
    {
        this.entity = entity;
        anim = entity.anim;
        stateMachine = entity.stateMachine;
    }

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void Update()
    {
        if (stateTimer > 0) stateTimer -= Time.deltaTime;
    }
    public virtual void LateUpdate() { }
    public virtual void FixedUpdate() { }
}
