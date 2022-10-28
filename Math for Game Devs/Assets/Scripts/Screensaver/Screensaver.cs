using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class Screensaver : MonoBehaviour
{
    [SerializeField] private Transform _logo;
    [SerializeField] private float _movementSpeed = 1;
    [SerializeField] private List<Transform> _corners = new List<Transform>();
    [SerializeField] private Transform _outerCube;
    [SerializeField] private Oscillator _outerCubeOscillator;
    [SerializeField] private ParticleSystem _bounceParticleSystem;
    [SerializeField] private Material _tvMaterial;
    [SerializeField] private List<Color> _colors;
    
    private Vector3 _movementVelocity
    {
        get => _movementDirection * _movementSpeed;
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
        _movementDirection = new Vector3(4.25f, -4.4f, 0f).normalized;
        _previousMovementDirection = _movementDirection;
    }

    private void FixedUpdate()
    {
        Move();
        
        UpdateCrowdEffects();
        
        CollisionCheck();
    }

    private void Move()
    {
        var speed = _movementSpeed * Mathf.Clamp((1 + 0.0125f * _consecutiveMissedCorners), 1f, 1.25f);
        
        _logo.position += _movementDirection * speed * Time.fixedDeltaTime;
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
                var magnitude = 0.5f; // This is the distance from the centre of the cube to to the edge of the cube
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

            if (raycastHit.distance < 0.4f) // Bounce artificially late, for more lenient corner detection and greater impact.
            {
                Bounce(raycastHit.point, raycastHit.normal);
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
        Maths.CosineRule((a + n).magnitude, out theta, n.magnitude, a.magnitude); // TODO: Implement homemade Magnitude().
        
        // Calculate cross product of A and N.
        var axisOfRotation = Vector3.Cross(a, n); // TODO: Implement homemade CrossProduct().

        // Rotate _movementDirection about the cross product's axis by THETA.
        theta = Mathf.Rad2Deg * 2f * (Mathf.PI / 2f - theta);
        var b = Quaternion.AngleAxis(theta, axisOfRotation) * a;

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
