public class MinerReturnToBaseState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.GoToBase();
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
        if (Owner.BaseStorage == null)
        {
            return;
        }

        if (Owner.HasReachedBase())
        {
            Owner.ChangeState(Owner.DepositingState);
        }
    }
}