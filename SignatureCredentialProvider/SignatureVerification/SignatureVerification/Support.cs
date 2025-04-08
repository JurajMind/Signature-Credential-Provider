using NDtw;

namespace SignatureVerification
{
    internal static class Support
    {
        /// <summary>
        ///     Function that use normLengt for dtw calculation, in final solusion is NOT USE,
        ///     thats the reason that it return 1
        /// </summary>
        /// <param name="dtw"></param>
        /// <returns></returns>
        public static double normLength(Dtw dtw)
        {
            var xLength = dtw.XLength;
            var yLength = dtw.YLength;
            var cost = dtw.GetCost();
            //return 1/Math.Sqrt(xLength*xLength + yLength*yLength);
            return 1;
        }
    }
}