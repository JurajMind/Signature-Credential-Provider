using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using NDtw;
using Brush = System.Windows.Media.Brush;
using Color = System.Drawing.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace SignatureVerification
{
    public static class SupportFunctions
    {
        private static double distanceEuklid(StylusPoint a, StylusPoint b,
            Func<StylusPoint, StylusPoint, double> distancePerPoint)
        {
            return Math.Sqrt(Math.Pow(distancePerPoint.Invoke(a, b), 2) + Math.Pow(b.Y - a.Y, 2));
        }

        public static double distance(StylusPoint a, StylusPoint b, Func<StylusPoint, StylusPoint, double> distance)
        {
            return Math.Abs(distance.Invoke(a, b));
        }

        public static List<double> distance(Stroke stroke, Func<StylusPoint, StylusPoint, double> distance)
        {
            var result = new List<double>();
            result.Add(0);
            for (int i = 0; i < stroke.StylusPoints.Count - 1; i++)
            {
                result.Add(distance(stroke.StylusPoints[i], stroke.StylusPoints[i + 1]));
            }

            return result;
        }

        public static double[] acceleration(List<double> values)
        {
            var result = new List<double>();
            result.Add(0d);
            for (int i = 0; i < values.Count - 1; i++)
            {
                result.Add(Math.Abs(values[i] - values[i + 1]));
            }

            return result.ToArray();
        }


        private static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create(); //or use SHA1.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GenerateToken(string inputString)
        {
            int stringLength = inputString.Length;
            const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            var chars = new char[stringLength];
            var rd = new Random(inputString.Length);

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        public static SecureString convertToSecureString(string strPassword)
        {
            var secureStr = new SecureString();
            if (strPassword.Length > 0)
            {
                foreach (char c in strPassword.ToCharArray()) secureStr.AppendChar(c);
            }
            return secureStr;
        }

        public static string GetRandomName(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return  new string(
                Enumerable.Repeat(chars, length)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
        }

        private static  double ONE_SIXTH =
    0.16666666666666666666666666666667;
        private static double ONE_THIRD =
            0.33333333333333333333333333333333;
        private static double TWO_THIRDS =
            0.66666666666666666666666666666667;
        private static double FIVE_SIXTHS =
            0.83333333333333333333333333333333;
        public static System.Windows.Media.Color WheelColor(double d)
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
                G = d / ONE_SIXTH;
                B = 0;
            }
            else if (d < ONE_THIRD)
            {
                R = 1 - ((d - ONE_SIXTH) / ONE_SIXTH);
                B = 0;
            }
            else if (d < 0.5)
            {
                R = 0;
                B = (d - ONE_THIRD) / ONE_SIXTH;
            }
            else if (d < TWO_THIRDS)
            {
                R = 0;
                G = 1 - ((d - 0.5) / ONE_SIXTH);
            }
            else if (d < FIVE_SIXTHS)
            {
                R = (d - TWO_THIRDS) / ONE_SIXTH;
                G = 0;
            }
            else
            {
                B = 1 - ((d - FIVE_SIXTHS) / ONE_SIXTH);
                G = 0;
            }
     
            var color =  Color.FromArgb((int)(R * 255),(int)(G * 255), (int)(B * 255));

            return  System.Windows.Media.Color.FromRgb(color.R,color.B,color.G);
        }


        public static double[] Normalize(double[] values)
        {
            var max = values.Max();
            var min = values.Min();
            var range = (double)(max - min);
            return values.Select(i => 1 * (i - min) / range)
                    .ToArray();
        }

        public static String SecureStringToString(this SecureString value)
        {
            IntPtr bstr = Marshal.SecureStringToBSTR(value);

            try
            {
                return Marshal.PtrToStringBSTR(bstr);
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        public static bool IsEqualTo(this SecureString ss1, SecureString ss2)
        {
            if (ss1 == null && ss2 == null)
                return true;
            if (ss1 == null || ss2 == null)
                return false;

            IntPtr bstr1 = IntPtr.Zero;
            IntPtr bstr2 = IntPtr.Zero;
            try
            {
                bstr1 = Marshal.SecureStringToBSTR(ss1);
                bstr2 = Marshal.SecureStringToBSTR(ss2);
                int length1 = Marshal.ReadInt32(bstr1, -4);
                int length2 = Marshal.ReadInt32(bstr2, -4);
                if (length1 == length2)
                {
                    for (int x = 0; x < length1; ++x)
                    {
                        byte b1 = Marshal.ReadByte(bstr1, x);
                        byte b2 = Marshal.ReadByte(bstr2, x);
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



    public class CustomStroke : Stroke
    {
        private Brush brush;
        private System.Windows.Media.Pen pen;
        private double[] _featureValues;

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
            System.Windows.Point prevPoint = new System.Windows.Point(double.NegativeInfinity,
                double.NegativeInfinity);

            // Draw linear gradient ellipses between  
            // all the StylusPoints in the Stroke. 
            for (int i = 0; i < this.StylusPoints.Count; i++)
            {
                System.Windows.Point pt = (System.Windows.Point)this.StylusPoints[i];
                Vector v = Point.Subtract(prevPoint, pt);

                // Only draw if we are at least 4 units away  
                // from the end of the last ellipse. Otherwise,  
                // we're just redrawing and wasting cycles. 
                if (v.Length > 1)
                {
                    // Set the thickness of the stroke  
                    // based on how hard the user pressed.
                    brush = new SolidColorBrush(SupportFunctions.WheelColor(_featureValues[i]));
                    double radius = 4d;
                    drawingContext.DrawEllipse(brush, pen, pt, radius, radius);
                    prevPoint = pt;
                }
            }
        }

    }
}