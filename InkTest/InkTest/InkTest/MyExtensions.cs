using System;
using NDtw;

namespace InkTest
{
    internal static class MyExtensions
    {
        public static double normLength(Dtw dtw)
        {
            int xLength = dtw.XLength;
            int yLength = dtw.YLength;
            double cost = dtw.GetCost();
            return cost/Math.Sqrt(xLength*xLength + yLength*yLength);
        }
    }
}