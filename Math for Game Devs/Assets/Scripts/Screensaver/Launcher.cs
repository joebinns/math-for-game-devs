using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Launcher : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 0.015f;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _container;

    private bool _hasHeldRegistered;
    private Vector2 _cursorPosition = Vector3.zero;
    private Vector2 _cursorDelta;
    private Vector3 _velocity;

    public void CursorDelta(InputAction.CallbackContext context)
    {
        _cursorDelta = context.ReadValue<Vector2>();
    }
    
    private void Update()
    {
        // Update cursor position
        _cursorPosition += _cursorDelta * _sensitivity;
        
        // Move line renderer start position to cursor position;
        _velocity = new Vector3(Mathf.Clamp(_cursorPosition.x, -0.45f, 0.45f) * _container.lossyScale.x,
            Mathf.Clamp(_cursorPosition.y, -0.45f, 0.45f) * _container.lossyScale.y, 0f);
        _lineRenderer.SetPosition(0, _velocity);
    }

    public void Launch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _hasHeldRegistered = false;
            
            // Disable camera rotator
            FindObjectOfType<CameraRotator>().enabled = false;
            
            // Enable line renderer, reset cursor position
            _lineRenderer.enabled = true;
            _cursorPosition = Vector3.zero;
        }
        else if (context.performed)
        {
            FindObjectOfType<Screensaver>().MovementSpeed = _velocity.magnitude * 0f;
            FindObjectOfType<Screensaver>().transform.position = Vector3.zero;
            _hasHeldRegistered = true;
        }
        else if (context.canceled)
        {
            if (_hasHeldRegistered)
            {
                // Fire!
                FindObjectOfType<Screensaver>().MovementSpeed = _velocity.magnitude * 2.5f; // TODO: SOMETHING WITH LOSING SPEED AFTER EACH HIT? GAIN SPEED WHEN CORNER HIT!
                FindObjectOfType<Screensaver>().MovementDirection = -_velocity.normalized;
            }
            else
            {
                // Cancel
            }
            _lineRenderer.enabled = false;
            FindObjectOfType<CameraRotator>().enabled = true;
        }
    }
    
    
}
