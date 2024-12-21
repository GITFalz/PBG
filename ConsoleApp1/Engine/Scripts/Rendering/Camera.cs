using System.Numerics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

public class Camera
{
    private static float SPEED = 50f;
    private static float SCREEN_WIDTH;
    private static float SCREEN_HEIGHT;
    private static float SENSITIVITY = 65f;
    private static float SCROLL_SENSITIVITY = 0.1f;
    
    public static float FOV = 45f;
    private static float targetFOV = 45f;
    private static float FOV_SMOTH_FACTOR = 10f;

    public static Vector3 position;
    
    private static Vector3 up = Vector3.UnitY;
    private static Vector3 front = -Vector3.UnitZ;
    private static Vector3 right = Vector3.UnitX;
    
    private static float CameraDistance = 5;
    
    private static float pitch = 0;
    private static float yaw = -90;

    public bool firstMove = true;
    public Vector2 lastPos;
    
    public static Matrix4 viewMatrix;
    public static Matrix4 projectionMatrix = GetProjectionMatrix();
    
    
    private Vector2 _smoothMouseDelta = Vector2.Zero;
    private const float SMOOTH_FACTOR = 5f;
    
    private Vector3 _targetPosition;
    private const float POSITION_SMOOTH_FACTOR = 50f;

    public Camera(float width, float height, Vector3 position)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
        Camera.position = position;
    }

    public static Matrix4 GetViewMatrix()
    {
        viewMatrix = Matrix4.LookAt(position, position + front, up);
        return viewMatrix;
    }
    
    public static Matrix4 GetProjectionMatrix()
    {
        float fov = Mathf.Lerp(FOV, targetFOV, FOV_SMOTH_FACTOR * GameTime.DeltaTime);
        FOV = Mathf.Clamp(1, 179, fov);
        
        projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
            MathHelper.DegreesToRadians(FOV),
            SCREEN_WIDTH / SCREEN_HEIGHT, 
            0.1f, 
            1000f
        );
        return projectionMatrix;
    }

    public static void SetFOV(float fov)
    {
        targetFOV = fov;
        projectionMatrix = GetProjectionMatrix();
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

    public void InputController()
    {
        if (!Game.MoveTest)
        {
            if (InputManager.IsDown(Keys.W))
            {
                position += Yto0(front) * GameTime.DeltaTime;
            }

            if (InputManager.IsDown(Keys.A))
            {
                position -= Yto0(right) * GameTime.DeltaTime;
            }

            if (InputManager.IsDown(Keys.S))
            {
                position -= Yto0(front) * GameTime.DeltaTime;
            }

            if (InputManager.IsDown(Keys.D))
            {
                position += Yto0(right) * GameTime.DeltaTime;
            }

            if (InputManager.IsDown(Keys.Space))
            {
                position.Y += GameTime.DeltaTime;
            }

            if (InputManager.IsDown(Keys.LeftShift))
            {
                position.Y -= GameTime.DeltaTime;
            }
        }

        else
        {
            float scroll = InputManager.GetMouseScroll().Y;
            
            CameraDistance -= scroll * SCROLL_SENSITIVITY;
            CameraDistance = Math.Clamp(CameraDistance, 3, 10);
    
            _targetPosition = PlayerData.EyePosition - front * CameraDistance;
            position = Vector3.Lerp(position, _targetPosition, POSITION_SMOOTH_FACTOR * GameTime.DeltaTime);
        }

        if (firstMove)
        {
            lastPos = InputManager.GetMousePosition();
            firstMove = false;
        }
        else
        {
            Vector2 pos = InputManager.GetMousePosition();
            
            Vector2 currentMouseDelta = new Vector2(pos.X - lastPos.X, pos.Y - lastPos.Y);
            _smoothMouseDelta = Vector2.Lerp(_smoothMouseDelta, currentMouseDelta, SMOOTH_FACTOR * GameTime.DeltaTime);
            lastPos = new Vector2(pos.X, pos.Y);

            float deltaX = _smoothMouseDelta.X;
            float deltaY = _smoothMouseDelta.Y;

            deltaX *= SENSITIVITY * GameTime.DeltaTime;
            deltaY *= SENSITIVITY * GameTime.DeltaTime;

            yaw += deltaX;
            pitch -= deltaY;

            pitch = Math.Clamp(pitch, -89.0f, 89.0f);

            UpdateVectors();
        }
    }

    public static Vector3 Yto0(Vector3 v)
    {
        v.Y = 0;
        return Vector3.Normalize(v);
    }
    
    public static Vector3 FrontYto0()
    {
        Vector3 v = front;
        v.Y = 0;
        return Vector3.Normalize(v);
    }
    
    public static Vector3 Front()
    {
        return front;
    }
    
    public static Vector3 RightYto0()
    {
        Vector3 v = right;
        v.Y = 0;
        return Vector3.Normalize(v);
    }
    
    private float GetSpeed(FrameEventArgs e)
    {
        return SPEED * (float)e.Time;
    }
    
    public static float GetYaw()
    {
        return yaw;
    }

    public void Update()
    {
        InputController();
    }
    
    public void UpdateProjectionMatrix(int width, int height)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
    }
}