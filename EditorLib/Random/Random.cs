using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxLib.RandomGeneration
{
    public static class Rand
    {
        public static Random Random { get; private set; }

        static Rand()
        {
            Random = new Random();
        }

        public static float GetRandomFloat(float fMin, float fMax)
        {
            return (float)Random.NextDouble() * (fMax - fMin) + fMin;
        }

        public static double GetRandomDouble(double dMin, double dMax)
        {
            return Random.NextDouble() * (dMax - dMin) + dMin;
        }

        public static Vector2 GetRandomVector2(float xMin, float xMax, float yMin, float yMax)
        {
            return new Vector2(GetRandomFloat(xMin, xMax), GetRandomFloat(yMin, yMax));
        }

        public static Vector3 GetRandomVector3(float xMin, float xMax, float yMin, float yMax,float zMin, float zMax)
        {
            return new Vector3(GetRandomFloat(xMin, xMax), GetRandomFloat(yMin, yMax), GetRandomFloat(zMin, zMax));
        }

        public static int GetRandomInt(int iMin, int iMax)
        {
            return Random.Next(iMin, iMax);
        }

        public static Color GetRandomColor(float alpha = 0.5f)
        {
            return new Color(GetRandomFloat(0,255), GetRandomFloat(0, 255), GetRandomFloat(0, 255), alpha);
        }

        public static Color GetNormalizedRandomColor(float alpha = 0.5f)
        {
            return new Color((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble(), alpha);
        }

    }
}
