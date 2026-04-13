public class FiniteStateMachine<T>
{
    private FsmState<T> currentState;

    public FsmState<T> CurrentState => currentState;

    public void SetInitialState(FsmState<T> initialState)
    {
        if (initialState == null)
        {
            return;
        }

        currentState = initialState;
        currentState.OnEnter();
    }

    public void ChangeState(FsmState<T> newState)
    {
        if (newState == null)
        {
            return;
        }

        if (currentState == newState)
        {
            return;
        }

        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    public void Update()
    {
        currentState?.OnUpdate();
    }
}