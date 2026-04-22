public class MinerSafeState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.PathAgent.StopMovement();
        Owner.ResetActionTimer();
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
        if (Owner.IsDead)
        {
            return;
        }

        Owner.AddToActionTimer();

        if (!Owner.HasReachedSafeDuration())
        {
            return;
        }

        Owner.ClearThreat();
        Owner.ChangeState(Owner.IdleState);
    }
}