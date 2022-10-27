using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class Screensaver : MonoBehaviour
{
    [SerializeField] private Transform _logo;
    [SerializeField] private float _movementSpeed = 1;
    [SerializeField] private List<Transform> _corners = new List<Transform>();
    [SerializeField] private Transform _outerCube;
    [SerializeField] private Oscillator _outerCubeOscillator;

    private Vector3 _movementVelocity
    {
        get => _movementDirection * _movementSpeed;
    }
    private Vector3 _movementDirection;
    private Vector3 _previousMovementDirection;
    private Vector3 _latestContactPosition;

    private void Start()
    {
        // Start moving in an arbitrary direction.
        //_movementDirection = Random.onUnitSphere;
        _movementDirection = new Vector3(4.75f, 2.7f, 0f).normalized;
        _previousMovementDirection = _movementDirection;
    }

    private void FixedUpdate()
    {
        // Move
        _logo.position += _movementDirection * _movementSpeed * Time.fixedDeltaTime;

        var displacementToNearestCorner = CalculateDisplacementToNearestCorner();
        FindObjectOfType<CrowdEffects>().DisplacementToNearestCorner = displacementToNearestCorner;
        FindObjectOfType<CrowdEffects>().CurrentDirection = _movementDirection;
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
    
    private void OnCollisionEnter(Collision collision)
    {
        _latestContactPosition = collision.GetContact(0).point;
        FindObjectOfType<CrowdEffects>().CrowdState = CrowdState.Disappointed; // TODO: Check if corner
        Reflect(collision);
    }
    
    private void OnCollisionStay(Collision collision)
    {
        _latestContactPosition = collision.GetContact(0).point;
        Reflect(collision);
    }

    private void Reflect(Collision collision)
    {
        var a = _movementDirection;
        var n = collision.GetContact(0).normal;
        
        // Let THETA = angle between A and N.
        float theta;
        Maths.CosineRule((a + n).magnitude, out theta, n.magnitude, a.magnitude); // TODO: Implement homemade Magnitude().
        
        // Calculate cross product of A and N.
        var axisOfRotation = Vector3.Cross(a, n); // TODO: Implement homemade CrossProduct().

        // Rotate _movementDirection about the cross product's axis by THETA.
        theta = Mathf.Rad2Deg * 2f * (Mathf.PI / 2f - theta);
        var b = Quaternion.AngleAxis(theta, axisOfRotation) * a;
        
        // Apply force to oscillator
        _outerCubeOscillator.ApplyForce(Vector3.Dot(_movementVelocity, n) * n * 200f);
        Debug.Log(Vector3.Dot(_movementVelocity, n) * n * 200f);

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
