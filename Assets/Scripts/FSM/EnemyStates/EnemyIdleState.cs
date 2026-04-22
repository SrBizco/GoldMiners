public class EnemyIdleState : EnemyStateBase
{
    public override void OnEnter()
    {
        Owner.NotifyStateEntered(StateName);
        Owner.ClearTarget();
        Owner.PathAgent.StopMovement();
    }

    public override void OnUpdate()
    {
        MinerController target = Owner.FindNearestTarget();

        if (target == null)
        {
            return;
        }

        Owner.SetTarget(target);
        Owner.ChangeState(Owner.ChaseState);
    }
}