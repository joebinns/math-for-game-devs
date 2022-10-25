using UnityEngine;

namespace Utilities
{
    public static class Maths
    {
        public static void SineRule(out float a, float alpha, float b, float beta)
        {
            a = b * Mathf.Sin(alpha) / Mathf.Sin(beta);
        }

        public static void SineRule(float a, out float alpha, float b, float beta)
        {
            alpha = Mathf.Asin(a * Mathf.Sin(beta) / b);
        }

        public static void CosineRule(float a, out float alpha, float b, float c)
        {
            alpha = Mathf.Acos((Mathf.Pow(b, 2) + Mathf.Pow(c, 2) - Mathf.Pow(a, 2)) / (2 * b * c));
        }
        
        public static void CosineRule(out float a, float alpha, float b, float c)
        {
            a = Mathf.Sqrt(Mathf.Pow(b, 2) + Mathf.Pow(c, 2) - 2 * b * c * Mathf.Cos(alpha));
        }
    }
}
