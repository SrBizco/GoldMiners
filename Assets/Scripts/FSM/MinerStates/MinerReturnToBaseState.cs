public class MinerReturnToBaseState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.AnimationController?.SetFleeing(false);
        Owner.AnimationController?.SetChopping(false);
        Owner.GoToBase();
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
        if (Owner.ShouldFlee())
        {
            Owner.ChangeState(Owner.FleeState);
            return;
        }

        if (Owner.HasReachedBase())
        {
            Owner.ChangeState(Owner.DepositingState);
        }
    }
}