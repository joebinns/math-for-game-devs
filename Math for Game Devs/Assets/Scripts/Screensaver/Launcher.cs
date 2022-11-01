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
    
    private Vector2 _cursorPosition = Vector3.zero;
    private Vector2 _cursorDelta;
    private Vector3 _launchVelocity;

    public void CursorDelta(InputAction.CallbackContext context)
    {
        _cursorDelta = context.ReadValue<Vector2>();
    }
    
    private void Update()
    {
        // Update cursor position
        _cursorPosition += _cursorDelta * _sensitivity;
        _cursorPosition.x = Mathf.Clamp(_cursorPosition.x, -0.5f, 0.5f);
        _cursorPosition.y = Mathf.Clamp(_cursorPosition.y, -0.5f, 0.5f);
        
        // Move line renderer start position to cursor position;
        var containerCursorPosition = new Vector3(_cursorPosition.x * _container.lossyScale.x, _cursorPosition.y * _container.lossyScale.y, 0f);
        _lineRenderer.SetPosition(0, containerCursorPosition);
        
        _launchVelocity = new Vector3(containerCursorPosition.normalized.x, containerCursorPosition.normalized.y, 0f);
    }

    public void Launch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Disable camera rotator
            FindObjectOfType<CameraRotator>().enabled = false;
            
            // Enable line renderer, reset cursor position
            _lineRenderer.enabled = true;
            _cursorPosition = Vector3.zero;
            
            // Reset cube
            // TODO: Change this to Lerp / spring.
            FindObjectOfType<CharacterController>().BaseMovementSpeed = _launchVelocity.magnitude * 0f;
            FindObjectOfType<CharacterController>().transform.position = Vector3.zero;
        }
        else if (context.canceled)
        {
            // Fire!
            FindObjectOfType<CharacterController>().MovementSpeedMultiplierCount = 0;
            FindObjectOfType<CharacterController>().BaseMovementSpeed = _launchVelocity.magnitude * 20f;
            FindObjectOfType<CharacterController>().MovementDirection = -_launchVelocity.normalized;

            _lineRenderer.enabled = false;
            FindObjectOfType<CameraRotator>().enabled = true;
        }
    }


}
