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
    private static float SCROLL_SENSITIVITY = 50f;

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
    public static Matrix4 projectionMatrix;
    public static Plane[] frustumPlanes;
    
    
    private Vector2 _smoothMouseDelta = Vector2.Zero;
    private const float SMOOTH_FACTOR = 0.2f;

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
        projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45),
            SCREEN_WIDTH / SCREEN_HEIGHT, 0.1f, 1000f);
        return projectionMatrix;
    }
    
    public static Plane[] GetFrustumPlanes()
    {   
        Matrix4 projectionViewMatrix = projectionMatrix * viewMatrix;

        Plane[] frustumPlanes = new Plane[6];
        frustumPlanes[0] = new Plane(Mathf.ToNumericsVector4(-projectionViewMatrix.Row3 - projectionViewMatrix.Row0)); // Left
        frustumPlanes[1] = new Plane(Mathf.ToNumericsVector4(-projectionViewMatrix.Row3 + projectionViewMatrix.Row0)); // Right
        frustumPlanes[2] = new Plane(Mathf.ToNumericsVector4(-projectionViewMatrix.Row3 - projectionViewMatrix.Row1)); // Bottom
        frustumPlanes[3] = new Plane(Mathf.ToNumericsVector4(-projectionViewMatrix.Row3 + projectionViewMatrix.Row1)); // Top
        frustumPlanes[4] = new Plane(Mathf.ToNumericsVector4(-projectionViewMatrix.Row3 - projectionViewMatrix.Row2)); // Near
        frustumPlanes[5] = new Plane(Mathf.ToNumericsVector4(-projectionViewMatrix.Row3 + projectionViewMatrix.Row2)); // Far

        for (int i = 0; i < frustumPlanes.Length; i++)
        {
            frustumPlanes[i] = Plane.Normalize(frustumPlanes[i]);
        }
        
        Camera.frustumPlanes = frustumPlanes;

        return frustumPlanes;
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
        if (!Game.MoveTest)
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
        }

        else
        {
            float scroll = mouse.ScrollDelta.Y;

            CameraDistance -= scroll * SCROLL_SENSITIVITY * GameTime.DeltaTime;
            CameraDistance = Math.Clamp(CameraDistance, 3, 10);
            
            Console.WriteLine(scroll);
            
            position = PlayerData.Position - front * CameraDistance;
        }

        if (firstMove)
        {
            lastPos = new Vector2(mouse.X, mouse.Y);
            firstMove = false;
        }
        else
        {
            Vector2 currentMouseDelta = new Vector2(mouse.X - lastPos.X, mouse.Y - lastPos.Y);
            _smoothMouseDelta = Vector2.Lerp(_smoothMouseDelta, currentMouseDelta, SMOOTH_FACTOR);
            lastPos = new Vector2(mouse.X, mouse.Y);

            float deltaX = _smoothMouseDelta.X;
            float deltaY = _smoothMouseDelta.Y;

            deltaX *= SENSITIVITY * (float)e.Time;
            deltaY *= SENSITIVITY * (float)e.Time;

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