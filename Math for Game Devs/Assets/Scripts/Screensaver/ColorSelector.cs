using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSelector : MonoBehaviour
{
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _flashColor;
    [SerializeField] private float _duration;
    [SerializeField] private List<Renderer> _renderersToFlash;
    [SerializeField] private bool _shouldFlashCameraBackground;

    public void Flash() // TODO: Make shader to have new colour spread out from hit point.
    {
        StartCoroutine(FlashCoroutine());
    }
    
    private IEnumerator FlashCoroutine()
    {
        ChangeColors(_flashColor);
        yield return new WaitForSeconds(_duration);
        ChangeColors(_defaultColor);
    }

    private void ChangeColors(Color color)
    {
        // Camera background colors
        if (_shouldFlashCameraBackground)
        {
            Camera.main.backgroundColor = color;
        }
        
        // Change renderer colors
        foreach (var renderer in _renderersToFlash)
        {
            if (renderer is SpriteRenderer spriteRenderer)
            {
                // If the renderer is for a sprite, then change the renderer color directly
                spriteRenderer.color = color;
                continue;
            }
            // Otherwise, change the material color
            renderer.material.color = color;
        }
    }
}
