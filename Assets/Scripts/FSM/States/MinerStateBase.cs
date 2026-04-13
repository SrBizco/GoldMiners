public abstract class MinerStateBase : FsmState<MinerController>
{
    public virtual string StateName => GetType().Name;
}