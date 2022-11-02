using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterPhysics))]
public class CharacterInputs : MonoBehaviour
{
    #region Input Buffer Fields
    private float _preInputBufferDistance = 0f; // To be reset when input pressed. Waits to see if condition is met.
    private float _postInputBufferDistance = 0f; // To be reset when condition met. Waits to see if input pressed.
    private const float INPUT_BUFFER = 1.5f; // Distance
    #endregion

    private CharacterPhysics _characterPhysics;

    private void Awake()
    {
        _characterPhysics = GetComponent<CharacterPhysics>();
    }

    private void OnEnable()
    {
        _characterPhysics.OnCollision += CheckPreInputBuffer;
    }

    private void OnDisable()
    {
        _characterPhysics.OnCollision -= CheckPreInputBuffer;
    }
    
    private void Update()
    {
        InputBuffer();
    }
    
    private void InputBuffer()
    {
        // Using distance (= speed * time) for the buffer, as time felt inconsistent with varying speeds.
        _preInputBufferDistance -= Time.deltaTime * _characterPhysics.MovementSpeed;
        _postInputBufferDistance -= Time.deltaTime * _characterPhysics.MovementSpeed;
    }

    public void BounceInput(InputAction.CallbackContext context)
    {
        // When button pressed, reset coyote time counter
        if (context.performed)
        {
            if (_postInputBufferDistance >= 0f)
            {
                SuccessfulBounce();
                _postInputBufferDistance = 0f;
            }
            else
            {
                _preInputBufferDistance = INPUT_BUFFER;
            }
        }
    }

    private void CheckPreInputBuffer()
    {
        // Check input buffer
        if (_preInputBufferDistance >= 0f)
        {
            SuccessfulBounce();
            _preInputBufferDistance = 0f;
        }
        else
        {
            StandardBounce();
            // NOTE: Uncomment to turn on post input buffer.
            _postInputBufferDistance = 0f; //INPUT_BUFFER;
        }
    }   
    
    private void SuccessfulBounce()
    {
        _characterPhysics.MovementSpeedMultiplierCount = 0;
        FindObjectOfType<HitEffects>().SuccessfulBounceEffects(_characterPhysics.LatestContactPosition, _characterPhysics.LatestContactNormal);
    }
    
    private void StandardBounce()
    {
        FindObjectOfType<HitEffects>().StandardBounceEffects(_characterPhysics.LatestContactPosition, _characterPhysics.LatestContactNormal);
    }
}
