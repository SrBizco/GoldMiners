public class MinerDepositingState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.ResetActionTimer();
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
        if (!Owner.HasCarriedGold)
        {
            Owner.ChangeState(Owner.IdleState);
            return;
        }

        Owner.AddToActionTimer();

        if (!Owner.HasReachedDepositInterval())
        {
            return;
        }

        Owner.ResetActionTimer();
        Owner.DepositOneUnitToBase();

        if (!Owner.HasCarriedGold)
        {
            Owner.ChangeState(Owner.IdleState);
        }
    }
}