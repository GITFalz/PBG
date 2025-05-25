public class PlayerGroundedState : PlayerGameBaseState
{
    public override void Enter()
    {
        Console.WriteLine("Entering grounded state");
        
        GameState.SwitchState(GameState.NextMovingState);
    }

    public override void Update()
    {

    }
    
    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {

    }
}