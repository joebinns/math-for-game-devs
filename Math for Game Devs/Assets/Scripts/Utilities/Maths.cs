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

        public static float CompositePerlinNoise(float x, float y, int octaves = 1)
        {
            var noise = 0f;
            var frequency = 1f;
            for (int i = 0; i < octaves; i++)
            {
                noise += Mathf.PerlinNoise(frequency * x, frequency * y);
                frequency *= 2f;
            }
            return noise / (float)octaves;
        }

        public static Vector3 ComponentWiseProduct(Vector3 a, Vector3 b)
        {
            var product = Vector3.zero;
            product.x = a.x * b.x;
            product.y = a.y * b.y;
            product.z = a.z * b.z;
            return product;
        }

        public static Vector3 Absolute(Vector3 a)
        {
            a.x = Mathf.Abs(a.x);
            a.y = Mathf.Abs(a.y);
            a.z = Mathf.Abs(a.z);
            return a;
        }
    }
}
