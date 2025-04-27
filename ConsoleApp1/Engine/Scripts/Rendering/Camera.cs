using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vortice.Mathematics;
using Plane = System.Numerics.Plane;

public class Camera
{
    public static Camera Empty = new();

    public float SPEED { get; private set; } = 50f;
    public float SCREEN_WIDTH { get; set; }
    public float SCREEN_HEIGHT { get; set; }
    public float VERTICAL_SENSITIVITY { get; private set; } = 110f;
    public float HORIZONTAL_SENSITIVITY { get; private set; } = 80f;
    public float SCROLL_SENSITIVITY { get; private set; } = 0.4f;
    
    public float FOV { get; private set; } = 45f;
    public float targetFOV { get; private set; } = 45f;
    public float FOV_SMOTH_FACTOR { get; private set; } = 10f;

    private Vector3 _lastPosition = (0, 0, 0);
    public Vector3 Position = (0, 0, 0);

    public Vector3 Center = (0, 0, 0);
    private Vector3 _currentCenter = (0, 0, 0);
    private float _centerLerpSpeed = 30f;

    public float Pitch = 0;
    public float Yaw = -90;
    
    public Vector3 up = Vector3.UnitY;
    private Vector3 _lastFront = Vector3.Zero;
    public Vector3 front = -Vector3.UnitZ;
    public Vector3 right = Vector3.UnitX;
    
    private float CameraDistance = 5;
    public float WantedCameraDistance = 5;
    private float oldScroll = 0;

    public Vector2 pos;
    public Vector2 lastPos;
    
    public Matrix4 ViewMatrix;
    public Matrix4 ProjectionMatrix;
    
    
    private Vector2 _smoothMouseDelta = Vector2.Zero;
    private bool _smooth = false;
    private float SMOOTH_FACTOR = 5f;
    
    private Vector3 _targetPosition;
    private bool _positionSmooth = false;
    private float POSITION_SMOOTH_FACTOR = 50f;
    
    private CameraMode _cameraMode = CameraMode.Fixed;
    
    private Dictionary<CameraMode, Action> _cameraModes;
    private Action _updateAction = () => { };
    
    public Action FirstMove = () => { };

    

    private Plane[] frustumPlanes = new Plane[6];
    public Plane LeftPlane => frustumPlanes[0];
    public Plane RightPlane => frustumPlanes[1];
    public Plane BottomPlane => frustumPlanes[2];
    public Plane TopPlane => frustumPlanes[3];
    public Plane NearPlane => frustumPlanes[4];
    public Plane FarPlane => frustumPlanes[5];

    public Camera()
    {
        SCREEN_WIDTH = 1280;
        SCREEN_HEIGHT = 720;
        
        _cameraModes = [];
    }

    public Camera(float width, float height, Vector3 position)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
        this.Position = position;
        
        _cameraModes = new Dictionary<CameraMode, Action>
        {
            {CameraMode.Free, FreeCamera},
            {CameraMode.Fixed, FixedCamera},
            {CameraMode.Follow, FollowCamera},
            {CameraMode.Centered, CenteredCamera}
        };

        FirstMove = FirstMove1;

        _updateAction = _cameraModes[_cameraMode];
    }

    public Matrix4 GetViewMatrix()
    {
        ViewMatrix = Matrix4.LookAt(Position, Position + front, up);
        return ViewMatrix;
    }
    
    public Matrix4 GetProjectionMatrix()
    {
        ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
            OpenTK.Mathematics.MathHelper.DegreesToRadians(FOV),
            SCREEN_WIDTH / SCREEN_HEIGHT, 
            0.1f, 
            1000f
        );
        return ProjectionMatrix;
    }

    public Matrix4 GetRotationMatrix()
    {
        return Matrix4.CreateRotationX(OpenTK.Mathematics.MathHelper.DegreesToRadians(Pitch)) * Matrix4.CreateRotationY(OpenTK.Mathematics.MathHelper.DegreesToRadians(Yaw));
    }

    public void CalculateFrustumPlanes()
    {
        Matrix4 viewProjectionMatrix = GetViewProjectionMatrix();
        System.Numerics.Matrix4x4 viewProjectionMatrixNumerics = Mathf.ToNumerics(viewProjectionMatrix);

        // Extract the frustum planes from the view-projection matrix
        frustumPlanes[0] = new Plane( // Left
            viewProjectionMatrixNumerics.M14 + viewProjectionMatrixNumerics.M11,
            viewProjectionMatrixNumerics.M24 + viewProjectionMatrixNumerics.M21,
            viewProjectionMatrixNumerics.M34 + viewProjectionMatrixNumerics.M31,
            viewProjectionMatrixNumerics.M44 + viewProjectionMatrixNumerics.M41
        );

        frustumPlanes[1] = new Plane( // Right
            viewProjectionMatrixNumerics.M14 - viewProjectionMatrixNumerics.M11,
            viewProjectionMatrixNumerics.M24 - viewProjectionMatrixNumerics.M21,
            viewProjectionMatrixNumerics.M34 - viewProjectionMatrixNumerics.M31,
            viewProjectionMatrixNumerics.M44 - viewProjectionMatrixNumerics.M41
        );

        frustumPlanes[2] = new Plane( // Bottom
            viewProjectionMatrixNumerics.M14 + viewProjectionMatrixNumerics.M12,
            viewProjectionMatrixNumerics.M24 + viewProjectionMatrixNumerics.M22,
            viewProjectionMatrixNumerics.M34 + viewProjectionMatrixNumerics.M32,
            viewProjectionMatrixNumerics.M44 + viewProjectionMatrixNumerics.M42
        );

        frustumPlanes[3] = new Plane( // Top
            viewProjectionMatrixNumerics.M14 - viewProjectionMatrixNumerics.M12,
            viewProjectionMatrixNumerics.M24 - viewProjectionMatrixNumerics.M22,
            viewProjectionMatrixNumerics.M34 - viewProjectionMatrixNumerics.M32,
            viewProjectionMatrixNumerics.M44 - viewProjectionMatrixNumerics.M42
        );

        frustumPlanes[4] = new Plane( // Near
            viewProjectionMatrixNumerics.M14 + viewProjectionMatrixNumerics.M13,
            viewProjectionMatrixNumerics.M24 + viewProjectionMatrixNumerics.M23,
            viewProjectionMatrixNumerics.M34 + viewProjectionMatrixNumerics.M33,
            viewProjectionMatrixNumerics.M44 + viewProjectionMatrixNumerics.M43
        );

        frustumPlanes[5] = new Plane( // Far
            viewProjectionMatrixNumerics.M14 - viewProjectionMatrixNumerics.M13,
            viewProjectionMatrixNumerics.M24 - viewProjectionMatrixNumerics.M23,
            viewProjectionMatrixNumerics.M34 - viewProjectionMatrixNumerics.M33,
            viewProjectionMatrixNumerics.M44 - viewProjectionMatrixNumerics.M43
        );

        // Normalize the planes
        for (int i = 0; i < 6; i++)
        {
            frustumPlanes[i] = Plane.Normalize(frustumPlanes[i]);
        }
    }

    public bool FrustumIntersects(BoundingBox boundingBox)
    {
        foreach (var plane in frustumPlanes)
        {
            // Get the positive vertex (furthest point in direction of the plane normal)
            var positive = new System.Numerics.Vector3(
                plane.Normal.X >= 0 ? boundingBox.Max.X : boundingBox.Min.X,
                plane.Normal.Y >= 0 ? boundingBox.Max.Y : boundingBox.Min.Y,
                plane.Normal.Z >= 0 ? boundingBox.Max.Z : boundingBox.Min.Z
            );

            // If the positive vertex is behind the plane, the box is fully outside
            if (Plane.DotCoordinate(plane, positive) < 0)
                return false;
        }
        return true;
    }
    
    public System.Numerics.Matrix4x4 GetNumericsViewMatrix()
    {
        return Mathf.ToNumerics(GetViewMatrix());
    }
    
    public System.Numerics.Matrix4x4 GetNumericsProjectionMatrix()
    {
        return Mathf.ToNumerics(GetProjectionMatrix());
    }
    
    public Matrix4 GetViewProjectionMatrix()
    {
        return GetViewMatrix() * GetProjectionMatrix();
    }

    public void SetFOV(float fov)
    {
        targetFOV = Mathf.Clamp(1, 179, fov);
    }
    
    public void SetCameraMode(CameraMode mode)
    {
        _cameraMode = mode;
        _updateAction = _cameraModes[mode];
    }

    public CameraMode GetCameraMode()
    {
        return _cameraMode;
    }

    public void UpdateVectors()
    {
        front.X = MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(Pitch)) * MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(Yaw));
        front.Y = MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(Pitch));
        front.Z = MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(Pitch)) * MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(Yaw));
        
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
        return Yaw;
    }

    public void Lock()
    {
        _updateAction = () => { };
    }

    public void Unlock()
    {
        _updateAction = _cameraModes[_cameraMode];
    }

    public void Update()
    {
        _lastPosition = Position;
        _lastFront = front;
        FOV = Mathf.Lerp(FOV, targetFOV, FOV_SMOTH_FACTOR * GameTime.DeltaTime);
        _updateAction.Invoke();
    }
    
    public void UpdateProjectionMatrix(int width, int height)
    {
        SCREEN_WIDTH = width;
        SCREEN_HEIGHT = height;
    }

    private void FreeCamera()
    {
        if (MenuManager.IsOpen)
            return;
        
        float speed = SPEED * GameTime.DeltaTime;
        Vector2 input = Input.GetMovementInput();

        if (input != Vector2.Zero)
        {
            Position += Yto0(front) * input.Y * speed;
            Position -= Yto0(right) * input.X * speed;
        }

        if (Input.IsKeyDown(Keys.Space))
        {
            Position.Y += speed;
        }

        if (Input.IsKeyDown(Keys.LeftShift))
        {
            Position.Y -= speed;
        }

        if (GetCameraMode() == CameraMode.Free)
            Info.SetPositionText(_lastPosition, Position);

        FirstMove.Invoke();

        RotateCamera();
        UpdateVectors();
    }

    private void FixedCamera()
    {
        if (MenuManager.IsOpen)
            return;

        CameraZoom();
    }

    private void FollowCamera()
    {
        if (MenuManager.IsOpen)
            return;

        RotateCamera();
        UpdateVectors();
        CameraZoom();
    }

    private void CenteredCamera()
    {
        if (MenuManager.IsOpen)
            return;
            
        Position = Center;
        RotateCamera();
        UpdateVectors();
    }


    public void SetMoveFirst()
    {
        FirstMove = FirstMove1;
    }

    public void FirstMove1()
    {
        lastPos = Input.GetMousePosition();
        FirstMove = FirstMove2;
    }

    public void FirstMove2()
    {
        lastPos = Input.GetMousePosition();
        FirstMove = () => { };
    }   

    public bool IsMoving()
    {
        return Position != _lastPosition || front != _lastFront;
    }


    private void RotateCamera()
    {
        pos = Input.GetMousePosition();

        Vector2 currentMouseDelta = new Vector2(pos.X - lastPos.X, pos.Y - lastPos.Y);
        _smoothMouseDelta = _smooth ? Vector2.Lerp(_smoothMouseDelta, currentMouseDelta, SMOOTH_FACTOR * GameTime.DeltaTime) : currentMouseDelta;

        float deltaX = _smoothMouseDelta.X;
        float deltaY = _smoothMouseDelta.Y;

        deltaX *= HORIZONTAL_SENSITIVITY * GameTime.DeltaTime;
        deltaY *= VERTICAL_SENSITIVITY * GameTime.DeltaTime;

        Yaw += deltaX;
        Pitch -= deltaY;

        Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);

        lastPos = new Vector2(pos.X, pos.Y);
    }

    private void CameraZoom()
    {
        float scroll = Input.GetMouseScroll().Y;
            
        CameraDistance -= (scroll - oldScroll) * SCROLL_SENSITIVITY;
        CameraDistance = Math.Clamp(CameraDistance, 3, 10);
        oldScroll = scroll;

        float distance = CameraDistance;

        if (VoxelData.Raycast(PlayerData.Position, -front, CameraDistance, out Hit hit))
        {
            distance = hit.Distance - 0.01f; // Adjust the distance to avoid clipping
        }

        _currentCenter = Mathf.Lerp(_currentCenter, Center, _centerLerpSpeed * GameTime.DeltaTime);
        _targetPosition = _currentCenter - front * distance;
        Position = _positionSmooth ? Vector3.Lerp(Position, _targetPosition, POSITION_SMOOTH_FACTOR * GameTime.DeltaTime) : _targetPosition;
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

public enum CameraMode
{
    Free,
    Fixed,
    Follow,
    Centered
}