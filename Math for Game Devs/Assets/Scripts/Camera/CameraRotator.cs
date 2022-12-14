using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float _sensitivity = 0.15f;
    
    private Vector2 _cursorPosition = Vector3.zero;
    private Vector2 _cursorDelta;
    private Transform _cameraPivot;
    private Quaternion _initialRotation;

    private void Awake()
    {
        _cameraPivot = this.transform;
        _initialRotation = _cameraPivot.rotation;
        CursorMode.DisableCursor();
    }
    
    public void CursorDelta(InputAction.CallbackContext context)
    {
        _cursorDelta = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        _cursorPosition += _cursorDelta * _sensitivity;
        _cursorPosition.y = Mathf.Clamp(_cursorPosition.y, -60f, 60f);

        var rotation = _initialRotation.eulerAngles; // TODO: Warning: This is gimbal-locked :(
        rotation.x += - _cursorPosition.y;
        rotation.y += _cursorPosition.x;
        
        _cameraPivot.rotation = Quaternion.Euler(rotation);
    }
}
