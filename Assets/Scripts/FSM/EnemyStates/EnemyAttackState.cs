public class EnemyAttackState : EnemyStateBase
{
    public override void OnEnter()
    {
        Owner.NotifyStateEntered(StateName);
        Owner.PathAgent.StopMovement();
        Owner.ResetAttackTimer();
    }

    public override void OnUpdate()
    {
        MinerController target = Owner.CurrentTarget;

        if (target == null || target.IsDead)
        {
            Owner.ClearTarget();
            Owner.ChangeState(Owner.ReturnState);
            return;
        }

        if (Owner.IsTargetInsideSafeZone())
        {
            Owner.ClearTarget();
            Owner.PathAgent.StopMovement();
            Owner.ChangeState(Owner.ReturnState);
            return;
        }

        if (!Owner.IsTargetInAttackRange())
        {
            Owner.ChangeState(Owner.ChaseState);
            return;
        }

        Owner.UpdateAttack();
    }
}