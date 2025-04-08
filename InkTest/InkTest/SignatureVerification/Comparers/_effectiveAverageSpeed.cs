using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using NDtw;
using NDtw.Visualization.Wpf;

namespace SignatureVerification
{
    public class _effectiveAverageSpeed : SignatureComparer
    {
        private bool _boundaryConstrainStart = true;
        private bool _boundaryConstraintEnd = true;
        private DistanceMeasure _distanceMeasure = DistanceMeasure.Euclidean;
        private Dtw _dtw;

        public _effectiveAverageSpeed()
            : base(new Guid("34E8BC40-AC54-496B-A2E7-14E0676F68BB"))
        {
        }

        public override double Compare(Stroke signature, Stroke patern, double weight)
        {
            var patternLength = SupportFunctions.distance(patern, (a, b) => Math.Sqrt(Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.X - b.X, 2))).Sum();
            var signatureLength = SupportFunctions.distance(signature, (a, b) => Math.Sqrt(Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.X - b.X, 2))).Sum();

            var patternAverageSpeed = patternLength/patern.StylusPoints.Count;
            var signatureAverageSpeed = signatureLength/signature.StylusPoints.Count;

            return weight* Math.Abs(patternLength - signatureLength) * 
                Math.Abs(patternAverageSpeed - signatureAverageSpeed);


        }

        public override UIElement DrawnGui(Stroke signature, Stroke patern, double weight)
        {
            var result = new Border();

            var stack = new StackPanel();


            var textBlock = new TextBlock();
            textBlock.Text = "Effective average speed";
            stack.Children.Add(textBlock);

            stack.Children.Add(new TextBlock() { Text = "Pattern Length:" });
            var patternLength = SupportFunctions.distance(patern, (a, b) => Math.Sqrt(Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.X - b.X, 2))).Sum();
            stack.Children.Add(new TextBlock() { Text = patternLength.ToString("F") });

            stack.Children.Add(new TextBlock() { Text = "Signature Length:" });
            var signatureLength = SupportFunctions.distance(signature, (a, b) => Math.Sqrt(Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.X - b.X, 2))).Sum();
            stack.Children.Add(new TextBlock() { Text = signatureLength.ToString("F") });

            stack.Children.Add(new TextBlock() { Text = "Pattern speed:" });
            var patternAverageSpeed = patternLength / patern.StylusPoints.Count;
            stack.Children.Add(new TextBlock() { Text = patternAverageSpeed.ToString("F") });

            stack.Children.Add(new TextBlock() { Text = "Signature speed:" });
            var signatureAverageSpeed = signatureLength / signature.StylusPoints.Count;
            stack.Children.Add(new TextBlock() { Text = signatureAverageSpeed.ToString("F") });

            stack.Children.Add(new TextBlock() { Text = "Gain:" });
            var resultOfCompare =  weight* Math.Abs(patternLength - signatureLength) * 
                Math.Abs(patternAverageSpeed - signatureAverageSpeed);

            stack.Children.Add(new TextBlock() {Text = resultOfCompare.ToString("F")});
          

            return stack;
        }

        public _effectiveAverageSpeed SetSettings(DistanceMeasure distanceMeasure = DistanceMeasure.Euclidean,
            bool boundaryConstrainStart = true, bool boundaryConstraintEnd = true)
        {
            _distanceMeasure = distanceMeasure;
            _boundaryConstrainStart = boundaryConstrainStart;
            _boundaryConstraintEnd = boundaryConstrainStart;
            return this;
        }

        public override double CalculateWeights(List<Stroke> patterns)
        {
            var partialThreshold = new List<double>();

            for (int i = 0; i < patterns.Count; i++)
            {
                for (int j = 0; j < patterns.Count; j++)
                {
                    if (i == j)
                        continue;

                    partialThreshold.Add(Compare(patterns[i], patterns[j], 1));
                }
            }


            double avrg = partialThreshold.Average();
            double min = partialThreshold.Min();
            double max = partialThreshold.Max();
            return 20*(max + min) / avrg;
        }

        public override string GetName()
        {
            return "Effective average speed";
        }
    }
}