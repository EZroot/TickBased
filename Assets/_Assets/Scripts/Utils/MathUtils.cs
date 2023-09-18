using System;

namespace TickBased.Utils
{
    public static class MathUtils
    {
        public static float CalculateDistance(int x, int y, int x1, int y1)
        {
            int deltaX = x1 - x;
            int deltaY = y1 - y;
            return (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
    }
}