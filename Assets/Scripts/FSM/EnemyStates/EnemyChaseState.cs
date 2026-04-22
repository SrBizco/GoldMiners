public class EnemyChaseState : EnemyStateBase
{
    public override void OnEnter()
    {
        Owner.NotifyStateEntered(StateName);
        Owner.ResetRepathTimer();
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
            Owner.ChangeState(Owner.ReturnState);
            return;
        }

        if (Owner.IsTargetInAttackRange())
        {
            Owner.ChangeState(Owner.AttackState);
            return;
        }

        Owner.UpdateChasePath();
    }
}