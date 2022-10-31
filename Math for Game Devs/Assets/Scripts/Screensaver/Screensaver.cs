using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class Screensaver : MonoBehaviour
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
    
    [SerializeField] private Transform _logo;
    [SerializeField] private float _baseMovementSpeed = 1;
    [SerializeField] private List<Transform> _corners = new List<Transform>();
    [SerializeField] private Transform _outerCube;
    [SerializeField] private Oscillator _outerCubeOscillator;
    [SerializeField] private ParticleSystem _bounceParticleSystem;
    [SerializeField] private Material _tvMaterial;
    [SerializeField] private List<Color> _colors;
    
    private Vector3 _movementVelocity
    {
        get => _movementDirection * MovementSpeed;
    }
    private Vector3 _movementDirection;
    private Vector3 _previousMovementDirection;
    private Vector3 _latestContactPosition;
    private int _colorIndex;
    private Vector3 _latestContactNormal;
    private int _consecutiveMissedCorners;

    private void Start()
    {
        // Start moving in an arbitrary direction.
        //_movementDirection = Random.onUnitSphere;
        //_movementDirection = new Vector3(4.25f, -4.4f, 0f).normalized;
        _previousMovementDirection = _movementDirection;
    }

    private void FixedUpdate()
    {
        Move();
        CollisionCheck();
        
        UpdateCrowdEffects();
    }

    private void Move()
    {
        // TODO: Reset speed multiplier if 'space' pressed when bouncing (with some input interval).
        var deltaPosition = _movementDirection * MovementSpeed * Time.fixedDeltaTime;
        
        // Prevent box missing collisions with walls
        (bool didRayHit, RaycastHit raycastHit) = Raycast(deltaPosition);
        if (didRayHit)
        {
            Bounce(raycastHit.point, raycastHit.normal); // TODO: Check that Bounce isn't getting called here AND in the collider thing
        }
        //_logo.position += didRayHit ? (raycastHit.point - transform.position) / 2f : deltaPosition;
        
        deltaPosition = _movementDirection * MovementSpeed * Time.fixedDeltaTime;

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
                var magnitude = 0.75f/2f; // This is the distance from the centre of the cube to to the edge of the cube
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
        for (int i = 0; i < raysThatHit.Count; i++)
        {
            var rayVector = raysThatHit[i];
            var raycastHit = raycastHits[i];

            if (raycastHit.distance < rayVector.magnitude * 0.8f) // Bounce artificially late, for more lenient corner detection and greater impact.
            {
                // TODO: Only bounce if moving in the direction of the wall (to prevent double bouncing)
                if (Vector3.Dot(_movementDirection, rayVector) > 0)
                {
                    Bounce(raycastHit.point, raycastHit.normal);
                }
            }
            
            if (raysThatHit.Count == 1)
            {
                _consecutiveMissedCorners++;
                return;
            }
            
            // Otherwise, a corner is hit.
            isCorner = true;
        }
        
        if (isCorner)
        {
            FindObjectOfType<ColorSelector>().Flash();
            _consecutiveMissedCorners = 0;
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

    private void NextColor()
    {
        var increment = _colors.Count / 2;
        if (_colors.Count % 2 == 0)
        {
            // If even number of colours, then increase the increment in order to go up an odd amount (So that no colours are left unused).
            increment++;
        }
        _colorIndex += increment;
        _colorIndex %= _colors.Count;
        var color = _colors[_colorIndex];
        _tvMaterial.color = color;
    }

    private void Bounce(Vector3 position, Vector3 normal)
    {
        _latestContactPosition = position;
        _latestContactNormal = normal;
        
        // TODO: Freeze time on hit

        Reflect(normal);
        
        ApplyForceToOscillator(normal);

        PlayBounceParticleSystem(position, normal);
        
        NextColor();

        FindObjectOfType<CrowdEffects>().CrowdState = CrowdState.Disappointed; // TODO: Check if corner
    }

    private void ApplyForceToOscillator(Vector3 n)
    {
        // Apply force to oscillator
        _outerCubeOscillator.ApplyForce(Vector3.Dot(_movementVelocity, n) * n * -25f);
    }

    private void PlayBounceParticleSystem(Vector3 p, Vector3 n)
    {
        // Set particle system to face normal direction
        _bounceParticleSystem.transform.LookAt(n); // Face normal
        //_bounceParticleSystem.transform.LookAt(_movementVelocity); // Face new velocity
        
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = p;
        emitParams.applyShapeToPosition = true;
        
        // Trigger particle emission burst
        _bounceParticleSystem.Emit(emitParams, 6);
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
        
        Debug.DrawRay(_bounceParticleSystem.transform.position, _latestContactNormal, Color.blue);
    }
}
