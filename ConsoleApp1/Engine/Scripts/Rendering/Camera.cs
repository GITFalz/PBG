using System.Numerics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

public class Camera
{
    public float SPEED { get; private set; } = 50f;
    public float SCREEN_WIDTH { get; private set; }
    public float SCREEN_HEIGHT { get; private set; }
    public float SENSITIVITY { get; private set; } = 65f;
    public float SCROLL_SENSITIVITY { get; private set; } = 0.4f;
    
    public float FOV { get; private set; } = 45f;
    public float targetFOV { get; private set; } = 45f;
    public float FOV_SMOTH_FACTOR { get; private set; } = 100f;

    public Vector3 position;
    public float pitch = 0;
    public float yaw = -90;
    
    public Vector3 up = Vector3.UnitY;
    public Vector3 front = -Vector3.UnitZ;
    public Vector3 right = Vector3.UnitX;
    
    private float CameraDistance = 5;
    private float oldScroll = 0;

    public bool firstMove = true;
    public Vector2 lastPos;
    
    public Matrix4 viewMatrix;
    public Matrix4 projectionMatrix;
    
    
    private Vector2 _smoothMouseDelta = Vector2.Zero;
    private bool _smooth = false;
    private float SMOOTH_FACTOR = 5f;
    
    private Vector3 _targetPosition;
    private bool _positionSmooth = false;
    private float POSITION_SMOOTH_FACTOR = 50f;
    
    private CameraMode _cameraMode = CameraMode.Fixed;
    
    private Dictionary<CameraMode, Action> _cameraModes;

    public Camera(float width, float height, Vector3 position)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
        this.position = position;
        
        _cameraModes = new Dictionary<CameraMode, Action>
        {
            {CameraMode.Free, FreeCamera},
            {CameraMode.Fixed, FixedCamera}
        };
    }

    public Matrix4 GetViewMatrix()
    {
        viewMatrix = Matrix4.LookAt(position, position + front, up);
        return viewMatrix;
    }
    
    public Matrix4 GetProjectionMatrix()
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
    
    public Matrix4x4 GetNumericsViewMatrix()
    {
        return Mathf.ToNumericsMatrix4(GetViewMatrix());
    }
    
    public Matrix4x4 GetNumericsProjectionMatrix()
    {
        return Mathf.ToNumericsMatrix4(GetProjectionMatrix());
    }
    
    public Matrix4 GetViewProjectionMatrix()
    {
        return GetProjectionMatrix() * GetViewMatrix();
    }

    public void SetFOV(float fov)
    {
        targetFOV = fov;
        projectionMatrix = GetProjectionMatrix();
    }
    
    public void SetCameraMode(CameraMode mode)
    {
        _cameraMode = mode;
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

    public Vector3 Yto0(Vector3 v)
    {
        v.Y = 0;
        return Vector3.Normalize(v);
    }
    
    public Vector3 FrontYto0()
    {
        Vector3 v = front;
        v.Y = 0;
        return Vector3.Normalize(v);
    }
    
    public Vector3 Front()
    {
        return front;
    }
    
    public Vector3 RightYto0()
    {
        Vector3 v = right;
        v.Y = 0;
        return Vector3.Normalize(v);
    }
    
    public float GetYaw()
    {
        return yaw;
    }

    public void Start()
    {
        
    }

    public void Update()
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

            if (Input.IsKeyDown(Keys.W))
            {
                position += Yto0(front) * speed;
            }

            if (Input.IsKeyDown(Keys.A))
            {
                position -= Yto0(right) * speed;
            }

            if (Input.IsKeyDown(Keys.S))
            {
                position -= Yto0(front) * speed;
            }

            if (Input.IsKeyDown(Keys.D))
            {
                position += Yto0(right) * speed;
            }

            if (Input.IsKeyDown(Keys.Space))
            {
                position.Y += speed;
            }

            if (Input.IsKeyDown(Keys.LeftShift))
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
            lastPos = Input.GetMousePosition();
            firstMove = false;
        }
        else
        {
            Vector2 pos = Input.GetMousePosition();
            
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
        float scroll = Input.GetMouseScroll().Y;
            
        CameraDistance -= (scroll - oldScroll) * SCROLL_SENSITIVITY;
        CameraDistance = Math.Clamp(CameraDistance, 3, 10);
        oldScroll = scroll;
    
        _targetPosition = (0, 0, 0) - front * CameraDistance;
        position = _positionSmooth ? Vector3.Lerp(position, _targetPosition, POSITION_SMOOTH_FACTOR * GameTime.DeltaTime) : _targetPosition;
    }
    
    public void SetSmoothFactor(bool value)
    {
        _smooth = value;
    }
    
    public void SetPositionSmoothFactor(bool value)
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