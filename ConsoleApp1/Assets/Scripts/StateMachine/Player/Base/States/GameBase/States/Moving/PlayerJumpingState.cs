using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerJumpingState : PlayerGameBaseState
{
    Camera Camera;
    Vector2 input = Vector2.Zero;
    
    public override void Enter()
    {
        Console.WriteLine("Entering jumping state");
        
        //StateMachine.physicsBody.Drag = 2;
        Camera = Game.Camera;
        
        StateMachine.physicsBody.AddForce(new Vector3(0, PlayerStateMachine.JUMP_SPEED, 0));
        
        OldAnimationManager.Instance.SetAnimation("Player", "jumping");
    }

    public override void Update()
    {
        input = Input.GetMovementInput();

        if (Input.IsKeyPressed(Keys.F))
        {
            GameState.SwitchState(GameState.GrapplingState);
            return;
        }
        
        if (StateMachine.physicsBody.Velocity.Y < 0)
        {
            GameState.SwitchState(GameState.FallingState);
            return;
        }
        
        if (StateMachine.IsGrounded() && StateMachine.physicsBody.Velocity.Y < 0)
        {
            GameState.SwitchState(GameState.GroundedState);
            return;
        }
    }
    
    public override void FixedUpdate()
    {
        if (input != Vector2.Zero)
            StateMachine.MovePlayer(PlayerMovementSpeed.Fall);
    }

    public override void Exit()
    {
        //StateMachine.physicsBody.Drag = 0.1f;
    }
}