using System.Numerics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Vortice.Mathematics;
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
    public float FOV_SMOTH_FACTOR { get; private set; } = 20f;

    public Vector3 Position = (0, 0, 0);

    public Vector3 Center = (0, 0, 0);
    private Vector3 _currentCenter = (0, 0, 0);
    private float _centerLerpSpeed = 30f;

    public float pitch = 0;
    public float yaw = -90;
    
    public Vector3 up = Vector3.UnitY;
    public Vector3 front = -Vector3.UnitZ;
    public Vector3 right = Vector3.UnitX;
    
    private float CameraDistance = 5;
    public float WantedCameraDistance = 5;
    private float oldScroll = 0;

    public Vector2 pos;
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
    
    public Action FirstMove;

    

    private Plane[] frustumPlanes = new Plane[6];

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
    }

    public Matrix4 GetViewMatrix()
    {
        viewMatrix = Matrix4.LookAt(Position, Position + front, up);
        return viewMatrix;
    }
    
    public Matrix4 GetProjectionMatrix()
    {
        projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
            OpenTK.Mathematics.MathHelper.DegreesToRadians(FOV),
            SCREEN_WIDTH / SCREEN_HEIGHT, 
            0.1f, 
            1000f
        );
        return projectionMatrix;
    }

    public void CalculateFrustumPlanes()
    {
        Matrix4 viewProjectionMatrix = GetViewProjectionMatrix();
        Matrix4x4 viewProjectionMatrixNumerics = Mathf.ToNumerics(viewProjectionMatrix);

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

    public bool IsPointInFrustum(Vector3 point)
    {
        foreach (var plane in frustumPlanes)
        {
            if (Plane.DotCoordinate(plane, Mathf.ToNumerics(point)) < 0)
                return false;
        }
        return true;
    }

    public bool FrustumIntersects(BoundingBox boundingBox)
    {
        foreach (var plane in frustumPlanes)
        {
            if (Plane.DotCoordinate(plane, boundingBox.Min) >= 0)
                continue;
            if (Plane.DotCoordinate(plane, new System.Numerics.Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Min.Z)) >= 0)
                continue;
            if (Plane.DotCoordinate(plane, new System.Numerics.Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z)) >= 0)
                continue;
            if (Plane.DotCoordinate(plane, new System.Numerics.Vector3(boundingBox.Max.X, boundingBox.Max.Y, boundingBox.Min.Z)) >= 0)
                continue;
            if (Plane.DotCoordinate(plane, new System.Numerics.Vector3(boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.Z)) >= 0)
                continue;
            if (Plane.DotCoordinate(plane, new System.Numerics.Vector3(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z)) >= 0)
                continue;
            if (Plane.DotCoordinate(plane, new System.Numerics.Vector3(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Max.Z)) >= 0)
                continue;
            if (Plane.DotCoordinate(plane, boundingBox.Max) >= 0)
                continue;
            return false;
        }
        return true;
    }
    
    public Matrix4x4 GetNumericsViewMatrix()
    {
        return Mathf.ToNumerics(GetViewMatrix());
    }
    
    public Matrix4x4 GetNumericsProjectionMatrix()
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
    }

    public CameraMode GetCameraMode()
    {
        return _cameraMode;
    }

    public void UpdateVectors()
    {
        front.X = MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(pitch)) * MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(yaw));
        front.Y = MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(pitch));
        front.Z = MathF.Cos(OpenTK.Mathematics.MathHelper.DegreesToRadians(pitch)) * MathF.Sin(OpenTK.Mathematics.MathHelper.DegreesToRadians(yaw));
        
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
        FOV = Mathf.Lerp(FOV, targetFOV, FOV_SMOTH_FACTOR * GameTime.DeltaTime);
        
        float speed = SPEED * GameTime.DeltaTime;
        Vector3 oldPos = Position;

        if (Input.IsKeyDown(Keys.W))
        {
            Position += Yto0(front) * speed;
        }

        if (Input.IsKeyDown(Keys.A))
        {
            Position -= Yto0(right) * speed;
        }

        if (Input.IsKeyDown(Keys.S))
        {
            Position -= Yto0(front) * speed;
        }

        if (Input.IsKeyDown(Keys.D))
        {
            Position += Yto0(right) * speed;
        }

        if (Input.IsKeyDown(Keys.Space))
        {
            Position.Y += speed;
        }

        if (Input.IsKeyDown(Keys.LeftShift))
        {
            Position.Y -= speed;
        }

        Info.SetPositionText(oldPos, Position);

        FirstMove.Invoke();

        RotateCamera();
        UpdateVectors();
    }

    private void FixedCamera()
    {
        CameraZoom();
    }

    private void FollowCamera()
    {
        RotateCamera();
        UpdateVectors();
        CameraZoom();
    }

    private void CenteredCamera()
    {
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

    private void RotateCamera()
    {
        pos = Input.GetMousePosition();

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
    }

    private void CameraZoom()
    {
        float scroll = Input.GetMouseScroll().Y;
            
        CameraDistance -= (scroll - oldScroll) * SCROLL_SENSITIVITY;
        CameraDistance = Math.Clamp(CameraDistance, 3, 10);
        oldScroll = scroll;

        _currentCenter = Mathf.Lerp(_currentCenter, Center, _centerLerpSpeed * GameTime.DeltaTime);
        _targetPosition = _currentCenter - front * CameraDistance;
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