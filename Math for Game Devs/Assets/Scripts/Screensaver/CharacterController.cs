using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

public class CharacterController : MonoBehaviour
{
    #region Movement Fields
    [SerializeField] private float _baseMovementSpeed = 1;
    private Vector3 _previousMovementDirection;
    private Vector3 _latestContactPosition;
    private Vector3 _latestContactNormal;
    #endregion
    
    #region Collider Fields
    private float _boxWidth = 0.75f;
    #endregion
    
    #region Input Buffer Fields
    private float _preInputBufferDistance = 0f; // To be reset when input pressed. Waits to see if condition is met.
    private float _postInputBufferDistance = 0f; // To be reset when condition met. Waits to see if input pressed.
    private const float INPUT_BUFFER = 1.5f; // Distance

    #endregion
    
    #region Movement Properties
    public float BaseMovementSpeed
    {
        get => _baseMovementSpeed;
        set => _baseMovementSpeed = value;
    }
    public Vector3 MovementDirection { get; set; }

    public int MovementSpeedMultiplierCount { get; set; }
    public float MovementSpeedMultiplier => Mathf.Clamp((1f - 0.025f * MovementSpeedMultiplierCount), 0.1f, 1f);
    public float MovementSpeed => BaseMovementSpeed * MovementSpeedMultiplier;
    public Vector3 MovementVelocity => MovementSpeed * MovementDirection;
    #endregion

    private void Awake()
    {
        _previousMovementDirection = MovementDirection;
    }

    private void Update()
    {
        InputBuffer();
    }
    
    private void InputBuffer()
    {
        // Using distance (= speed * time) for the buffer, as time felt inconsistent with varying speeds.
        _preInputBufferDistance -= Time.deltaTime * MovementSpeed;
        _postInputBufferDistance -= Time.deltaTime * MovementSpeed;
    }

    public void BounceInput(InputAction.CallbackContext context)
    {
        // When button pressed, reset coyote time counter
        if (context.performed)
        {
            if (_postInputBufferDistance >= 0f)
            {
                MovementSpeedMultiplierCount = 0;
                FindObjectOfType<HitEffects>().BounceEffects(_latestContactPosition, _latestContactNormal);
                _postInputBufferDistance = 0f;
            }
            else
            {
                _preInputBufferDistance = INPUT_BUFFER;
            }
        }
    }
    
    private void FixedUpdate()
    {
        Move();
        CollisionCheck();
    }

    private void Move()
    {
        var deltaPosition = MovementVelocity * Time.fixedDeltaTime;

        CheckPath(deltaPosition);

        deltaPosition = (Time.timeScale == 0f) ? Vector3.zero : MovementVelocity * Time.fixedDeltaTime;
        
        transform.position += deltaPosition;
    }

    private void CheckPath(Vector3 deltaPosition)
    {
        // Prevent box missing collisions with walls
        (bool didRayHit, RaycastHit raycastHit) = Utilities.Unity.Raycast(transform.position, deltaPosition, gameObject.layer);
        if (didRayHit)
        {
            // Set position to be flush with wall
            transform.position = raycastHit.point - deltaPosition.normalized * _boxWidth / 2f;
            
            Reflect(raycastHit.normal);
            
            // TODO: Move this out of here. Have separate scripts for the physics and for the inputs.
            // Check input buffer
            _latestContactPosition = raycastHit.point;
            _latestContactNormal = raycastHit.normal;
            if (_preInputBufferDistance >= 0f)
            {
                SuccessfulBounce();
                _preInputBufferDistance = 0f;
            }
            else
            {
                _postInputBufferDistance = INPUT_BUFFER;
            }
        }
    }

    private void SuccessfulBounce()
    {
        MovementSpeedMultiplierCount = 0;
        FindObjectOfType<HitEffects>().BounceEffects(_latestContactPosition, _latestContactNormal);
    }

    private void CollisionCheck()
    {
        var raysThatHit = new List<Vector3>();
        var raycastHits = new List<RaycastHit>();
        for (int dimension = 0; dimension < 3; dimension++)
        {
            for (int direction = -1; direction <= 1; direction += 2)
            {
                var rayVector = Vector3.zero;
                var magnitude = _boxWidth / 2f; // This is the distance from the centre of the cube to to the edge of the cube
                rayVector[dimension] = 1;
                rayVector *= direction;
                rayVector *= magnitude;
                (bool didRayHit, RaycastHit raycastHit) = Utilities.Unity.Raycast(transform.position, rayVector, gameObject.layer);

                if (didRayHit)
                {
                    raysThatHit.Add(rayVector);
                    raycastHits.Add(raycastHit);
                }
            }
        }

        bool isCorner = false;
        var point = Vector3.zero;
        var normal = Vector3.zero;
        var overshoots = new List<Vector3>();
        for (int i = 0; i < raysThatHit.Count; i++)
        {
            var rayVector = raysThatHit[i];
            var raycastHit = raycastHits[i];

            if (raycastHit.distance <= rayVector.magnitude * 0.8f) // Bounce artificially late, for more lenient corner detection and greater impact.
            {
                // Only bounce if moving in the direction of the wall (to prevent double bouncing)
                if (Vector3.Dot(MovementDirection, rayVector) > 0f)
                {
                    point += raycastHit.point;
                    normal += raycastHit.normal;
                    
                    Reflect(raycastHit.normal);
                    overshoots.Add(Maths.ComponentWiseProduct((raycastHit.point - rayVector), Maths.Absolute(rayVector.normalized)));
                }
            }

            if (raysThatHit.Count == 1)
            {
                MovementSpeedMultiplierCount++;
                break;
            }
            
            // Otherwise, a corner is hit.
            isCorner = true;
        }
        
        if (overshoots.Count > 0)
        {
            point /= overshoots.Count;
            normal /= overshoots.Count;
            var totalOvershoot = point;
            foreach (var overshoot in overshoots)
            {
                for (int i = 0; i < 3; i++)
                {
                    var axis = overshoot[i];
                    if (axis != 0)
                    {
                        totalOvershoot[i] = axis;
                    }
                }
            }
            transform.position = totalOvershoot;
            //BounceEffects(point, normal);
            
            _latestContactPosition = point;
            _latestContactNormal = normal;
            if (_preInputBufferDistance >= 0f)
            {
                MovementSpeedMultiplierCount = 0;
                FindObjectOfType<HitEffects>().BounceEffects(point, normal); // Change this to an event, which is subscribed to by HitEffects
                _preInputBufferDistance = 0f;
            }
            else
            {
                _postInputBufferDistance = INPUT_BUFFER;
            }
        }

        if (isCorner)
        {
            //FindObjectOfType<ColorSelector>().Flash();
            //_consecutiveMissedCorners = 0;
        }
    }

    private void Reflect(Vector3 n)
    {
        var a = MovementDirection;
        
        // Let THETA = angle between A and N.
        float theta;
        Maths.CosineRule((a + n).magnitude, out theta, n.magnitude, a.magnitude);

        // Calculate cross product of A and N.
        var axisOfRotation = Vector3.Cross(a, n);

        // Rotate _movementDirection about the cross product's axis by THETA.
        theta = Mathf.Rad2Deg * 2f * (Mathf.PI / 2f - theta);
        var b = Quaternion.AngleAxis(theta, axisOfRotation) * a;
        
        // Reverse the direction in the case that theta = 0;
        if (a == b)
        {
            b = -a;
        }

        _previousMovementDirection = MovementDirection;
        MovementDirection = b;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Debug.DrawRay(_latestContactPosition, MovementDirection * 3f, Gizmos.color);
        Gizmos.color = Color.red;
        Debug.DrawRay(_latestContactPosition, -_previousMovementDirection * 3f, Gizmos.color);
    }
}
