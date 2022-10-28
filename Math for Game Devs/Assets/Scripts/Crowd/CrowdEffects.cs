using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdEffects : MonoBehaviour
{
    public Vector3 CurrentDirection;
    public Vector3 DisplacementToNearestCorner;
    [SerializeField] private List<Vibrate> _vibrates = new List<Vibrate>();
    
    
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
        var proximityToCorner = Mathf.Clamp01(1f - Mathf.Pow(DisplacementToNearestCorner.magnitude, 1));
        foreach (var vibrate in _vibrates)
        {
            vibrate.Rate = 8f * proximityToCorner;
        }
        
        // Take dot product of velocity and vector to nearest corner.
        // Use this to adjust magnitude.
        var alignment = Vector3.Dot(CurrentDirection, DisplacementToNearestCorner.normalized); // '+1' for directly towards corner '-1' for directly away from corner. 
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