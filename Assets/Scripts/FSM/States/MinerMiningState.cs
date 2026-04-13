public class MinerMiningState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.ResetActionTimer();
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
        GoldVein targetVein = Owner.CurrentTargetVein;

        if (targetVein == null)
        {
            Owner.ChangeState(Owner.IdleState);
            return;
        }

        if (!targetVein.HasGold)
        {
            Owner.ClearCurrentTargetVein();

            if (Owner.HasCarriedGold)
            {
                Owner.ChangeState(Owner.ReturnToBaseState);
            }
            else
            {
                Owner.ChangeState(Owner.IdleState);
            }

            return;
        }

        if (Owner.IsCarryFull)
        {
            Owner.ClearCurrentTargetVein();
            Owner.ChangeState(Owner.ReturnToBaseState);
            return;
        }

        Owner.AddToActionTimer();

        if (!Owner.HasReachedMiningInterval())
        {
            return;
        }

        Owner.ResetActionTimer();
        Owner.TryMineOneUnit();

        if (Owner.IsCarryFull || !targetVein.HasGold)
        {
            Owner.ClearCurrentTargetVein();
            Owner.ChangeState(Owner.ReturnToBaseState);
        }
    }
}