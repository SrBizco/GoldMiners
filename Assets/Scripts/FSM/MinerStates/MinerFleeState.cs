public class MinerFleeState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.ClearCurrentTargetVein();
        Owner.GoToBase();
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
        if (Owner.IsDead)
        {
            return;
        }

        if (Owner.HasReachedBase())
        {
            Owner.ChangeState(Owner.SafeState);
            return;
        }

        if (!Owner.PathAgent.HasDestination)
        {
            Owner.GoToBase();
        }
    }
}