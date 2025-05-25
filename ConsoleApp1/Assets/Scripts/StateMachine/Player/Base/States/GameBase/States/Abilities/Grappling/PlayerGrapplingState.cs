using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerGrapplingState : PlayerGameBaseState
{
    Camera Camera;
    
    float timer = 0;
    Vector3 grappleDirection;

    public PlayerGrapplingState(PlayerGameState gameState) : base(gameState)
    {
    }

    public override void Enter()
    {
        Console.WriteLine("Entering grappling state");
        
        Camera = Game.Camera;
        
        Camera.SetFOV(90);
        
        StateMachine.physicsBody.DisableGravity();
        
        timer = 0;
        grappleDirection = Camera.Front();
    }

    public override void Update()
    {
        timer += GameTime.DeltaTime;
        
        if (Input.IsKeyPressed(Keys.LeftShift))
        {
            GameState.SwitchState(GameState.GrapplingSwingOutState);
            return;
        }

        if (timer > 1f)
        {
            OldAnimationManager.Instance.SetAnimation("Player", "grappleOut");
            GameState.SwitchState(GameState.FallingState);
            return;
        }
    }

    public override void FixedUpdate()
    {
        StateMachine.MovePlayer(PlayerMovementSpeed.Grappling, grappleDirection);
    }

    public override void Exit()
    {
        StateMachine.physicsBody.EnableGravity();
    }
}