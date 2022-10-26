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
        foreach (var vibrate in _vibrates)
        {
            vibrate.Rate = 4f * (1f - Mathf.Pow(DisplacementToNearestCorner.magnitude, 2));
        }
        
        // TODO: Instead of enabling and disabling vibrate...
        // Take dot product of velocity and vector to nearest corner.
        // Use this to adjust magnitude (?).
        var alignment = Vector3.Dot(CurrentDirection, DisplacementToNearestCorner); // '+1' for directly towards corner '-1' for directly away from corner. 
        alignment = Mathf.Clamp(alignment, 0f, 1f);
        
        foreach (var vibrate in _vibrates)
        {
            //vibrate.Magnitude = 2f * alignment; // TODO: Lerp this...
            vibrate.Magnitude = Mathf.Lerp(vibrate.Magnitude, 2f * alignment, 0.1f);
        }
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
                _animator.SetTrigger("Dissapointed");
                break;
            case CrowdState.Ecstatic:
                // TODO: Create ecstatic animation
                break;
        }
        _crowdState = crowdState;
    }
}

public enum CrowdState
{
    Bored,
    Excited,
    Disappointed,
    Ecstatic
}