using UnityEngine;

public abstract class FsmState<T>
{
    protected T Owner;

    public void Initialize(T owner)
    {
        Owner = owner;
        OnInitialize();
    }

    protected virtual void OnInitialize()
    {
    }

    public virtual void OnEnter()
    {
    }

    public virtual void OnExit()
    {
    }

    public virtual void OnUpdate()
    {
    }
}