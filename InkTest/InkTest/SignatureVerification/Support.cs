using System;
using NDtw;

namespace SignatureVerification
{
    internal static class Support
    {
        public static double normLength(Dtw dtw)
        {
            int xLength = dtw.XLength;
            int yLength = dtw.YLength;
            double cost = dtw.GetCost();
            //return 1/Math.Sqrt(xLength*xLength + yLength*yLength);
            return 1;
        }
    }
}