using UnityEngine;
using UnityEngine.InputSystem;

public class CameraRotator : MonoBehaviour
{
    private Vector2 _cursorPosition = Vector3.zero;
    private Vector2 _cursorDelta;
    private Transform _cameraPivot;

    private void Awake()
    {
        _cameraPivot = this.transform;
        CursorMode.DisableCursor();
    }
    
    public void CursorDelta(InputAction.CallbackContext context)
    {
        _cursorDelta = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        _cursorPosition += _cursorDelta;
        _cursorPosition.y = Mathf.Clamp(_cursorPosition.y, -60f, 60f);
        
        var rotation = _cameraPivot.rotation.eulerAngles;
        rotation.x = - _cursorPosition.y;
        rotation.y = _cursorPosition.x;

        _cameraPivot.rotation = Quaternion.Euler(rotation);
    }
}
