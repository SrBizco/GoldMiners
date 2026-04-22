public class EnemyReturnState : EnemyStateBase
{
    public override void OnEnter()
    {
        Owner.NotifyStateEntered(StateName);
        Owner.GoToSpawn();
    }

    public override void OnUpdate()
    {
        if (Owner.HasReachedSpawn())
        {
            Owner.ChangeState(Owner.IdleState);
            return;
        }

        Owner.UpdateReturnPath();
    }
}