using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

public class CharacterController : MonoBehaviour
{
    public float BaseMovementSpeed
    {
        get => _baseMovementSpeed;
        set => _baseMovementSpeed = value;
    }
    public float MovementSpeedMultiplier
    {
        get => Mathf.Clamp((1f - 0.025f * _consecutiveMissedCorners), 0.1f, 1f);
    }
    public float MovementSpeed
    {
        get => BaseMovementSpeed * MovementSpeedMultiplier;
    }
    public Vector3 MovementDirection
    {
        get => _movementDirection;
        set => _movementDirection = value;
    }
    public int ConsecutiveMissedCorners
    {
        get => _consecutiveMissedCorners;
        set => _consecutiveMissedCorners = value;
    }
    public Vector3 MovementVelocity
    {
        get =>  MovementSpeed * MovementDirection;
    }
    
    [SerializeField] private Transform _logo;
    [SerializeField] private float _baseMovementSpeed = 1;
    [SerializeField] private List<Transform> _corners = new List<Transform>();
    [SerializeField] private Transform _outerCube;
    


    
    private Vector3 _movementDirection;
    private Vector3 _previousMovementDirection;
    private Vector3 _latestContactPosition;
    private Vector3 _latestContactNormal;
    private int _consecutiveMissedCorners;
    private float _boxWidth = 0.75f;

    private void Start()
    {
        // Start moving in an arbitrary direction.
        //_movementDirection = Random.onUnitSphere;
        //_movementDirection = new Vector3(4.25f, -4.4f, 0f).normalized;
        _previousMovementDirection = _movementDirection;
    }

    private void Update()
    {
        InputBuffer();
    }

    private void FixedUpdate()
    {
        Move();
        CollisionCheck();
        
        UpdateCrowdEffects();
    }


    private float _preInputBufferTimer = 0f; // To be reset when input pressed. Waits to see if condition is met.
    private float _postInputBufferTimer = 0f; // To be reset when condition met. Waits to see if input pressed.
    private float _inputBuffer = 1.5f; // Distance

    private void InputBuffer()
    {
        _preInputBufferTimer -= Time.deltaTime * MovementSpeed; // Speed = Distance / Time --> Distance = Speed * Time
        _postInputBufferTimer -= Time.deltaTime * MovementSpeed; // Speed = Distance / Time --> Distance = Speed * Time
    }
    
    public void BounceInput(InputAction.CallbackContext context)
    {
        // When button pressed, reset coyote time counter
        if (context.performed)
        {
            if (_postInputBufferTimer >= 0f)
            {
                _consecutiveMissedCorners = 0;
                FindObjectOfType<HitEffects>().BounceEffects(_latestContactPosition, _latestContactNormal);
                _postInputBufferTimer = 0f;
            }
            else
            {
                _preInputBufferTimer = _inputBuffer;
            }
        }
    }

    private void Move()
    {
        // TODO: Reset speed multiplier if 'space' pressed when bouncing (with some input interval).
        var deltaPosition = _movementDirection * MovementSpeed * Time.fixedDeltaTime;
        
        // Prevent box missing collisions with walls
        (bool didRayHit, RaycastHit raycastHit) = Raycast(deltaPosition);
        if (didRayHit)
        {
            Reflect(raycastHit.normal);
            _logo.position = raycastHit.point - deltaPosition.normalized * _boxWidth / 2f;

            //BounceEffects(raycastHit.point, raycastHit.normal);
            
            _latestContactPosition = raycastHit.point;
            _latestContactNormal = raycastHit.normal;
            if (_preInputBufferTimer >= 0f)
            {
                _consecutiveMissedCorners = 0;
                FindObjectOfType<HitEffects>().BounceEffects(raycastHit.point, raycastHit.normal);
                _preInputBufferTimer = 0f;
            }
            else
            {
                _postInputBufferTimer = _inputBuffer;
            }
        }
        deltaPosition = (Time.timeScale == 0f) ? Vector3.zero : _movementDirection * MovementSpeed * Time.fixedDeltaTime;
        
        _logo.position += deltaPosition;
    }

    private void UpdateCrowdEffects()
    {
        var displacementToNearestCorner = CalculateDisplacementToNearestCorner();
        FindObjectOfType<CrowdEffects>().DisplacementToNearestCorner = displacementToNearestCorner;
        FindObjectOfType<CrowdEffects>().CurrentDirection = _movementDirection;
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
                (bool didRayHit, RaycastHit raycastHit) = Raycast(rayVector);

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
                if (Vector3.Dot(_movementDirection, rayVector) > 0f)
                {
                    point += raycastHit.point;
                    normal += raycastHit.normal;
                    
                    Reflect(raycastHit.normal);
                    overshoots.Add(Maths.ComponentWiseProduct((raycastHit.point - rayVector), Maths.Absolute(rayVector.normalized)));
                }
            }

            if (raysThatHit.Count == 1)
            {
                _consecutiveMissedCorners++;
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
                        //Debug.Log(i + " " + axis);
                        totalOvershoot[i] = axis;
                    }
                }
            }
            //Debug.Log(totalOvershoot);
            _logo.position = totalOvershoot;
            //BounceEffects(point, normal);
            
            _latestContactPosition = point;
            _latestContactNormal = normal;
            if (_preInputBufferTimer >= 0f)
            {
                _consecutiveMissedCorners = 0;
                FindObjectOfType<HitEffects>().BounceEffects(point, normal);
                _preInputBufferTimer = 0f;
            }
            else
            {
                _postInputBufferTimer = _inputBuffer;
            }
        }

        if (isCorner)
        {
            //FindObjectOfType<ColorSelector>().Flash();
            //_consecutiveMissedCorners = 0;
        }
    }

    private Vector3 CalculateDisplacementToNearestCorner()
    {
        var shortestDisplacement = Vector3.one * 100f;
        // Check nearest corner
        foreach (Transform corner in _corners)
        {
            // Calculate displacement
            var displacement = (corner.position - _logo.position);
            // Store shortest displacement
            if (displacement.magnitude < shortestDisplacement.magnitude)
            {
                shortestDisplacement = displacement;
            }
        }

        var maximumDistance = _outerCube.lossyScale.magnitude / 2f;
        var displacementToNearestCorner = shortestDisplacement / maximumDistance;
        return (displacementToNearestCorner);
    }
    
    private (bool, RaycastHit) Raycast(Vector3 rayVector)
    {
        RaycastHit raycastHit;
        Ray ray = new Ray(transform.position, rayVector.normalized);
        int layerMask =~ gameObject.layer;
        bool didRayHit = Physics.Raycast(ray, out raycastHit, rayVector.magnitude, layerMask);
        //Debug.DrawRay(transform.position, rayVector, Color.blue);
        return (didRayHit, raycastHit);
    }








    private void Reflect(Vector3 n)
    {
        var a = _movementDirection;
        
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

        _previousMovementDirection = _movementDirection;
        _movementDirection = b;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Debug.DrawRay(_latestContactPosition, _movementDirection * 3f, Gizmos.color);
        Gizmos.color = Color.red;
        Debug.DrawRay(_latestContactPosition, -_previousMovementDirection * 3f, Gizmos.color);
    }
}
