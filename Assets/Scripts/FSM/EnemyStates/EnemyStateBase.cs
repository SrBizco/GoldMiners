public abstract class EnemyStateBase : FsmState<EnemyController>
{
    public virtual string StateName => GetType().Name;
}