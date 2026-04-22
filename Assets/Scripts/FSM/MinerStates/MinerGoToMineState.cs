public class MinerGoToMineState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
        if (Owner.ShouldFlee())
        {
            Owner.ChangeState(Owner.FleeState);
            return;
        }

        GoldVein targetVein = Owner.CurrentTargetVein;

        if (targetVein == null)
        {
            Owner.ChangeState(Owner.IdleState);
            return;
        }

        if (!targetVein.HasGold)
        {
            Owner.ClearCurrentTargetVein();
            Owner.ChangeState(Owner.IdleState);
            return;
        }

        if (Owner.HasReachedCurrentVein())
        {
            Owner.ChangeState(Owner.MiningState);
        }
    }
}