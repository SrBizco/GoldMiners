public class MinerDeadState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.HandleDeath();
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
    }
}