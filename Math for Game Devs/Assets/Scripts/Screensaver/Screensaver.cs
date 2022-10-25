using System;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

public class Screensaver : MonoBehaviour
{
    [SerializeField] private Transform logo;
    [SerializeField] private float _movementSpeed = 1;
    
    private Vector3 _movementDirection;
    private Vector3 _previousMovementDirection;

    private void Start()
    {
        // Start moving in an arbitrary direction.
        _movementDirection = Random.onUnitSphere;
        _previousMovementDirection = _movementDirection;
    }

    private void FixedUpdate()
    {
        logo.position += _movementDirection * _movementSpeed * Time.fixedDeltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
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
        
        _previousMovementDirection = _movementDirection;
        _movementDirection = b;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Debug.DrawRay(logo.position, _movementDirection * 1.5f, Gizmos.color);
        Gizmos.color = Color.red;
        Debug.DrawRay(logo.position, -_previousMovementDirection * 1.5f, Gizmos.color);
    }
}
