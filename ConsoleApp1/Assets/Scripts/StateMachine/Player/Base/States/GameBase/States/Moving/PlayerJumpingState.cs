using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerJumpingState : PlayerGameBaseState
{
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering jumping state");
        
        playerGameState.PlayerStateMachine.physicsBody.doGravity = true;
        Vector3 velocity = playerGameState.PlayerStateMachine.physicsBody.Velocity;
        
        velocity.Y = 0;
        velocity = Mathf.Normalize(velocity) * 0.05f;
        
        playerGameState.PlayerStateMachine.physicsBody.AddForce(new Vector3(velocity.X, 0.3f, velocity.Z));
        
        AnimationManager.Instance.SetAnimation("Player", "jumping");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        if (playerGameState.PlayerStateMachine.IsHuggingWall() && InputManager.IsKeyPressed(Keys.Space))
        {
            Camera.SetFOV(45);
            playerGameState.PlayerStateMachine.physicsBody.Velocity.Y = 0;
            playerGameState.SwitchState(playerGameState.JumpingState);
            return;
        }

        if (InputManager.IsKeyPressed(Keys.F))
        {
            playerGameState.SwitchState(playerGameState.GrapplingState);
            return;
        }
        
        if (playerGameState.PlayerStateMachine.physicsBody.Velocity.Y < 0)
        {
            Camera.SetFOV(45);
            playerGameState.SwitchState(playerGameState.FallingState);
            return;
        }
        
        if (playerGameState.PlayerStateMachine.IsGrounded())
        {
            Camera.SetFOV(45);
            playerGameState.SwitchState(playerGameState.GroundedState);
            return;
        }
        
        playerGameState.PlayerStateMachine.MeshUpdate();
        return;
    }
    
    public override void FixedUpdate(PlayerGameState playerGameState)
    {

    }

    public override void Exit(PlayerGameState playerGameState)
    {

    }
}