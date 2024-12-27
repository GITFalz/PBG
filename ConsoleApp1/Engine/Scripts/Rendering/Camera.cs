using System.Numerics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

public class Camera : Component
{
    private static Camera Instance;

    public static float SPEED { get; private set; } = 50f;
    public static float SCREEN_WIDTH { get; private set; }
    public static float SCREEN_HEIGHT { get; private set; }
    public static float SENSITIVITY { get; private set; } = 65f;
    public static float SCROLL_SENSITIVITY { get; private set; } = 0.4f;
    
    public static float FOV { get; private set; } = 45f;
    public static float targetFOV { get; private set; } = 45f;
    public static float FOV_SMOTH_FACTOR { get; private set; } = 100f;

    public static Vector3 position;
    public static float pitch = 0;
    public static float yaw = -90;
    
    private static Vector3 up = Vector3.UnitY;
    private static Vector3 front = -Vector3.UnitZ;
    private static Vector3 right = Vector3.UnitX;
    
    private static float CameraDistance = 5;
    private static float oldScroll = 0;

    public bool firstMove = true;
    public Vector2 lastPos;
    
    public static Matrix4 viewMatrix;
    public static Matrix4 projectionMatrix = GetProjectionMatrix();
    
    
    private Vector2 _smoothMouseDelta = Vector2.Zero;
    private static bool _smooth = false;
    private static float SMOOTH_FACTOR = 5f;
    
    private Vector3 _targetPosition;
    private static bool _positionSmooth = false;
    private static float POSITION_SMOOTH_FACTOR = 50f;
    
    private static CameraMode _cameraMode = CameraMode.Fixed;
    
    private static Dictionary<CameraMode, Action> _cameraModes;

    public Camera(float width, float height, Vector3 position)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
        Camera.position = position;
        
        Instance = this;
        
        _cameraModes = new Dictionary<CameraMode, Action>
        {
            {CameraMode.Free, Instance.FreeCamera},
            {CameraMode.Fixed, Instance.FixedCamera}
        };
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
    
    public static void SetCameraMode(CameraMode mode)
    {
        _cameraMode = mode;
    }
    

    public static void UpdateVectors()
    {
        front.X = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Cos(MathHelper.DegreesToRadians(yaw));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
        front.Z = MathF.Cos(MathHelper.DegreesToRadians(pitch)) * MathF.Sin(MathHelper.DegreesToRadians(yaw));
        
        front = Vector3.Normalize(front);
        right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
        up = Vector3.Normalize(Vector3.Cross(right, front));
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
    
    public static float GetYaw()
    {
        return yaw;
    }

    public override void Start()
    {
        
    }

    public override void Update()
    {
        _cameraModes[_cameraMode].Invoke();
    }
    
    public void UpdateProjectionMatrix(int width, int height)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
    }


    private void FreeCamera()
    {
        if (!Game.MoveTest)
        {
            float speed = SPEED * GameTime.DeltaTime;

            if (InputManager.IsDown(Keys.W))
            {
                position += Yto0(front) * speed;
            }

            if (InputManager.IsDown(Keys.A))
            {
                position -= Yto0(right) * speed;
            }

            if (InputManager.IsDown(Keys.S))
            {
                position -= Yto0(front) * speed;
            }

            if (InputManager.IsDown(Keys.D))
            {
                position += Yto0(right) * speed;
            }

            if (InputManager.IsDown(Keys.Space))
            {
                position.Y += speed;
            }

            if (InputManager.IsDown(Keys.LeftShift))
            {
                position.Y -= speed;
            }
        }
        else
        {
            CameraZoom();
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
            _smoothMouseDelta = _smooth ? Vector2.Lerp(_smoothMouseDelta, currentMouseDelta, SMOOTH_FACTOR * GameTime.DeltaTime) : currentMouseDelta;
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

    private void FixedCamera()
    {
        CameraZoom();
    }

    private void CameraZoom()
    {
        float scroll = InputManager.GetMouseScroll().Y;
            
        CameraDistance -= (scroll - oldScroll) * SCROLL_SENSITIVITY;
        CameraDistance = Math.Clamp(CameraDistance, 3, 10);
        oldScroll = scroll;
    
        _targetPosition = PlayerData.EyePosition - front * CameraDistance;
        position = _positionSmooth ? Vector3.Lerp(position, _targetPosition, POSITION_SMOOTH_FACTOR * GameTime.DeltaTime) : _targetPosition;
    }
    
    public static void SetSmoothFactor(bool value)
    {
        _smooth = value;
    }
    
    public static void SetPositionSmoothFactor(bool value)
    {
        _positionSmooth = value;
    }
}

public enum CameraType
{
    FirstPerson,
    ThirdPerson
}

public enum CameraMode
{
    Free,
    Fixed
}