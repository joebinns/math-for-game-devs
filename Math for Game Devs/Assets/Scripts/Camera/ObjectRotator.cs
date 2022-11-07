using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectRotator : MonoBehaviour
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
        //_cursorPosition.y = Mathf.Clamp(_cursorPosition.y, -60f, 60f);

        float polar = _cursorPosition.x;
        float elevation = _cursorPosition.y;
        
        var rotation = Quaternion.AngleAxis(elevation, Vector3.right);
        rotation = Quaternion.AngleAxis(polar, Vector3.forward) * rotation;
        
        _cameraPivot.rotation = rotation;
    }
}