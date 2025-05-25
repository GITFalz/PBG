using OpenTK.Mathematics;

public class PlayerGrapplingSwingOutState : PlayerGameBaseState
{
    Camera Camera;
    
    Vector3 direction;
    
    public override void Enter()
    {
        Console.WriteLine("Entering grappling swing out state");
        
        Camera = Game.Camera;

        StateMachine.physicsBody.EnableGravity();

        direction = Camera.FrontYto0();
    }

    public override void Update()
    {
        if (StateMachine.physicsBody.Velocity.Y < -2f)
        {
            Camera.SetFOV(60);
            GameState.SwitchState(GameState.FallingState);
            return;
        }
        
        if (StateMachine.IsGrounded())
        {
            GameState.SwitchState(GameState.GroundedState);
            return;
        }
    }

    public override void FixedUpdate()
    {

    }

    public override void Exit()
    {

    }
}