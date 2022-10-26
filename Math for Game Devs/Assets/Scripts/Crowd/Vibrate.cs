using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

public class Vibrate : MonoBehaviour
{
    public float Rate = 1f;
    public float Magnitude = 1f;
    [SerializeField] [Range(1, 4)] private int _octaves = 1;
    [SerializeField] private bool[] _localActiveAxes = new bool[3];

    private float _t = 0f;
    private float _defaultMagnitude;
    private Vector3 _defaultPosition;

    private void Awake()
    {
        _defaultMagnitude = Magnitude;
        _defaultPosition = transform.position;
    }

    private void FixedUpdate()
    {
        _t += Time.fixedDeltaTime * Rate;
        var positionNoise = Vector3.zero;
        var rotationNoise = Vector3.zero;
        for (int i = 0; i < 3; i++)
        {
            if (!_localActiveAxes[i])
            {
                rotationNoise[i] = Maths.CompositePerlinNoise(_t, i, _octaves) - 0.5f;
                continue;
            }
            positionNoise[i] = Maths.CompositePerlinNoise(_t, i, _octaves) - 0.5f;
        }
        transform.localPosition = _defaultPosition + (Magnitude * positionNoise);
        transform.localRotation = Quaternion.Euler(Mathf.Rad2Deg * 0.1f * Magnitude * rotationNoise);
    }
}
