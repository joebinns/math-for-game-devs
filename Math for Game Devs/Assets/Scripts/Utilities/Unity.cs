using UnityEngine;

namespace Utilities
{
    public static class Unity
    {
        public static (bool, RaycastHit) Raycast(Vector3 rayOrigin, Vector3 rayVector, int layerToIgnore)
        {
            RaycastHit raycastHit;
            Ray ray = new Ray(rayOrigin, rayVector.normalized);
            int layerMask = ~layerToIgnore;
            bool didRayHit = Physics.Raycast(ray, out raycastHit, rayVector.magnitude, layerMask);
            //Debug.DrawRay(transform.position, rayVector, Color.blue);
            return (didRayHit, raycastHit);
        }
    }
}
