using System.Collections.Generic;
using UnityEngine;

public class CrowdEffects : MonoBehaviour
{
    [SerializeField] private List<Vibrate> _vibrates = new List<Vibrate>();
    [SerializeField] private CharacterPhysics _characterPhysics;
    [SerializeField] private List<Transform> _corners = new List<Transform>();

    private Animator _animator;
    private CrowdState _crowdState = CrowdState.Bored;
    
    public CrowdState CrowdState
    {
        get => _crowdState;
        set => SetCrowdState(value);
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        var displacementToNearestCorner = CalculateDisplacementToNearestCorner();
        var proximityToCorner = Mathf.Clamp01(1f - Mathf.Pow(displacementToNearestCorner.magnitude, 1));
        foreach (var vibrate in _vibrates)
        {
            vibrate.Rate = 8f * proximityToCorner;
        }
        
        // Take dot product of velocity and vector to nearest corner.
        // Use this to adjust magnitude.
        var alignment = Vector3.Dot(_characterPhysics.MovementDirection, displacementToNearestCorner.normalized); // '+1' for directly towards corner '-1' for directly away from corner. 
        alignment = Mathf.Clamp(alignment, 0f, 1f);
        alignment = Mathf.Pow(alignment, 4);
        
        foreach (var vibrate in _vibrates)
        {
            vibrate.Magnitude = Mathf.Lerp(vibrate.Magnitude, 2f * alignment, 0.1f); // TODO: Try using 0.5f * Time.fixedDeltaTime for t.
        }

        if (alignment > 0.5f)
        {
            CrowdState = CrowdState.Excited;
        }
        
        // When you hit a wall, if it is not a corner, then trigger disappointed
    }

    private Vector3 CalculateDisplacementToNearestCorner()
    {
        var shortestDisplacement = Vector3.one * 100f;
        // Check nearest corner
        foreach (Transform corner in _corners)
        {
            // Calculate displacement
            var displacement = (corner.position - transform.position);
            // Store shortest displacement
            if (displacement.magnitude < shortestDisplacement.magnitude)
            {
                shortestDisplacement = displacement;
            }
        }

        var maximumDistance = (_corners[0].position - Vector3.zero).magnitude; //_outerCube.lossyScale.magnitude / 2f;
        var displacementToNearestCorner = shortestDisplacement / maximumDistance;
        return (displacementToNearestCorner);
    }

    private void SetCrowdState(CrowdState crowdState)
    {
        switch (crowdState)
        {
            case CrowdState.Bored:
                break;
            case CrowdState.Excited:
                _animator.SetTrigger("Excited");
                break;
            case CrowdState.Disappointed:
                _animator.SetTrigger("Disappointed");
                break;
            case CrowdState.Ecstatic:
                _animator.SetTrigger("Ecstatic");
                break;
        }
        _crowdState = crowdState; // TODO: Handle this exclusively within the animator!
    }
}

public enum CrowdState
{
    Bored,
    Excited,
    Disappointed,
    Ecstatic
}