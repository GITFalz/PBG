using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class PlayerJumpingState : PlayerGameBaseState
{
    Camera Camera;
    
    public override void Enter(PlayerGameState playerGameState)
    {
        Console.WriteLine("Entering jumping state");
        
        Camera = Game.camera;
        
        playerGameState.PlayerStateMachine.physicsBody.EnableGravity();
        playerGameState.PlayerStateMachine.physicsBody.AddForce(new Vector3(0, PlayerStateMachine.JUMP_SPEED, 0));
        
        OldAnimationManager.Instance.SetAnimation("Player", "jumping");
    }

    public override void Update(PlayerGameState playerGameState)
    {
        if (Input.IsKeyPressed(Keys.F))
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
        
        if (playerGameState.PlayerStateMachine.IsGrounded() && playerGameState.PlayerStateMachine.physicsBody.Velocity.Y < 0)
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