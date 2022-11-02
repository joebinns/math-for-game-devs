using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterPhysics))]
public class HitEffects : MonoBehaviour
{
    private CharacterPhysics _characterController;
    
    [SerializeField] private Oscillator _outerCubeOscillator;
    [SerializeField] private ParticleSystem _bounceParticleSystem;
    [SerializeField] private Material _tvMaterial;
    [SerializeField] private List<Color> _colors;
    private int _colorIndex;

    private void Awake()
    {
        _characterController = GetComponent<CharacterPhysics>();
    }

    public void StandardBounceEffects(Vector3 position, Vector3 normal)
    {
        ApplyForceToOscillator(Vector3.Dot(_characterController.MovementVelocity, normal) * normal * -12.5f);
        
        PlayBounceParticleSystem(position, normal, 1.5f);
    }

    public void SuccessfulBounceEffects(Vector3 position, Vector3 normal)
    {
        ApplyForceToOscillator(Vector3.Dot(_characterController.MovementVelocity, normal) * normal * -37.5f);

        NextColor();
        
        FindObjectOfType<ColorSelector>().Flash();

        //FindObjectOfType<CrowdEffects>().CrowdState = CrowdState.Disappointed; // TODO: Check if corner
        
        _outerCubeOscillator.transform.localPosition = Vector3.zero;
        FindObjectOfType<HitStop>().Stop();
        
        PlayBounceParticleSystem(position, normal, 4.5f);
    }
    
    private void ApplyForceToOscillator(Vector3 force)
    {
        // Apply force to oscillator
        _outerCubeOscillator.ApplyForce(force);
    }
    
    private void PlayBounceParticleSystem(Vector3 p, Vector3 n, float speedMultiplier)
    {
        // Set particle system speed
        var main = _bounceParticleSystem.main;
        main.startSpeedMultiplier = speedMultiplier;
        
        // Set particle system to face normal direction
        _bounceParticleSystem.transform.LookAt(n); // Face normal
        //_bounceParticleSystem.transform.LookAt(_movementVelocity); // Face new velocity

        // Set particles start position
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = p;
        emitParams.applyShapeToPosition = true;

        // Trigger particle emission burst
        _bounceParticleSystem.Emit(emitParams, 6);
    }
    
    private void NextColor()
    {
        var increment = _colors.Count / 2;
        if (_colors.Count % 2 == 0)
        {
            // If even number of colours, then increase the increment in order to go up an odd amount (So that no colours are left unused).
            increment++;
        }
        _colorIndex += increment;
        _colorIndex %= _colors.Count;
        var color = _colors[_colorIndex];
        _tvMaterial.color = color;
    }

}
