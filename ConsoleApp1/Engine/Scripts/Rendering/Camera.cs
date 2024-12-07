using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ConsoleApp1.Engine.Scripts.Core.Rendering;

public class Camera
{
    private float SPEED = 8f;
    private float SCREEN_WIDTH;
    private float SCREEN_HEIGHT;
    private float SENSITIVITY = 80f;

    public Vector3 position;
    
    private Vector3 up = Vector3.UnitY;
    private Vector3 front = -Vector3.UnitZ;
    private Vector3 right = Vector3.UnitX;
    
    private float pitch = 0;
    private float yaw = -90;

    public bool firstMove = true;
    public Vector2 lastPos;

    public Camera(float width, float height, Vector3 position)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
        this.position = position;
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt(position, position + front, up);
    }
    
    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), SCREEN_WIDTH / SCREEN_HEIGHT, 0.1f, 1000f);
    }

    public void UpdateVectors()
    {
        front.X = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Cos(MathHelper.DegreesToRadians(yaw));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
        front.Z = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Sin(MathHelper.DegreesToRadians(yaw));
        
        front = Vector3.Normalize(front);
        right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
        up = Vector3.Normalize(Vector3.Cross(right, front));
    }

    public void InputController(KeyboardState input, MouseState mouse, FrameEventArgs e)
    {
        if (input.IsKeyDown(Keys.W))
        {
            position += Yto0(front) * GetSpeed(e);
        }
        if (input.IsKeyDown(Keys.A))
        {
            position -= Yto0(right) * GetSpeed(e);
        }
        if (input.IsKeyDown(Keys.S))
        {
            position -= Yto0(front) * GetSpeed(e);
        }
        if (input.IsKeyDown(Keys.D))
        {
            position += Yto0(right) * GetSpeed(e);
        }
        if (input.IsKeyDown(Keys.Space))
        {
            position.Y += GetSpeed(e);
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            position.Y -= GetSpeed(e);
        }

        if (firstMove)
        {
            lastPos = new Vector2(mouse.X, mouse.Y);
            firstMove = false;
        }
        else
        {
            float deltaX = mouse.X - lastPos.X;
            float deltaY = mouse.Y - lastPos.Y;
            lastPos = new Vector2(mouse.X, mouse.Y);

            deltaX *= SENSITIVITY * (float)e.Time;
            deltaY *= SENSITIVITY * (float)e.Time;

            yaw += deltaX;
            pitch -= deltaY;
            
            if (pitch > 89.0f)
            {
                pitch = 89.0f;
            }
            if (pitch < -89.0f)
            {
                pitch = -89.0f;
            }

            UpdateVectors();
        }
    }

    private Vector3 Yto0(Vector3 v)
    {
        v.Y = 0;
        return Vector3.Normalize(v);
    }
    
    private float GetSpeed(FrameEventArgs e)
    {
        return SPEED * (float)e.Time;
    }

    public void Update(KeyboardState input, MouseState mouse, FrameEventArgs e)
    {
        InputController(input, mouse, e);
    }
    
    public void UpdateProjectionMatrix(int width, int height)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
    }
}