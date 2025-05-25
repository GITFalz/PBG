using ConsoleApp1.Assets.Scripts.Inputs;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerWalkingState : PlayerGameBaseState
{
    Vector2 input = Vector2.Zero;
    
    public override void Enter()
    {
        Console.WriteLine("Entering walking state");
        
        GameState.NextMovingState = GameState.WalkingState;
        //StateMachine.physicsBody.Drag = 10f;
        GameState.MovementSpeed = PlayerMovementSpeed.Walk;
        
        OldAnimationManager.Instance.LoopAnimation("Player", "walking");
        Game.Camera.SetFOV(60);
    }

    public override void Update()
    {
        input = Input.GetMovementInput();
        
        if (Input.IsKeyDown(Keys.LeftControl))
        {
            GameState.SwitchState(GameState.RunningState);
            return;
        }
        
        if (input == Vector2.Zero)
        {
            GameState.SwitchState(GameState.IdleState);
            return;
        }

        if (Input.IsKeyPressed(Keys.F))
        {
            GameState.SwitchState(GameState.GrapplingState);
            return;
        }

        if (!StateMachine.BlockSwitch)
        {
            if (Input.IsMousePressed(MouseButton.Right))
            {
                GameState.SwitchState(GameState.DashState);
                return;
            }
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
    }
    
    public override void FixedUpdate()
    {
        StateMachine.MovePlayer(PlayerMovementSpeed.Walk);
    }

    public override void Exit()
    {
        //StateMachine.physicsBody.Drag = 0.3f;
    }
}