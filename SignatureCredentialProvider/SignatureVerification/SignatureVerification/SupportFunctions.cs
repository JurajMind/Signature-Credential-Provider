using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace SignatureVerification
{
    /// <summary>
    ///     Class that covers all lonly support functions
    /// </summary>
    public static class SupportFunctions
    {
        private static readonly double ONE_SIXTH =
            0.16666666666666666666666666666667;

        private static readonly double ONE_THIRD =
            0.33333333333333333333333333333333;

        private static readonly double TWO_THIRDS =
            0.66666666666666666666666666666667;

        private static readonly double FIVE_SIXTHS =
            0.83333333333333333333333333333333;

        /// <summary>
        ///     Caltulation of euklid distance between two given stylus points and function to compute distance
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="distancePerPoint">Function tthat for two poitns return their distance</param>
        /// <returns></returns>
        private static double distanceEuklid(StylusPoint a, StylusPoint b,
            Func<StylusPoint, StylusPoint, double> distancePerPoint)
        {
            return Math.Sqrt(Math.Pow(distancePerPoint.Invoke(a, b), 2) + Math.Pow(b.Y - a.Y, 2));
        }

        /// <summary>
        ///     Basic distance between two points by function
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="distance">function for calculation distnance betwrrn given point</param>
        /// <returns></returns>
        public static double distance(StylusPoint a, StylusPoint b, Func<StylusPoint, StylusPoint, double> distance)
        {
            return Math.Abs(distance.Invoke(a, b));
        }

        /// <summary>
        ///     Funtion that compute distance between adjancet stylus point in stroke
        /// </summary>
        /// <param name="stroke"></param>
        /// <param name="distance">Function for calculating distance between two points</param>
        /// <returns></returns>
        public static List<double> distance(Stroke stroke, Func<StylusPoint, StylusPoint, double> distance)
        {
            var result = new List<double>();
            result.Add(0);
            for (var i = 0; i < stroke.StylusPoints.Count - 1; i++)
            {
                result.Add(distance(stroke.StylusPoints[i], stroke.StylusPoints[i + 1]));
            }

            return result;
        }

        /// <summary>
        ///     "Derivate" function, that use fact that points are equaly distance in time
        /// </summary>
        /// <param name="values">Double values to calculate</param>
        /// <returns></returns>
        public static double[] acceleration(List<double> values)
        {
            var result = new List<double>();
            result.Add(0d);
            for (var i = 0; i < values.Count - 1; i++)
            {
                result.Add(Math.Abs(values[i] - values[i + 1]));
            }

            return result.ToArray();
        }

        /// <summary>
        ///     Function for generating acces token from inputString. We use user password as input string
        ///     This function return string substain only with alfanumeric chars and only !@$?_-
        /// </summary>
        /// <param name="inputString">Seed of generator</param>
        /// <returns></returns>
        public static string GenerateToken(string inputString)
        {
            var stringLength = inputString.Length;
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            var chars = new char[stringLength];
            var rd = new Random(inputString.Length);

            for (var i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        /// <summary>
        ///     Suppor function that create secury string from standard string
        /// </summary>
        /// <param name="strPassword"></param>
        /// <returns></returns>
        public static SecureString convertToSecureString(string strPassword)
        {
            var secureStr = new SecureString();
            if (strPassword.Length > 0)
            {
                foreach (var c in strPassword) secureStr.AppendChar(c);
            }
            return secureStr;
        }

        /// <summary>
        ///     Function that create random alphanumeric string by given lenght
        /// </summary>
        /// <param name="length">lenght of random alphanumeric string</param>
        /// <returns></returns>
        public static string GetRandomName(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
        }

        /// <summary>
        ///     Function that returns collor from double walue between 0-1
        /// </summary>
        /// <param name="d">color coded value , muse be between 0-1</param>
        /// <returns></returns>
        public static Color WheelColor(double d)
        {
            if ((d < 0.0) || (d > 1.0))
            {
                throw new ArgumentOutOfRangeException("d",
                    d, "d must be between 0.0 and 1.0, inclusive");
            }
            double R = 1;
            double G = 1;
            double B = 1;
            if (d < ONE_SIXTH)
            {
                G = d/ONE_SIXTH;
                B = 0;
            }
            else if (d < ONE_THIRD)
            {
                R = 1 - ((d - ONE_SIXTH)/ONE_SIXTH);
                B = 0;
            }
            else if (d < 0.5)
            {
                R = 0;
                B = (d - ONE_THIRD)/ONE_SIXTH;
            }
            else if (d < TWO_THIRDS)
            {
                R = 0;
                G = 1 - ((d - 0.5)/ONE_SIXTH);
            }
            else if (d < FIVE_SIXTHS)
            {
                R = (d - TWO_THIRDS)/ONE_SIXTH;
                G = 0;
            }
            else
            {
                B = 1 - ((d - FIVE_SIXTHS)/ONE_SIXTH);
                G = 0;
            }

            var color = System.Drawing.Color.FromArgb((int) (R*255), (int) (G*255), (int) (B*255));

            return Color.FromRgb(color.R, color.B, color.G);
        }

        //Function that normalaze given array to range 0-1
        public static double[] Normalize(double[] values)
        {
            var max = values.Max();
            var min = values.Min();
            var range = max - min;
            return values.Select(i => 1*(i - min)/range)
                .ToArray();
        }

        /// <summary>
        ///     Convert function that return true string from secure string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SecureStringToString(this SecureString value)
        {
            var bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        /// <summary>
        ///     Comparer function to secure strings, two null secure strings ARE EQUAL
        /// </summary>
        /// <param name="ss1"></param>
        /// <param name="ss2"></param>
        /// <returns></returns>
        public static bool IsEqualTo(this SecureString ss1, SecureString ss2)
        {
            if (ss1 == null && ss2 == null)
                return true;
            if (ss1 == null || ss2 == null)
                return false;

            var bstr1 = IntPtr.Zero;
            var bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                var length1 = Marshal.ReadInt32(bstr1, -4);
                var length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    for (var x = 0; x < length1; ++x)
                    {
                        var b1 = Marshal.ReadByte(bstr1, x);
                        var b2 = Marshal.ReadByte(bstr2, x);
                        if (b1 != b2) return false;
                    }
                }
                else return false;
                return true;
            }
            finally
            {
                if (bstr2 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr2);
                if (bstr1 != IntPtr.Zero) Marshal.ZeroFreeBSTR(bstr1);
            }
        }
    }

    /// <summary>
    ///     Class that inherence stroke class, this custom stroke have
    ///     capability to colorize each stylus point by diferent color,
    ///     we use it for vizualization of signature feature
    /// </summary>
    public class CustomStroke : Stroke
    {
        private readonly double[] _featureValues;
        private readonly Pen pen;
        private Brush brush;

        public CustomStroke(StylusPointCollection stylusPoints, double[] featureValues)
            : base(stylusPoints)
        {
            // Create the Brush and Pen used for drawing.

            pen = new Pen(brush, 1d);
            _featureValues = featureValues;
        }

        protected override void DrawCore(DrawingContext drawingContext,
            DrawingAttributes drawingAttributes)
        {
            // Allocate memory to store the previous point to draw from.
            var prevPoint = new Point(double.NegativeInfinity,
                double.NegativeInfinity);

            // Draw linear gradient ellipses between  
            // all the StylusPoints in the Stroke. 
            for (var i = 0; i < StylusPoints.Count; i++)
            {
                var pt = (Point) StylusPoints[i];
                var v = Point.Subtract(prevPoint, pt);

                // Only draw if we are at least 4 units away  
                // from the end of the last ellipse. Otherwise,  
                // we're just redrawing and wasting cycles. 
                if (v.Length > 1)
                {
                    // Set the thickness of the stroke  
                    // based on how hard the user pressed.
                    brush = new SolidColorBrush(SupportFunctions.WheelColor(_featureValues[i]));
                    var radius = 4d;
                    drawingContext.DrawEllipse(brush, pen, pt, radius, radius);
                    prevPoint = pt;
                }
            }
        }
    }
}