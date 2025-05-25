using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerIdleState : PlayerGameBaseState
{
    public PlayerIdleState(PlayerGameState stateMachine) : base(stateMachine){}
    
    public override void Enter()
    {
        Console.WriteLine("Entering idle state");
        //GameState.PlayerStateMachine.physicsBody.Drag = 10f;
        GameState.NextMovingState = GameState.WalkingState;
    }

    public override void Update()
    {
        Vector2 input = Input.GetMovementInput();

        if (input != Vector2.Zero && Game.MoveTest)
        {
            GameState.SwitchState(GameState.NextMovingState);
            return;
        }
        
        if (Input.IsKeyDown(Keys.Space) && Game.MoveTest)
        {
            GameState.SwitchState(GameState.JumpingState);
            return;
        }
        
        if (!StateMachine.IsGrounded())
        {
            GameState.SwitchState(GameState.FallingState);
            return;
        }
        
        //GameState.PlayerStateMachine.MoveMeshUpdate();
    }
    
    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {
        //GameState.PlayerStateMachine.physicsBody.Drag = 0.3f;
    }
}