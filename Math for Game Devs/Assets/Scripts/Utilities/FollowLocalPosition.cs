using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLocalPosition : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
     
    private void FixedUpdate()
    {
        transform.localPosition = followTarget.localPosition;
    }
}
