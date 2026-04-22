public class MinerDeadState : MinerStateBase
{
    public override void OnEnter()
    {
        Owner.AnimationController?.SetDead(true);
        Owner.HandleDeath();
        Owner.NotifyStateEntered(StateName);
    }

    public override void OnUpdate()
    {
    }
}