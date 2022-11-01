using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public class CharacterPhysics : MonoBehaviour
{
    #region Movement Fields
    [SerializeField] private float _baseMovementSpeed = 20f;
    private Vector3 _previousMovementDirection;

    #endregion
    
    #region Collider Fields
    private float _boxWidth = 0.75f;
    #endregion

    #region Movement Properties
    public Vector3 LatestContactPosition { get; set; }
    public Vector3 LatestContactNormal { get; set; }
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
    public Vector3 DeltaPosition => (Time.timeScale == 0f) ? Vector3.zero : MovementVelocity * Time.fixedDeltaTime;
    #endregion

    public event Action OnCollision;
    
    private void FixedUpdate()
    {
        Move();
        ColliderCheck();
    }

    private void Move()
    {
        ResolvePotentialPathClipping(DeltaPosition);
        transform.position += DeltaPosition;
    }

    private void ResolvePotentialPathClipping(Vector3 deltaPosition)
    {
        // Prevent box missing collisions with walls
        (bool didRayHit, RaycastHit raycastHit) = Utilities.Unity.Raycast(transform.position, deltaPosition, gameObject.layer);
        if (didRayHit)
        {
            // Set position to be flush with wall
            transform.position = raycastHit.point - deltaPosition.normalized * _boxWidth / 2f;
            
            Reflect(raycastHit.normal);
            
            LatestContactPosition = raycastHit.point;
            LatestContactNormal = raycastHit.normal;
            OnCollision?.Invoke();
        }
    }
    
    private void ResolvePotentialColliderClipping(List<Vector3> overshoots)
    {
        if (overshoots.Count > 0)
        {
            var totalOvershoot = LatestContactPosition;
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
            OnCollision?.Invoke();
        }
    }

    private void ColliderCheck() // TODO: Reduce this.
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
        
        // TODO: I'm confused with what this is doing now. 
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
        LatestContactPosition = point / overshoots.Count;
        LatestContactNormal = normal / overshoots.Count;
        
        ResolvePotentialColliderClipping(overshoots);
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
        Debug.DrawRay(LatestContactPosition, MovementDirection * 3f, Gizmos.color);
        Gizmos.color = Color.red;
        Debug.DrawRay(LatestContactPosition, -_previousMovementDirection * 3f, Gizmos.color);
    }
}
