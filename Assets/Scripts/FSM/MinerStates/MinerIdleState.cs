public class MinerIdleState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.AnimationController?.SetFleeing(false);
        Owner.AnimationController?.SetChopping(false);
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
        if (Owner.ShouldFlee())
        {
            Owner.ChangeState(Owner.FleeState);
            return;
        }

        if (Owner.IsCarryFull)
        {
            Owner.ChangeState(Owner.ReturnToBaseState);
            return;
        }

        GoldVein bestVein = Owner.FindBestAvailableVein();

        if (bestVein == null)
        {
            return;
        }

        bool reserved = bestVein.TryReserve(Owner);

        if (!reserved)
        {
            return;
        }

        Owner.SetCurrentTargetVein(bestVein);
        Owner.PathAgent.SetDestination(bestVein.transform.position);
        Owner.ChangeState(Owner.GoToMineState);
    }
}